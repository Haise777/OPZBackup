using Discord.Interactions;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;


namespace OPZBackup.Services;

public class BackupService
{
    private readonly Utils.MessageFetcher _messageFetcher;
    private readonly MyDbContext _dbContext; //Needed dependency of the BackupContext
    private readonly BackupUtils _backupUtils;
    private readonly BackupContextFactory _contextFactory;
    private BackupContext _context;

    public BackupService(Utils.MessageFetcher messageFetcher, MyDbContext dbContext, BackupUtils backupUtils,
        BackupContextFactory contextFactory)
    {
        _messageFetcher = messageFetcher;
        _dbContext = dbContext;
        _backupUtils = backupUtils;
        _contextFactory = contextFactory;
    }

    public async Task StartBackupAsync(SocketInteractionContext context, int choice)
    {
        //1 get channel and author from socketcontext and maps to Channel and User entity
        Channel channel = null;
        User author = null;

        //because it saves in batches, and message batches need to point to their related BackupRegistry before being saved
        _context = await _contextFactory.RegisterNewBackup(channel, author, choice == 1);

        try
        {
            await BackupMessages(context);
        }
        catch (Exception ex)
        {
            //7.a Revert the transaction, cancel and cleanup the whole operation
        }

        throw new NotImplementedException();
    }

    private async Task BackupMessages(SocketInteractionContext context)
    {
        long lastMessageId = 0;
        //Needs to be in a loop to keep batching
        while (_continueLoop)
        {
            //1 Gets a collection of fetched messages
            var fetchedMessages = _messageFetcher.FetchAsync(context.Channel, lastMessageId);

            //2 Check if fetched messages are not empty
            if (fetchedMessages.Count() == 0)
            {
                //blabla
            }

            //3 Defines the next starting point for the next fetch (the last message of the stack)
            lastMessageId = fetchedMessages.Last().MessageId;

            //4 Process messages, whatever that means
            //5 Check if processed messages are empty
            //6 Saves the batch
            //7 Updates the statistics, whatever that means
            await _context.BackupAsync(fetchedMessages);
        }

        throw new NotImplementedException();
    }
}