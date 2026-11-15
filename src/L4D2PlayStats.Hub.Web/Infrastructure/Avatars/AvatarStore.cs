namespace L4D2PlayStats.Hub.Web.Infrastructure.Avatars;

public class AvatarStore(HubPaths paths)
{
    private readonly string _directory = paths.Avatars;

    public string PathFor(long communityId)
    {
        return Path.Combine(_directory, $"{communityId}.jpg");
    }

    public async Task SaveAsync(long communityId, byte[] image, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directory);

        await File.WriteAllBytesAsync(PathFor(communityId), image, cancellationToken);
    }
}
