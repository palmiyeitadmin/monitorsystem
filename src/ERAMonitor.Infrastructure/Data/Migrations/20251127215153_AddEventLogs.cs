using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERAMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailSubscribed",
                table: "StatusPageSubscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "StatusPageSubscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SmsSubscribed",
                table: "StatusPageSubscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StatusPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleAnalyticsId",
                table: "StatusPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportUrl",
                table: "StatusPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "StatusPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "StatusPages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "StatusPageComponents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StatusPageComponents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StartupType",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogName = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventLogs_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_HostId",
                table: "EventLogs",
                column: "HostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropColumn(
                name: "EmailSubscribed",
                table: "StatusPageSubscribers");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "StatusPageSubscribers");

            migrationBuilder.DropColumn(
                name: "SmsSubscribed",
                table: "StatusPageSubscribers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StatusPages");

            migrationBuilder.DropColumn(
                name: "GoogleAnalyticsId",
                table: "StatusPages");

            migrationBuilder.DropColumn(
                name: "SupportUrl",
                table: "StatusPages");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "StatusPages");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "StatusPages");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "StatusPageComponents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StatusPageComponents");

            migrationBuilder.DropColumn(
                name: "StartupType",
                table: "Services");
        }
    }
}
