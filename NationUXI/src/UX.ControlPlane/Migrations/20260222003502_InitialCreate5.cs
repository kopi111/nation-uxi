using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UX.ControlPlane.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Actor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScopeType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NodeType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrgNodes_OrgNodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OrgNodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConfigAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigAssignments_ConfigProfiles_ConfigProfileId",
                        column: x => x.ConfigProfileId,
                        principalTable: "ConfigProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrgNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedUser = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OsFamily = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    OsVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AgentVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastGateway = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastSsid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastBssid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastAppliedConfigVersion = table.Column<long>(type: "bigint", nullable: false),
                    CanSend = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.AgentId);
                    table.ForeignKey(
                        name: "FK_Agents_OrgNodes_OrgNodeId",
                        column: x => x.OrgNodeId,
                        principalTable: "OrgNodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AgentTelemetryLatest",
                columns: table => new
                {
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastPayloadReceivedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentTelemetryLatest", x => x.AgentId);
                    table.ForeignKey(
                        name: "FK_AgentTelemetryLatest_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "AgentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_LastSeenUtc",
                table: "Agents",
                column: "LastSeenUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_MachineName",
                table: "Agents",
                column: "MachineName");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_OrgNodeId",
                table: "Agents",
                column: "OrgNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Actor",
                table: "AuditLog",
                column: "Actor");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Entity",
                table: "AuditLog",
                column: "Entity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Utc",
                table: "AuditLog",
                column: "Utc");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigAssignments_ConfigProfileId",
                table: "ConfigAssignments",
                column: "ConfigProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigAssignments_ScopeType_ScopeId",
                table: "ConfigAssignments",
                columns: new[] { "ScopeType", "ScopeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigVersions_ScopeType_ScopeId_Hash",
                table: "ConfigVersions",
                columns: new[] { "ScopeType", "ScopeId", "Hash" });

            migrationBuilder.CreateIndex(
                name: "IX_OrgNodes_ParentId",
                table: "OrgNodes",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentTelemetryLatest");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "ConfigAssignments");

            migrationBuilder.DropTable(
                name: "ConfigVersions");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "ConfigProfiles");

            migrationBuilder.DropTable(
                name: "OrgNodes");
        }
    }
}
