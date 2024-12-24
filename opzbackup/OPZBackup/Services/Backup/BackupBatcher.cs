using Discord;
using Discord.WebSocket;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services.Backup;


public class BackupBatcher
{
    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly BackupContext _backupContext;
    private readonly ISocketMessageChannel _socketMessageChannel;

    public IEnumerable<IMessage> RawMessages = [];
    public IEnumerable<Message> ProcessedMessages = [];
    public IEnumerable<Downloadable> Attachments = [];
    public IEnumerable<User> NewUsers = [];


    public BackupBatcher(MessageFetcher messageFetcher, MessageProcessor messageProcessor, BackupContext backupContext, ISocketMessageChannel socketMessageChannel)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _backupContext = backupContext;
        _socketMessageChannel = socketMessageChannel;
    }

    public async Task StartBatchingAsync(ulong startAfterMessageId, CancellationToken cancellationToken) 
    {
        RawMessages = await FetchMessagesAsync(startAfterMessageId);

        if (!RawMessages.Any())
            return;

        var processedBatch = await ProcessAsync(cancellationToken);
        ProcessedMessages = processedBatch.Messages;
        Attachments = processedBatch.ToDownload;
        NewUsers = processedBatch.Users;
    }

    private async Task<IEnumerable<IMessage>> FetchMessagesAsync(ulong startAfterMessageId)
    {
        //_logger.Log.Information("Fetching messages...");

        return startAfterMessageId switch
        {
            0 => await _messageFetcher.FetchAsync(_socketMessageChannel),
            _ => await _messageFetcher.FetchAsync(_socketMessageChannel, startAfterMessageId)
        };
    }

    private async Task<BackupBatch2> ProcessAsync(CancellationToken cancellationToken)
    {
        return await _messageProcessor.ProcessAsync(RawMessages, _backupContext, cancellationToken);
    }
}