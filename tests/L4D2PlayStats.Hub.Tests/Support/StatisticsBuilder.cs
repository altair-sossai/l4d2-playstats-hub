using System.Reflection;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Tests.Support;

public class StatisticsBuilder
{
    public const long PlayerA1 = 76561197990571514;
    public const string SampleFileName = "2023-02-04_03-13_0011_c8m1_apartment.txt";

    private StatisticsBuilder(string content)
    {
        Statistics = new ParsedStatistics(content);
    }

    public ParsedStatistics Statistics { get; }

    public string FileName => $"{Statistics.GameRound!.When:yyyy-MM-dd_HH-mm}_{Statistics.GameRound.Round:0000}_{Statistics.GameRound.MapName}.txt";

    public string Content => Statistics.ToString();

    public static string SampleContent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames().First(name => name.EndsWith(SampleFileName, StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static StatisticsBuilder Sample()
    {
        return new StatisticsBuilder(SampleContent());
    }

    public static StatisticsBuilder Map(string mapName, DateTime when, int round, int teamAScore, int teamBScore)
    {
        return Sample().WithMap(mapName).At(when).WithRound(round).WithScores(teamAScore, teamBScore);
    }

    public StatisticsBuilder WithMap(string mapName)
    {
        Statistics.GameRound!.MapName = mapName;

        return this;
    }

    public StatisticsBuilder At(DateTime when)
    {
        Statistics.GameRound!.When = when;

        return this;
    }

    public StatisticsBuilder WithRound(int round)
    {
        Statistics.GameRound!.Round = round;

        return this;
    }

    public StatisticsBuilder WithConfiguration(string configurationName, int teamSize = 4)
    {
        Statistics.GameRound!.ConfigurationName = configurationName;
        Statistics.GameRound.TeamSize = teamSize;

        return this;
    }

    public StatisticsBuilder WithScores(int teamAScore, int teamBScore)
    {
        Statistics.Scoring!.TeamA!.Score = teamAScore;
        Statistics.Scoring.TeamB!.Score = teamBScore;

        return this;
    }

    public StatisticsBuilder WithName(long communityId, string name)
    {
        var id = communityId.ToString();

        foreach (var playerName in Statistics.PlayerNames.Where(p => p.CommunityId == id))
            playerName.Name = name;

        return this;
    }

    public StatisticsBuilder WithoutPlayer(long communityId)
    {
        var id = communityId.ToString();

        foreach (var half in Statistics.Halves)
        {
            half.Players.RemoveAll(p => p.CommunityId == id);
            half.InfectedPlayers.RemoveAll(p => p.CommunityId == id);
        }

        return this;
    }

    public List<long> Survivors(char team)
    {
        var half = Statistics.Halves.First(h => h.RoundHalf!.Team == team);

        return [.. half.Players.Select(p => long.Parse(p.CommunityId!))];
    }
}
