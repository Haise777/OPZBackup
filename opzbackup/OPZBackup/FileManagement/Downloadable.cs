using Discord;
using OPZBackup.Data.Dto;

namespace OPZBackup.FileManagement;

public class Downloadable
{
    private static readonly SemaphoreSlim _downloadLimiter = new(50, 50);
    public readonly IEnumerable<OnlineAttachment> Attachments;
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
            var attachmentList = GetAttachments(attachments);
            Attachments = attachmentList;
            return;
        }

        var attachment = new OnlineAttachment(
            attachments.First().Url,
            messageId.ToString()
        );

        Attachments = [attachment];
    }

    public async Task<IEnumerable<AttachmentFile>> DownloadAsync(HttpClient client)
    {
        var downloadedFiles = new List<AttachmentFile>();
        foreach (var attachment in Attachments)
        {
            await _downloadLimiter.WaitAsync();
            try
            {
                downloadedFiles.Add(await DownloadAttachmentsAsync(attachment, client));
            }
            finally
            {
                _downloadLimiter.Release();
            }
        }

        return downloadedFiles;
    }

    private static async Task<AttachmentFile> DownloadAttachmentsAsync(OnlineAttachment onlineAttachment,
        HttpClient client)
    {
        var attempts = 0;
        while (true)
            try
            {
                var file = await client.GetByteArrayAsync(onlineAttachment.Url);
                return new AttachmentFile(file, onlineAttachment.GetFullName());
            }
            catch (HttpRequestException ex) //TODO-3 Deal with httpclient exceptions
            {
                if (++attempts > 3) throw;
                await Task.Delay(5000);
            }
    }

    private static IEnumerable<OnlineAttachment> GetAttachments(IEnumerable<IAttachment> attachments)
    {
        var attachmentList = new List<OnlineAttachment>();
        var count = 1;
        
        foreach (var attachment1 in attachments)
            attachmentList.Add(new OnlineAttachment(
                attachment1.Url,
                $"file{count++}"
            ));

        return attachmentList;
    }
}