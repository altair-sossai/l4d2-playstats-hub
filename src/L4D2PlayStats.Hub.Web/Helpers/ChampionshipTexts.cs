using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Web.Models.Championships;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ChampionshipTexts
{
    public static string Status(IStringLocalizer L, ChampionshipStatus status)
    {
        return status switch
        {
            ChampionshipStatus.Draft => L["ChampionshipStatus.Draft"],
            ChampionshipStatus.Participants => L["Participants"],
            ChampionshipStatus.TeamFormation => L["Team formation"],
            ChampionshipStatus.Running => L["In progress"],
            ChampionshipStatus.Finished => L["Finished"],
            _ => L["Cancelled"]
        };
    }

    public static string StatusLabel(ChampionshipStatus status)
    {
        return status switch
        {
            ChampionshipStatus.Draft => "label-default",
            ChampionshipStatus.Running => "label-success",
            ChampionshipStatus.Finished => "label-primary",
            ChampionshipStatus.Cancelled => "label-danger",
            _ => "label-info"
        };
    }

    public static string SuggestedTime(IStringLocalizer L, Championship championship)
    {
        if (championship.SuggestedMinute == null)
            return string.Empty;

        var time = ChampionshipDates.Time(championship.SuggestedMinute.Value).ToString("t", CultureInfo.CurrentCulture);

        return championship.SuggestedWeekday == null ? time : L["{0} at {1}", Weekday(championship.SuggestedWeekday.Value), time];
    }

    public static string Weekday(DayOfWeek weekday)
    {
        var name = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(weekday);

        return char.ToUpper(name[0], CultureInfo.CurrentUICulture) + name[1..];
    }

    public static string StageType(IStringLocalizer L, ChampionshipStageType type)
    {
        return type == ChampionshipStageType.Knockout ? L["Knockout"] : L["Round robin"];
    }

    public static string Seeding(IStringLocalizer L, ChampionshipSeeding seeding)
    {
        return seeding switch
        {
            ChampionshipSeeding.Seeds => L["By the seeds of the teams"],
            ChampionshipSeeding.Random => L["By draw"],
            _ => L["By the standings of the previous stage"]
        };
    }

    public static string CampaignRule(IStringLocalizer L, ChampionshipCampaignRule rule)
    {
        return rule switch
        {
            ChampionshipCampaignRule.RoundDraw => L["One campaign drawn for each round"],
            ChampionshipCampaignRule.Picks => L["Each team chooses one, the decider is drawn"],
            _ => L["The administration chooses"]
        };
    }

    public static string Tiebreaker(IStringLocalizer L, ChampionshipTiebreaker tiebreaker)
    {
        return tiebreaker switch
        {
            ChampionshipTiebreaker.Difference => L["Point difference"],
            ChampionshipTiebreaker.ScoreFor => L["Points scored"],
            ChampionshipTiebreaker.HeadToHead => L["Head-to-head"],
            _ => L["Wins"]
        };
    }

    public static string DraftMethod(IStringLocalizer L, ChampionshipDraftMethod method)
    {
        return method switch
        {
            ChampionshipDraftMethod.Roulette => L["Roulette"],
            ChampionshipDraftMethod.Pots => L["Pots"],
            ChampionshipDraftMethod.Snake => L["Snake draft"],
            ChampionshipDraftMethod.Linear => L["Linear draft"],
            _ => L["Manual"]
        };
    }

    public static string Template(IStringLocalizer L, ChampionshipTemplate template)
    {
        return template switch
        {
            ChampionshipTemplate.RoundRobin => L["Round robin"],
            ChampionshipTemplate.RoundRobinAndKnockout => L["Round robin + knockout"],
            ChampionshipTemplate.Knockout => L["Knockout"],
            _ => L["Groups + knockout"]
        };
    }

    public static string Round(IStringLocalizer L, ChampionshipStage stage, int round, int rounds)
    {
        if (!stage.IsKnockout)
            return L["Round {0}", round];

        return (rounds - round) switch
        {
            0 => L["Final"],
            1 => L["Semifinals"],
            2 => L["Quarterfinals"],
            3 => L["Round of 16"],
            _ => L["Round {0}", round]
        };
    }

    public static string SeriesStatus(IStringLocalizer L, ChampionshipSeriesStatus status)
    {
        return status switch
        {
            ChampionshipSeriesStatus.Waiting => L["Waiting for the teams"],
            ChampionshipSeriesStatus.Bye => L["Bye"],
            ChampionshipSeriesStatus.Pending => L["Not played yet"],
            ChampionshipSeriesStatus.InProgress => L["In progress"],
            _ => L["Finished"]
        };
    }

    public static string CampaignSource(IStringLocalizer L, ChampionshipCampaignSource? source, string? team)
    {
        return source switch
        {
            ChampionshipCampaignSource.Pick => L["Chosen by {0}", team ?? "?"],
            ChampionshipCampaignSource.Draw => L["Drawn"],
            ChampionshipCampaignSource.Admin => L["Set by the administration"],
            _ => string.Empty
        };
    }

    public static string CaptainTask(IStringLocalizer L, ChampionshipCaptainTask task, string? opponent)
    {
        return task.Kind switch
        {
            ChampionshipCaptainTaskKind.DraftTurn => L["It is the turn of your team in the draft"],
            ChampionshipCaptainTaskKind.IdentityRejected => L["The administration refused the new name, tag or flag"],
            ChampionshipCaptainTaskKind.Veto => opponent == null ? L["Veto a campaign"] : L["Veto a campaign against {0}", opponent],
            ChampionshipCaptainTaskKind.Pick => opponent == null ? L["Choose the campaign of game {0}", task.GameNumber ?? 1] : L["Choose the campaign of game {0} against {1}", task.GameNumber ?? 1, opponent],
            ChampionshipCaptainTaskKind.AnswerDate => opponent == null ? L["Answer a date proposal"] : L["Answer the date proposed by {0}", opponent],
            _ => opponent == null ? L["Propose a date"] : L["Propose a date to {0}", opponent]
        };
    }

    public static string StageState(IStringLocalizer L, ChampionshipStageState state)
    {
        return state switch
        {
            ChampionshipStageState.Finished => L["Finished"],
            ChampionshipStageState.Running => L["In progress"],
            ChampionshipStageState.Next => L["Up next"],
            _ => L["Later"]
        };
    }

    public static string StageStateLabel(ChampionshipStageState state)
    {
        return state switch
        {
            ChampionshipStageState.Finished => "label-primary",
            ChampionshipStageState.Running => "label-success",
            ChampionshipStageState.Next => "label-info",
            _ => "label-default"
        };
    }
}
