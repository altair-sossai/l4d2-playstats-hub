using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.PostgreSql.Migrations
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
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChampionshipJoinRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChampionshipId = table.Column<int>(type: "integer", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
