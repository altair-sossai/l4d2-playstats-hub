using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.PostgreSql.Migrations
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
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrators", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatBans",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedByCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatBans", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityOffer",
                columns: table => new
                {
                    VoterId = table.Column<long>(type: "bigint", nullable: false),
                    LeftPlayerId = table.Column<long>(type: "bigint", nullable: false),
                    RightPlayerId = table.Column<long>(type: "bigint", nullable: false),
                    OfferedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityOffer", x => x.VoterId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityRating",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    Comparisons = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityRating", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityVote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoterId = table.Column<long>(type: "bigint", nullable: false),
                    LeftPlayerId = table.Column<long>(type: "bigint", nullable: false),
                    RightPlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SiteDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityVote", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameModeRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConfigurationPattern = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: true),
                    MinimumMaps = table.Column<int>(type: "integer", nullable: false),
                    CountsForRanking = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameModeRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KeyHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerConnections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                columns: table => new
                {
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AvatarUpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.CommunityId);
                });

            migrationBuilder.CreateTable(
                name: "RemoteCommandLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameServerId = table.Column<int>(type: "integer", nullable: false),
                    GameServerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AdministratorCommunityId = table.Column<long>(type: "bigint", nullable: false),
                    AdministratorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Command = table.Column<string>(type: "character varying(259)", maxLength: 259, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeliveredAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemoteCommandLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Skin",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Weapon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    DefaultPrice = table.Column<int>(type: "integer", nullable: false),
                    WorkshopId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AuthorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AuthorCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    OriginalTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    OriginalDescription = table.Column<string>(type: "text", nullable: true),
                    Shots = table.Column<int>(type: "integer", nullable: false),
                    Available = table.Column<bool>(type: "boolean", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    MapName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameServerId = table.Column<int>(type: "integer", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfigurationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: false),
                    FirstFileDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastFileDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TeamAScore = table.Column<int>(type: "integer", nullable: false),
                    TeamBScore = table.Column<int>(type: "integer", nullable: false),
                    MapCount = table.Column<int>(type: "integer", nullable: false),
                    CountsForRanking = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedByCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Weapon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SkinId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    SkinId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    SkinId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    StarsBefore = table.Column<int>(type: "integer", nullable: false),
                    Gift = table.Column<bool>(type: "boolean", nullable: false),
                    PlayerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GiftedByCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    GiftedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchasedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkinId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    TeamA = table.Column<bool>(type: "boolean", nullable: false),
                    TeamB = table.Column<bool>(type: "boolean", nullable: false),
                    Win = table.Column<bool>(type: "boolean", nullable: false),
                    Loss = table.Column<bool>(type: "boolean", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false),
                    RageQuits = table.Column<int>(type: "integer", nullable: false),
                    Participant = table.Column<bool>(type: "boolean", nullable: false),
                    Died = table.Column<int>(type: "integer", nullable: false),
                    Incaps = table.Column<int>(type: "integer", nullable: false),
                    DmgTaken = table.Column<int>(type: "integer", nullable: false),
                    Common = table.Column<int>(type: "integer", nullable: false),
                    SiKilled = table.Column<int>(type: "integer", nullable: false),
                    SiDamage = table.Column<int>(type: "integer", nullable: false),
                    TankDamage = table.Column<int>(type: "integer", nullable: false),
                    RockEats = table.Column<int>(type: "integer", nullable: false),
                    WitchDamage = table.Column<int>(type: "integer", nullable: false),
                    Skeets = table.Column<int>(type: "integer", nullable: false),
                    SkeetsMelee = table.Column<int>(type: "integer", nullable: false),
                    Levels = table.Column<int>(type: "integer", nullable: false),
                    Crowns = table.Column<int>(type: "integer", nullable: false),
                    FfGiven = table.Column<int>(type: "integer", nullable: false),
                    DmgTotal = table.Column<int>(type: "integer", nullable: false),
                    DmgTank = table.Column<int>(type: "integer", nullable: false),
                    DmgSpit = table.Column<int>(type: "integer", nullable: false),
                    HunterDpDmg = table.Column<int>(type: "integer", nullable: false),
                    MvpSiDamage = table.Column<int>(type: "integer", nullable: false),
                    MvpCommon = table.Column<int>(type: "integer", nullable: false),
                    LvpFfGiven = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameServerId = table.Column<int>(type: "integer", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MapName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeamAScore = table.Column<int>(type: "integer", nullable: false),
                    TeamBScore = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MatchId = table.Column<int>(type: "integer", nullable: true),
                    MatchOrder = table.Column<int>(type: "integer", nullable: true)
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
