using System.Text.RegularExpressions;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public class MessageStatsProcessor
{
    private readonly Regex _wordRegex = new(@"[\w']+");
    private readonly Regex _mentionRegex = new(@"<@!?(\d+)>");
    private readonly char[] _punctuationChars;
    public readonly FileTypeStats _fileTypeStats;

    public MessageStatsProcessor(FileTypeStats fileTypeStats)
    {
        _fileTypeStats = fileTypeStats;

        _punctuationChars = Enumerable.Range(0, char.MaxValue + 1)
            .Select(c => (char)c)
            .Where(c => char.IsPunctuation(c))
            .ToArray();
    }

    // Show a embed with all of the above, plus
    // total num of mention to other users with then number of mention for each individual user
    // each file type sent with their count
    // like: image: 20, video: 7, audio: 2, others: 34
    // top most common words sent inside a message
    // The top most common words, total num of mentions and for whom and 
    // total files with each file type count should all be done in the same loop
    public async Task<UserStats> AnalyzeMessageListAsync(IEnumerable<Message> messageList)
    {
        var wordCounts = new Dictionary<string, int>();
        var mentionCounts = new Dictionary<ulong, int>();
        //TODO: Do something about this

        foreach (var message in messageList)
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                AnalyzeForMentions(message, mentionCounts);
                AnalyzeForCommonWords(message, wordCounts);
            }

            if (message.HasFile)
                AnalyzeForFileTypes(message);
        }

        return new UserStats(
            null,
            mentionCounts.Chunk(10).ToArray(),
            wordCounts.OrderByDescending(kv => kv.Value).Chunk(10).ToArray(),
            _fileTypeStats
        );
    }

    private void AnalyzeForMentions(Message message, Dictionary<ulong, int> mentionCounts)
    {
        var mentionMatches = _mentionRegex.Matches(message.Content!);
        foreach (Match match in mentionMatches)
        {
            var mention = match.Groups[1].Value;
            var userId = ulong.Parse(mention);

            if (mentionCounts.TryGetValue(userId, out int count))
            {
                mentionCounts[userId] = count + 1;
            }
            else
            {
                mentionCounts[userId] = 1;
            }
        }
    }

    private void AnalyzeForCommonWords(Message message, Dictionary<string, int> wordCounts)
    {
        //TODO: Add a way for it to pick from a localFile which words should be skipped and not counted
        var wordMatches = _wordRegex.Matches(message.Content!);
        foreach (Match match in wordMatches)
        {
            var word = match.Value;
            var trimmed = word.Trim(_punctuationChars);
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var lowerWord = trimmed.ToLowerInvariant();
            if (wordCounts.TryGetValue(lowerWord, out int count))
            {
                wordCounts[lowerWord] = count + 1;
            }
            else
            {
                wordCounts[lowerWord] = 1;
            }
        }
    }

    private void AnalyzeForFileTypes(Message message)
    {
        foreach (var attach in message.Attachments)
            _fileTypeStats.Increment(attach.Extension);
    }
}

public class FileTypeStats
{
    public int ImageCount { get; private set; }
    public int VideoCount { get; private set; }
    public int AudioCount { get; private set; }
    public int OtherCount { get; private set; }

    public void Increment(string ext)
    {
        switch (GetFileType(ext))
        {
            case FileType.Image: ImageCount++; break;
            case FileType.Video: VideoCount++; break;
            case FileType.Audio: AudioCount++; break;
            default: OtherCount++; break;
        }
    }

    private static FileType GetFileType(string extension)
        => extension.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" or "png" or "gif" => FileType.Image,
            "mp4" or "mov" or "avi" => FileType.Video,
            "mp3" or "wav" => FileType.Audio,
            _ => FileType.Other,
        };
}

public enum FileType { Image, Video, Audio, Other }
