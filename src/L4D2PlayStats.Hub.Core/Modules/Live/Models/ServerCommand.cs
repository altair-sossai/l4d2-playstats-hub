using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class ServerCommand
{
    private static readonly Regex Part = new(AppConsts.ServerCommandPattern, RegexOptions.Compiled);

    private ServerCommand(Match match)
    {
        Prefix = match.Groups[1].Value;
        Command = match.Groups[2].Value;
        Arguments = match.Groups[4].Success ? match.Groups[4].Value.Trim() : string.Empty;
    }

    public string Prefix { get; }
    public string Arguments { get; }
    public string Command { get; }
    public string FullCommand => $"{AppConsts.ServerCommandPrefix}{Command} {Arguments}".Trim();

    public override string ToString()
    {
        return FullCommand;
    }

    public static ServerCommand? Parse(string? message)
    {
        var text = message?.Trim();

        if (string.IsNullOrEmpty(text) || text.Length > AppConsts.ServerCommandMaxLength)
            return null;

        var match = Part.Match(text);

        return match.Success ? new ServerCommand(match) : null;
    }
}