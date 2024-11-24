using Discord;
using OPZBackup.Data.Dto;

namespace OPZBackup.FileManagement;

//TODO-1 Rethink the whole Downloadable/AttachmentOnline/AttachmentFile logic and separate the download functionality from the Downloadable
public class Downloadable
{
    public readonly IEnumerable<OnlineFile> Attachments;
    public readonly ulong ChannelId;
    public readonly ulong MessageId;

    public Downloadable(ulong messageId, ulong channelId, IEnumerable<IAttachment> attachments)
    {
        if (!attachments.Any())
            throw new ArgumentException("No attachments provided.");

        MessageId = messageId;
        ChannelId = channelId;

        if (attachments.Count() > 1)
        {
            Attachments = GetAttachments(attachments);
            return;
        }

        var attachment = new OnlineFile(
            attachments.First().Url,
            messageId.ToString()
        );

        Attachments = [attachment];
    }

    private static IEnumerable<OnlineFile> GetAttachments(IEnumerable<IAttachment> attachments)
    {
        var attachmentList = new List<OnlineFile>();
        var count = 1;

        foreach (var attachment1 in attachments)
            attachmentList.Add(new OnlineFile(
                attachment1.Url,
                $"file{count++}"
            ));

        return attachmentList;
    }
}