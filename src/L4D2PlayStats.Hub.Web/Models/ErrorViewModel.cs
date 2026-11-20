namespace L4D2PlayStats.Hub.Web.Models;

public class ErrorViewModel(int statusCode)
{
    public int StatusCode { get; } = statusCode;
    public bool NotFound => StatusCode == StatusCodes.Status404NotFound;
    public bool Forbidden => StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden;
    public bool ServerError => StatusCode >= StatusCodes.Status500InternalServerError;
}