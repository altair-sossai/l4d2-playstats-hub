using System.Text.Json.Serialization;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class ClientVersionResponse
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("sizeBytes")]
    public long? SizeBytes { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; set; }
}
