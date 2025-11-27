PHASE 1: Project Setup & Database Schema (Days 1-3)
1.1 Solution Structure Setup
.NET Solution Creation
bash# Create solution directory
mkdir ERAMonitor
cd ERAMonitor

# Create solution file
dotnet new sln -n ERAMonitor

# Create projects
dotnet new webapi -n ERAMonitor.API -o src/ERAMonitor.API
dotnet new classlib -n ERAMonitor.Core -o src/ERAMonitor.Core
dotnet new classlib -n ERAMonitor.Infrastructure -o src/ERAMonitor.Infrastructure
dotnet new classlib -n ERAMonitor.BackgroundJobs -o src/ERAMonitor.BackgroundJobs
dotnet new xunit -n ERAMonitor.API.Tests -o tests/ERAMonitor.API.Tests
dotnet new xunit -n ERAMonitor.Infrastructure.Tests -o tests/ERAMonitor.Infrastructure.Tests

# Add projects to solution
dotnet sln add src/ERAMonitor.API/ERAMonitor.API.csproj
dotnet sln add src/ERAMonitor.Core/ERAMonitor.Core.csproj
dotnet sln add src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj
dotnet sln add src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj
dotnet sln add tests/ERAMonitor.API.Tests/ERAMonitor.API.Tests.csproj
dotnet sln add tests/ERAMonitor.Infrastructure.Tests/ERAMonitor.Infrastructure.Tests.csproj

# Add project references
dotnet add src/ERAMonitor.API/ERAMonitor.API.csproj reference src/ERAMonitor.Core/ERAMonitor.Core.csproj
dotnet add src/ERAMonitor.API/ERAMonitor.API.csproj reference src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj
dotnet add src/ERAMonitor.API/ERAMonitor.API.csproj reference src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj
dotnet add src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj reference src/ERAMonitor.Core/ERAMonitor.Core.csproj
dotnet add src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj reference src/ERAMonitor.Core/ERAMonitor.Core.csproj
dotnet add src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj reference src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj
```

### Complete Project Structure
```
ERAMonitor/
├── src/
│   ├── ERAMonitor.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── AgentController.cs
│   │   │   ├── HostsController.cs
│   │   │   ├── ServicesController.cs
│   │   │   ├── ChecksController.cs
│   │   │   ├── IncidentsController.cs
│   │   │   ├── CustomersController.cs
│   │   │   ├── LocationsController.cs
│   │   │   ├── UsersController.cs
│   │   │   ├── ReportsController.cs
│   │   │   ├── NotificationsController.cs
│   │   │   ├── DashboardController.cs
│   │   │   └── PortalController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   ├── ApiKeyAuthMiddleware.cs
│   │   │   └── TenantResolutionMiddleware.cs
│   │   ├── Filters/
│   │   │   ├── ValidationFilter.cs
│   │   │   └── AuditActionFilter.cs
│   │   ├── Hubs/
│   │   │   └── MonitoringHub.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   ├── ApplicationBuilderExtensions.cs
│   │   │   └── ClaimsPrincipalExtensions.cs
│   │   ├── Configuration/
│   │   │   ├── JwtSettings.cs
│   │   │   ├── SmtpSettings.cs
│   │   │   ├── TelegramSettings.cs
│   │   │   └── AppSettings.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Production.json
│   │   ├── Program.cs
│   │   └── ERAMonitor.API.csproj
│   │
│   ├── ERAMonitor.Core/
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Organization.cs
│   │   │   ├── User.cs
│   │   │   ├── UserSession.cs
│   │   │   ├── Customer.cs
│   │   │   ├── CustomerUser.cs
│   │   │   ├── UserCustomerAssignment.cs
│   │   │   ├── Location.cs
│   │   │   ├── Host.cs
│   │   │   ├── HostDisk.cs
│   │   │   ├── HostMetric.cs
│   │   │   ├── Service.cs
│   │   │   ├── ServiceStatusHistory.cs
│   │   │   ├── Check.cs
│   │   │   ├── CheckResult.cs
│   │   │   ├── Incident.cs
│   │   │   ├── IncidentTimeline.cs
│   │   │   ├── IncidentResource.cs
│   │   │   ├── NotificationRule.cs
│   │   │   ├── Notification.cs
│   │   │   ├── Report.cs
│   │   │   ├── ScheduledReport.cs
│   │   │   ├── AuditLog.cs
│   │   │   └── SystemSetting.cs
│   │   ├── Enums/
│   │   │   ├── UserRole.cs
│   │   │   ├── OsType.cs
│   │   │   ├── HostCategory.cs
│   │   │   ├── LocationCategory.cs
│   │   │   ├── CheckType.cs
│   │   │   ├── StatusType.cs
│   │   │   ├── IncidentStatus.cs
│   │   │   ├── IncidentSeverity.cs
│   │   │   ├── NotificationChannel.cs
│   │   │   ├── NotificationStatus.cs
│   │   │   └── ServiceType.cs
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── ICustomerRepository.cs
│   │   │   │   ├── IHostRepository.cs
│   │   │   │   ├── IServiceRepository.cs
│   │   │   │   ├── ICheckRepository.cs
│   │   │   │   ├── IIncidentRepository.cs
│   │   │   │   ├── INotificationRepository.cs
│   │   │   │   └── IReportRepository.cs
│   │   │   └── Services/
│   │   │       ├── IAuthService.cs
│   │   │       ├── IPasswordHasher.cs
│   │   │       ├── ITokenService.cs
│   │   │       ├── IEmailService.cs
│   │   │       ├── ITelegramService.cs
│   │   │       ├── IWebhookService.cs
│   │   │       ├── INotificationService.cs
│   │   │       ├── IIncidentService.cs
│   │   │       ├── IHeartbeatService.cs
│   │   │       ├── IHttpCheckerService.cs
│   │   │       ├── ITcpCheckerService.cs
│   │   │       ├── IReportService.cs
│   │   │       └── IAuditService.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── LoginResponse.cs
│   │   │   │   ├── RefreshTokenRequest.cs
│   │   │   │   ├── ForgotPasswordRequest.cs
│   │   │   │   ├── ResetPasswordRequest.cs
│   │   │   │   └── ChangePasswordRequest.cs
│   │   │   ├── Agent/
│   │   │   │   ├── HeartbeatRequest.cs
│   │   │   │   ├── HeartbeatResponse.cs
│   │   │   │   ├── SystemInfoDto.cs
│   │   │   │   ├── DiskInfoDto.cs
│   │   │   │   ├── ServiceInfoDto.cs
│   │   │   │   └── NetworkInfoDto.cs
│   │   │   ├── Hosts/
│   │   │   │   ├── HostDto.cs
│   │   │   │   ├── HostDetailDto.cs
│   │   │   │   ├── HostListItemDto.cs
│   │   │   │   ├── CreateHostRequest.cs
│   │   │   │   ├── UpdateHostRequest.cs
│   │   │   │   └── HostMetricsDto.cs
│   │   │   ├── Services/
│   │   │   │   ├── ServiceDto.cs
│   │   │   │   ├── ServiceDetailDto.cs
│   │   │   │   └── ServiceStatusHistoryDto.cs
│   │   │   ├── Checks/
│   │   │   │   ├── CheckDto.cs
│   │   │   │   ├── CheckDetailDto.cs
│   │   │   │   ├── CreateHttpCheckRequest.cs
│   │   │   │   ├── CreateTcpCheckRequest.cs
│   │   │   │   ├── UpdateCheckRequest.cs
│   │   │   │   ├── CheckResultDto.cs
│   │   │   │   └── RunCheckResponse.cs
│   │   │   ├── Incidents/
│   │   │   │   ├── IncidentDto.cs
│   │   │   │   ├── IncidentDetailDto.cs
│   │   │   │   ├── IncidentListItemDto.cs
│   │   │   │   ├── CreateIncidentRequest.cs
│   │   │   │   ├── UpdateIncidentRequest.cs
│   │   │   │   ├── AcknowledgeIncidentRequest.cs
│   │   │   │   ├── AssignIncidentRequest.cs
│   │   │   │   ├── ResolveIncidentRequest.cs
│   │   │   │   ├── IncidentTimelineDto.cs
│   │   │   │   └── AddTimelineEntryRequest.cs
│   │   │   ├── Customers/
│   │   │   │   ├── CustomerDto.cs
│   │   │   │   ├── CustomerDetailDto.cs
│   │   │   │   ├── CustomerListItemDto.cs
│   │   │   │   ├── CreateCustomerRequest.cs
│   │   │   │   ├── UpdateCustomerRequest.cs
│   │   │   │   └── CustomerNotificationSettingsDto.cs
│   │   │   ├── Locations/
│   │   │   │   ├── LocationDto.cs
│   │   │   │   ├── CreateLocationRequest.cs
│   │   │   │   └── UpdateLocationRequest.cs
│   │   │   ├── Users/
│   │   │   │   ├── UserDto.cs
│   │   │   │   ├── UserDetailDto.cs
│   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   ├── UpdateUserRequest.cs
│   │   │   │   └── UserPermissionsDto.cs
│   │   │   ├── Notifications/
│   │   │   │   ├── NotificationDto.cs
│   │   │   │   ├── NotificationDetailDto.cs
│   │   │   │   ├── NotificationRuleDto.cs
│   │   │   │   ├── CreateNotificationRuleRequest.cs
│   │   │   │   └── NotificationStatsDto.cs
│   │   │   ├── Reports/
│   │   │   │   ├── ReportDto.cs
│   │   │   │   ├── GenerateUptimeReportRequest.cs
│   │   │   │   ├── GenerateIncidentReportRequest.cs
│   │   │   │   ├── ScheduledReportDto.cs
│   │   │   │   └── CreateScheduledReportRequest.cs
│   │   │   ├── Dashboard/
│   │   │   │   ├── DashboardSummaryDto.cs
│   │   │   │   ├── SystemHealthDto.cs
│   │   │   │   ├── DatacenterStatusDto.cs
│   │   │   │   └── RecentIncidentsDto.cs
│   │   │   └── Common/
│   │   │       ├── PagedRequest.cs
│   │   │       ├── PagedResponse.cs
│   │   │       ├── ApiResponse.cs
│   │   │       └── ErrorResponse.cs
│   │   ├── Exceptions/
│   │   │   ├── NotFoundException.cs
│   │   │   ├── UnauthorizedException.cs
│   │   │   ├── ForbiddenException.cs
│   │   │   ├── ValidationException.cs
│   │   │   └── BusinessException.cs
│   │   ├── Constants/
│   │   │   ├── Roles.cs
│   │   │   ├── Permissions.cs
│   │   │   └── CacheKeys.cs
│   │   └── ERAMonitor.Core.csproj
│   │
│   ├── ERAMonitor.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── OrganizationConfiguration.cs
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── CustomerConfiguration.cs
│   │   │   │   ├── LocationConfiguration.cs
│   │   │   │   ├── HostConfiguration.cs
│   │   │   │   ├── ServiceConfiguration.cs
│   │   │   │   ├── CheckConfiguration.cs
│   │   │   │   ├── IncidentConfiguration.cs
│   │   │   │   ├── NotificationConfiguration.cs
│   │   │   │   └── ReportConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── Repository.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── CustomerRepository.cs
│   │   │   ├── HostRepository.cs
│   │   │   ├── ServiceRepository.cs
│   │   │   ├── CheckRepository.cs
│   │   │   ├── CheckResultRepository.cs
│   │   │   ├── IncidentRepository.cs
│   │   │   ├── NotificationRepository.cs
│   │   │   └── ReportRepository.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── PasswordHasher.cs
│   │   │   ├── TokenService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── TelegramService.cs
│   │   │   ├── WebhookService.cs
│   │   │   ├── NotificationService.cs
│   │   │   ├── IncidentService.cs
│   │   │   ├── HeartbeatService.cs
│   │   │   ├── ReportService.cs
│   │   │   └── AuditService.cs
│   │   ├── Caching/
│   │   │   ├── ICacheService.cs
│   │   │   └── RedisCacheService.cs
│   │   └── ERAMonitor.Infrastructure.csproj
│   │
│   └── ERAMonitor.BackgroundJobs/
│       ├── Jobs/
│       │   ├── HttpCheckerJob.cs
│       │   ├── TcpCheckerJob.cs
│       │   ├── PingCheckerJob.cs
│       │   ├── DnsCheckerJob.cs
│       │   ├── HostDownDetectorJob.cs
│       │   ├── NotificationRetryJob.cs
│       │   ├── MetricsCleanupJob.cs
│       │   ├── ReportGeneratorJob.cs
│       │   └── ScheduledReportJob.cs
│       ├── Services/
│       │   ├── CheckSchedulerService.cs
│       │   └── JobRegistrationService.cs
│       └── ERAMonitor.BackgroundJobs.csproj
│
├── tests/
│   ├── ERAMonitor.API.Tests/
│   │   ├── Controllers/
│   │   │   ├── AuthControllerTests.cs
│   │   │   ├── AgentControllerTests.cs
│   │   │   ├── HostsControllerTests.cs
│   │   │   └── IncidentsControllerTests.cs
│   │   └── ERAMonitor.API.Tests.csproj
│   │
│   └── ERAMonitor.Infrastructure.Tests/
│       ├── Repositories/
│       │   ├── HostRepositoryTests.cs
│       │   └── CheckRepositoryTests.cs
│       ├── Services/
│       │   ├── HeartbeatServiceTests.cs
│       │   └── NotificationServiceTests.cs
│       └── ERAMonitor.Infrastructure.Tests.csproj
│
├── docker/
│   ├── Dockerfile.api
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── docker-compose.prod.yml
│
├── scripts/
│   ├── init-db.sql
│   ├── seed-data.sql
│   └── migrate.sh
│
├── docs/
│   ├── api/
│   │   └── openapi.yaml
│   ├── architecture.md
│   └── deployment.md
│
├── .github/
│   └── workflows/
│       ├── build.yml
│       ├── test.yml
│       └── deploy.yml
│
├── .gitignore
├── README.md
├── ERAMonitor.sln
└── global.json

