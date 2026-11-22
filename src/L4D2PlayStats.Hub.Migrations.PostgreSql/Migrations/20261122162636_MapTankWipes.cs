using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class MapTankWipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TankWipeByTeamA",
                table: "StatisticsFiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TankWipeByTeamB",
                table: "StatisticsFiles",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TankWipeByTeamA",
                table: "StatisticsFiles");

            migrationBuilder.DropColumn(
                name: "TankWipeByTeamB",
                table: "StatisticsFiles");
        }
    }
}
