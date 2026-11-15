using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administrators",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AddedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrators", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TeamSize = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameModeRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ConfigurationPattern = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TeamSize = table.Column<int>(type: "INTEGER", nullable: true),
                    MinimumMaps = table.Column<int>(type: "INTEGER", nullable: false),
                    CountsForRanking = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameModeRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    KeyHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AvatarUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CampaignMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    MapName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignMaps_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Campaign = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ConfigurationName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TeamSize = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstFileDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastFileDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TeamAScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamBScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MapCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CountsForRanking = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_GameServers_GameServerId",
                        column: x => x.GameServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Punishments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Punishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Punishments_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamA = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeamB = table.Column<bool>(type: "INTEGER", nullable: false),
                    Win = table.Column<bool>(type: "INTEGER", nullable: false),
                    Loss = table.Column<bool>(type: "INTEGER", nullable: false),
                    Draws = table.Column<int>(type: "INTEGER", nullable: false),
                    RageQuits = table.Column<int>(type: "INTEGER", nullable: false),
                    Participant = table.Column<bool>(type: "INTEGER", nullable: false),
                    Died = table.Column<int>(type: "INTEGER", nullable: false),
                    Incaps = table.Column<int>(type: "INTEGER", nullable: false),
                    DmgTaken = table.Column<int>(type: "INTEGER", nullable: false),
                    Common = table.Column<int>(type: "INTEGER", nullable: false),
                    SiKilled = table.Column<int>(type: "INTEGER", nullable: false),
                    SiDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    TankDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    RockEats = table.Column<int>(type: "INTEGER", nullable: false),
                    WitchDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    Skeets = table.Column<int>(type: "INTEGER", nullable: false),
                    SkeetsMelee = table.Column<int>(type: "INTEGER", nullable: false),
                    Levels = table.Column<int>(type: "INTEGER", nullable: false),
                    Crowns = table.Column<int>(type: "INTEGER", nullable: false),
                    FfGiven = table.Column<int>(type: "INTEGER", nullable: false),
                    DmgTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    DmgTank = table.Column<int>(type: "INTEGER", nullable: false),
                    DmgSpit = table.Column<int>(type: "INTEGER", nullable: false),
                    HunterDpDmg = table.Column<int>(type: "INTEGER", nullable: false),
                    MvpSiDamage = table.Column<int>(type: "INTEGER", nullable: false),
                    MvpCommon = table.Column<int>(type: "INTEGER", nullable: false),
                    LvpFfGiven = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPlayers_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatisticsFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FileDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Round = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamSize = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MapName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TeamAScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamBScore = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MatchId = table.Column<int>(type: "INTEGER", nullable: true),
                    MatchOrder = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticsFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatisticsFiles_GameServers_GameServerId",
                        column: x => x.GameServerId,
                        principalTable: "GameServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StatisticsFiles_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StatisticsFiles_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMaps_CampaignId",
                table: "CampaignMaps",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMaps_MapName",
                table: "CampaignMaps",
                column: "MapName");

            migrationBuilder.CreateIndex(
                name: "IX_GameConfigurations_Name_TeamSize",
                table: "GameConfigurations",
                columns: new[] { "Name", "TeamSize" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameServers_PublicId",
                table: "GameServers",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_GameServerId",
                table: "Matches",
                column: "GameServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LastFileDate",
                table: "Matches",
                column: "LastFileDate");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SeasonId_LastFileDate",
                table: "Matches",
                columns: new[] { "SeasonId", "LastFileDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_CommunityId",
                table: "MatchPlayers",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_MatchId",
                table: "MatchPlayers",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Punishments_SeasonId_CommunityId",
                table: "Punishments",
                columns: new[] { "SeasonId", "CommunityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_StartsAt",
                table: "Seasons",
                column: "StartsAt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsFiles_GameServerId_FileName",
                table: "StatisticsFiles",
                columns: new[] { "GameServerId", "FileName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsFiles_GameServerId_SeasonId_FileDate",
                table: "StatisticsFiles",
                columns: new[] { "GameServerId", "SeasonId", "FileDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsFiles_MatchId",
                table: "StatisticsFiles",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsFiles_SeasonId",
                table: "StatisticsFiles",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrators");

            migrationBuilder.DropTable(
                name: "CampaignMaps");

            migrationBuilder.DropTable(
                name: "GameConfigurations");

            migrationBuilder.DropTable(
                name: "GameModeRules");

            migrationBuilder.DropTable(
                name: "MatchPlayers");

            migrationBuilder.DropTable(
                name: "PlayerProfiles");

            migrationBuilder.DropTable(
                name: "Punishments");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "StatisticsFiles");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "GameServers");

            migrationBuilder.DropTable(
                name: "Seasons");
        }
    }
}
