using System.Text.RegularExpressions;
using Discord;

namespace OPZBot.Services.MessageBackup.FileBackup;

public class FileBackupService
{
    private readonly HttpClient _client;
    private readonly Regex _regex = new(@"([^\.]+)(?=\?ex)");
    public FileBackupService(HttpClient client)
    {
        _client = client;
        if (!Directory.Exists(Program.FileBackupPath))
            Directory.CreateDirectory(Program.FileBackupPath);
    }

    public async Task BackupMessageFilesAsync(IMessage message)
    {
        if (message.Attachments.Count > 1)
        {
            await MultipleFiles(message);
            return;
        }
        
        var fileUrl = message.Attachments.First().Url;
        var extension = _regex.Match(fileUrl).Value;
        
        byte[] file = await _client.GetByteArrayAsync(fileUrl);
        
        await File.WriteAllBytesAsync(
            $@"{Program.FileBackupPath}\{message.Id}.{extension}", file);
    }

    private async Task MultipleFiles(IMessage message)
    {
        var dirPath = @$"{Program.FileBackupPath}\{message.Id}";
        
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        var n = 0;
        foreach (var attachment in message.Attachments)
        {
            var file = await _client.GetByteArrayAsync(attachment.Url);
            var extension = _regex.Match(attachment.Url).Value;

            await File.WriteAllBytesAsync(@$"{dirPath}\file{++n}.{extension}", file);
        }
    }
}