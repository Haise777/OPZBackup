using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Services.Backup;

//This is the class responsible for applying the processing part of bussiness logic to it
public class MessageProcessor
{
    private readonly Mapper _mapper;
    private readonly CacheManager _cacheManager;

    public MessageProcessor(Mapper mapper, CacheManager cacheManager)
    {
        _mapper = mapper;
        _cacheManager = cacheManager;
    }

    public async Task<ProcessedBatch> ProcessAsync(IEnumerable<IMessage> fetchedMessages, BackupContext context,
        CancellationToken cancellationToken)
    {
        var users = new List<User>();
        var messages = new List<Message>();
        var toDownload = new List<Downloadable>();

        foreach (var message in fetchedMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsBotEmbedMessage(message))
                continue;

            //Checks if the message already exists on the db
            if (_cacheManager.IsMessageIdCached(message.Id))
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
            {
                var fileCount = GetAttachmentsAsDownloadable(message, toDownload, mappedMessage);
                context.StatisticTracker.IncrementFileCounter(message.Author.Id, fileCount);
            }

            //If the author of this message needs to be saved
            if (!_cacheManager.IsUserIdCached(message.Author.Id))
            {
                var user = _mapper.Map(message.Author);
                users.Add(user);
            }

            context.StatisticTracker.IncrementMessageCounter(message.Author.Id);
            messages.Add(mappedMessage);
        }

        return new ProcessedBatch(users, messages, toDownload);
    }

    private static int GetAttachmentsAsDownloadable(IMessage message, List<Downloadable> toDownload,
        Message mappedMessage)
    {
        var downloadable = new Downloadable(message);
        toDownload.Add(downloadable);

        return downloadable.Attachments.Count();

        //FEATURE: Make so that the messages saved on the database points to their correct files
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