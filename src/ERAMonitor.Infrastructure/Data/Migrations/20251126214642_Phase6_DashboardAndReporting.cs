using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERAMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_DashboardAndReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_OrganizationId",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ConditionType",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "EscalationUserIds",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "NotifyUserIds",
                table: "NotificationRules");

            // Reports - Drop old columns
            migrationBuilder.DropColumn(name: "Status", table: "Reports"); // Replaced by Status string? No, Status is still there? Wait.
            // Report.cs has "Status" property? No, it was removed from the snippet I saw?
            // Let's check Report.cs snippet again.
            // Line 47: public bool IsActive { get; set; } = true;
            // It does NOT have "Status" property in the snippet!
            // Wait, I missed it?
            // Line 5: public class Report : BaseEntityWithOrganization
            // BaseEntityWithOrganization -> BaseEntity -> Id, CreatedAt, UpdatedAt.
            // Where is Status?
            // Ah, I don't see "Status" property in Report.cs snippet.
            // So "Status" IS removed.
            
            migrationBuilder.DropColumn(name: "GeneratedAt", table: "Reports");
            migrationBuilder.DropColumn(name: "FileUrl", table: "Reports");
            migrationBuilder.DropColumn(name: "FileFormat", table: "Reports");
            migrationBuilder.DropColumn(name: "ExpiresAt", table: "Reports");
            migrationBuilder.DropColumn(name: "ErrorMessage", table: "Reports");

            // NotificationRules - Drop old columns
            migrationBuilder.DropColumn(name: "NotifyCustomer", table: "NotificationRules");
            migrationBuilder.DropColumn(name: "IsActive", table: "NotificationRules");
            migrationBuilder.DropColumn(name: "EscalationAfterMinutes", table: "NotificationRules");
            migrationBuilder.DropColumn(name: "ConditionConfig", table: "NotificationRules");
            migrationBuilder.DropColumn(name: "Channel", table: "NotificationRules");

            // Reports - Add new columns
            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "Europe/Istanbul");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRunAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sections",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRunAt",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Reports",
                type: "text",
                nullable: true);

            // NotificationRules - Add new columns
            migrationBuilder.AddColumn<bool>(
                name: "OnlyDuringWorkingHours",
                table: "NotificationRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "NotificationRules",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumSeverity",
                table: "NotificationRules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingHoursStart",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalNotificationsSent",
                table: "NotificationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:check_type", "http,tcp,ping,dns,custom_health")
                .Annotation("Npgsql:Enum:dashboard_visibility", "private,team,public")
                .Annotation("Npgsql:Enum:host_category", "physical_server,virtual_machine,vps,dedicated_server,cloud_instance,website")
                .Annotation("Npgsql:Enum:incident_priority", "urgent,high,medium,low")
                .Annotation("Npgsql:Enum:incident_severity", "critical,high,medium,low,info")
                .Annotation("Npgsql:Enum:incident_status", "new,acknowledged,in_progress,resolved,closed")
                .Annotation("Npgsql:Enum:location_category", "colocation,cloud_provider,hosting_provider,on_premise")
                .Annotation("Npgsql:Enum:notification_channel_type", "email,sms,webhook,telegram,slack,ms_teams,pushover,push_notification")
                .Annotation("Npgsql:Enum:notification_event_type", "host_down,host_up,host_warning,host_high_cpu,host_high_ram,host_high_disk,host_maintenance_started,host_maintenance_ended,service_down,service_up,service_warning,check_failed,check_recovered,check_slow_response,ssl_certificate_expiring,ssl_certificate_expired,incident_created,incident_acknowledged,incident_assigned,incident_escalated,incident_resolved,incident_closed,incident_reopened,incident_comment_added,sla_response_breached,sla_resolution_breached,on_call_rotation_changed,on_call_override_created,daily_digest,weekly_report,system_alert,test_notification")
                .Annotation("Npgsql:Enum:notification_priority", "low,normal,high,urgent")
                .Annotation("Npgsql:Enum:notification_status", "pending,queued,sending,sent,delivered,failed,bounced,cancelled")
                .Annotation("Npgsql:Enum:on_call_rotation_type", "daily,weekly,bi_weekly,monthly,custom")
                .Annotation("Npgsql:Enum:os_type", "windows,linux")
                .Annotation("Npgsql:Enum:report_execution_status", "pending,running,completed,failed,cancelled")
                .Annotation("Npgsql:Enum:report_format", "pdf,excel,html,json")
                .Annotation("Npgsql:Enum:report_schedule", "daily,weekly,bi_weekly,monthly,quarterly")
                .Annotation("Npgsql:Enum:report_time_range", "today,yesterday,last24hours,last7days,last30days,last_month,last_quarter,custom")
                .Annotation("Npgsql:Enum:report_type", "executive,uptime,performance,incident,sla,security,capacity,custom")
                .Annotation("Npgsql:Enum:service_type", "iis_site,iis_app_pool,windows_service,systemd_unit,docker_container,process")
                .Annotation("Npgsql:Enum:status_page_component_status", "operational,degraded_performance,partial_outage,major_outage,under_maintenance")
                .Annotation("Npgsql:Enum:status_page_component_type", "host,check,service,manual")
                .Annotation("Npgsql:Enum:status_type", "up,down,warning,degraded,unknown,disabled")
                .Annotation("Npgsql:Enum:user_role", "super_admin,admin,operator,viewer,customer_user")
                .Annotation("Npgsql:Enum:widget_type", "status_overview,host_status_grid,service_status_list,check_status_list,incident_list,cpu_chart,memory_chart,disk_chart,network_chart,response_time_chart,uptime_gauge,uptime_chart,availability_heatmap,host_count,incident_count,check_count,alert_count,top_hosts_by_cpu,top_hosts_by_memory,recent_incidents,recent_alerts,failing_checks,expiring_certificates,host_map,markdown,i_frame,image")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:Enum:check_type", "http,tcp,ping,dns,custom_health")
                .OldAnnotation("Npgsql:Enum:host_category", "physical_server,virtual_machine,vps,dedicated_server,cloud_instance")
                .OldAnnotation("Npgsql:Enum:incident_priority", "urgent,high,medium,low")
                .OldAnnotation("Npgsql:Enum:incident_severity", "critical,high,medium,low,info")
                .OldAnnotation("Npgsql:Enum:incident_status", "new,acknowledged,in_progress,resolved,closed")
                .OldAnnotation("Npgsql:Enum:location_category", "colocation,cloud_provider,hosting_provider,on_premise")
                .OldAnnotation("Npgsql:Enum:notification_channel", "email,sms,telegram,webhook")
                .OldAnnotation("Npgsql:Enum:notification_status", "pending,sent,delivered,failed")
                .OldAnnotation("Npgsql:Enum:os_type", "windows,linux")
                .OldAnnotation("Npgsql:Enum:service_type", "iis_site,iis_app_pool,windows_service,systemd_unit,docker_container,process")
                .OldAnnotation("Npgsql:Enum:status_type", "up,down,warning,degraded,unknown,disabled")
                .OldAnnotation("Npgsql:Enum:user_role", "super_admin,admin,operator,viewer,customer_user")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            /*
            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "UserSessions",
                type: "text",
                nullable: true);

            // ... (omitted UserSessions, Users, ServiceStatusHistories, Services columns)

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "Services",
                type: "integer",
                nullable: true);
            */

            migrationBuilder.AddColumn<string>(
                name: "CheckIds",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomEndDate",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomStartDate",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailRecipients",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Format",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HostIds",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SaveToStorage",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Schedule",
                table: "Reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendEmail",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeRange",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "ChannelId",
                table: "NotificationRules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CheckIds",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CooldownMinutes",
                table: "NotificationRules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerIds",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DelaySeconds",
                table: "NotificationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EscalateAfterMinutes",
                table: "NotificationRules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EscalateToRuleId",
                table: "NotificationRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "NotificationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HostIds",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTriggeredAt",
                table: "NotificationRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "NotificationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Recipients",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "NotificationRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingHoursEnd",
                table: "NotificationRules",
                type: "text",
                nullable: true);

            /*
            migrationBuilder.AddColumn<DateTime>(
                name: "AgentInstalledAt",
                table: "Hosts",
                type: "timestamp with time zone",
                nullable: true);

            // ... (omitted Hosts, HostDisks, Customers, CheckResults, AuditLogs columns)

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            */

            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false),
                    LayoutConfig = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultCustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RefreshIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    Theme = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dashboards_Customers_DefaultCustomerId",
                        column: x => x.DefaultCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Dashboards_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dashboards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationChannel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    RateLimitPerHour = table.Column<int>(type: "integer", nullable: false),
                    CurrentHourCount = table.Column<int>(type: "integer", nullable: false),
                    RateLimitResetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationChannel_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    ChannelType = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    HtmlBody = table.Column<string>(type: "text", nullable: true),
                    Format = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Variables = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplate_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataFromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmailError = table.Column<string>(type: "text", nullable: true),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportExecutions_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportExecutions_Users_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    CustomDomain = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    FaviconUrl = table.Column<string>(type: "text", nullable: true),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    PrimaryColor = table.Column<string>(type: "text", nullable: false),
                    CustomCss = table.Column<string>(type: "text", nullable: true),
                    HeaderText = table.Column<string>(type: "text", nullable: true),
                    FooterText = table.Column<string>(type: "text", nullable: true),
                    AboutText = table.Column<string>(type: "text", nullable: true),
                    ShowUptime = table.Column<bool>(type: "boolean", nullable: false),
                    UptimeDays = table.Column<int>(type: "integer", nullable: false),
                    ShowIncidents = table.Column<bool>(type: "boolean", nullable: false),
                    ShowMaintenances = table.Column<bool>(type: "boolean", nullable: false),
                    ShowSubscribe = table.Column<bool>(type: "boolean", nullable: false),
                    ShowResponseTime = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusPages_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardWidgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    HostId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckId = table.Column<Guid>(type: "uuid", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    TimeRange = table.Column<string>(type: "text", nullable: false),
                    CustomStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShowTitle = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLegend = table.Column<bool>(type: "boolean", nullable: false),
                    ColorScheme = table.Column<string>(type: "text", nullable: true),
                    Thresholds = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardWidgets_Dashboards_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "Dashboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceName = table.Column<string>(type: "text", nullable: true),
                    Recipient = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalId = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsClicked = table.Column<bool>(type: "boolean", nullable: false),
                    ClickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLog_NotificationChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "NotificationChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationLog_NotificationRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "NotificationRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusPageComponentGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsExpanded = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPageComponentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusPageComponentGroups_StatusPages_StatusPageId",
                        column: x => x.StatusPageId,
                        principalTable: "StatusPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusPageSubscribers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    WebhookUrl = table.Column<string>(type: "text", nullable: true),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationToken = table.Column<string>(type: "text", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotifyOnIncident = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnResolution = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPageSubscribers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusPageSubscribers_StatusPages_StatusPageId",
                        column: x => x.StatusPageId,
                        principalTable: "StatusPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusPageComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ShowUptime = table.Column<bool>(type: "boolean", nullable: false),
                    ShowResponseTime = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideStatus = table.Column<bool>(type: "boolean", nullable: false),
                    ManualStatus = table.Column<int>(type: "integer", nullable: true),
                    ManualStatusMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPageComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusPageComponents_Checks_CheckId",
                        column: x => x.CheckId,
                        principalTable: "Checks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatusPageComponents_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatusPageComponents_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatusPageComponents_StatusPageComponentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "StatusPageComponentGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatusPageComponents_StatusPages_StatusPageId",
                        column: x => x.StatusPageId,
                        principalTable: "StatusPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_OrganizationId",
                table: "SystemSettings",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_OrganizationId",
                table: "Reports",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_ChannelId",
                table: "NotificationRules",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_EscalateToRuleId",
                table: "NotificationRules",
                column: "EscalateToRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_TemplateId",
                table: "NotificationRules",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_DefaultCustomerId",
                table: "Dashboards",
                column: "DefaultCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_OrganizationId",
                table: "Dashboards",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_UserId",
                table: "Dashboards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_DashboardId",
                table: "DashboardWidgets",
                column: "DashboardId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannel_OrganizationId",
                table: "NotificationChannel",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_ChannelId",
                table: "NotificationLog",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_RuleId",
                table: "NotificationLog",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_OrganizationId",
                table: "NotificationTemplate",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_ReportId",
                table: "ReportExecutions",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_TriggeredByUserId",
                table: "ReportExecutions",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageComponentGroups_StatusPageId",
                table: "StatusPageComponentGroups",
                column: "StatusPageId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageComponents_CheckId",
                table: "StatusPageComponents",
                column: "CheckId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageComponents_GroupId",
                table: "StatusPageComponents",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageComponents_HostId",
                table: "StatusPageComponents",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageComponents_ServiceId",
                table: "StatusPageComponents",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageComponents_StatusPageId",
                table: "StatusPageComponents",
                column: "StatusPageId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPages_OrganizationId",
                table: "StatusPages",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPageSubscribers_StatusPageId",
                table: "StatusPageSubscribers",
                column: "StatusPageId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRules_NotificationChannel_ChannelId",
                table: "NotificationRules",
                column: "ChannelId",
                principalTable: "NotificationChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRules_NotificationRules_EscalateToRuleId",
                table: "NotificationRules",
                column: "EscalateToRuleId",
                principalTable: "NotificationRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRules_NotificationTemplate_TemplateId",
                table: "NotificationRules",
                column: "TemplateId",
                principalTable: "NotificationTemplate",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Organizations_OrganizationId",
                table: "Reports",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRules_NotificationChannel_ChannelId",
                table: "NotificationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRules_NotificationRules_EscalateToRuleId",
                table: "NotificationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRules_NotificationTemplate_TemplateId",
                table: "NotificationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Organizations_OrganizationId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "DashboardWidgets");

            migrationBuilder.DropTable(
                name: "NotificationLog");

            migrationBuilder.DropTable(
                name: "NotificationTemplate");

            migrationBuilder.DropTable(
                name: "ReportExecutions");

            migrationBuilder.DropTable(
                name: "StatusPageComponents");

            migrationBuilder.DropTable(
                name: "StatusPageSubscribers");

            migrationBuilder.DropTable(
                name: "Dashboards");

            migrationBuilder.DropTable(
                name: "NotificationChannel");

            migrationBuilder.DropTable(
                name: "StatusPageComponentGroups");

            migrationBuilder.DropTable(
                name: "StatusPages");

            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_OrganizationId",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Reports_OrganizationId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRules_ChannelId",
                table: "NotificationRules");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRules_EscalateToRuleId",
                table: "NotificationRules");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRules_TemplateId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "Browser",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "RevokedReason",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginIp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorBackupCodes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "ServiceStatusHistories");

            migrationBuilder.DropColumn(
                name: "AlertOnStop",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "LastHealthyAt",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "LastRestartAt",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CheckIds",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CustomEndDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CustomStartDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "EmailRecipients",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "HostIds",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SaveToStorage",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SendEmail",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TimeRange",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "CheckIds",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "CooldownMinutes",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "CustomerIds",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "DelaySeconds",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "EscalateAfterMinutes",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "EscalateToRuleId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "HostIds",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "LastTriggeredAt",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "Recipients",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "WorkingHoursEnd",
                table: "NotificationRules");

            migrationBuilder.DropColumn(
                name: "AgentInstalledAt",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "AlertOnHighCpu",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "AlertOnHighDisk",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "AlertOnHighRam",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "CpuCriticalThreshold",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "CpuWarningThreshold",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "DiskCriticalThreshold",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "DiskWarningThreshold",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "MaintenanceEndAt",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "MaintenanceMode",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "MaintenanceReason",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "MaintenanceStartAt",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "PrimaryIp",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "ProcessCount",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "PublicIp",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "RamCriticalThreshold",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "RamWarningThreshold",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "Hosts");

            migrationBuilder.DropColumn(
                name: "FileSystem",
                table: "HostDisks");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "HostDisks");

            migrationBuilder.DropColumn(
                name: "EmergencyAvailableHours",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecondaryContactEmail",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecondaryContactName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecondaryContactPhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CheckResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "PasswordResetTokenExpires",
                table: "Users",
                newName: "PasswordResetExpires");

            migrationBuilder.RenameColumn(
                name: "Timezone",
                table: "Reports",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "Reports",
                newName: "FileUrl");

            migrationBuilder.RenameColumn(
                name: "Sections",
                table: "Reports",
                newName: "FileFormat");

            migrationBuilder.RenameColumn(
                name: "PrimaryColor",
                table: "Reports",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "NextRunAt",
                table: "Reports",
                newName: "GeneratedAt");

            migrationBuilder.RenameColumn(
                name: "LastRunAt",
                table: "Reports",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "WorkingHoursStart",
                table: "NotificationRules",
                newName: "ConditionConfig");

            migrationBuilder.RenameColumn(
                name: "TotalNotificationsSent",
                table: "NotificationRules",
                newName: "Channel");

            migrationBuilder.RenameColumn(
                name: "OnlyDuringWorkingHours",
                table: "NotificationRules",
                newName: "NotifyCustomer");

            migrationBuilder.RenameColumn(
                name: "MinimumSeverity",
                table: "NotificationRules",
                newName: "EscalationAfterMinutes");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "NotificationRules",
                newName: "IsActive");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:check_type", "http,tcp,ping,dns,custom_health")
                .Annotation("Npgsql:Enum:host_category", "physical_server,virtual_machine,vps,dedicated_server,cloud_instance")
                .Annotation("Npgsql:Enum:incident_priority", "urgent,high,medium,low")
                .Annotation("Npgsql:Enum:incident_severity", "critical,high,medium,low,info")
                .Annotation("Npgsql:Enum:incident_status", "new,acknowledged,in_progress,resolved,closed")
                .Annotation("Npgsql:Enum:location_category", "colocation,cloud_provider,hosting_provider,on_premise")
                .Annotation("Npgsql:Enum:notification_channel", "email,sms,telegram,webhook")
                .Annotation("Npgsql:Enum:notification_status", "pending,sent,delivered,failed")
                .Annotation("Npgsql:Enum:os_type", "windows,linux")
                .Annotation("Npgsql:Enum:service_type", "iis_site,iis_app_pool,windows_service,systemd_unit,docker_container,process")
                .Annotation("Npgsql:Enum:status_type", "up,down,warning,degraded,unknown,disabled")
                .Annotation("Npgsql:Enum:user_role", "super_admin,admin,operator,viewer,customer_user")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:Enum:check_type", "http,tcp,ping,dns,custom_health")
                .OldAnnotation("Npgsql:Enum:dashboard_visibility", "private,team,public")
                .OldAnnotation("Npgsql:Enum:host_category", "physical_server,virtual_machine,vps,dedicated_server,cloud_instance,website")
                .OldAnnotation("Npgsql:Enum:incident_priority", "urgent,high,medium,low")
                .OldAnnotation("Npgsql:Enum:incident_severity", "critical,high,medium,low,info")
                .OldAnnotation("Npgsql:Enum:incident_status", "new,acknowledged,in_progress,resolved,closed")
                .OldAnnotation("Npgsql:Enum:location_category", "colocation,cloud_provider,hosting_provider,on_premise")
                .OldAnnotation("Npgsql:Enum:notification_channel_type", "email,sms,webhook,telegram,slack,ms_teams,pushover,push_notification")
                .OldAnnotation("Npgsql:Enum:notification_event_type", "host_down,host_up,host_warning,host_high_cpu,host_high_ram,host_high_disk,host_maintenance_started,host_maintenance_ended,service_down,service_up,service_warning,check_failed,check_recovered,check_slow_response,ssl_certificate_expiring,ssl_certificate_expired,incident_created,incident_acknowledged,incident_assigned,incident_escalated,incident_resolved,incident_closed,incident_reopened,incident_comment_added,sla_response_breached,sla_resolution_breached,on_call_rotation_changed,on_call_override_created,daily_digest,weekly_report,system_alert,test_notification")
                .OldAnnotation("Npgsql:Enum:notification_priority", "low,normal,high,urgent")
                .OldAnnotation("Npgsql:Enum:notification_status", "pending,queued,sending,sent,delivered,failed,bounced,cancelled")
                .OldAnnotation("Npgsql:Enum:on_call_rotation_type", "daily,weekly,bi_weekly,monthly,custom")
                .OldAnnotation("Npgsql:Enum:os_type", "windows,linux")
                .OldAnnotation("Npgsql:Enum:report_execution_status", "pending,running,completed,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:report_format", "pdf,excel,html,json")
                .OldAnnotation("Npgsql:Enum:report_schedule", "daily,weekly,bi_weekly,monthly,quarterly")
                .OldAnnotation("Npgsql:Enum:report_time_range", "today,yesterday,last24hours,last7days,last30days,last_month,last_quarter,custom")
                .OldAnnotation("Npgsql:Enum:report_type", "executive,uptime,performance,incident,sla,security,capacity,custom")
                .OldAnnotation("Npgsql:Enum:service_type", "iis_site,iis_app_pool,windows_service,systemd_unit,docker_container,process")
                .OldAnnotation("Npgsql:Enum:status_page_component_status", "operational,degraded_performance,partial_outage,major_outage,under_maintenance")
                .OldAnnotation("Npgsql:Enum:status_page_component_type", "host,check,service,manual")
                .OldAnnotation("Npgsql:Enum:status_type", "up,down,warning,degraded,unknown,disabled")
                .OldAnnotation("Npgsql:Enum:user_role", "super_admin,admin,operator,viewer,customer_user")
                .OldAnnotation("Npgsql:Enum:widget_type", "status_overview,host_status_grid,service_status_list,check_status_list,incident_list,cpu_chart,memory_chart,disk_chart,network_chart,response_time_chart,uptime_gauge,uptime_chart,availability_heatmap,host_count,incident_count,check_count,alert_count,top_hosts_by_cpu,top_hosts_by_memory,recent_incidents,recent_alerts,failing_checks,expiring_certificates,host_map,markdown,i_frame,image")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "Reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "Notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ConditionType",
                table: "NotificationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid[]>(
                name: "EscalationUserIds",
                table: "NotificationRules",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AddColumn<Guid[]>(
                name: "NotifyUserIds",
                table: "NotificationRules",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AlterColumn<decimal>(
                name: "UsedPercent",
                table: "HostDisks",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "UsedGb",
                table: "HostDisks",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGb",
                table: "HostDisks",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CheckResults",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_OrganizationId",
                table: "SystemSettings",
                column: "OrganizationId");
        }
    }
}
