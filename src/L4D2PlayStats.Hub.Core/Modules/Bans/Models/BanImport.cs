namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public record BanImport(int Added, int Unchanged, IReadOnlyList<string> Invalid);
