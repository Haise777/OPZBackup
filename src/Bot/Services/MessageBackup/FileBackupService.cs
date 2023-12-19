﻿using System.Text.RegularExpressions;
using Discord;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;

namespace OPZBot.Services.MessageBackup.FileBackup;

public class FileBackupService
{
    private readonly HttpClient _client;
    private readonly ILogger<FileBackupService> _logger;
    private readonly Regex _matchFileExtension = new(@"([^\.]+)(?=\?ex)");

    public FileBackupService(HttpClient client, ILogger<FileBackupService> logger)
    {
        _client = client;
        _logger = logger;
        if (!Directory.Exists(Program.FileBackupPath))
            Directory.CreateDirectory(Program.FileBackupPath);
    }

    public async Task BackupFilesAsync(IMessage message)
    {
        if (!message.Attachments.Any()) return;
        if (message.Attachments.Count > 1)
        {
            await BackupMultipleFiles(message);
            return;
        }

        var fileUrl = message.Attachments.First().Url;
        var extension = _matchFileExtension.Match(fileUrl).Value;

        var file = await DownloadFile(fileUrl);

        await File.WriteAllBytesAsync(
            $@"{Program.FileBackupPath}\{message.Id}.{extension}", file);
    }

    private async Task BackupMultipleFiles(IMessage message)
    {
        var dirPath = @$"{Program.FileBackupPath}\{message.Id}";

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        var n = 0;
        foreach (var attachment in message.Attachments)
        {
            var file = await DownloadFile(attachment.Url);
            var extension = _matchFileExtension.Match(attachment.Url).Value;

            await File.WriteAllBytesAsync(@$"{dirPath}\file{++n}.{extension}", file);
        }
    }

    private async Task<byte[]> DownloadFile(string url)
    {
        var attempts = 0;
        while (true)
            try
            {
                var file = await _client.GetByteArrayAsync(url);
                return file;
            }
            catch (HttpRequestException ex)
            {
                if (attempts++ < 3)
                {
                    await _logger.RichLogErrorAsync(ex, "{service}: ", nameof(FileBackupService));
                    await Task.Delay(5000);
                    continue;
                }

                throw;
            }
    }
}