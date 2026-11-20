namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;

public interface IBackupStorage
{
    Task PrepareAsync(CancellationToken cancellationToken);
    Task UploadAsync(string name, Stream content, string contentType, CancellationToken cancellationToken);
    Task<List<string>> ListAsync(string prefix, CancellationToken cancellationToken);
    Task DownloadAsync(string name, Stream destination, CancellationToken cancellationToken);
    Task DeleteAsync(string name, CancellationToken cancellationToken);
}