1.2 NuGet Packages Installation
ERAMonitor.API Packages
xml<!-- src/ERAMonitor.API/ERAMonitor.API.csproj -->

<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Authentication & Authorization -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    
    <!-- API Documentation -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    
    <!-- SignalR for real-time -->
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
    
    <!-- Background Jobs -->
    <PackageReference Include="Hangfire.AspNetCore" Version="1.8.6" />
    <PackageReference Include="Hangfire.PostgreSql" Version="1.20.4" />
    
    <!-- Validation -->
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    
    <!-- Mapping -->
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="6.0.0" />
    
    <!-- Health Checks -->
    <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.0" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.0" />
    <PackageReference Include="AspNetCore.HealthChecks.Hangfire" Version="8.0.0" />
    
    <!-- Rate Limiting -->
    <PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
    
    <!-- CORS -->
    <PackageReference Include="Microsoft.AspNetCore.Cors" Version="2.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ERAMonitor.Core\ERAMonitor.Core.csproj" />
    <ProjectReference Include="..\ERAMonitor.Infrastructure\ERAMonitor.Infrastructure.csproj" />
    <ProjectReference Include="..\ERAMonitor.BackgroundJobs\ERAMonitor.BackgroundJobs.csproj" />
  </ItemGroup>

</Project>
ERAMonitor.Core Packages
xml<!-- src/ERAMonitor.Core/ERAMonitor.Core.csproj -->

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- For JSON serialization in DTOs -->
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
    
    <!-- For data annotations -->
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>

</Project>
ERAMonitor.Infrastructure Packages
xml<!-- src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj -->

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    
    <!-- Redis -->
    <PackageReference Include="StackExchange.Redis" Version="2.7.10" />
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
    
    <!-- HTTP Client for checks and webhooks -->
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
    <PackageReference Include="Polly" Version="8.2.0" />
    <PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
    
    <!-- Email -->
    <PackageReference Include="MailKit" Version="4.3.0" />
    
    <!-- Password hashing -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    
    <!-- JWT -->
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
    
    <!-- PDF Generation -->
    <PackageReference Include="QuestPDF" Version="2023.12.0" />
    
    <!-- Excel Generation -->
    <PackageReference Include="ClosedXML" Version="0.102.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ERAMonitor.Core\ERAMonitor.Core.csproj" />
  </ItemGroup>

</Project>
ERAMonitor.BackgroundJobs Packages
xml<!-- src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj -->

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Hangfire.Core" Version="1.8.6" />
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ERAMonitor.Core\ERAMonitor.Core.csproj" />
    <ProjectReference Include="..\ERAMonitor.Infrastructure\ERAMonitor.Infrastructure.csproj" />
  </ItemGroup>

</Project>

1.3 Database Schema (PostgreSQL)
Complete Database Creation Script
sql-- scripts/init-db.sql
-- ERA Monitor Database Schema
-- PostgreSQL 15+
-- Run this script to create the complete database schema

-- =============================================
-- DATABASE SETUP
-- =============================================

-- Create database (run as superuser)
-- CREATE DATABASE eramonitor;

-- Connect to eramonitor database
\c eramonitor;

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =============================================
-- DROP EXISTING TYPES IF RECREATING
-- =============================================

DROP TYPE IF EXISTS user_role CASCADE;
DROP TYPE IF EXISTS os_type CASCADE;
DROP TYPE IF EXISTS host_category CASCADE;
DROP TYPE IF EXISTS location_category CASCADE;
DROP TYPE IF EXISTS check_type CASCADE;
DROP TYPE IF EXISTS status_type CASCADE;
DROP TYPE IF EXISTS incident_status CASCADE;
DROP TYPE IF EXISTS incident_severity CASCADE;
DROP TYPE IF EXISTS notification_channel CASCADE;
DROP TYPE IF EXISTS notification_status CASCADE;
DROP TYPE IF EXISTS service_type CASCADE;

-- =============================================
-- ENUM TYPES
-- =============================================

-- User roles for authorization
CREATE TYPE user_role AS ENUM (
    'SuperAdmin',      -- Full system access
    'Admin',           -- Can manage assigned customers
    'Operator',        -- Can view and manage incidents
    'Viewer',          -- Read-only access
    'CustomerUser'     -- Customer portal access only
);

-- Operating system types
CREATE TYPE os_type AS ENUM (
    'Windows',
    'Linux'
);

-- Host/Server categories
CREATE TYPE host_category AS ENUM (
    'PhysicalServer',
    'VirtualMachine',
    'VPS',
    'DedicatedServer',
    'CloudInstance'
);

-- Datacenter/Location categories
CREATE TYPE location_category AS ENUM (
    'Colocation',
    'CloudProvider',
    'HostingProvider',
    'OnPremise'
);

-- Monitoring check types
CREATE TYPE check_type AS ENUM (
    'HTTP',
    'TCP',
    'Ping',
    'DNS',
    'CustomHealth'
);

-- Resource status types
CREATE TYPE status_type AS ENUM (
    'Up',
    'Down',
    'Warning',
    'Degraded',
    'Unknown',
    'Disabled'
);

-- Incident workflow statuses
CREATE TYPE incident_status AS ENUM (
    'New',
    'Acknowledged',
    'InProgress',
    'Resolved',
    'Closed'
);

-- Incident severity levels
CREATE TYPE incident_severity AS ENUM (
    'Critical',
    'High',
    'Medium',
    'Low',
    'Info'
);

-- Notification delivery channels
CREATE TYPE notification_channel AS ENUM (
    'Email',
    'SMS',
    'Telegram',
    'Webhook'
);

-- Notification delivery status
CREATE TYPE notification_status AS ENUM (
    'Pending',
    'Sent',
    'Delivered',
    'Failed'
);

-- Service types for monitoring
CREATE TYPE service_type AS ENUM (
    'IIS_Site',
    'IIS_AppPool',
    'WindowsService',
    'SystemdUnit',
    'DockerContainer',
    'Process'
);

-- =============================================
-- TABLES
-- =============================================

-- ---------------------------------------------
-- Organizations (Multi-tenancy support)
-- ---------------------------------------------
CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    logo_url TEXT,
    settings JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE organizations IS 'Multi-tenant organizations';
