using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class ChampionshipBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerImage",
                table: "Championships",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GalleryImages",
                table: "Championships",
                type: "varchar(972)",
                maxLength: 972,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoImage",
                table: "Championships",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerImage",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "GalleryImages",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "LogoImage",
                table: "Championships");
        }
    }
}
