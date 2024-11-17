using OPZBackup.Data.Dto;
using Serilog;

namespace OPZBackup.FileManagement;

public class AttachmentDownloader
{
    private static readonly SemaphoreSlim
        _downloadLimiter = new(50, 50); //TODO-3 Make the value be configurable in the appsettings
    private readonly ILogger _logger;

    private readonly HttpClient _client;

    public AttachmentDownloader(HttpClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task DownloadRangeAsync(IEnumerable<Downloadable> toDownload)
    {
        var concurrentDownloads = new List<Task>();
        await CreateChannelDirIfNotExists(toDownload.First().ChannelId);

        foreach (var downloadable in toDownload)
            concurrentDownloads.Add(DownloadAndWriteFile(downloadable));
        
        try
        {
            await Task.WhenAll(concurrentDownloads);
        }
        catch (Exception ex)
        {
            //TODO log exception
            if (ex is AggregateException aggr)
            {
                //TODO log all aggregated exceptions
            }

            throw;
        }
    }

    private async Task DownloadAndWriteFile(Downloadable downloadable)
    {
        var files = await DownloadAttachments(downloadable);
        var channelPath = $"{App.TempPath}/{downloadable.ChannelId}/";

        if (files.Count() == 1)
        {
            var file = files.First();
            await File.WriteAllBytesAsync(channelPath + file.FullFileName, file.FileBytes);

            return;
        }

        foreach (var file in files)
            await File.WriteAllBytesAsync(channelPath + file.FullFileName, file.FileBytes);
    }

    private Task CreateChannelDirIfNotExists(ulong channelId)
    {
        return Task.Run(() =>
        {
            var path = $"{App.TempPath}/{channelId}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        });
    }

    private async Task<IEnumerable<DownloadedFile>> DownloadAttachments(Downloadable downloadable)
    {
        var downloadedFiles = new List<DownloadedFile>();

        foreach (var attachment in downloadable.Attachments)
        {
            await _downloadLimiter.WaitAsync();
            try
            {
                downloadedFiles.Add(await AttemptDownload(attachment));
            }
            finally
            {
                _downloadLimiter.Release();
            }
        }

        return downloadedFiles;
    }

    private async Task<DownloadedFile> AttemptDownload(OnlineFile onlineFile)
    {
        var attempts = 0;
        while (true)
            try
            {
                var fileBytes = await _client.GetByteArrayAsync(onlineFile.Url);
                return new DownloadedFile(
                    fileBytes,
                    onlineFile.FileName,
                    onlineFile.FileExtension
                );
            }
            catch (HttpRequestException ex) //TODO-3 Deal with httpclient exceptions
            {
                //Log.
                if (++attempts >= 3) throw;
                await Task.Delay(5000);
            }
    }
}