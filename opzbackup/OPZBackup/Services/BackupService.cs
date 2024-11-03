using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

public class BackupService
{
    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly BackupContextFactory _contextFactory;
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly MyDbContext _dbContext;
    private readonly BackupResponseHandler _responseHandler;
    private readonly Mapper _mapper;
    private BackupContext _context;
    private bool _forcedStop;

    public BackupService(MessageFetcher messageFetcher, MessageProcessor messageProcessor,
        BackupContextFactory contextFactory, MyDbContext dbContext, AttachmentDownloader attachmentDownloader,
        Mapper mapper, BackupResponseHandler responseHandler)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _mapper = mapper;
        _responseHandler = responseHandler;
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext, bool isUntilLast)
    {
        var channel = _mapper.Map(interactionContext.Channel);
        var author = _mapper.Map(interactionContext.User);

        _context = await _contextFactory.RegisterNewBackup(channel, author, isUntilLast);

        try
        {
            await _responseHandler.SendStartNotificationAsync(interactionContext, _context);
            await BackupMessages(interactionContext);
        }
        catch (Exception ex)
        {
            await _responseHandler.SendFailedAsync(interactionContext, _context);
            await _context.RollbackAsync();
            throw;
            //TODO Deletes all stored files from this backup
        }

        await _responseHandler.SendCompletedAsync(interactionContext, _context);
    }

    private async Task BackupMessages(SocketInteractionContext interactionContext)
    {
        ulong lastMessageId = 0;

        while (true)
        {
            if (_forcedStop)
                throw new BackupCanceledException();
            if (_context.IsStopped) //Reached the last backuped message
                break;

            var fetchedMessages = await FetchMessages(interactionContext.Channel, lastMessageId);

            if (!fetchedMessages.Any()) //Reached the end of channel
                break;

            lastMessageId = fetchedMessages.Last().Id;

            var backupBatch = await _messageProcessor.ProcessAsync(fetchedMessages, _context);

            if (!backupBatch.Messages.Any())
                continue;

            await SaveBatch(backupBatch);
            _context.MessageCount += backupBatch.Messages.Count();
            await _responseHandler.SendBatchFinishedAsync(interactionContext, _context);
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
        {
            _context.FileCount += downloadable.Attachments.Count();
            //TODO add to BackupContext all of the filePaths of the new files for later processing
        }

        await _attachmentDownloader.DownloadAsync(toDownload);
    }

    private async Task<IEnumerable<IMessage>> FetchMessages(ISocketMessageChannel channelContext, ulong lastMessageId)
    {
        if (lastMessageId == 0)
            return await _messageFetcher.FetchAsync(channelContext);
        else
            return await _messageFetcher.FetchAsync(channelContext, lastMessageId);
    }

    public void Cancel()
    {
        _forcedStop = true;
    }
}