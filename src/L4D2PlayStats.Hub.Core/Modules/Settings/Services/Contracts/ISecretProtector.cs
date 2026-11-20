namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

public interface ISecretProtector
{
    bool IsAvailable { get; }
    string Open(Type type, string json);
    string Seal(Type type, string json, string? stored);
    string? EncryptPlain(Type type, string stored);
}