using System.Text.Json.Serialization;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public abstract class FeedItem
{
    public DateTime When { get; set; }

    [JsonIgnore]
    public long Ticks => When.Ticks;

    [JsonIgnore]
    public long Sequence { get; set; }
}