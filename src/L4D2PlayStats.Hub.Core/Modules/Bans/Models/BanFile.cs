namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public record BanFile(IReadOnlyList<ImportedBan> Bans, IReadOnlyList<string> Invalid);
