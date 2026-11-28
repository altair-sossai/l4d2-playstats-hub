using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Audience = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    Topic = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Values = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReads",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationReads", x => new { x.NotificationId, x.CommunityId });
                    table.ForeignKey(
                        name: "FK_NotificationReads_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReads_CommunityId",
                table: "NotificationReads",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Audience_CommunityId_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "Audience", "CommunityId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAtUtc",
                table: "Notifications",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationReads");

            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
