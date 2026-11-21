using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class PlayerRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountCreatedAtUtc",
                table: "PlayerProfiles",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysSinceLastBan",
                table: "PlayerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "PlayerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForcedName",
                table: "PlayerProfiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ForcedNameUpdatedAtUtc",
                table: "PlayerProfiles",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ForcedNameUpdatedBy",
                table: "PlayerProfiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameBans",
                table: "PlayerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Left4Dead2Minutes",
                table: "PlayerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Left4Dead2RecentMinutes",
                table: "PlayerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MostUsedName",
                table: "PlayerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SteamLevel",
                table: "PlayerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SteamName",
                table: "PlayerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SteamUpdatedAtUtc",
                table: "PlayerProfiles",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VacBans",
                table: "PlayerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_DisplayName",
                table: "PlayerProfiles",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_ForcedName",
                table: "PlayerProfiles",
                column: "ForcedName");

            migrationBuilder.Sql(@"INSERT INTO ""PlayerProfiles"" (""CommunityId"", ""FirstSeenAtUtc"", ""LastSeenAtUtc"") SELECT f.""CommunityId"", f.""UpdatedAtUtc"", f.""UpdatedAtUtc"" FROM ""ForcedName"" f WHERE NOT EXISTS (SELECT 1 FROM ""PlayerProfiles"" p WHERE p.""CommunityId"" = f.""CommunityId"");");

            migrationBuilder.Sql(@"UPDATE ""PlayerProfiles"" SET ""ForcedName"" = (SELECT f.""Name"" FROM ""ForcedName"" f WHERE f.""CommunityId"" = ""PlayerProfiles"".""CommunityId""), ""ForcedNameUpdatedAtUtc"" = (SELECT f.""UpdatedAtUtc"" FROM ""ForcedName"" f WHERE f.""CommunityId"" = ""PlayerProfiles"".""CommunityId""), ""ForcedNameUpdatedBy"" = (SELECT f.""UpdatedBy"" FROM ""ForcedName"" f WHERE f.""CommunityId"" = ""PlayerProfiles"".""CommunityId"") WHERE EXISTS (SELECT 1 FROM ""ForcedName"" f WHERE f.""CommunityId"" = ""PlayerProfiles"".""CommunityId"");");

            migrationBuilder.DropTable(
                name: "ForcedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForcedName",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForcedName", x => x.CommunityId);
                });

            migrationBuilder.Sql(@"INSERT INTO ""ForcedName"" (""CommunityId"", ""Name"", ""UpdatedAtUtc"", ""UpdatedBy"") SELECT ""CommunityId"", ""ForcedName"", COALESCE(""ForcedNameUpdatedAtUtc"", ""LastSeenAtUtc""), ""ForcedNameUpdatedBy"" FROM ""PlayerProfiles"" WHERE ""ForcedName"" IS NOT NULL;");

            migrationBuilder.DropIndex(
                name: "IX_PlayerProfiles_DisplayName",
                table: "PlayerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_PlayerProfiles_ForcedName",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "AccountCreatedAtUtc",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "DaysSinceLastBan",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "ForcedName",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "ForcedNameUpdatedAtUtc",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "ForcedNameUpdatedBy",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "GameBans",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "Left4Dead2Minutes",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "Left4Dead2RecentMinutes",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "MostUsedName",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "SteamLevel",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "SteamName",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "SteamUpdatedAtUtc",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "VacBans",
                table: "PlayerProfiles");
        }
    }
}
