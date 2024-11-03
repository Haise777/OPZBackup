using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

public class BackupService
{
    private readonly Utils.MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly BackupContextFactory _contextFactory;
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly MyDbContext _dbContext;
    private readonly Mapper _mapper;
    private BackupContext _context;
    private bool _forcedStop;

    public BackupService(Utils.MessageFetcher messageFetcher, MessageProcessor messageProcessor,
        BackupContextFactory contextFactory, MyDbContext dbContext, AttachmentDownloader attachmentDownloader,
        Mapper mapper)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        _mapper = mapper;
    }

    public async Task StartBackupAsync(SocketInteractionContext context, int choice)
    {
        var channel = _mapper.Map(context.Channel);
        var author = _mapper.Map(context.User);

        _context = await _contextFactory.RegisterNewBackup(channel, author, choice == 1);

        try
        {
            await BackupMessages(context);
        }
        catch (Exception ex)
        {
            //TODO Revert the transaction, cancel and cleanup the whole operation
        }
    }

    private async Task BackupMessages(SocketInteractionContext context)
    {
        ulong lastMessageId = 0;
        var continueLoop = true;

        while (continueLoop)
        {
            if (_context.IsStopped)
                break;
            else if (_forcedStop)
                throw new BackupCanceledException();

            var fetchedMessages = await FetchMessages(context.Channel, lastMessageId);

            if (!fetchedMessages.Any())
                break;

            lastMessageId = fetchedMessages.Last().Id;

            var backupBatch = await _messageProcessor.ProcessAsync(fetchedMessages, _context);

            if (!backupBatch.Messages.Any())
                continue;

            await SaveBatch(backupBatch);
            _context.MessageCount += backupBatch.Messages.Count();
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

    private async Task<IEnumerable<IMessage>> FetchMessages(ISocketMessageChannel channelContext, ulong lastMessageId)
    {
        if (lastMessageId == 0)
            return await _messageFetcher.FetchAsync(channelContext);
        else
            return await _messageFetcher.FetchAsync(channelContext, lastMessageId);
    }

    public async Task CancelAsync(ISocketMessageChannel contextChannel)
    {
        if (_context.BackupRegistry.Channel.Id == contextChannel.Id)
            _forcedStop = true;
        else
        {
            //TODO Do something
        }
    }
}