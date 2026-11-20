namespace L4D2PlayStats.Hub.Web.Models;

public class ServerForm
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? AlternativeAddress { get; set; }
    public bool Enabled { get; set; } = true;
}
