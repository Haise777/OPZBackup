using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;
using Serilog;

namespace OPZBackup.FileManagement;

public class AttachmentDownloader
{
    private static readonly SemaphoreSlim
        _downloadLimiter = new(75, 75); //TODO: Make the value be configurable in the appsettings

    private readonly HttpClient _client;

    private readonly ILogger _logger;

    public AttachmentDownloader(HttpClient client, ILogger logger, StatisticTracker statisticTracker)
    {
        _client = client;
        _logger = logger.ForContext("System", "FILE MANAGEMENT");
    }

    public async Task<IEnumerable<AttachmentFile>> DownloadRangeAsync(IEnumerable<Downloadable> toDownload, BackupContext context,
        CancellationToken cancellationToken)
    {
        var concurrentDownloads = new List<Task<IEnumerable<AttachmentFile>>>();
        await CreateChannelDirIfNotExists(toDownload.First().ChannelId);

        foreach (var downloadable in toDownload)
            concurrentDownloads.Add(DownloadAndWriteFile(downloadable, context.StatisticTracker, cancellationToken));

        try
        {
            var finishedTasks = await Task.WhenAll(concurrentDownloads);
            var writtenAttachments = new List<AttachmentFile>();
            
            foreach (var attachments in finishedTasks)
            {
                writtenAttachments.AddRange(attachments);
            }

            return writtenAttachments;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occured while attempting to concurrently download and write attachments");
            if (ex is AggregateException aggr)
            {
                foreach (var aggregate in aggr.InnerExceptions)
                {
                    _logger.Error(aggregate, "Failed to concurrently download and write file");
                }
            }

            throw;
        }
    }

    private async Task<IEnumerable<AttachmentFile>> DownloadAndWriteFile(Downloadable downloadable, StatisticTracker statisticTracker,
        CancellationToken cancellationToken)
    {
        var files = await DownloadAttachments(downloadable, cancellationToken);
        var channelPath = $"{App.TempPath}/{downloadable.ChannelId}/";

        cancellationToken.ThrowIfCancellationRequested();

        if (files.Count() == 1)
        {
            var file = files.First();
            statisticTracker.IncrementByteSize(file.SenderId, (ulong)file.FileBytes.Length);

            var filePath = channelPath + file.FullFileName;
            await File.WriteAllBytesAsync(filePath, file.FileBytes);

            return [new AttachmentFile
            {
                Name = file.FileName,
                Extension = file.FileExtension,
                Path = filePath,
                ByteSize = (ulong)file.FileBytes.LongLength,
                MessageId = downloadable.MessageId
            }];
        }

        var basePath = $"{channelPath}/{downloadable.MessageId}";
        await CreateDirAsync(basePath);

        var writtenAttachments = new List<AttachmentFile>();

        foreach (var file in files)
        {
            statisticTracker.IncrementByteSize(file.SenderId, (ulong)file.FileBytes.Length);
            var filePath = basePath + '/' + file.FullFileName;

            await File.WriteAllBytesAsync(filePath, file.FileBytes);

            writtenAttachments.Add(new AttachmentFile
            {
                                Name = file.FileName,
                Extension = file.FileExtension,
                Path = filePath,
                ByteSize = (ulong)file.FileBytes.LongLength,
                MessageId = downloadable.MessageId
            });
        }

        return writtenAttachments;
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
            catch (HttpRequestException ex)
            {
                if (++attempts >= 3) throw;
                _logger.Warning(ex, "Failed to download file\n URL: {file}. \nRetrying...", onlineFile.Url);
                await Task.Delay(5000);
            }
    }
}