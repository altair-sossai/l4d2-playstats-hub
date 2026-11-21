namespace L4D2PlayStats.Hub.Web.Models.Commands;

public record CommandExample
{
    public string Code { get; init; } = string.Empty;
    public string? Text { get; init; }
}
