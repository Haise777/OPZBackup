namespace OPZBackup.FileManagement;

public class AttachmentDownloader
{
    private readonly HttpClient _client;

    public AttachmentDownloader(HttpClient client)
    {
        _client = client;
        if (!Directory.Exists(App.FileBackupPath))
            Directory.CreateDirectory(App.FileBackupPath);
    }

    public async Task DownloadAsync(IEnumerable<Downloadable> toDownload)
    {
        var concurrentDownloads = new List<Task>();
        await CreateChannelDirIfNotExists(toDownload.First().ChannelId);

        foreach (var downloadable in toDownload)
            concurrentDownloads.Add(DownloadAndWriteFile(downloadable));

        await Task.WhenAll(concurrentDownloads);
    }

    private async Task DownloadAndWriteFile(Downloadable downloadable)
    {
        var files = await downloadable.DownloadAsync(_client);
        foreach (var file in files)
            await File.WriteAllBytesAsync(file.FullFilePath, file.FileBytes);
    }

    private Task CreateChannelDirIfNotExists(ulong channelId)
    {
        return Task.Run(() =>
        {
            var path = $"{App.FileBackupPath}/{channelId}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        });
    }
}