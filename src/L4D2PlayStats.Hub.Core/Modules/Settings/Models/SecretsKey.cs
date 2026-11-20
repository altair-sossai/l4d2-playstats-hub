namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public sealed class SecretsKey(byte[]? value, string location, string? file, bool created, string? error)
{
    public byte[]? Value { get; } = value;
    public string Location { get; } = location;
    public string? File { get; } = file;
    public bool Created { get; } = created;
    public string? Error { get; } = error;

    public bool IsAvailable => Value != null;
}
