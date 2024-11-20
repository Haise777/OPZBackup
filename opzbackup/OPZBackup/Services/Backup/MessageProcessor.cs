using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services.Backup;

//This is the class responsible for applying the processing part of bussiness logic to it
public class MessageProcessor
{
    private readonly MyDbContext _dbContext;
    private readonly Mapper _mapper;

    public MessageProcessor(MyDbContext dbContext, Mapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<BackupBatch> ProcessAsync(IEnumerable<IMessage> fetchedMessages, BackupContext context, CancellationToken cancellationToken)
    {
        //TODO-3 Separate this to get its 'needed values' from some sort of 'caching system'
        var existingMessageIds = await _dbContext.Messages
            .Where(x => x.ChannelId == fetchedMessages.First().Channel.Id)
            .Select(m => m.Id)
            .ToArrayAsync();
        var existingUserIds = await _dbContext.Users
            .Select(u => u.Id)
            .ToListAsync();

        var users = new List<User>();
        var messages = new List<Message>();
        var toDownload = new List<Downloadable>();

        foreach (var message in fetchedMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsBotEmbedMessage(message))
                continue;

            //Checks if the message already exists on the db
            if (existingMessageIds.Any(m => m == message.Id))
            {
                if (context.IsUntilLastBackup)
                {
                    context.Stop();
                    break;
                }

                continue;
            }

            var mappedMessage = _mapper.Map(message, context.BackupRegistry.Id);

            if (message.Attachments.Any())
                GetAttachmentsAsDownloadable(message, toDownload, mappedMessage);

            //TODO-4 If the author of this message needs to be saved
            if (!existingUserIds.Contains(message.Author.Id))
            {
                var user = _mapper.Map(message.Author);
                existingUserIds.Add(user.Id);
                users.Add(user);
            }

            messages.Add(mappedMessage);
        }

        return new BackupBatch(users, messages, toDownload);
    }

    private static void GetAttachmentsAsDownloadable(IMessage message, List<Downloadable> toDownload,
        Message mappedMessage)
    {
        var downloadable = new Downloadable(
            message.Id,
            message.Channel.Id,
            message.Attachments
        );
        toDownload.Add(downloadable);
        
        //TODO-Feature Make so that the messages saved on the database points to their correct files
        // var downloadableAttachments = downloadable.Attachments.ToArray();

        // if (downloadableAttachments.Count() == 1)
        //     mappedMessage.File = $"{downloadableAttachments.First().GetFullName()}";
        // else
        //     mappedMessage.File = $"{downloadableAttachments.First().FilePath}";
    }

    private static bool IsBotEmbedMessage(IMessage message)
    {
        return message.Content == "" && message.Author.Id == App.BotUserId;
    }
}