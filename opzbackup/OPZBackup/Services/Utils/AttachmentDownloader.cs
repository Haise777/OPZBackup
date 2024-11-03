using System.Text.RegularExpressions;
using Discord;
using Microsoft.Extensions.Logging;

namespace OPZBackup.Services.Utils;

public class AttachmentDownloader
{
    private readonly HttpClient _client;

    public AttachmentDownloader(HttpClient client)
    {
        _client = client;
        if (!Directory.Exists(AppInfo.FileBackupPath))
            Directory.CreateDirectory(AppInfo.FileBackupPath);
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
            var path = $"{AppInfo.FileBackupPath}/{channelId}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        });
    }
}