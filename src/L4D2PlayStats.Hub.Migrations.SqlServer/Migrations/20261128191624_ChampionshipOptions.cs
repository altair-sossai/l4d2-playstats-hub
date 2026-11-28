using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class ChampionshipOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AntiCheatRequired",
                table: "Championships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SuggestedMinute",
                table: "Championships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuggestedWeekday",
                table: "Championships",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChampionshipCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipCampaigns_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChampionshipCampaigns_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipCampaigns_CampaignId",
                table: "ChampionshipCampaigns",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipCampaigns_ChampionshipId_CampaignId",
                table: "ChampionshipCampaigns",
                columns: new[] { "ChampionshipId", "CampaignId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChampionshipCampaigns");

            migrationBuilder.DropColumn(
                name: "AntiCheatRequired",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "SuggestedMinute",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "SuggestedWeekday",
                table: "Championships");
        }
    }
}
