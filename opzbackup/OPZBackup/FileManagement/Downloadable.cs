using Discord;
using OPZBackup.Data.Dto;

namespace OPZBackup.FileManagement;

public class Downloadable
{
    public readonly ulong MessageId;
    public readonly ulong ChannelId;
    public readonly string ChannelDirPath;
    public readonly IEnumerable<OnlineAttachment> Attachments;
    private static readonly SemaphoreSlim _downloadLimiter = new(50, 50);

    public Downloadable(ulong messageId, ulong channelId, IEnumerable<IAttachment> attachments)
    {
        if (attachments.Count() == 0)
            throw new ArgumentException("No attachments provided.");

        MessageId = messageId;
        ChannelId = channelId;
        ChannelDirPath = $"{App.FileBackupPath}/{channelId}";

        if (attachments.Count() > 1)
        {
            var attachmentList = GetAttachments(attachments, channelId, messageId);
            Attachments = attachmentList;
            return;
        }

        var attachment = new OnlineAttachment(
            attachments.First().Url,
            messageId.ToString(),
            ChannelDirPath
        );

        Attachments = new[] { attachment };
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

    private static async Task<AttachmentFile> DownloadAttachmentsAsync(OnlineAttachment onlineAttachment, HttpClient client)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                var file = await client.GetByteArrayAsync(onlineAttachment.Url);
                return new AttachmentFile(file, onlineAttachment.GetFullPath());
            }
            catch (HttpRequestException ex)
            {
                if (++attempts > 3) throw;
                await Task.Delay(5000);
            }
        }
    }

    private static IEnumerable<OnlineAttachment> GetAttachments(IEnumerable<IAttachment> attachments, ulong channelId,
        ulong messageId)
    {
        var attachmentList = new List<OnlineAttachment>();
        var count = 1;
        foreach (var attachment1 in attachments)
        {
            attachmentList.Add(new OnlineAttachment(
                attachment1.Url,
                $"file{count++}",
                $"{App.FileBackupPath}/{channelId}/{messageId}"
            ));
        }

        return attachmentList;
    }
}