COMMENT ON COLUMN organizations.slug IS 'URL-friendly unique identifier';
COMMENT ON COLUMN organizations.settings IS 'JSON configuration for organization-specific settings';

-- ---------------------------------------------
-- Users
-- ---------------------------------------------
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255),
    full_name VARCHAR(200) NOT NULL,
    avatar_url TEXT,
    phone VARCHAR(50),
    role user_role NOT NULL DEFAULT 'Viewer',
    permissions JSONB DEFAULT '{}',
    notification_preferences JSONB DEFAULT '{
        "email": true,
        "sms": false,
        "telegram": false,
        "receiveIncidentAlerts": true,
        "receiveStatusChanges": true,
        "receiveWeeklySummary": false
    }',
    timezone VARCHAR(50) DEFAULT 'UTC',
    is_active BOOLEAN DEFAULT TRUE,
    email_verified BOOLEAN DEFAULT FALSE,
    email_verification_token VARCHAR(255),
    password_reset_token VARCHAR(255),
    password_reset_expires TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    failed_login_attempts INT DEFAULT 0,
    locked_until TIMESTAMPTZ,
    two_factor_enabled BOOLEAN DEFAULT FALSE,
    two_factor_secret VARCHAR(255),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE users IS 'System users including admins and operators';
COMMENT ON COLUMN users.password_hash IS 'BCrypt hashed password, NULL if using SSO';
COMMENT ON COLUMN users.permissions IS 'Fine-grained permission overrides';

-- ---------------------------------------------
-- User Sessions (for active session tracking)
-- ---------------------------------------------
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    refresh_token_hash VARCHAR(255) NOT NULL,
    device_info VARCHAR(500),
    ip_address INET,
    location VARCHAR(200),
    user_agent TEXT,
    expires_at TIMESTAMPTZ NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    revoked_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    last_active_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE user_sessions IS 'Active user sessions for token management';

-- ---------------------------------------------
-- Customers (Companies being monitored)
-- ---------------------------------------------
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    logo_url TEXT,
    industry VARCHAR(100),
    
    -- Primary Contact
    contact_name VARCHAR(200),
    contact_email VARCHAR(255),
    contact_phone VARCHAR(50),
    contact_mobile VARCHAR(50),
    contact_job_title VARCHAR(100),
    
    -- Secondary Contact
    secondary_contact_name VARCHAR(200),
    secondary_contact_email VARCHAR(255),
    secondary_contact_phone VARCHAR(50),
    
    -- Emergency Contact
    emergency_contact_name VARCHAR(200),
    emergency_contact_phone VARCHAR(50),
    emergency_available_hours VARCHAR(50),
    
    -- Address
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    country VARCHAR(100),
    postal_code VARCHAR(20),
    
    -- Notification Settings (JSON for flexibility)
    notification_settings JSONB DEFAULT '{
        "channels": {
            "email": true,
            "sms": false,
            "telegram": false,
            "webhook": false
        },
        "emailRecipients": [],
        "smsNumbers": [],
        "telegramChatId": null,
        "webhookUrl": null,
        "notifyOn": {
            "hostDown": true,
            "serviceStopped": true,
            "highCpu": true,
            "highRam": true,
            "diskCritical": true,
            "sslExpiring": true,
            "allIncidents": false,
            "weeklySummary": false
        },
        "quietHours": {
            "enabled": false,
            "from": "22:00",
            "to": "07:00",
            "timezone": "Europe/Istanbul"
        }
    }',
    
    -- Portal Access
    portal_enabled BOOLEAN DEFAULT TRUE,
    api_enabled BOOLEAN DEFAULT FALSE,
    api_key VARCHAR(64) UNIQUE,
    api_rate_limit INT DEFAULT 1000,
    
    -- Assignment
    assigned_admin_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    notes TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT unique_customer_slug_per_org UNIQUE(organization_id, slug)
);

COMMENT ON TABLE customers IS 'Customer companies whose infrastructure is monitored';
COMMENT ON COLUMN customers.notification_settings IS 'JSON configuration for customer notifications';
COMMENT ON COLUMN customers.api_key IS 'API key for customer API access';
COMMENT ON COLUMN customers.api_rate_limit IS 'Rate limit in requests per hour';

-- ---------------------------------------------
-- Customer Users (Portal access)
-- ---------------------------------------------
CREATE TABLE customer_users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    is_primary BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT unique_customer_user UNIQUE(customer_id, user_id)
);

COMMENT ON TABLE customer_users IS 'Links users to customers for portal access';

-- ---------------------------------------------
-- User-Customer Assignments (for internal users)
-- ---------------------------------------------
CREATE TABLE user_customer_assignments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT unique_user_customer_assignment UNIQUE(user_id, customer_id)
);

COMMENT ON TABLE user_customer_assignments IS 'Assigns internal users to manage specific customers';

-- ---------------------------------------------
-- Locations/Datacenters
-- ---------------------------------------------
CREATE TABLE locations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    category location_category NOT NULL,
    provider_name VARCHAR(200),
    city VARCHAR(100),
    country VARCHAR(100),
    address TEXT,
    contact_info TEXT,
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    notes TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE locations IS 'Datacenters and hosting locations';
COMMENT ON COLUMN locations.latitude IS 'GPS latitude for map display';
COMMENT ON COLUMN locations.longitude IS 'GPS longitude for map display';

-- ---------------------------------------------
-- Hosts (Servers)
-- ---------------------------------------------
CREATE TABLE hosts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    location_id UUID REFERENCES locations(id) ON DELETE SET NULL,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    -- Basic Info
    name VARCHAR(200) NOT NULL,
    hostname VARCHAR(255),
    description TEXT,
    os_type os_type NOT NULL,
    os_version VARCHAR(100),
    category host_category NOT NULL DEFAULT 'VirtualMachine',
    tags TEXT[] DEFAULT '{}',
    
    -- Agent Configuration
    api_key VARCHAR(64) UNIQUE NOT NULL DEFAULT encode(gen_random_bytes(32), 'hex'),
    agent_version VARCHAR(20),
    agent_installed_at TIMESTAMPTZ,
    check_interval_seconds INT DEFAULT 60,
    
    -- Current Status (updated by agent heartbeat)
    current_status status_type DEFAULT 'Unknown',
    last_seen_at TIMESTAMPTZ,
    last_heartbeat JSONB,
    
    -- Current Metrics (denormalized for quick access)
    uptime_seconds BIGINT,
    cpu_percent DECIMAL(5,2),
    ram_percent DECIMAL(5,2),
    ram_used_mb BIGINT,
    ram_total_mb BIGINT,
    
    -- Monitoring Settings
    monitoring_enabled BOOLEAN DEFAULT TRUE,
    alert_on_down BOOLEAN DEFAULT TRUE,
    alert_delay_seconds INT DEFAULT 60,
    
    -- Metadata
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE hosts IS 'Monitored servers and hosts';
COMMENT ON COLUMN hosts.api_key IS 'Unique API key for agent authentication';
COMMENT ON COLUMN hosts.last_heartbeat IS 'Complete last heartbeat JSON for debugging';
COMMENT ON COLUMN hosts.alert_delay_seconds IS 'Seconds to wait before alerting on down status';

-- ---------------------------------------------
-- Host Disks
-- ---------------------------------------------
CREATE TABLE host_disks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    host_id UUID NOT NULL REFERENCES hosts(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    mount_point VARCHAR(255),
    filesystem VARCHAR(50),
    total_gb DECIMAL(10,2),
    used_gb DECIMAL(10,2),
    used_percent DECIMAL(5,2),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT unique_host_disk UNIQUE(host_id, name)
);

COMMENT ON TABLE host_disks IS 'Current disk status for each host';
COMMENT ON COLUMN host_disks.name IS 'Disk identifier (C:, /dev/sda1, etc.)';

