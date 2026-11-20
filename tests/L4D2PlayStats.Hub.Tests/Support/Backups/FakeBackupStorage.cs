using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;

namespace L4D2PlayStats.Hub.Tests.Support.Backups;

public class FakeBackupStorage : IBackupStorage
{
    public ConcurrentDictionary<string, byte[]> Blobs { get; } = new();
    public ConcurrentDictionary<string, string> ContentTypes { get; } = new();
    public List<string> Deleted { get; } = [];
    public string? FailUploadWith { get; set; }
    public string? FailListWith { get; set; }
    public bool HideFromList { get; set; }
    public int Prepared { get; private set; }
    public Func<Task>? DuringUpload { get; set; }
    public string? FailDownloadWith { get; set; }
    public Func<byte[], byte[]>? ChangeDownload { get; set; }
    public List<string> DownloadedTo { get; } = [];

    public Task PrepareAsync(CancellationToken cancellationToken)
    {
        Prepared++;

        return Task.CompletedTask;
    }

    public async Task UploadAsync(string name, Stream content, string contentType, CancellationToken cancellationToken)
    {
        if (FailUploadWith != null)
            throw new InvalidOperationException(FailUploadWith);

        if (DuringUpload != null)
            await DuringUpload();

        using var memory = new MemoryStream();

        await content.CopyToAsync(memory, cancellationToken);

        Blobs[name] = memory.ToArray();
        ContentTypes[name] = contentType;
    }

    public Task<List<string>> ListAsync(string prefix, CancellationToken cancellationToken)
    {
        if (FailListWith != null)
            throw new InvalidOperationException(FailListWith);

        var names = HideFromList ? [] : Blobs.Keys.Where(name => name.StartsWith(prefix, StringComparison.Ordinal)).ToList();

        return Task.FromResult(names);
    }

    public async Task DownloadAsync(string name, Stream destination, CancellationToken cancellationToken)
    {
        if (FailDownloadWith != null)
            throw new InvalidOperationException(FailDownloadWith);

        if (destination is FileStream file)
            DownloadedTo.Add(file.Name);

        var content = Blobs[name];
        var served = ChangeDownload == null ? content : ChangeDownload(content);

        await destination.WriteAsync(served, cancellationToken);
    }

    public Task DeleteAsync(string name, CancellationToken cancellationToken)
    {
        Blobs.TryRemove(name, out _);

        lock (Deleted)
        {
            Deleted.Add(name);
        }

        return Task.CompletedTask;
    }
}