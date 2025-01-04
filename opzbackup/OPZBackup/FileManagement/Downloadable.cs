using Discord;
using OPZBackup.Data.Dto;

namespace OPZBackup.FileManagement;

public class Downloadable
{
    public readonly IEnumerable<OnlineFile> Attachments;
    public readonly ulong ChannelId;
    public readonly ulong MessageId;

    public Downloadable(IMessage message)
    {
        var attachments = message.Attachments;
        if (!attachments.Any())
            throw new ArgumentException("No attachments provided.");

        MessageId = message.Id;
        ChannelId = message.Channel.Id;

        if (attachments.Count() > 1)
        {
            Attachments = GetAttachments(message);
            return;
        }

        var attachment = new OnlineFile(
            attachments.First().Url,
            MessageId.ToString(),
            message.Author.Id
        );

        Attachments = [attachment];
    }

    private static IEnumerable<OnlineFile> GetAttachments(IMessage message)
    {
        var attachments = message.Attachments;
        var attachmentList = new List<OnlineFile>();
        var count = 1;

        foreach (var attachment1 in attachments)
            attachmentList.Add(new OnlineFile(
                attachment1.Url,
                $"file{count++}",
                message.Author.Id
            ));

        return attachmentList;
    }
}