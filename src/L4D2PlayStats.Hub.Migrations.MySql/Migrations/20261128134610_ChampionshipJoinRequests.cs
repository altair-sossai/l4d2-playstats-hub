using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.MySql.Migrations
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
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChampionshipJoinRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AnsweredBy = table.Column<long>(type: "bigint", nullable: true)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
