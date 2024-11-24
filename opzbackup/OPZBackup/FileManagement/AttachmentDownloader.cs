using OPZBackup.Data.Dto;
using Serilog;

namespace OPZBackup.FileManagement;

public class AttachmentDownloader
{
    private static readonly SemaphoreSlim
        _downloadLimiter = new(50, 50); //TODO-3 Make the value be configurable in the appsettings

    private readonly HttpClient _client;

    private readonly ILogger _logger;

    public AttachmentDownloader(HttpClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task DownloadRangeAsync(IEnumerable<Downloadable> toDownload, CancellationToken cancellationToken)
    {
        var concurrentDownloads = new List<Task>();
        await CreateChannelDirIfNotExists(toDownload.First().ChannelId);

        foreach (var downloadable in toDownload)
            concurrentDownloads.Add(DownloadAndWriteFile(downloadable, cancellationToken));

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

    private async Task DownloadAndWriteFile(Downloadable downloadable, CancellationToken cancellationToken)
    {
        var files = await DownloadAttachments(downloadable, cancellationToken);
        var channelPath = $"{App.TempPath}/{downloadable.ChannelId}/";

        cancellationToken.ThrowIfCancellationRequested();

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

    private async Task<IEnumerable<DownloadedFile>> DownloadAttachments(Downloadable downloadable,
        CancellationToken cancellationToken)
    {
        var downloadedFiles = new List<DownloadedFile>();

        foreach (var attachment in downloadable.Attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _downloadLimiter.WaitAsync(cancellationToken);
            try
            {
                downloadedFiles.Add(await AttemptDownload(attachment, cancellationToken));
            }
            finally
            {
                _downloadLimiter.Release();
            }
        }

        return downloadedFiles;
    }

    private async Task<DownloadedFile> AttemptDownload(OnlineFile onlineFile, CancellationToken cancellationToken)
    {
        var attempts = 0;
        while (true)
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

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
                _logger.Warning(ex, "Failed to download file\n URL: {file}. \nRetrying...", onlineFile.Url);
                await Task.Delay(5000);
            }
    }
}