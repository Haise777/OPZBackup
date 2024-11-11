using Discord;
using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.Exceptions;
using OPZBackup.FileManagement;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services.Utils;

// ReSharper disable PossibleMultipleEnumeration TODO-4

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

    public BackupService(MessageFetcher messageFetcher,
        MessageProcessor messageProcessor,
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader,
        DirCompressor dirCompressor)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _dirCompressor = dirCompressor;
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
            var sendCancelled = _responseHandler.SendProcessCancelledAsync();
            await sendCancelled;
        }
        catch (Exception)
        {
            //TODO-3 Log exception and also send the error name back to the client
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
        await _responseHandler.SendCompressingFilesAsync(_context);
        await _dirCompressor.CompressAsync(_context.ChannelDirPath);
        await FileCleaner.DeleteDirAsync(_context.ChannelDirPath);
    }

    private async Task BackupMessages()
    {
        ulong lastMessageId = 0;

        while (true)
        {
            if (_forcedStop)
                throw new BackupCanceledException();
            if (_context.IsStopped) //Flag becomes true when reached the last backuped message
                break;

            var fetchedMessages = await FetchMessages(lastMessageId);


            if (!fetchedMessages.Any()) //Reached the end of channel
                break;

            var backupBatch = await _messageProcessor.ProcessAsync(fetchedMessages, _context);

            lastMessageId = fetchedMessages.Last().Id;

            if (!backupBatch.Messages.Any())
                continue;

            await SaveBatch(backupBatch);

            _context.MessageCount += backupBatch.Messages.Count();
            _context.BatchNumber++;
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

        _context.ChannelDirPath = toDownload.First().ChannelDirPath;
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