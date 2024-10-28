using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

public class BackupContext
{
    public BackupRegistry BackupRegistry { get; set; }

    private readonly MyDbContext _dbContext;
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly bool IsUntilLastBackup;

    private BackupContext(MyDbContext dbContext, AttachmentDownloader attachmentDownloader, bool isUntilLastBackup)
    {
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
        IsUntilLastBackup = isUntilLastBackup;
    }

    [Obsolete(message: $"Use {nameof(BackupContextFactory)}.{nameof(BackupContextFactory.RegisterNewBackup)} instead.")]
    public static async Task<BackupContext> CreateInstanceAsync(Channel channel, User author, bool isUntilLastBackup,
        AttachmentDownloader attachmentDownloader,
        MyDbContext dbContext)
    {
        var backupContext = new BackupContext(dbContext, attachmentDownloader, isUntilLastBackup);
        await backupContext.RegisterNewBackup(channel, author);

        return backupContext;
    }

    public async Task BackupAsync(IMessage[] fetchedMessages)
    {
        //Separate this to get its 'needed values' from some sort of 'caching system'
        var existingMessageIds = await _dbContext.Messages
            .Where(x => x.ChannelId == fetchedMessages.First().Channel.Id)
            .Select(m => m.Id)
            .ToArrayAsync();
        var existingUsersIds = await _dbContext.Users
            .Select(u => u.Id)
            .ToArrayAsync();

        var users = new List<User>();
        var messages = new List<Message>();
        var fileCount = 0;
        var concurrentDownloads = new List<Task>();

        try
        {
            foreach (var message in fetchedMessages)
            {
                if (IsBotEmbedMessage(message))
                    continue;

                //Checks if the message already exists on the db
                if (existingMessageIds.Any(m => m == message.Id))
                {
                    if (IsUntilLastBackup)
                    {
                        EndBackupProcess?.Invoke();
                        break;
                    }
                    else continue;
                }

                var mappedMessage = mapper.Map(message, BackupRegistry.Id);

                //Checks if there's file attached to this message
                if (message.Attachments.Any())
                {
                    //Make it so that it 'fetchs from the message' all of the needed 'file downloads' as a 'Downloadable'
                    //So that 'all of the downloads can be done later' to separate the downloading process from
                    //all of this
                    concurrentDownloads.Add(fileBackup.BackupFilesAsync(message));
                    mappedMessage.File =
                        $"Backup/Files/{message.Channel.Id}/{message.Id}{fileBackup.GetExtension(message)}";
                    fileCount += message.Attachments.Count;
                }

                //If the author of this message needs to be saved
                if (!existingUsersIds.Contains(message.Author.Id))
                    if (users.Any(u => u.Id == message.Author.Id))
                        users.Add(mapper.Map(message.Author));

                messages.Add(mappedMessage);
            }
        }
        finally
        {
            await Task.WhenAll(concurrentDownloads);
        }

        //return new MessageBatchData(users, messages, fileCount);
    }

    private async Task RegisterNewBackup(Channel channel, User author)
    {
        var backupRegistry = new BackupRegistry
        {
            AuthorId = author.Id,
            ChannelId = channel.Id,
            Date = DateTime.Now
        };

        if (!await _dbContext.Channels.AnyAsync(c => c.Id == channel.Id))
            _dbContext.Channels.Add(channel);
        if (!await _dbContext.Users.AnyAsync(u => u.Id == author.Id))
            _dbContext.Users.Add(author);

        _dbContext.BackupRegistries.Add(backupRegistry);
        await _dbContext.SaveChangesAsync();
    }

    private bool IsBotEmbedMessage(IMessage message) =>
        message.Content == "" && message.Author.Id == AppInfo.BotUserId;
}