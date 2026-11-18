using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AntiCheatAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AntiCheatAccessSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedByName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    UpdatedFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntiCheatAccessSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AntiCheatRequired",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AddedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    AddedByName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AddedFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntiCheatRequired", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "AntiCheatWhitelist",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AddedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    AddedByName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AddedFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntiCheatWhitelist", x => x.CommunityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AntiCheatRequired_CreatedAtUtc",
                table: "AntiCheatRequired",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AntiCheatWhitelist_CreatedAtUtc",
                table: "AntiCheatWhitelist",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AntiCheatAccessSettings");

            migrationBuilder.DropTable(
                name: "AntiCheatRequired");

            migrationBuilder.DropTable(
                name: "AntiCheatWhitelist");
        }
    }
}
