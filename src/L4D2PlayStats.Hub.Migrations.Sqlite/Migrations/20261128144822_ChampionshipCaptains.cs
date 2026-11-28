using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
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
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingAtUtc",
                table: "ChampionshipTeams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PendingByCommunityId",
                table: "ChampionshipTeams",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingFlag",
                table: "ChampionshipTeams",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingName",
                table: "ChampionshipTeams",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingTag",
                table: "ChampionshipTeams",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ChampionshipTeams",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutoCloseMinutes",
                table: "Championships",
                type: "INTEGER",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "AutoLinkHours",
                table: "Championships",
                type: "INTEGER",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.AddColumn<bool>(
                name: "AutoLinkMatches",
                table: "Championships",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoLinked",
                table: "ChampionshipGames",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatisticsAtUtc",
                table: "ChampionshipGames",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnlinkedMatchId",
                table: "ChampionshipGames",
                type: "INTEGER",
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
