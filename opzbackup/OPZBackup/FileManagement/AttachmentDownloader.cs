namespace OPZBackup.FileManagement;

public class AttachmentDownloader
{
    private readonly HttpClient _client;

    public AttachmentDownloader(HttpClient client)
    {
        _client = client;
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
        var filePath = $"{App.TempFilePath}/{downloadable.ChannelId}/";
        
        if (files.Count() == 1)
        {
            var file = files.First();
            await File.WriteAllBytesAsync(filePath + file.FileName, file.FileBytes);
            
            return;
        }

        foreach (var file in files)
            await File.WriteAllBytesAsync(filePath + file.FileName, file.FileBytes);
    }

    private Task CreateChannelDirIfNotExists(ulong channelId)
    {
        return Task.Run(() =>
        {
            var path = $"{App.TempFilePath}/{channelId}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        });
    }
}