using Discord;
using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.Exceptions;
using OPZBackup.FileManagement;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services.Utils;

// ReSharper disable PossibleMultipleEnumeration TODO-4
//TODO-Feature Have every backup process from the start to finish be logged in a specific file
//TODO-3 Better organize the logfiles in a log folder and have specific behavior for different types of logs, like append, new file, etc
//TODO-Feature Implement passive benchmarking capabilities to various bot functions, time per batch, average per batch, response time, etc


namespace OPZBackup.Services.Backup;

public class BackupService
{
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly BackupContextFactory _contextFactory;
    private readonly MyDbContext _dbContext;
    private readonly DirCompressor _dirCompressor;
    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private BackupContext _context = null!;
    private bool _forcedStop;
    private ServiceResponseHandler _responseHandler = null!;
    private readonly FileCleaner _fileCleaner;

    public BackupService(MessageFetcher messageFetcher,
        MessageProcessor messageProcessor,
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader,
        DirCompressor dirCompressor, FileCleaner fileCleaner)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _dirCompressor = dirCompressor;
        _fileCleaner = fileCleaner;
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext,
        ServiceResponseHandler responseHandler, bool isUntilLast)
    {
        _responseHandler = responseHandler;
        _context = await _contextFactory.RegisterNewBackup(interactionContext, isUntilLast);

        try
        {
            await _responseHandler.SendStartNotificationAsync(_context);

            await BackupMessages();
            await CompressFilesAsync();
        }
        catch (BackupCanceledException)
        {
            //TODO-2 Log cancellation
            var sendCancelled = _responseHandler.SendProcessCancelledAsync();
            var rollBack = _context.RollbackAsync();
            await rollBack;
            await sendCancelled;
            return;
        }
        catch (Exception)
        {
            //TODO-2 Log exception and also send the error name back to the client
            var sendFailed = _responseHandler.SendFailedAsync(_context);
            var rollBack = _context.RollbackAsync();
            await rollBack;
            await sendFailed;

            throw;
        }

        await _responseHandler.SendCompletedAsync(_context);
    }

    private async Task CompressFilesAsync()
    {
        //TODO-3 Implement a way of tracking the progress of the compression
        await _responseHandler.SendCompressingFilesAsync(_context);
        await _dirCompressor.CompressAsync(
            $"{App.TempFilePath}/{_context.BackupRegistry.ChannelId}",
            $"{App.FileBackupPath}"
        );
        await _fileCleaner.DeleteDirAsync(App.TempFilePath);
    }

    private async Task BackupMessages()
    {
        ulong lastMessageId = 0;

        while (true)
        {
            if (_forcedStop)
                throw new BackupCanceledException(); //TODO-4 Should I use a CancellationToken here to throw instead?
            if (_context.IsStopped)
                break;

            var fetchedMessages = await FetchMessages(lastMessageId);
            if (!fetchedMessages.Any()) break;
            lastMessageId = fetchedMessages.Last().Id;

            var backupBatch = await _messageProcessor.ProcessAsync(fetchedMessages, _context);
            if (!backupBatch.Messages.Any())
                continue;

            await SaveBatch(backupBatch);
            _context.BatchNumber++;
            _context.MessageCount += backupBatch.Messages.Count();
            await _responseHandler.SendBatchFinishedAsync(_context, backupBatch);
        }
    }

    private async Task SaveBatch(BackupBatch batch)
    {
        _dbContext.Messages.AddRange(batch.Messages);

        if (batch.Users.Any())
            _dbContext.Users.AddRange(batch.Users);

        await _dbContext.SaveChangesAsync();

        if (batch.ToDownload.Any())
            await DownloadMessageAttachments(batch.ToDownload);
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload)
    {
        foreach (var downloadable in toDownload)
            _context.FileCount += downloadable.Attachments.Count();

        await _attachmentDownloader.DownloadAsync(toDownload);
    }

    private async Task<IEnumerable<IMessage>> FetchMessages(ulong lastMessageId)
    {
        var channelContext = _context.InteractionContext.Channel;

        return lastMessageId switch
        {
            0 => await _messageFetcher.FetchAsync(channelContext),
            _ => await _messageFetcher.FetchAsync(channelContext, lastMessageId)
        };
    }

    public void Cancel()
    {
        _forcedStop = true;
    }
}