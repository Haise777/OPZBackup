using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services.Utils;

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

    public async Task<BackupBatch> ProcessAsync(IEnumerable<IMessage> fetchedMessages, BackupContext context)
    {
        //TODO Separate this to get its 'needed values' from some sort of 'caching system'
        var existingMessageIds = await _dbContext.Messages
            .Where(x => x.ChannelId == fetchedMessages.First().Channel.Id)
            .Select(m => m.Id)
            .ToArrayAsync();
        var existingUserIds = await _dbContext.Users
            .Select(u => u.Id)
            .ToArrayAsync();

        var users = new List<User>();
        var messages = new List<Message>();
        var toDownload = new List<Downloadable>();
        
        foreach (var message in fetchedMessages)
        {
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
                else continue;
            }

            var mappedMessage = _mapper.Map(message, context.BackupRegistry.Id);

            if (message.Attachments.Any())
                GetAttachmentsAsDownloadable(message, toDownload, mappedMessage);
            
            //TODO If the author of this message needs to be saved
            if (!existingUserIds.Contains(message.Author.Id))
                if (users.Any(u => u.Id == message.Author.Id))
                    users.Add(_mapper.Map(message.Author));

            messages.Add(mappedMessage);
        }
        
        return new BackupBatch(users, messages, toDownload);
    }

    private static void GetAttachmentsAsDownloadable(IMessage message, List<Downloadable> toDownload, Message mappedMessage)
    {
        var downloadable = new Downloadable(
            message.Id,
            message.Channel.Id,
            message.Attachments
        );
        toDownload.Add(downloadable);

        var downloadableAttachments = downloadable.Attachments.ToArray();

        if (downloadableAttachments.Count() == 1)
            mappedMessage.File = $"{downloadableAttachments.First().GetFullPath()}";
        else
            mappedMessage.File = $"{downloadableAttachments.First().FilePath}";
    }

    private static bool IsBotEmbedMessage(IMessage message) =>
        message.Content == "" && message.Author.Id == AppInfo.BotUserId;
}