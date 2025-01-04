using OPZBackup.Data.Dto;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;
using Serilog;

namespace OPZBackup.FileManagement;

public class AttachmentDownloader
{
    private static readonly SemaphoreSlim
        _downloadLimiter = new(50, 50); //TODO: Make the value be configurable in the appsettings

    private readonly HttpClient _client;

    private readonly ILogger _logger;

    public AttachmentDownloader(HttpClient client, ILogger logger, StatisticTracker statisticTracker)
    {
        _client = client;
        _logger = logger.ForContext("System", "FILE MANAGEMENT");
    }

    public async Task DownloadRangeAsync(IEnumerable<Downloadable> toDownload, BackupContext context,
        CancellationToken cancellationToken)
    {
        var concurrentDownloads = new List<Task>();
        await CreateChannelDirIfNotExists(toDownload.First().ChannelId);

        foreach (var downloadable in toDownload)
            concurrentDownloads.Add(DownloadAndWriteFile(downloadable, context.StatisticTracker, cancellationToken));

        try
        {
            await Task.WhenAll(concurrentDownloads);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            //TODO: log exception
            if (ex is AggregateException aggr)
            {
                //TODO: log all aggregated exceptions
            }

            throw;
        }
    }

    private async Task DownloadAndWriteFile(Downloadable downloadable, StatisticTracker statisticTracker,
        CancellationToken cancellationToken)
    {
        var files = await DownloadAttachments(downloadable, cancellationToken);
        var channelPath = $"{App.TempPath}/{downloadable.ChannelId}/";

        cancellationToken.ThrowIfCancellationRequested();

        if (files.Count() == 1)
        {
            var file = files.First();
            statisticTracker.IncrementByteSize(file.SenderId, (ulong)file.FileBytes.Length);
            
            await File.WriteAllBytesAsync(channelPath + file.FullFileName, file.FileBytes);

            return;
        }

        var basePath = $"{channelPath}/{downloadable.MessageId}";
        await CreateDirAsync(basePath);

        foreach (var file in files)
        {
            statisticTracker.IncrementByteSize(file.SenderId, (ulong)file.FileBytes.Length);
            
            await File.WriteAllBytesAsync(basePath + '/' + file.FullFileName, file.FileBytes);
        }
    }

    private Task CreateDirAsync(string dirPath)
    {
        return Task.Run(() => Directory.CreateDirectory(dirPath));
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
                    onlineFile.SenderId,
                    onlineFile.FileName,
                    onlineFile.FileExtension
                );
            }
            catch (HttpRequestException ex) //TODO: Deal with httpclient exceptions
            {
                //Log.
                if (++attempts >= 3) throw;
                _logger.Warning(ex, "Failed to download file\n URL: {file}. \nRetrying...", onlineFile.Url);
                await Task.Delay(5000);
            }
    }
}