-- ---------------------------------------------
-- Host Metrics (Time series data)
-- ---------------------------------------------
CREATE TABLE host_metrics (
    id BIGSERIAL PRIMARY KEY,
    host_id UUID NOT NULL REFERENCES hosts(id) ON DELETE CASCADE,
    cpu_percent DECIMAL(5,2),
    ram_percent DECIMAL(5,2),
    ram_used_mb BIGINT,
    ram_total_mb BIGINT,
    disk_info JSONB,
    network_in_bytes BIGINT,
    network_out_bytes BIGINT,
    uptime_seconds BIGINT,
    process_count INT,
    recorded_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE host_metrics IS 'Historical metrics for hosts (time series)';
COMMENT ON COLUMN host_metrics.disk_info IS 'JSON array of disk usage at this point in time';

-- ---------------------------------------------
-- Services (IIS, Windows Services, Systemd, Docker)
-- ---------------------------------------------
CREATE TABLE services (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    host_id UUID NOT NULL REFERENCES hosts(id) ON DELETE CASCADE,
    service_type service_type NOT NULL,
    service_name VARCHAR(200) NOT NULL,
    display_name VARCHAR(300),
    description TEXT,
    
    -- Current Status
    current_status status_type DEFAULT 'Unknown',
    last_status_change TIMESTAMPTZ,
    
    -- Service-specific configuration
    config JSONB DEFAULT '{}',
    
    -- Monitoring
    monitoring_enabled BOOLEAN DEFAULT TRUE,
    restart_count INT DEFAULT 0,
    last_restart_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT unique_service_per_host UNIQUE(host_id, service_type, service_name)
);

COMMENT ON TABLE services IS 'Services running on hosts (IIS, Windows Services, Systemd units, Docker containers)';
COMMENT ON COLUMN services.service_name IS 'Internal service name (w3svc, nginx.service, etc.)';
COMMENT ON COLUMN services.config IS 'Service-specific configuration JSON';

-- ---------------------------------------------
-- Service Status History
-- ---------------------------------------------
CREATE TABLE service_status_history (
    id BIGSERIAL PRIMARY KEY,
    service_id UUID NOT NULL REFERENCES services(id) ON DELETE CASCADE,
    status status_type NOT NULL,
    message TEXT,
    recorded_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE service_status_history IS 'Historical status changes for services';

-- ---------------------------------------------
-- Checks (HTTP, TCP, Ping, DNS, Custom Health)
-- ---------------------------------------------
CREATE TABLE checks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    host_id UUID REFERENCES hosts(id) ON DELETE SET NULL,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    location_id UUID REFERENCES locations(id) ON DELETE SET NULL,
    
    -- Basic Info
    name VARCHAR(200) NOT NULL,
    check_type check_type NOT NULL,
    
    -- Target Configuration
    target VARCHAR(500) NOT NULL,
    
    -- HTTP-specific
    http_method VARCHAR(10) DEFAULT 'GET',
    expected_status_code INT DEFAULT 200,
    expected_keyword VARCHAR(500),
    keyword_should_exist BOOLEAN DEFAULT TRUE,
    request_headers JSONB DEFAULT '{}',
    request_body TEXT,
    follow_redirects BOOLEAN DEFAULT TRUE,
    
    -- TCP-specific
    tcp_port INT,
    send_data TEXT,
    
    -- SSL Monitoring
    monitor_ssl BOOLEAN DEFAULT TRUE,
    ssl_expiry_warning_days INT DEFAULT 14,
    
    -- Timing
    timeout_seconds INT DEFAULT 30,
    interval_seconds INT DEFAULT 60,
    
    -- Current Status
    current_status status_type DEFAULT 'Unknown',
    last_check_at TIMESTAMPTZ,
    last_response_time_ms INT,
    last_status_code INT,
    last_error_message TEXT,
    ssl_expiry_date DATE,
    ssl_days_remaining INT,
    
    -- Flags
    monitoring_enabled BOOLEAN DEFAULT TRUE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE checks IS 'HTTP, TCP, Ping, DNS and custom health checks';
COMMENT ON COLUMN checks.target IS 'URL for HTTP checks, host:port for TCP';
COMMENT ON COLUMN checks.expected_keyword IS 'Text to search for in response body';
COMMENT ON COLUMN checks.keyword_should_exist IS 'TRUE = keyword must exist, FALSE = keyword must not exist';

-- ---------------------------------------------
-- Check Results (History)
-- ---------------------------------------------
CREATE TABLE check_results (
    id BIGSERIAL PRIMARY KEY,
    check_id UUID NOT NULL REFERENCES checks(id) ON DELETE CASCADE,
    status status_type NOT NULL,
    response_time_ms INT,
    status_code INT,
    error_message TEXT,
    response_body_preview TEXT,
    ssl_expiry_date DATE,
    ssl_days_remaining INT,
    headers JSONB,
    checked_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE check_results IS 'Historical results for checks';
COMMENT ON COLUMN check_results.response_body_preview IS 'First 500 characters of response';

-- ---------------------------------------------
-- Incidents
-- ---------------------------------------------
CREATE TABLE incidents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    -- Incident Identification
    incident_number SERIAL,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    
    -- Status & Classification
    status incident_status DEFAULT 'New',
    severity incident_severity DEFAULT 'Medium',
    priority VARCHAR(10) DEFAULT 'P3',
    impact VARCHAR(50),
    
    -- Source (what triggered this incident)
    source_type VARCHAR(50),
    source_id UUID,
    
    -- Assignment
    assigned_to_id UUID REFERENCES users(id) ON DELETE SET NULL,
    acknowledged_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    resolved_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    -- Timestamps
    acknowledged_at TIMESTAMPTZ,
    first_response_at TIMESTAMPTZ,
    resolved_at TIMESTAMPTZ,
    closed_at TIMESTAMPTZ,
    
    -- SLA Tracking
    response_sla_minutes INT DEFAULT 15,
    resolution_sla_minutes INT DEFAULT 240,
    response_sla_met BOOLEAN,
    resolution_sla_met BOOLEAN,
    
    -- Resolution Details
    root_cause_category VARCHAR(100),
    root_cause_description TEXT,
    resolution_steps TEXT,
    preventive_actions TEXT,
    
    -- Affected Resources (denormalized for quick display)
    affected_resources JSONB DEFAULT '[]',
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE incidents IS 'Incident tickets for outages and issues';
COMMENT ON COLUMN incidents.incident_number IS 'Human-readable incident number (INC-00001)';
COMMENT ON COLUMN incidents.source_type IS 'What created this: Host, Service, Check, Manual';
COMMENT ON COLUMN incidents.affected_resources IS 'JSON array of affected resources for display';

-- ---------------------------------------------
-- Incident Timeline/Comments
-- ---------------------------------------------
CREATE TABLE incident_timeline (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    incident_id UUID NOT NULL REFERENCES incidents(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    event_type VARCHAR(50) NOT NULL,
    content TEXT,
    is_internal BOOLEAN DEFAULT TRUE,
    metadata JSONB DEFAULT '{}',
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE incident_timeline IS 'Timeline entries and comments for incidents';
COMMENT ON COLUMN incident_timeline.event_type IS 'Created, Acknowledged, Assigned, StatusChanged, NoteAdded, NotificationSent, Escalated, Resolved, Closed';
COMMENT ON COLUMN incident_timeline.is_internal IS 'FALSE = visible to customer in portal';

-- ---------------------------------------------
-- Incident-Resource Associations
-- ---------------------------------------------
CREATE TABLE incident_resources (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    incident_id UUID NOT NULL REFERENCES incidents(id) ON DELETE CASCADE,
    resource_type VARCHAR(50) NOT NULL,
    resource_id UUID NOT NULL,
    is_primary BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE incident_resources IS 'Links incidents to affected hosts, services, checks';
COMMENT ON COLUMN incident_resources.resource_type IS 'Host, Service, Check';

-- ---------------------------------------------
-- Notification Rules
-- ---------------------------------------------
CREATE TABLE notification_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    
    -- Condition
    condition_type VARCHAR(50) NOT NULL,
    condition_config JSONB DEFAULT '{}',
    
    -- Target
    notify_user_ids UUID[] DEFAULT '{}',
    notify_customer BOOLEAN DEFAULT FALSE,
    channel notification_channel NOT NULL,
    
    -- Escalation
    escalation_after_minutes INT,
    escalation_user_ids UUID[] DEFAULT '{}',
    
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE notification_rules IS 'Rules that define when and how to send notifications';
COMMENT ON COLUMN notification_rules.condition_type IS 'HostDown, ServiceStopped, HighCPU, SSLExpiring, IncidentCreated, etc.';
COMMENT ON COLUMN notification_rules.condition_config IS 'JSON configuration for condition (threshold, duration, etc.)';

-- ---------------------------------------------
-- Notifications
-- ---------------------------------------------
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    
    -- Trigger
    trigger_type VARCHAR(50),
    trigger_id UUID,
    rule_id UUID REFERENCES notification_rules(id) ON DELETE SET NULL,
    
    -- Notification Details
    channel notification_channel NOT NULL,
    recipient VARCHAR(500) NOT NULL,
    subject VARCHAR(500),
    content TEXT NOT NULL,
    content_html TEXT,
    
    -- Delivery Status
    status notification_status DEFAULT 'Pending',
    sent_at TIMESTAMPTZ,
    delivered_at TIMESTAMPTZ,
    error_message TEXT,
    retry_count INT DEFAULT 0,
    max_retries INT DEFAULT 3,
    next_retry_at TIMESTAMPTZ,
    
    -- External tracking
    external_message_id VARCHAR(255),
    
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE notifications IS 'All sent and pending notifications';
COMMENT ON COLUMN notifications.trigger_type IS 'Incident, Check, Host, ScheduledReport';
COMMENT ON COLUMN notifications.recipient IS 'Email address, phone number, chat ID, or webhook URL';

-- ---------------------------------------------
-- Reports
-- ---------------------------------------------
CREATE TABLE reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    name VARCHAR(200) NOT NULL,
    report_type VARCHAR(50) NOT NULL,
    
    -- Parameters
    parameters JSONB DEFAULT '{}',
    
    -- Generated File
    file_url TEXT,
    file_format VARCHAR(10),
    file_size_bytes BIGINT,
    
    -- Status
    status VARCHAR(20) DEFAULT 'Pending',
    error_message TEXT,
    
    generated_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    generated_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE reports IS 'Generated and pending reports';
COMMENT ON COLUMN reports.report_type IS 'Uptime, Incidents, Performance, Security, Customer';
COMMENT ON COLUMN reports.parameters IS 'JSON with host_ids, check_ids, date_from, date_to, etc.';

-- ---------------------------------------------
-- Scheduled Reports
-- ---------------------------------------------
CREATE TABLE scheduled_reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    
    name VARCHAR(200) NOT NULL,
    report_type VARCHAR(50) NOT NULL,
    parameters JSONB DEFAULT '{}',
    
    -- Schedule
    schedule_cron VARCHAR(100),
    schedule_timezone VARCHAR(50) DEFAULT 'UTC',
    
    -- Delivery
    delivery_emails TEXT[] DEFAULT '{}',
    delivery_webhook_url TEXT,
    
    last_run_at TIMESTAMPTZ,
    next_run_at TIMESTAMPTZ,
    last_run_status VARCHAR(20),
    last_run_error TEXT,
    
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE scheduled_reports IS 'Automatically scheduled recurring reports';
COMMENT ON COLUMN scheduled_reports.schedule_cron IS 'Cron expression (e.g., "0 8 * * 1" for Monday 8 AM)';

-- ---------------------------------------------
-- Audit Log
-- ---------------------------------------------
CREATE TABLE audit_logs (
    id BIGSERIAL PRIMARY KEY,
    organization_id UUID REFERENCES organizations(id) ON DELETE SET NULL,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID,
    entity_name VARCHAR(255),
    
    old_values JSONB,
    new_values JSONB,
    
    ip_address INET,
    user_agent TEXT,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE audit_logs IS 'Audit trail for all system changes';
COMMENT ON COLUMN audit_logs.action IS 'Create, Update, Delete, Login, Logout, etc.';
COMMENT ON COLUMN audit_logs.entity_type IS 'Host, Customer, User, Check, Incident, etc.';

-- ---------------------------------------------
-- System Settings
-- ---------------------------------------------
CREATE TABLE system_settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    
    -- Email (SMTP)
    smtp_host VARCHAR(255),
    smtp_port INT DEFAULT 587,
    smtp_username VARCHAR(255),
    smtp_password_encrypted TEXT,
    smtp_from_email VARCHAR(255),
    smtp_from_name VARCHAR(100),
    smtp_use_ssl BOOLEAN DEFAULT TRUE,
    smtp_verified BOOLEAN DEFAULT FALSE,
    
    -- Telegram
    telegram_bot_token_encrypted TEXT,
    telegram_verified BOOLEAN DEFAULT FALSE,
    
    -- Defaults
    default_check_interval_seconds INT DEFAULT 60,
    default_alert_delay_seconds INT DEFAULT 60,
    
    -- Data Retention
    retention_days_metrics INT DEFAULT 30,
    retention_days_check_results INT DEFAULT 30,
    retention_days_logs INT DEFAULT 90,
    retention_days_audit INT DEFAULT 365,
    
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    CONSTRAINT one_setting_per_org UNIQUE(organization_id)
);

COMMENT ON TABLE system_settings IS 'System-wide settings per organization';
COMMENT ON COLUMN system_settings.smtp_password_encrypted IS 'AES-256 encrypted SMTP password';
COMMENT ON COLUMN system_settings.telegram_bot_token_encrypted IS 'AES-256 encrypted Telegram bot token';

-- =============================================
-- INDEXES
-- =============================================

-- Users
CREATE INDEX idx_users_organization ON users(organization_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_active ON users(is_active) WHERE is_active = TRUE;

-- User Sessions
CREATE INDEX idx_user_sessions_user ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_expires ON user_sessions(expires_at);
CREATE INDEX idx_user_sessions_token ON user_sessions(refresh_token_hash);

-- Customers
CREATE INDEX idx_customers_organization ON customers(organization_id);
CREATE INDEX idx_customers_assigned_admin ON customers(assigned_admin_id);
CREATE INDEX idx_customers_api_key ON customers(api_key) WHERE api_key IS NOT NULL;
CREATE INDEX idx_customers_active ON customers(is_active) WHERE is_active = TRUE;

-- Locations
CREATE INDEX idx_locations_organization ON locations(organization_id);
CREATE INDEX idx_locations_category ON locations(category);

-- Hosts
CREATE INDEX idx_hosts_organization ON hosts(organization_id);
CREATE INDEX idx_hosts_customer ON hosts(customer_id);
CREATE INDEX idx_hosts_location ON hosts(location_id);
CREATE INDEX idx_hosts_api_key ON hosts(api_key);
CREATE INDEX idx_hosts_status ON hosts(current_status);
CREATE INDEX idx_hosts_last_seen ON hosts(last_seen_at DESC);
CREATE INDEX idx_hosts_active ON hosts(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_hosts_monitoring ON hosts(monitoring_enabled) WHERE monitoring_enabled = TRUE;

-- Host Metrics (Time series optimization)
CREATE INDEX idx_host_metrics_host_time ON host_metrics(host_id, recorded_at DESC);
CREATE INDEX idx_host_metrics_recorded ON host_metrics(recorded_at DESC);

-- Services
CREATE INDEX idx_services_host ON services(host_id);
CREATE INDEX idx_services_status ON services(current_status);
CREATE INDEX idx_services_type ON services(service_type);
CREATE INDEX idx_services_monitoring ON services(monitoring_enabled) WHERE monitoring_enabled = TRUE;

-- Service Status History
CREATE INDEX idx_service_status_history_service_time ON service_status_history(service_id, recorded_at DESC);

-- Checks
CREATE INDEX idx_checks_organization ON checks(organization_id);
CREATE INDEX idx_checks_customer ON checks(customer_id);
CREATE INDEX idx_checks_host ON checks(host_id);
CREATE INDEX idx_checks_status ON checks(current_status);
CREATE INDEX idx_checks_type ON checks(check_type);
CREATE INDEX idx_checks_monitoring ON checks(monitoring_enabled) WHERE monitoring_enabled = TRUE;
CREATE INDEX idx_checks_ssl_expiry ON checks(ssl_days_remaining) WHERE ssl_days_remaining IS NOT NULL;

-- Check Results
CREATE INDEX idx_check_results_check_time ON check_results(check_id, checked_at DESC);
CREATE INDEX idx_check_results_checked ON check_results(checked_at DESC);

-- Incidents
CREATE INDEX idx_incidents_organization ON incidents(organization_id);
CREATE INDEX idx_incidents_customer ON incidents(customer_id);
CREATE INDEX idx_incidents_status ON incidents(status);
CREATE INDEX idx_incidents_severity ON incidents(severity);
CREATE INDEX idx_incidents_assigned ON incidents(assigned_to_id);
CREATE INDEX idx_incidents_created ON incidents(created_at DESC);
CREATE INDEX idx_incidents_source ON incidents(source_type, source_id);
CREATE INDEX idx_incidents_open ON incidents(status) WHERE status NOT IN ('Resolved', 'Closed');

-- Incident Timeline
CREATE INDEX idx_incident_timeline_incident ON incident_timeline(incident_id, created_at DESC);

-- Incident Resources
CREATE INDEX idx_incident_resources_incident ON incident_resources(incident_id);
CREATE INDEX idx_incident_resources_resource ON incident_resources(resource_type, resource_id);

-- Notification Rules
CREATE INDEX idx_notification_rules_organization ON notification_rules(organization_id);
CREATE INDEX idx_notification_rules_condition ON notification_rules(condition_type);
CREATE INDEX idx_notification_rules_active ON notification_rules(is_active) WHERE is_active = TRUE;

-- Notifications
CREATE INDEX idx_notifications_organization ON notifications(organization_id);
CREATE INDEX idx_notifications_status ON notifications(status);
CREATE INDEX idx_notifications_created ON notifications(created_at DESC);
CREATE INDEX idx_notifications_trigger ON notifications(trigger_type, trigger_id);
CREATE INDEX idx_notifications_pending ON notifications(status, next_retry_at) WHERE status = 'Pending';

-- Reports
CREATE INDEX idx_reports_organization ON reports(organization_id);
CREATE INDEX idx_reports_customer ON reports(customer_id);
CREATE INDEX idx_reports_status ON reports(status);

-- Scheduled Reports
CREATE INDEX idx_scheduled_reports_organization ON scheduled_reports(organization_id);
CREATE INDEX idx_scheduled_reports_next_run ON scheduled_reports(next_run_at) WHERE is_active = TRUE;

-- Audit Logs
CREATE INDEX idx_audit_logs_organization ON audit_logs(organization_id);
CREATE INDEX idx_audit_logs_user ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_created ON audit_logs(created_at DESC);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);

-- =============================================
-- TRIGGERS
-- =============================================

-- Function to auto-update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply updated_at trigger to all relevant tables
CREATE TRIGGER update_organizations_updated_at 
    BEFORE UPDATE ON organizations 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_users_updated_at 
    BEFORE UPDATE ON users 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_customers_updated_at 
    BEFORE UPDATE ON customers 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_locations_updated_at 
    BEFORE UPDATE ON locations 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_hosts_updated_at 
    BEFORE UPDATE ON hosts 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_services_updated_at 
    BEFORE UPDATE ON services 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_checks_updated_at 
    BEFORE UPDATE ON checks 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_incidents_updated_at 
    BEFORE UPDATE ON incidents 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_notification_rules_updated_at 
    BEFORE UPDATE ON notification_rules 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_scheduled_reports_updated_at 
    BEFORE UPDATE ON scheduled_reports 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Function to auto-generate incident number
CREATE OR REPLACE FUNCTION generate_incident_number()
RETURNS TRIGGER AS $$
BEGIN
    -- incident_number is auto-generated by SERIAL, format as INC-XXXXX in application
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- VIEWS (Optional - for common queries)
-- =============================================

-- View for host summary with latest metrics
CREATE OR REPLACE VIEW v_host_summary AS
SELECT 
    h.id,
    h.name,
    h.hostname,
    h.os_type,
    h.category,
    h.current_status,
    h.last_seen_at,
    h.cpu_percent,
    h.ram_percent,
    h.uptime_seconds,
    h.monitoring_enabled,
    c.id AS customer_id,
    c.name AS customer_name,
    l.id AS location_id,
    l.name AS location_name,
    l.category AS location_category,
    (SELECT COUNT(*) FROM services s WHERE s.host_id = h.id) AS service_count,
    (SELECT COUNT(*) FROM services s WHERE s.host_id = h.id AND s.current_status = 'Down') AS services_down,
    (SELECT COUNT(*) FROM incidents i WHERE i.source_type = 'Host' AND i.source_id = h.id AND i.status NOT IN ('Resolved', 'Closed')) AS open_incidents
FROM hosts h
LEFT JOIN customers c ON h.customer_id = c.id
LEFT JOIN locations l ON h.location_id = l.id
WHERE h.is_active = TRUE;

-- View for active incidents with details
CREATE OR REPLACE VIEW v_active_incidents AS
SELECT 
    i.id,
    i.incident_number,
    i.title,
    i.status,
    i.severity,
    i.priority,
    i.source_type,
    i.source_id,
    i.created_at,
    i.acknowledged_at,
    i.assigned_to_id,
    u.full_name AS assigned_to_name,
    c.id AS customer_id,
    c.name AS customer_name,
    EXTRACT(EPOCH FROM (NOW() - i.created_at)) / 60 AS age_minutes,
    CASE 
        WHEN i.acknowledged_at IS NULL THEN 
            i.response_sla_minutes - EXTRACT(EPOCH FROM (NOW() - i.created_at)) / 60
        ELSE NULL 
    END AS response_sla_remaining_minutes
FROM incidents i
LEFT JOIN users u ON i.assigned_to_id = u.id
LEFT JOIN customers c ON i.customer_id = c.id
WHERE i.status NOT IN ('Resolved', 'Closed');

-- =============================================
-- SEED DATA
-- =============================================

-- Default Organization
INSERT INTO organizations (id, name, slug, settings) VALUES 
    ('00000000-0000-0000-0000-000000000001', 'ERA Cloud', 'era-cloud', '{"theme": "dark", "language": "tr"}')
ON CONFLICT (id) DO NOTHING;

-- Super Admin User (password: Admin123!)
-- BCrypt hash of "Admin123!" with cost 12
INSERT INTO users (id, organization_id, email, password_hash, full_name, role, email_verified) VALUES 
    ('00000000-0000-0000-0000-000000000002', 
     '00000000-0000-0000-0000-000000000001', 
     'admin@eracloud.com.tr', 
     '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S3rgGC6Y9.P5Aq', 
     'System Administrator', 
     'SuperAdmin',
     TRUE)
ON CONFLICT (id) DO NOTHING;

-- Default System Settings
INSERT INTO system_settings (organization_id, smtp_from_email, smtp_from_name) VALUES 
    ('00000000-0000-0000-0000-000000000001', 'noreply@eracloud.com.tr', 'ERA Monitor')
ON CONFLICT (organization_id) DO NOTHING;

-- Sample Locations
INSERT INTO locations (organization_id, name, category, provider_name, city, country, latitude, longitude) VALUES 
    ('00000000-0000-0000-0000-000000000001', 'Turkcell Datacenter Istanbul', 'Colocation', 'Turkcell', 'Istanbul', 'Turkey', 41.0082, 28.9784),
    ('00000000-0000-0000-0000-000000000001', 'Hetzner Cloud - Falkenstein', 'CloudProvider', 'Hetzner', 'Falkenstein', 'Germany', 50.4778, 12.3706),
    ('00000000-0000-0000-0000-000000000001', 'Azure West Europe', 'CloudProvider', 'Microsoft Azure', 'Amsterdam', 'Netherlands', 52.3676, 4.9041),
    ('00000000-0000-0000-0000-000000000001', 'AWS EU-Central-1', 'CloudProvider', 'Amazon Web Services', 'Frankfurt', 'Germany', 50.1109, 8.6821),
    ('00000000-0000-0000-0000-000000000001', 'Hostinger VPS', 'HostingProvider', 'Hostinger', 'Vilnius', 'Lithuania', 54.6872, 25.2797)
ON CONFLICT DO NOTHING;

-- Sample Customer
INSERT INTO customers (id, organization_id, name, slug, contact_name, contact_email, industry) VALUES 
    ('00000000-0000-0000-0000-000000000003',
     '00000000-0000-0000-0000-000000000001',
     'Demo Company',
     'demo-company',
     'John Doe',
     'john@demo.com',
     'Technology')
ON CONFLICT (id) DO NOTHING;

-- =============================================
-- GRANTS (adjust based on your user setup)
-- =============================================

-- Create application user if not exists
-- CREATE USER eramonitor_app WITH PASSWORD 'secure_password_here';

-- Grant permissions
-- GRANT CONNECT ON DATABASE eramonitor TO eramonitor_app;
-- GRANT USAGE ON SCHEMA public TO eramonitor_app;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO eramonitor_app;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO eramonitor_app;

-- =============================================
-- COMPLETION
-- =============================================

-- Analyze tables for query optimization
ANALYZE;

-- Display summary
DO $$
DECLARE
    table_count INT;
    index_count INT;
BEGIN
    SELECT COUNT(*) INTO table_count FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
    SELECT COUNT(*) INTO index_count FROM pg_indexes WHERE schemaname = 'public';
    
    RAISE NOTICE '========================================';
    RAISE NOTICE 'ERA Monitor Database Setup Complete';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Tables created: %', table_count;
    RAISE NOTICE 'Indexes created: %', index_count;
    RAISE NOTICE '========================================';
END $$;

1.4 Entity Framework Core Setup
ApplicationDbContext
csharp// src/ERAMonitor.Infrastructure/Data/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using System.Reflection;

namespace ERAMonitor.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Core entities
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // Customer management
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerUser> CustomerUsers => Set<CustomerUser>();
    public DbSet<UserCustomerAssignment> UserCustomerAssignments => Set<UserCustomerAssignment>();

    // Infrastructure
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Host> Hosts => Set<Host>();
    public DbSet<HostDisk> HostDisks => Set<HostDisk>();
    public DbSet<HostMetric> HostMetrics => Set<HostMetric>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceStatusHistory> ServiceStatusHistories => Set<ServiceStatusHistory>();

    // Monitoring
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<CheckResult> CheckResults => Set<CheckResult>();

    // Incidents
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentTimeline> IncidentTimelines => Set<IncidentTimeline>();
    public DbSet<IncidentResource> IncidentResources => Set<IncidentResource>();

    // Notifications
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Reports
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();

    // System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure PostgreSQL enums
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<OsType>();
        modelBuilder.HasPostgresEnum<HostCategory>();
        modelBuilder.HasPostgresEnum<LocationCategory>();
        modelBuilder.HasPostgresEnum<CheckType>();
        modelBuilder.HasPostgresEnum<StatusType>();
        modelBuilder.HasPostgresEnum<IncidentStatus>();
        modelBuilder.HasPostgresEnum<IncidentSeverity>();
        modelBuilder.HasPostgresEnum<NotificationChannel>();
        modelBuilder.HasPostgresEnum<NotificationStatus>();
        modelBuilder.HasPostgresEnum<ServiceType>();

        // Global query filters for soft delete
        modelBuilder.Entity<Host>().HasQueryFilter(h => h.IsActive);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.IsActive);
        modelBuilder.Entity<Check>().HasQueryFilter(c => c.IsActive);
        modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
        modelBuilder.Entity<Location>().HasQueryFilter(l => l.IsActive);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set UpdatedAt for modified entities
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime))
                {
                    updatedAtProperty.SetValue(entry.Entity, DateTime.UtcNow);
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
Sample Entity Configuration
csharp// src/ERAMonitor.Infrastructure/Data/Configurations/HostConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Infrastructure.Data.Configurations;

public class HostConfiguration : IEntityTypeConfiguration<Host>
{
    public void Configure(EntityTypeBuilder<Host> builder)
    {
        builder.ToTable("hosts");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(h => h.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.Hostname)
            .HasColumnName("hostname")
            .HasMaxLength(255);

        builder.Property(h => h.Description)
            .HasColumnName("description");

        builder.Property(h => h.OsType)
            .HasColumnName("os_type")
            .IsRequired();

        builder.Property(h => h.OsVersion)
            .HasColumnName("os_version")
            .HasMaxLength(100);

        builder.Property(h => h.Category)
            .HasColumnName("category")
            .IsRequired();

        builder.Property(h => h.Tags)
            .HasColumnName("tags")
            .HasColumnType("text[]");

        builder.Property(h => h.ApiKey)
            .HasColumnName("api_key")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(h => h.AgentVersion)
            .HasColumnName("agent_version")
            .HasMaxLength(20);

        builder.Property(h => h.CheckIntervalSeconds)
            .HasColumnName("check_interval_seconds")
            .HasDefaultValue(60);

        builder.Property(h => h.CurrentStatus)
            .HasColumnName("current_status")
            .HasDefaultValue(StatusType.Unknown);

        builder.Property(h => h.LastSeenAt)
            .HasColumnName("last_seen_at");

        builder.Property(h => h.LastHeartbeat)
            .HasColumnName("last_heartbeat")
            .HasColumnType("jsonb");

        builder.Property(h => h.UptimeSeconds)
            .HasColumnName("uptime_seconds");

        builder.Property(h => h.CpuPercent)
            .HasColumnName("cpu_percent")
            .HasPrecision(5, 2);

        builder.Property(h => h.RamPercent)
            .HasColumnName("ram_percent")
            .HasPrecision(5, 2);

        builder.Property(h => h.RamUsedMb)
            .HasColumnName("ram_used_mb");

        builder.Property(h => h.RamTotalMb)
            .HasColumnName("ram_total_mb");

        builder.Property(h => h.MonitoringEnabled)
            .HasColumnName("monitoring_enabled")
            .HasDefaultValue(true);

        builder.Property(h => h.AlertOnDown)
            .HasColumnName("alert_on_down")
            .HasDefaultValue(true);

        builder.Property(h => h.AlertDelaySeconds)
            .HasColumnName("alert_delay_seconds")
            .HasDefaultValue(60);

        builder.Property(h => h.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(h => h.Organization)
            .WithMany(o => o.Hosts)
            .HasForeignKey(h => h.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Location)
            .WithMany(l => l.Hosts)
            .HasForeignKey(h => h.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.Customer)
            .WithMany(c => c.Hosts)
            .HasForeignKey(h => h.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(h => h.Disks)
            .WithOne(d => d.Host)
            .HasForeignKey(d => d.HostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Metrics)
            .WithOne(m => m.Host)
            .HasForeignKey(m => m.HostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Services)
            .WithOne(s => s.Host)
            .HasForeignKey(s => s.HostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(h => h.OrganizationId)
            .HasDatabaseName("idx_hosts_organization");

        builder.HasIndex(h => h.CustomerId)
            .HasDatabaseName("idx_hosts_customer");

        builder.HasIndex(h => h.LocationId)
            .HasDatabaseName("idx_hosts_location");

        builder.HasIndex(h => h.ApiKey)
            .IsUnique()
            .HasDatabaseName("idx_hosts_api_key");

        builder.HasIndex(h => h.CurrentStatus)
            .HasDatabaseName("idx_hosts_status");

        builder.HasIndex(h => h.LastSeenAt)
            .IsDescending()
            .HasDatabaseName("idx_hosts_last_seen");
    }
}
Base Entity
csharp// src/ERAMonitor.Core/Entities/BaseEntity.cs

namespace ERAMonitor.Core.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public abstract class BaseEntityWithOrganization : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = null!;
}
Host Entity
csharp// src/ERAMonitor.Core/Entities/Host.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Host : BaseEntityWithOrganization
{
    public Guid? LocationId { get; set; }
    public Guid? CustomerId { get; set; }
    
    // Basic Info
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Description { get; set; }
    public OsType OsType { get; set; }
    public string? OsVersion { get; set; }
    public HostCategory Category { get; set; } = HostCategory.VirtualMachine;
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Agent Configuration
    public string ApiKey { get; set; } = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    public string? AgentVersion { get; set; }
    public DateTime? AgentInstalledAt { get; set; }
    public int CheckIntervalSeconds { get; set; } = 60;
    
    // Current Status
    public StatusType CurrentStatus { get; set; } = StatusType.Unknown;
    public DateTime? LastSeenAt { get; set; }
    public string? LastHeartbeat { get; set; } // JSON
    
    // Current Metrics
    public long? UptimeSeconds { get; set; }
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public long? RamUsedMb { get; set; }
    public long? RamTotalMb { get; set; }
    
    // Monitoring Settings
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnDown { get; set; } = true;
    public int AlertDelaySeconds { get; set; } = 60;
    
    // Soft Delete
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Location? Location { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<HostDisk> Disks { get; set; } = new List<HostDisk>();
    public virtual ICollection<HostMetric> Metrics { get; set; } = new List<HostMetric>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Check> Checks { get; set; } = new List<Check>();
}

1.5 Application Configuration
appsettings.json
json// src/ERAMonitor.API/appsettings.json

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=eramonitor;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "ERAMonitor",
    "Audience": "ERAMonitor",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@eracloud.com.tr",
    "FromName": "ERA Monitor",
    "UseSsl": true
  },
  "Telegram": {
    "BotToken": ""
  },
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "WorkerCount": 5
  },
  "Monitoring": {
    "DefaultCheckIntervalSeconds": 60,
    "HostDownThresholdSeconds": 90,
    "MetricsRetentionDays": 30,
    "CheckResultsRetentionDays": 30
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://monitor.eracloud.com.tr"
    ]
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Hangfire": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/eramonitor-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
Program.cs
csharp// src/ERAMonitor.API/Program.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using ERAMonitor.API.Configuration;
using ERAMonitor.API.Extensions;
using ERAMonitor.API.Middleware;
using ERAMonitor.API.Hubs;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Services;
using ERAMonitor.BackgroundJobs.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("ERAMonitor.Infrastructure");
            npgsqlOptions.EnableRetryOnFailure(3);
        }));

// Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ERAMonitor_";
});

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));

// Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero
    };

    // Configure SignalR authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("RequireOperatorRole", policy => 
        policy.RequireRole("SuperAdmin", "Admin", "Operator"));
});

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// SignalR
builder.Services.AddSignalR();

// Hangfire
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", 5);
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERA Monitor API",
        Version = "v1",
        Description = "Server and service monitoring platform API",
        Contact = new OpenApiContact
        {
            Name = "ERA Cloud",
            Email = "support@eracloud.com.tr",
            Url = new Uri("https://eracloud.com.tr")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key for agent authentication",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenRetryRDContinuecsharp// src/ERAMonitor.API/Program.cs (continued)

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis")
    .AddHangfire(options => options.MinimumAvailableServers = 1, name: "hangfire");

// Register Application Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddBackgroundJobServices();

// HTTP Client Factory for external checks
builder.Services.AddHttpClient("HealthChecker", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "ERAMonitor-HealthChecker/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true // Allow self-signed for checks
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger (always enabled for now, can be restricted in production)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERA Monitor API v1");
    options.RoutePrefix = "swagger";
});

// HTTPS Redirection (if not behind reverse proxy)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors("AllowDashboard");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Check Endpoint
app.MapHealthChecks("/health");

// API Controllers
app.MapControllers();

// SignalR Hubs
app.MapHub<MonitoringHub>("/hubs/monitoring");

