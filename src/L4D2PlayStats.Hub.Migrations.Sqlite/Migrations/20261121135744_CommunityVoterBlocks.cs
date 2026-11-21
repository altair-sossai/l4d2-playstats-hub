using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class CommunityVoterBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityVoterBlock",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    BlockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BlockedBy = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityVoterBlock", x => x.CommunityId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityVoterBlock");
        }
    }
}
