using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipBracket
{
    public static int Size(int teams)
    {
        var size = 2;

        while (size < teams)
            size *= 2;

        return size;
    }

    public static int Rounds(int teams)
    {
        return (int)Math.Round(Math.Log2(Size(teams)));
    }

    public static List<int> SeedOrder(int size)
    {
        var order = new List<int> { 1, 2 };

        while (order.Count < size)
        {
            var next = order.Count * 2;

            order = order.SelectMany(seed => new[] { seed, next + 1 - seed }).ToList();
        }

        return order;
    }

    public static List<BracketSlot> FirstRound(IReadOnlyList<int> seededTeams)
    {
        var size = Size(seededTeams.Count);
        var order = SeedOrder(size);
        var slots = new List<BracketSlot>();

        for (var position = 0; position < size / 2; position++)
        {
            var seedA = order[position * 2];
            var seedB = order[position * 2 + 1];

            slots.Add(new BracketSlot(position + 1, Team(seededTeams, seedA), Team(seededTeams, seedB)));
        }

        return slots;
    }

    public static string RoundKey(int round, int rounds)
    {
        return (rounds - round) switch
        {
            0 => "Final",
            1 => "Semifinals",
            2 => "Quarterfinals",
            3 => "Round of 16",
            _ => "Round {0}"
        };
    }

    private static int? Team(IReadOnlyList<int> seededTeams, int seed)
    {
        return seed <= seededTeams.Count ? seededTeams[seed - 1] : null;
    }
}