// Hangfire Dashboard (protected)
app.MapHangfireDashboard(builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "ERA Monitor - Background Jobs"
});

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply pending migrations
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
        
        // Seed initial data if needed
        await DatabaseSeeder.SeedAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

// Register recurring background jobs
using (var scope = app.Services.CreateScope())
{
    var jobRegistration = scope.ServiceProvider.GetRequiredService<IJobRegistrationService>();
    jobRegistration.RegisterRecurringJobs();
}

// Log startup
Log.Information("ERA Monitor API started successfully");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Swagger UI: {SwaggerUrl}", "/swagger");
Log.Information("Hangfire Dashboard: {HangfireUrl}", builder.Configuration["Hangfire:DashboardPath"]);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
Service Registration Extensions
csharp// src/ERAMonitor.API/Extensions/ServiceCollectionExtensions.cs

using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Repositories;
using ERAMonitor.Infrastructure.Services;
using ERAMonitor.Infrastructure.Caching;
using ERAMonitor.BackgroundJobs.Jobs;
using ERAMonitor.BackgroundJobs.Services;

namespace ERAMonitor.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper profiles can be registered here if needed
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IHostRepository, HostRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ICheckRepository, CheckRepository>();
        services.AddScoped<ICheckResultRepository, CheckResultRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IHeartbeatService, HeartbeatService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Caching
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddBackgroundJobServices(this IServiceCollection services)
    {
        // Job services
        services.AddScoped<IJobRegistrationService, JobRegistrationService>();
        services.AddScoped<ICheckSchedulerService, CheckSchedulerService>();

        // Jobs
        services.AddScoped<HttpCheckerJob>();
        services.AddScoped<TcpCheckerJob>();
        services.AddScoped<PingCheckerJob>();
        services.AddScoped<HostDownDetectorJob>();
        services.AddScoped<NotificationRetryJob>();
        services.AddScoped<MetricsCleanupJob>();
        services.AddScoped<ReportGeneratorJob>();
        services.AddScoped<ScheduledReportJob>();

        return services;
    }
}
Exception Handling Middleware
csharp// src/ERAMonitor.API/Middleware/ExceptionHandlingMiddleware.cs

