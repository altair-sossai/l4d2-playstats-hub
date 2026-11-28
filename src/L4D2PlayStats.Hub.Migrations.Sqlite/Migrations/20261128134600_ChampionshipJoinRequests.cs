using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class ChampionshipJoinRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "JoinRequests",
                table: "Championships",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChampionshipJoinRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChampionshipId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AnsweredBy = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipJoinRequests_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipJoinRequests_ChampionshipId_CommunityId",
                table: "ChampionshipJoinRequests",
                columns: new[] { "ChampionshipId", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipJoinRequests_ChampionshipId_Status",
                table: "ChampionshipJoinRequests",
                columns: new[] { "ChampionshipId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChampionshipJoinRequests");

            migrationBuilder.DropColumn(
                name: "JoinRequests",
                table: "Championships");
        }
    }
}
