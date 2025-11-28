using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ERAMonitor.Core.Configuration;
using ERAMonitor.API.Middleware;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Services;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Data.Repositories;
using ERAMonitor.Infrastructure.Repositories;
using ERAMonitor.BackgroundJobs.Jobs;
using ERAMonitor.BackgroundJobs;
using ERAMonitor.BackgroundJobs.Services;
using ERAMonitor.Infrastructure.Jobs;
using ERAMonitor.API.Services;
using ERAMonitor.API.Hubs;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.OpenApi.Models;
using ERAMonitor.Core.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:3008",
            "http://dashboard:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ERA Monitor API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IHeartbeatService, HeartbeatService>();
builder.Services.AddScoped<HttpCheckerJob>();
builder.Services.AddHostedService<CheckSchedulerService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IStatusPageService, StatusPageService>();
builder.Services.AddScoped<ReportGeneratorJob>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddSignalR();

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IHostRepository, HostRepository>();
builder.Services.AddScoped<IHostMetricRepository, HostMetricRepository>();
builder.Services.AddScoped<IHostDiskRepository, HostDiskRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceStatusHistoryRepository, ServiceStatusHistoryRepository>();
builder.Services.AddScoped<IEventLogRepository, EventLogRepository>();
builder.Services.AddScoped<ICheckRepository, CheckRepository>();
builder.Services.AddScoped<ICheckResultRepository, CheckResultRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IStatusPageRepository, StatusPageRepository>();
builder.Services.AddScoped<IStatusPageComponentRepository, StatusPageComponentRepository>();
builder.Services.AddScoped<IStatusPageComponentGroupRepository, StatusPageComponentGroupRepository>();
builder.Services.AddScoped<IStatusPageSubscriberRepository, StatusPageSubscriberRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Phase 2 Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Phase 3 Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IHostService, HostService>();
builder.Services.AddScoped<IServiceMonitorService, ServiceMonitorService>();
builder.Services.AddScoped<IRealTimeService, SignalRRealTimeService>();
builder.Services.AddScoped<ICheckExecutorService, CheckExecutorService>();
builder.Services.AddScoped<CheckJob>();
builder.Services.AddHttpContextAccessor();

// Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "super_secret_key_that_should_be_long_enough_at_least_32_bytes");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Set to true in production
        ValidateAudience = false // Set to true in production
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole(UserRole.SuperAdmin.ToString(), UserRole.Admin.ToString()));

    options.AddPolicy("RequireOperatorRole", policy => 
        policy.RequireRole(
            UserRole.SuperAdmin.ToString(), 
            UserRole.Admin.ToString(), 
            UserRole.Operator.ToString()));
});

var app = builder.Build();

