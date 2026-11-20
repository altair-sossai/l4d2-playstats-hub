namespace L4D2PlayStats.Hub.Web.Models;

public class PageMetadata
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? Image { get; init; }
    public string? ImageAlt { get; init; }
    public string OpenGraphType { get; init; } = AppConsts.DefaultOpenGraphType;
    public bool NoIndex { get; init; }
}