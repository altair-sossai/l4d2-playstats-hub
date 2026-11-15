using System;
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
                name: "ChatBans",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedByName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatBans", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityOffer",
                columns: table => new
                {
                    VoterId = table.Column<long>(type: "INTEGER", nullable: false),
                    LeftPlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    RightPlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    OfferedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityOffer", x => x.VoterId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityRating",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    Comparisons = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    Draws = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityRating", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityVote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoterId = table.Column<long>(type: "INTEGER", nullable: false),
                    LeftPlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    RightPlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SiteDay = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityVote", x => x.Id);
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
                name: "PlayerConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Count = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerConnections", x => x.Id);
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
                name: "RemoteCommandLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameServerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AdministratorCommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    AdministratorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Command = table.Column<string>(type: "TEXT", maxLength: 259, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeliveredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteCommandLogs", x => x.Id);
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
                name: "Skin",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Weapon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultPrice = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkshopId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    AuthorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AuthorCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    OriginalTitle = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    OriginalDescription = table.Column<string>(type: "TEXT", nullable: true),
                    Shots = table.Column<int>(type: "INTEGER", nullable: false),
                    Available = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skin", x => x.Id);
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
                name: "SkinEquipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Weapon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SkinId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkinEquipment_Skin_SkinId",
                        column: x => x.SkinId,
                        principalTable: "Skin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkinPriceOverride",
                columns: table => new
                {
                    SkinId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinPriceOverride", x => x.SkinId);
                    table.ForeignKey(
                        name: "FK_SkinPriceOverride_Skin_SkinId",
                        column: x => x.SkinId,
                        principalTable: "Skin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkinPurchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    SkinId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false),
                    StarsBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    Gift = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GiftedByCommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    GiftedByName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PurchasedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinPurchase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkinPurchase_Skin_SkinId",
                        column: x => x.SkinId,
                        principalTable: "Skin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkinText",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SkinId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkinText_Skin_SkinId",
                        column: x => x.SkinId,
                        principalTable: "Skin",
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
                name: "IX_CommunityRating_Rating",
                table: "CommunityRating",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityVote_VoterId_SiteDay",
                table: "CommunityVote",
                columns: new[] { "VoterId", "SiteDay" });

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
                name: "IX_PlayerConnections_CommunityId",
                table: "PlayerConnections",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerConnections_IpAddress_CommunityId",
                table: "PlayerConnections",
                columns: new[] { "IpAddress", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerConnections_LastSeenAtUtc",
                table: "PlayerConnections",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Punishments_SeasonId_CommunityId",
                table: "Punishments",
                columns: new[] { "SeasonId", "CommunityId" });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteCommandLogs_CreatedAtUtc",
                table: "RemoteCommandLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_StartsAt",
                table: "Seasons",
                column: "StartsAt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skin_Weapon",
                table: "Skin",
                column: "Weapon");

            migrationBuilder.CreateIndex(
                name: "IX_SkinEquipment_CommunityId_Weapon",
                table: "SkinEquipment",
                columns: new[] { "CommunityId", "Weapon" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkinEquipment_SkinId",
                table: "SkinEquipment",
                column: "SkinId");

            migrationBuilder.CreateIndex(
                name: "IX_SkinPurchase_CommunityId_SkinId",
                table: "SkinPurchase",
                columns: new[] { "CommunityId", "SkinId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkinPurchase_PurchasedAtUtc",
                table: "SkinPurchase",
                column: "PurchasedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SkinPurchase_SkinId",
                table: "SkinPurchase",
                column: "SkinId");

            migrationBuilder.CreateIndex(
                name: "IX_SkinText_SkinId_Language",
                table: "SkinText",
                columns: new[] { "SkinId", "Language" },
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
                name: "ChatBans");

            migrationBuilder.DropTable(
                name: "CommunityOffer");

            migrationBuilder.DropTable(
                name: "CommunityRating");

            migrationBuilder.DropTable(
                name: "CommunityVote");

            migrationBuilder.DropTable(
                name: "GameConfigurations");

            migrationBuilder.DropTable(
                name: "GameModeRules");

            migrationBuilder.DropTable(
                name: "MatchPlayers");

            migrationBuilder.DropTable(
                name: "PlayerConnections");

            migrationBuilder.DropTable(
                name: "PlayerProfiles");

            migrationBuilder.DropTable(
                name: "Punishments");

            migrationBuilder.DropTable(
                name: "RemoteCommandLogs");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "SkinEquipment");

            migrationBuilder.DropTable(
                name: "SkinPriceOverride");

            migrationBuilder.DropTable(
                name: "SkinPurchase");

            migrationBuilder.DropTable(
                name: "SkinText");

            migrationBuilder.DropTable(
                name: "StatisticsFiles");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Skin");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "GameServers");

            migrationBuilder.DropTable(
                name: "Seasons");
        }
    }
}
