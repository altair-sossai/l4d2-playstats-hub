using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public class AzureBlobBackupStorage(BlobContainerClient container, bool createContainer) : IBackupStorage
{
    public async Task PrepareAsync(CancellationToken cancellationToken)
    {
        if (createContainer)
            await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task UploadAsync(string name, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var options = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };

        await container.GetBlobClient(name).UploadAsync(content, options, cancellationToken);
    }

    public async Task<List<string>> ListAsync(string prefix, CancellationToken cancellationToken)
    {
        var names = new List<string>();

        await foreach (var blob in container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
            names.Add(blob.Name);

        return names;
    }

    public async Task DownloadAsync(string name, Stream destination, CancellationToken cancellationToken)
    {
        await container.GetBlobClient(name).DownloadToAsync(destination, cancellationToken);
    }

    public async Task DeleteAsync(string name, CancellationToken cancellationToken)
    {
        await container.GetBlobClient(name).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
