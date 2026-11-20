using System.Text;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

public static class ChatBindBlacklist
{
    private static readonly HashSet<string> Normalized = [.. AppConsts.LiveChatBindPhrases.Select(Normalize).Where(phrase => phrase.Length > 0)];

    public static bool IsBind(string? message)
    {
        return !string.IsNullOrWhiteSpace(message) && Normalized.Contains(Normalize(message));
    }

    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        var pendingSpace = false;

        foreach (var character in value.ToLowerInvariant())
            if (char.IsLetterOrDigit(character))
            {
                if (pendingSpace && builder.Length > 0)
                    builder.Append(' ');

                builder.Append(character);
                pendingSpace = false;
            }
            else
                pendingSpace = true;

        return builder.ToString();
    }
}