using System.Net;
using System.Text.Json;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.Exceptions;

namespace ERAMonitor.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case NotFoundException notFound:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFound.Message;
                errorResponse.Code = "NOT_FOUND";
                _logger.LogWarning("Resource not found: {Message}", notFound.Message);
                break;

            case UnauthorizedException unauthorized:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthorized.Message;
                errorResponse.Code = "UNAUTHORIZED";
                _logger.LogWarning("Unauthorized access attempt: {Message}", unauthorized.Message);
                break;

            case ForbiddenException forbidden:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = forbidden.Message;
                errorResponse.Code = "FORBIDDEN";
                _logger.LogWarning("Forbidden access attempt: {Message}", forbidden.Message);
                break;

            case ValidationException validation:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = validation.Message;
                errorResponse.Code = "VALIDATION_ERROR";
                errorResponse.Errors = validation.Errors;
                _logger.LogWarning("Validation error: {Message}", validation.Message);
                break;

            case BusinessException business:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = business.Message;
                errorResponse.Code = business.Code ?? "BUSINESS_ERROR";
                _logger.LogWarning("Business error: {Message}", business.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = _env.IsDevelopment() 
                    ? exception.Message 
                    : "An unexpected error occurred. Please try again later.";
                errorResponse.Code = "INTERNAL_ERROR";
                
                if (_env.IsDevelopment())
                {
                    errorResponse.StackTrace = exception.StackTrace;
                }
                
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}
API Key Authentication Middleware
csharp// src/ERAMonitor.API/Middleware/ApiKeyAuthMiddleware.cs

using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.API.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHostRepository hostRepository)
    {
        // Only apply to agent endpoints
        if (!context.Request.Path.StartsWithSegments("/api/agent"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is required" });
            return;
        }

        var apiKey = apiKeyHeader.ToString();
        var host = await hostRepository.GetByApiKeyAsync(apiKey);

        if (host == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
            return;
        }

        if (!host.IsActive)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Host is deactivated" });
            return;
        }

        // Store host info in HttpContext for use in controllers
        context.Items["HostId"] = host.Id;
        context.Items["Host"] = host;

        await _next(context);
    }
}
SignalR Hub
csharp// src/ERAMonitor.API/Hubs/MonitoringHub.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ERAMonitor.API.Hubs;

[Authorize]
public class MonitoringHub : Hub
{
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(ILogger<MonitoringHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to MonitoringHub", userId);
        
        // Add to dashboard group for global updates
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from MonitoringHub", userId);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific host's update group to receive real-time metrics
    /// </summary>
    public async Task JoinHostGroup(string hostId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"host_{hostId}");
        _logger.LogDebug("Connection {ConnectionId} joined host group {HostId}", 
            Context.ConnectionId, hostId);
    }

    /// <summary>
    /// Leave a specific host's update group
    /// </summary>
    public async Task LeaveHostGroup(string hostId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"host_{hostId}");
        _logger.LogDebug("Connection {ConnectionId} left host group {HostId}", 
            Context.ConnectionId, hostId);
    }

    /// <summary>
    /// Join a customer's group to receive their resource updates
    /// </summary>
    public async Task JoinCustomerGroup(string customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{customerId}");
        _logger.LogDebug("Connection {ConnectionId} joined customer group {CustomerId}", 
            Context.ConnectionId, customerId);
    }

    /// <summary>
    /// Leave a customer's group
    /// </summary>
    public async Task LeaveCustomerGroup(string customerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer_{customerId}");
    }

    /// <summary>
    /// Join incident updates group
    /// </summary>
    public async Task JoinIncidentGroup(string incidentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"incident_{incidentId}");
    }

    /// <summary>
    /// Leave incident updates group
    /// </summary>
    public async Task LeaveIncidentGroup(string incidentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"incident_{incidentId}");
    }
}

/// <summary>
/// Extension methods for broadcasting updates via SignalR
/// </summary>
public static class MonitoringHubExtensions
{
    /// <summary>
    /// Broadcast host status update to dashboard and host-specific groups
    /// </summary>
    public static async Task BroadcastHostUpdate(
        this IHubContext<MonitoringHub> hubContext,
        Guid hostId,
        object update)
    {
        await hubContext.Clients.Group("dashboard").SendAsync("HostUpdated", update);
        await hubContext.Clients.Group($"host_{hostId}").SendAsync("HostUpdated", update);
    }

