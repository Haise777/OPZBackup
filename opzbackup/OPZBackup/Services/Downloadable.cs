using System.Collections.Frozen;
using System.Text.RegularExpressions;
using Discord;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

using Attachment = Utils.Attachment;

public class Downloadable
{
    public readonly ulong MessageId;
    public readonly ulong ChannelId;
    public readonly IEnumerable<Attachment> Attachments;
    private static readonly SemaphoreSlim _downloadLimiter = new(50, 50);

    public Downloadable(ulong messageId, ulong channelId, IEnumerable<IAttachment> attachments)
    {
        if (attachments.Count() == 0)
            throw new ArgumentException("No attachments provided.");

        MessageId = messageId;
        ChannelId = channelId;

        if (attachments.Count() > 1)
        {
            var attachmentList = GetAttachments(attachments, channelId, messageId);
            Attachments = attachmentList;
            return;
        }

        var attachment = new Attachment(
            attachments.First().Url,
            messageId.ToString(),
            $"{AppInfo.FileBackupPath}/{channelId}"
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

    private static async Task<AttachmentFile> DownloadAttachmentsAsync(Attachment attachment, HttpClient client)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                var file = await client.GetByteArrayAsync(attachment.Url);
                return new AttachmentFile(file, attachment.GetFullPath());
            }
            catch (HttpRequestException ex)
            {
                if (++attempts > 3) throw;
                await Task.Delay(5000);
            }
        }
    }

    private static IEnumerable<Attachment> GetAttachments(IEnumerable<IAttachment> attachments, ulong channelId,
        ulong messageId)
    {
        var attachmentList = new List<Attachment>();
        var count = 1;
        foreach (var attachment1 in attachments)
        {
            attachmentList.Add(new Attachment(
                attachment1.Url,
                $"file{count++}",
                $"{AppInfo.FileBackupPath}/{channelId}/{messageId}"
            ));
        }

        return attachmentList;
    }
}