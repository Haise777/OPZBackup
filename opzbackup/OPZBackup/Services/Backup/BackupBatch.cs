using Discord.WebSocket;

namespace OPZBackup.Services.Backup;


public class BackupBatch 
{
    public BackupBatch() 
    {
    }

    public async Task FetchMessagesAsync(ISocketMessageChannel socketMessageChannel, ulong lastMessageId)
    {
        //_logger.Log.Information("Fetching messages...");

        return lastMessageId switch
        {
            0 => await _messageFetcher.FetchAsync(channelContext),
            _ => await _messageFetcher.FetchAsync(channelContext, lastMessageId)
        };

        throw new NotImplementedException();
    }

        public async Task ProcessAsync()
    {
        throw new NotImplementedException();
    }
        public async Task SaveBatchedMessagesAsync()
    {
        throw new NotImplementedException();
    }
        public async Task DownloadBatchedAttachmentsAsync()
    {
        throw new NotImplementedException();
    }
        public async Task CompleteBatchAsync()
    {
        throw new NotImplementedException();
    }

}