// Run migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // Ensure legacy columns exist when using an existing database
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        // Ensure all User columns exist
        var columnsToCheck = new[]
        {
            ("EmailVerificationTokenExpires", "timestamp with time zone NULL"),
            ("JobTitle", "text NULL"),
            ("LastLoginIp", "text NULL"),
            ("LastLoginAt", "timestamp with time zone NULL"),
            ("FailedLoginAttempts", "integer NOT NULL DEFAULT 0"),
            ("LockedUntil", "timestamp with time zone NULL"),
            ("TwoFactorEnabled", "boolean NOT NULL DEFAULT false"),
            ("TwoFactorSecret", "text NULL"),
            ("TwoFactorBackupCodes", "text[] NULL"),
            ("PasswordResetToken", "text NULL"),
            ("PasswordResetTokenExpires", "timestamp with time zone NULL"),
            ("EmailVerificationToken", "text NULL"),
            ("Theme", "text NOT NULL DEFAULT 'light'"),
            ("Locale", "text NOT NULL DEFAULT 'en'"),
            ("Timezone", "text NOT NULL DEFAULT 'UTC'"),
            ("NotificationPreferences", "text NULL"),
            ("Permissions", "text NULL"),
            ("AvatarUrl", "text NULL"),
            ("PhoneNumber", "text NULL")
        };

        foreach (var (colName, colType) in columnsToCheck)
        {
            await context.Database.ExecuteSqlRawAsync($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Users' AND column_name = '{colName}'
                    ) THEN
                        ALTER TABLE ""Users"" ADD COLUMN ""{colName}"" {colType};
                    END IF;
                END
                $$;");
        }

        // Ensure all Host columns exist
        var hostColumnsToCheck = new[]
        {
            ("PrimaryIp", "text NULL"),
            ("PublicIp", "text NULL"),
            ("Hostname", "text NULL"),
            ("Description", "text NULL"),
            ("OsType", "integer NOT NULL DEFAULT 0"),
            ("OsVersion", "text NULL"),
            ("Category", "integer NOT NULL DEFAULT 0"),
            ("Tags", "text[] NULL"),
            ("AgentInstalledAt", "timestamp with time zone NULL"),
            ("AgentVersion", "text NULL"),
            ("CheckIntervalSeconds", "integer NOT NULL DEFAULT 60"),
            ("CurrentStatus", "integer NOT NULL DEFAULT 0"),
            ("LastSeenAt", "timestamp with time zone NULL"),
            ("LastHeartbeat", "text NULL"),
            ("StatusChangedAt", "timestamp with time zone NULL"),
            ("PreviousStatus", "integer NULL"),
            ("UptimeSeconds", "bigint NULL"),
            ("CpuPercent", "numeric NULL"),
            ("RamPercent", "numeric NULL"),
            ("RamUsedMb", "bigint NULL"),
            ("RamTotalMb", "bigint NULL"),
            ("ProcessCount", "integer NULL"),
            ("CpuWarningThreshold", "integer NOT NULL DEFAULT 80"),
            ("CpuCriticalThreshold", "integer NOT NULL DEFAULT 95"),
            ("RamWarningThreshold", "integer NOT NULL DEFAULT 80"),
            ("RamCriticalThreshold", "integer NOT NULL DEFAULT 95"),
            ("DiskWarningThreshold", "integer NOT NULL DEFAULT 80"),
            ("DiskCriticalThreshold", "integer NOT NULL DEFAULT 95"),
            ("MonitoringEnabled", "boolean NOT NULL DEFAULT true"),
            ("AlertOnDown", "boolean NOT NULL DEFAULT true"),
            ("AlertDelaySeconds", "integer NOT NULL DEFAULT 60"),
            ("AlertOnHighCpu", "boolean NOT NULL DEFAULT true"),
            ("AlertOnHighRam", "boolean NOT NULL DEFAULT true"),
            ("AlertOnHighDisk", "boolean NOT NULL DEFAULT true"),
            ("MaintenanceMode", "boolean NOT NULL DEFAULT false"),
            ("MaintenanceStartAt", "timestamp with time zone NULL"),
            ("MaintenanceEndAt", "timestamp with time zone NULL"),
            ("MaintenanceReason", "text NULL"),
            ("Notes", "text NULL"),
            ("IsActive", "boolean NOT NULL DEFAULT true")
        };

        foreach (var (colName, colType) in hostColumnsToCheck)
        {
            await context.Database.ExecuteSqlRawAsync($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Hosts' AND column_name = '{colName}'
                    ) THEN
                        ALTER TABLE ""Hosts"" ADD COLUMN ""{colName}"" {colType};
                    END IF;
                END
                $$;");
        }

        // Ensure all HostDisk columns exist
        var hostDiskColumnsToCheck = new[]
        {
            ("FileSystem", "text NULL"),
            ("Label", "text NULL")
        };

        foreach (var (colName, colType) in hostDiskColumnsToCheck)
        {
            await context.Database.ExecuteSqlRawAsync($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'HostDisks' AND column_name = '{colName}'
                    ) THEN
                        ALTER TABLE ""HostDisks"" ADD COLUMN ""{colName}"" {colType};
                    END IF;
                END
                $$;");
        }

        // Ensure all UserSession columns exist
        var sessionColumnsToCheck = new[]
        {
            ("TokenHash", "text NOT NULL DEFAULT ''"),
            ("DeviceInfo", "text NULL"),
            ("DeviceName", "text NULL"),
            ("DeviceType", "text NULL"),
            ("Browser", "text NULL"),
            ("OperatingSystem", "text NULL"),
            ("IpAddress", "text NULL"),
            ("Location", "text NULL"),
            ("UserAgent", "text NULL"),
            ("ExpiresAt", "timestamp with time zone NOT NULL DEFAULT now()"),
            ("IsRevoked", "boolean NOT NULL DEFAULT false"),
            ("RevokedAt", "timestamp with time zone NULL"),
            ("RevokedReason", "text NULL"),
            ("LastActiveAt", "timestamp with time zone NOT NULL DEFAULT now()")
        };

        foreach (var (colName, colType) in sessionColumnsToCheck)
        {
            await context.Database.ExecuteSqlRawAsync($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'UserSessions' AND column_name = '{colName}'
                    ) THEN
                        ALTER TABLE ""UserSessions"" ADD COLUMN ""{colName}"" {colType};
                    END IF;
                END
                $$;");
        }

        // Ensure all Service columns exist
        var serviceColumnsToCheck = new[]
        {
            ("ServiceType", "integer NOT NULL DEFAULT 0"),
            ("ServiceName", "text NOT NULL DEFAULT ''"),
            ("DisplayName", "text NULL"),
            ("Description", "text NULL"),
            ("CurrentStatus", "integer NOT NULL DEFAULT 0"),
            ("LastStatusChange", "timestamp with time zone NULL"),
            ("PreviousStatus", "integer NULL"),
            ("Config", "text NULL"),
            ("MonitoringEnabled", "boolean NOT NULL DEFAULT true"),
            ("AlertOnStop", "boolean NOT NULL DEFAULT true"),
            ("RestartCount", "integer NOT NULL DEFAULT 0"),
            ("LastRestartAt", "timestamp with time zone NULL"),
            ("LastHealthyAt", "timestamp with time zone NULL")
        };

        foreach (var (colName, colType) in serviceColumnsToCheck)
        {
            await context.Database.ExecuteSqlRawAsync($@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Services' AND column_name = '{colName}'
                    ) THEN
                        ALTER TABLE ""Services"" ADD COLUMN ""{colName}"" {colType};
                    END IF;
                END
                $$;");
        }

        await SeedDataTurkish.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<ApiKeyAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();
JobRegistration.RegisterJobs();

app.MapControllers();
app.MapHub<MonitoringHub>("/hubs/monitoring");

app.Run();

