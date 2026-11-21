using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class CommunityRatingDeviation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Deviation",
                table: "CommunityRating",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deviation",
                table: "CommunityRating");
        }
    }
}
