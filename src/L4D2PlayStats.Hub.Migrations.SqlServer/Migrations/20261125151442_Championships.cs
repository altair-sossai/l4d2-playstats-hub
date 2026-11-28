using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4D2PlayStats.Hub.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Championships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Championships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Rules = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: true),
                    RulesPortuguese = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: true),
                    RulesSpanish = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StreamUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VideosUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GameConfiguration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlayersPerGame = table.Column<int>(type: "int", nullable: false),
                    RosterSize = table.Column<int>(type: "int", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: true),
                    Substitutes = table.Column<bool>(type: "bit", nullable: false),
                    DraftMethod = table.Column<int>(type: "int", nullable: false),
                    StartsOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndsOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Championships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Values = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipLogs_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    Pot = table.Column<int>(type: "int", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipParticipants_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BestOf = table.Column<int>(type: "int", nullable: false),
                    FinalBestOf = table.Column<int>(type: "int", nullable: false),
                    Legs = table.Column<int>(type: "int", nullable: false),
                    GroupCount = table.Column<int>(type: "int", nullable: false),
                    PointsForWin = table.Column<int>(type: "int", nullable: false),
                    PointsForDraw = table.Column<int>(type: "int", nullable: false),
                    PointsForLoss = table.Column<int>(type: "int", nullable: false),
                    Tiebreakers = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Qualifiers = table.Column<int>(type: "int", nullable: false),
                    Seeding = table.Column<int>(type: "int", nullable: false),
                    ThirdPlace = table.Column<bool>(type: "bit", nullable: false),
                    CampaignRule = table.Column<int>(type: "int", nullable: false),
                    Veto = table.Column<bool>(type: "bit", nullable: false),
                    OncePerTeam = table.Column<bool>(type: "bit", nullable: false),
                    MinimumMaps = table.Column<int>(type: "int", nullable: false),
                    Generated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipStages_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Seed = table.Column<int>(type: "int", nullable: false),
                    DraftPosition = table.Column<int>(type: "int", nullable: true),
                    CaptainCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    ViceCaptainCommunityId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipTeams_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipGroups_ChampionshipStages_StageId",
                        column: x => x.StageId,
                        principalTable: "ChampionshipStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    Round = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    TeamAId = table.Column<int>(type: "int", nullable: true),
                    TeamBId = table.Column<int>(type: "int", nullable: true),
                    BestOf = table.Column<int>(type: "int", nullable: false),
                    NextSeriesId = table.Column<int>(type: "int", nullable: true),
                    NextSlotA = table.Column<bool>(type: "bit", nullable: false),
                    LoserNextSeriesId = table.Column<int>(type: "int", nullable: true),
                    LoserNextSlotA = table.Column<bool>(type: "bit", nullable: false),
                    ThirdPlace = table.Column<bool>(type: "bit", nullable: false),
                    Bye = table.Column<bool>(type: "bit", nullable: false),
                    Finished = table.Column<bool>(type: "bit", nullable: false),
                    WinnerTeamId = table.Column<int>(type: "int", nullable: true),
                    Walkover = table.Column<bool>(type: "bit", nullable: false),
                    DeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeamAVetoCampaignId = table.Column<int>(type: "int", nullable: true),
                    TeamBVetoCampaignId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipSeries_ChampionshipStages_StageId",
                        column: x => x.StageId,
                        principalTable: "ChampionshipStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipDraftPicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChampionshipId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    PickedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipDraftPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipDraftPicks_ChampionshipTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "ChampionshipTeams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChampionshipDraftPicks_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipPenalties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedByCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipPenalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipPenalties_ChampionshipStages_StageId",
                        column: x => x.StageId,
                        principalTable: "ChampionshipStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChampionshipPenalties_ChampionshipTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "ChampionshipTeams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipTeamMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CommunityId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipTeamMembers_ChampionshipTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "ChampionshipTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipGroupTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Seed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipGroupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipGroupTeams_ChampionshipGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ChampionshipGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChampionshipGroupTeams_ChampionshipTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "ChampionshipTeams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipDateProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesId = table.Column<int>(type: "int", nullable: false),
                    DateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    ProposedByCommunityId = table.Column<long>(type: "bigint", nullable: true),
                    TeamAAnswer = table.Column<int>(type: "int", nullable: false),
                    TeamBAnswer = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipDateProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipDateProposals_ChampionshipSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "ChampionshipSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionshipGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    CampaignSource = table.Column<int>(type: "int", nullable: true),
                    PickedByTeamId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResultSource = table.Column<int>(type: "int", nullable: true),
                    MatchId = table.Column<int>(type: "int", nullable: true),
                    MatchTeamAIsTeamA = table.Column<bool>(type: "bit", nullable: false),
                    TeamAScore = table.Column<int>(type: "int", nullable: false),
                    TeamBScore = table.Column<int>(type: "int", nullable: false),
                    TeamAPenalty = table.Column<int>(type: "int", nullable: false),
                    TeamBPenalty = table.Column<int>(type: "int", nullable: false),
                    PlayedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Replay = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionshipGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionshipGames_ChampionshipSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "ChampionshipSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChampionshipGames_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipDateProposals_SeriesId",
                table: "ChampionshipDateProposals",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipDraftPicks_ChampionshipId_Number",
                table: "ChampionshipDraftPicks",
                columns: new[] { "ChampionshipId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipDraftPicks_TeamId",
                table: "ChampionshipDraftPicks",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipGames_MatchId",
                table: "ChampionshipGames",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipGames_SeriesId_Number",
                table: "ChampionshipGames",
                columns: new[] { "SeriesId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipGroups_StageId",
                table: "ChampionshipGroups",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipGroupTeams_GroupId_TeamId",
                table: "ChampionshipGroupTeams",
                columns: new[] { "GroupId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipGroupTeams_TeamId",
                table: "ChampionshipGroupTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipLogs_ChampionshipId_CreatedAtUtc",
                table: "ChampionshipLogs",
                columns: new[] { "ChampionshipId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipParticipants_ChampionshipId_CommunityId",
                table: "ChampionshipParticipants",
                columns: new[] { "ChampionshipId", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipParticipants_CommunityId",
                table: "ChampionshipParticipants",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipPenalties_StageId",
                table: "ChampionshipPenalties",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipPenalties_TeamId",
                table: "ChampionshipPenalties",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Championships_Slug",
                table: "Championships",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipSeries_StageId_Round_Position",
                table: "ChampionshipSeries",
                columns: new[] { "StageId", "Round", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipSeries_TeamAId",
                table: "ChampionshipSeries",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipSeries_TeamBId",
                table: "ChampionshipSeries",
                column: "TeamBId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipStages_ChampionshipId_Order",
                table: "ChampionshipStages",
                columns: new[] { "ChampionshipId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipTeamMembers_CommunityId",
                table: "ChampionshipTeamMembers",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipTeamMembers_TeamId_CommunityId",
                table: "ChampionshipTeamMembers",
                columns: new[] { "TeamId", "CommunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampionshipTeams_ChampionshipId",
                table: "ChampionshipTeams",
                column: "ChampionshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChampionshipDateProposals");

            migrationBuilder.DropTable(
                name: "ChampionshipDraftPicks");

            migrationBuilder.DropTable(
                name: "ChampionshipGames");

            migrationBuilder.DropTable(
                name: "ChampionshipGroupTeams");

            migrationBuilder.DropTable(
                name: "ChampionshipLogs");

            migrationBuilder.DropTable(
                name: "ChampionshipParticipants");

            migrationBuilder.DropTable(
                name: "ChampionshipPenalties");

            migrationBuilder.DropTable(
                name: "ChampionshipTeamMembers");

            migrationBuilder.DropTable(
                name: "ChampionshipSeries");

            migrationBuilder.DropTable(
                name: "ChampionshipGroups");

            migrationBuilder.DropTable(
                name: "ChampionshipTeams");

            migrationBuilder.DropTable(
                name: "ChampionshipStages");

            migrationBuilder.DropTable(
                name: "Championships");
        }
    }
}
