using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class SkinAuthors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkinAuthors",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SyncedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RenamedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinAuthors", x => x.CommunityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkinAuthors_SyncedAtUtc",
                table: "SkinAuthors",
                column: "SyncedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkinAuthors");
        }
    }
}
