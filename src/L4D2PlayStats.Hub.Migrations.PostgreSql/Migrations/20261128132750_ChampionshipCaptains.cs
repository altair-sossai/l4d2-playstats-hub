using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class ChampionshipCaptains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Flag",
                table: "ChampionshipTeams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingAtUtc",
                table: "ChampionshipTeams",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PendingByCommunityId",
                table: "ChampionshipTeams",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingFlag",
                table: "ChampionshipTeams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingName",
                table: "ChampionshipTeams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingTag",
                table: "ChampionshipTeams",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ChampionshipTeams",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutoCloseMinutes",
                table: "Championships",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "AutoLinkHours",
                table: "Championships",
                type: "integer",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.AddColumn<bool>(
                name: "AutoLinkMatches",
                table: "Championships",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoLinked",
                table: "ChampionshipGames",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatisticsAtUtc",
                table: "ChampionshipGames",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnlinkedMatchId",
                table: "ChampionshipGames",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flag",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "PendingAtUtc",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "PendingByCommunityId",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "PendingFlag",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "PendingName",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "PendingTag",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ChampionshipTeams");

            migrationBuilder.DropColumn(
                name: "AutoCloseMinutes",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "AutoLinkHours",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "AutoLinkMatches",
                table: "Championships");

            migrationBuilder.DropColumn(
                name: "AutoLinked",
                table: "ChampionshipGames");

            migrationBuilder.DropColumn(
                name: "LastStatisticsAtUtc",
                table: "ChampionshipGames");

            migrationBuilder.DropColumn(
                name: "UnlinkedMatchId",
                table: "ChampionshipGames");
        }
    }
}
