using System.Text.Json;
using System.Text.Json.Nodes;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services;

public class SecretProtector(SecretsKey key, ILogger<SecretProtector> logger) : ISecretProtector
{
    public bool IsAvailable => key.IsAvailable;

    public string Open(Type type, string json)
    {
        var names = SecretProperties.Of(type);

        if (names.Count == 0 || Parse(json) is not { } node)
            return json;

        var changed = false;

        foreach (var name in names)
        {
            var value = Text(node, name);

            if (!SecretCipher.IsEncrypted(value))
                continue;

            node[name] = Decrypt(type, name, value!);
            changed = true;
        }

        return changed ? node.ToJsonString() : json;
    }

    public string Seal(Type type, string json, string? stored)
    {
        var names = SecretProperties.Of(type);

        if (names.Count == 0 || Parse(json) is not { } node)
            return json;

        var previous = stored == null ? null : Parse(stored);

        foreach (var name in names)
            node[name] = Seal(type, name, Text(node, name), previous == null ? null : Text(previous, name));

        return node.ToJsonString();
    }

    public string? EncryptPlain(Type type, string stored)
    {
        var names = SecretProperties.Of(type);

        if (key.Value == null || names.Count == 0 || Parse(stored) is not { } node)
            return null;

        var changed = false;

        foreach (var name in names)
        {
            var value = Text(node, name);

            if (string.IsNullOrEmpty(value) || SecretCipher.IsEncrypted(value))
                continue;

            node[name] = SecretCipher.Encrypt(key.Value, value);
            changed = true;
        }

        return changed ? node.ToJsonString() : null;
    }

    private string? Decrypt(Type type, string name, string value)
    {
        if (key.Value != null && SecretCipher.TryDecrypt(key.Value, value, out var plain))
            return plain;

        logger.LogWarning("The secret {Property} of the {Setting} settings could not be decrypted with the secrets key from {Location}, so it is treated as empty. Put back the secrets key used when it was saved, or type the secret again.", name, type.Name, key.Location);

        return null;
    }

    private string? Seal(Type type, string name, string? plain, string? previous)
    {
        if (string.IsNullOrEmpty(plain))
            return SecretCipher.IsEncrypted(previous) && !CanDecrypt(previous!) ? previous : plain;

        if (key.Value != null)
            return SecretCipher.Encrypt(key.Value, plain);

        logger.LogError("The secret {Property} of the {Setting} settings was not saved because the secrets key from {Location} is not available. The value stored before was kept.", name, type.Name, key.Location);

        return previous;
    }

    private bool CanDecrypt(string value)
    {
        return key.Value != null && SecretCipher.TryDecrypt(key.Value, value, out _);
    }

    private static string? Text(JsonObject node, string name)
    {
        return node[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
    }

    private static JsonObject? Parse(string json)
    {
        try
        {
            return JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
