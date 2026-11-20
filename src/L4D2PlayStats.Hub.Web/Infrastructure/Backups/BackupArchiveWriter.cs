using System.IO.Compression;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Secrets;
using L4D2PlayStats.Hub.Web.Models.Backups;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public static class BackupArchiveWriter
{
    private static readonly JsonSerializerOptions ManifestOptions = new() { WriteIndented = true };

    public static BackupManifest Write(string zipPath, string? databaseSnapshot, string dataDirectory, BackupManifest manifest, SecretsKey? secretsKey = null)
    {
        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);

        if (databaseSnapshot != null)
            Add(zip, databaseSnapshot, AppConsts.BackupDatabaseEntry, manifest);

        foreach (var folder in new[] { AppConsts.KeysFolder, AppConsts.BrandingFolder })
            AddFolder(zip, dataDirectory, folder, manifest, secretsKey);

        manifest.IncludesDatabase = databaseSnapshot != null;

        var entry = zip.CreateEntry(AppConsts.BackupManifestEntry, CompressionLevel.Optimal);

        using (var stream = entry.Open())
        {
            JsonSerializer.Serialize(stream, manifest, ManifestOptions);
        }

        return manifest;
    }

    private static void AddFolder(ZipArchive zip, string dataDirectory, string folder, BackupManifest manifest, SecretsKey? secretsKey)
    {
        var root = Path.Combine(dataDirectory, folder);

        if (!Directory.Exists(root))
            return;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            if (secretsKey != null && SecretsKeyLoader.IsKeyFile(file, secretsKey))
                continue;

            var relative = Path.GetRelativePath(dataDirectory, file).Replace(Path.DirectorySeparatorChar, AppConsts.BackupEntrySeparator);

            Add(zip, file, relative, manifest);
        }
    }

    private static void Add(ZipArchive zip, string file, string entryName, BackupManifest manifest)
    {
        zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
        manifest.Files.Add(entryName);
    }
}
