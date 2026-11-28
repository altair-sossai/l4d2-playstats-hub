using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Bans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    Command = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    GameServerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RelatedToBanId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedByName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedByName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LiftedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    LiftedByName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LiftedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LiftReason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BanAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BanId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanAlerts_Bans_BanId",
                        column: x => x.BanId,
                        principalTable: "Bans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanAlerts_BanId_CommunityId",
                table: "BanAlerts",
                columns: new[] { "BanId", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BanAlerts_Status_CommunityId",
                table: "BanAlerts",
                columns: new[] { "Status", "CommunityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bans_CommunityId_LiftedAtUtc_EndsAtUtc",
                table: "Bans",
                columns: new[] { "CommunityId", "LiftedAtUtc", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bans_CreatedAtUtc",
                table: "Bans",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanAlerts");

            migrationBuilder.DropTable(
                name: "Bans");
        }
    }
}