    /// <summary>
    /// Broadcast service status change
    /// </summary>
    public static async Task BroadcastServiceUpdate(
        this IHubContext<MonitoringHub> hubContext,
        Guid hostId,
        object update)
    {
        await hubContext.Clients.Group("dashboard").SendAsync("ServiceUpdated", update);
        await hubContext.Clients.Group($"host_{hostId}").SendAsync("ServiceUpdated", update);
    }

    /// <summary>
    /// Broadcast check result
    /// </summary>
    public static async Task BroadcastCheckResult(
        this IHubContext<MonitoringHub> hubContext,
        Guid checkId,
        Guid? customerId,
        object result)
    {
        await hubContext.Clients.Group("dashboard").SendAsync("CheckResultReceived", result);
        
        if (customerId.HasValue)
        {
            await hubContext.Clients.Group($"customer_{customerId}").SendAsync("CheckResultReceived", result);
        }
    }

    /// <summary>
    /// Broadcast new incident
    /// </summary>
    public static async Task BroadcastIncidentCreated(
        this IHubContext<MonitoringHub> hubContext,
        Guid? customerId,
        object incident)
    {
        await hubContext.Clients.Group("dashboard").SendAsync("IncidentCreated", incident);
        
        if (customerId.HasValue)
        {
            await hubContext.Clients.Group($"customer_{customerId}").SendAsync("IncidentCreated", incident);
        }
    }

    /// <summary>
    /// Broadcast incident update
    /// </summary>
    public static async Task BroadcastIncidentUpdated(
        this IHubContext<MonitoringHub> hubContext,
        Guid incidentId,
        Guid? customerId,
        object incident)
    {
        await hubContext.Clients.Group("dashboard").SendAsync("IncidentUpdated", incident);
        await hubContext.Clients.Group($"incident_{incidentId}").SendAsync("IncidentUpdated", incident);
        
        if (customerId.HasValue)
        {
            await hubContext.Clients.Group($"customer_{customerId}").SendAsync("IncidentUpdated", incident);
        }
    }
}

1.6 Docker Configuration
Dockerfile for API
dockerfile# docker/Dockerfile.api

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/ERAMonitor.API/ERAMonitor.API.csproj", "ERAMonitor.API/"]
COPY ["src/ERAMonitor.Core/ERAMonitor.Core.csproj", "ERAMonitor.Core/"]
COPY ["src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj", "ERAMonitor.Infrastructure/"]
COPY ["src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj", "ERAMonitor.BackgroundJobs/"]
RUN dotnet restore "ERAMonitor.API/ERAMonitor.API.csproj"

# Copy everything and build
COPY src/ .
WORKDIR "/src/ERAMonitor.API"
RUN dotnet build "ERAMonitor.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ERAMonitor.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Create non-root user
RUN useradd -m -s /bin/bash appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ERAMonitor.API.dll"]
Docker Compose for Development
yaml# docker/docker-compose.yml

version: '3.8'

services:
  api:
    build:
      context: ../
      dockerfile: docker/Dockerfile.api
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=eramonitor;Username=postgres;Password=postgres123
      - ConnectionStrings__Redis=redis:6379
      - Jwt__Secret=your-super-secret-key-minimum-32-characters-long
      - Jwt__Issuer=ERAMonitor
      - Jwt__Audience=ERAMonitor
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_started
    networks:
      - era-network
    volumes:
      - api-logs:/app/logs

  postgres:
    image: postgres:15-alpine
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
      - POSTGRES_DB=eramonitor
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ../scripts/init-db.sql:/docker-entrypoint-initdb.d/01-init.sql
    networks:
      - era-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes
    networks:
      - era-network

  pgadmin:
    image: dpage/pgadmin4:latest
    environment:
      - PGADMIN_DEFAULT_EMAIL=admin@eracloud.com.tr
      - PGADMIN_DEFAULT_PASSWORD=admin123
    ports:
      - "5050:80"
    depends_on:
      - postgres
    networks:
      - era-network
    profiles:
      - tools

volumes:
  postgres-data:
  redis-data:
  api-logs:

networks:
  era-network:
    driver: bridge
Docker Compose Override for Development
yaml# docker/docker-compose.override.yml

version: '3.8'

services:
  api:
    build:
      context: ../
      dockerfile: docker/Dockerfile.api
      target: build
    volumes:
      - ../src:/src
      - ~/.nuget/packages:/root/.nuget/packages:ro
    command: dotnet watch run --project ERAMonitor.API/ERAMonitor.API.csproj
    environment:
      - DOTNET_USE_POLLING_FILE_WATCHER=1

1.7 Initial Migration
Create First Migration
bash# Navigate to solution root
cd ERAMonitor

# Create initial migration
dotnet ef migrations add InitialCreate \
    --project src/ERAMonitor.Infrastructure \
    --startup-project src/ERAMonitor.API \
    --output-dir Data/Migrations

# Apply migration (if database exists)
dotnet ef database update \
    --project src/ERAMonitor.Infrastructure \
    --startup-project src/ERAMonitor.API
Database Seeder
csharp// src/ERAMonitor.Infrastructure/Data/DatabaseSeeder.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Check if already seeded
            if (await context.Organizations.AnyAsync())
            {
                logger.LogInformation("Database already seeded, skipping");
                return;
            }

            logger.LogInformation("Seeding database...");

            // Create default organization
            var organization = new Organization
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "ERA Cloud",
                Slug = "era-cloud",
                Settings = "{\"theme\": \"dark\", \"language\": \"tr\"}"
            };
            context.Organizations.Add(organization);

            // Create super admin user (password: Admin123!)
            var adminUser = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                OrganizationId = organization.Id,
                Email = "admin@eracloud.com.tr",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FullName = "System Administrator",
                Role = UserRole.SuperAdmin,
                EmailVerified = true,
                IsActive = true
            };
            context.Users.Add(adminUser);

            // Create default system settings
            var systemSettings = new SystemSetting
            {
                OrganizationId = organization.Id,
                SmtpFromEmail = "noreply@eracloud.com.tr",
                SmtpFromName = "ERA Monitor",
                DefaultCheckIntervalSeconds = 60,
                DefaultAlertDelaySeconds = 60,
                RetentionDaysMetrics = 30,
                RetentionDaysCheckResults = 30,
                RetentionDaysLogs = 90,
                RetentionDaysAudit = 365
            };
            context.SystemSettings.Add(systemSettings);

            // Create sample locations
            var locations = new[]
            {
                new Location
                {
                    OrganizationId = organization.Id,
                    Name = "Turkcell Datacenter Istanbul",
                    Category = LocationCategory.Colocation,
                    ProviderName = "Turkcell",
                    City = "Istanbul",
                    Country = "Turkey",
                    Latitude = 41.0082m,
                    Longitude = 28.9784m
                },
                new Location
                {
                    OrganizationId = organization.Id,
                    Name = "Hetzner Cloud - Falkenstein",
                    Category = LocationCategory.CloudProvider,
                    ProviderName = "Hetzner",
                    City = "Falkenstein",
                    Country = "Germany",
                    Latitude = 50.4778m,
                    Longitude = 12.3706m
                },
                new Location
                {
                    OrganizationId = organization.Id,
                    Name = "Azure West Europe",
                    Category = LocationCategory.CloudProvider,
                    ProviderName = "Microsoft Azure",
                    City = "Amsterdam",
                    Country = "Netherlands",
                    Latitude = 52.3676m,
                    Longitude = 4.9041m
                }
            };
            context.Locations.AddRange(locations);

            // Create sample customer
            var customer = new Customer
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                OrganizationId = organization.Id,
                Name = "Demo Company",
                Slug = "demo-company",
                ContactName = "John Doe",
                ContactEmail = "john@demo.com",
                Industry = "Technology",
                AssignedAdminId = adminUser.Id,
                PortalEnabled = true
            };
            context.Customers.Add(customer);

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}

1.8 Development Scripts
Run Development Environment
bash#!/bin/bash
# scripts/dev-start.sh

echo "Starting ERA Monitor Development Environment..."

# Start Docker services
cd docker
docker-compose up -d postgres redis

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL..."
until docker-compose exec -T postgres pg_isready -U postgres; do
    sleep 1
done

# Run migrations
cd ..
dotnet ef database update \
    --project src/ERAMonitor.Infrastructure \
    --startup-project src/ERAMonitor.API

# Start API in watch mode
cd src/ERAMonitor.API
dotnet watch run
Run Tests
bash#!/bin/bash
# scripts/run-tests.sh

echo "Running ERA Monitor Tests..."

# Run all tests
dotnet test ERAMonitor.sln --verbosity normal

# Run with coverage
dotnet test ERAMonitor.sln \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults

1.9 Phase 1 Checklist
markdown# Phase 1 Completion Checklist

## Project Setup
- [ ] Solution structure created
- [ ] All projects added to solution
- [ ] Project references configured
- [ ] NuGet packages installed
- [ ] .gitignore configured

## Database
- [ ] PostgreSQL schema created
- [ ] All tables created with proper relationships
- [ ] Indexes created for performance
- [ ] Triggers for updated_at columns
- [ ] Seed data inserted (organization, admin user, locations)

## Entity Framework
- [ ] DbContext configured with all DbSets
- [ ] Entity configurations created
- [ ] Enum mappings configured
- [ ] Initial migration created and applied
- [ ] Database seeder working

## API Configuration
- [ ] appsettings.json configured
- [ ] JWT authentication configured
- [ ] CORS policy configured
- [ ] Swagger/OpenAPI configured
- [ ] Health checks configured
- [ ] Serilog logging configured

## Infrastructure
- [ ] Docker Compose for development
- [ ] PostgreSQL container running
- [ ] Redis container running
- [ ] API container building and running

## Middleware
- [ ] Exception handling middleware
- [ ] Request logging middleware
- [ ] API key authentication middleware

## SignalR
- [ ] MonitoringHub created
- [ ] Hub extension methods for broadcasting

## Scripts
- [ ] Database initialization script
- [ ] Development start script
- [ ] Test runner script

## Verification
- [ ] API starts without errors
- [ ] Swagger UI accessible at /swagger
- [ ] Health endpoint returns healthy
- [ ] Database connection working
- [ ] Redis connection working
- [ ] Can login with admin user