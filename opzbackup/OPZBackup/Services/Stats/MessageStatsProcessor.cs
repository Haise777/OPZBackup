using System.Text.RegularExpressions;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public class MessageStatsProcessor
{

    private readonly Regex _wordRegex = new(@"[\w']+");
    private readonly Regex _mentionRegex = new(@"<@!?(\d+)>"); //TODO: to match only mention parts inside the string
    private readonly char[] _punctuationChars;

    public MessageStatsProcessor()
    {
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
        var fileTypesCounts = new Dictionary<string, int>();

        foreach (var message in messageList)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            AnalyzeForMentions(message, mentionCounts);
            AnalyzeForCommonWords(message, wordCounts);

            if (message.HasFile)
                AnalyzeForFileTypes(message, fileTypesCounts);

        }

        return new UserStats(
            mentionCounts,
            wordCounts,
            new FileTypeStats(
                fileTypesCounts["image"],
                fileTypesCounts["video"],
                fileTypesCounts["audio"],
                fileTypesCounts["other"]
            )
        );
        // return wordCounts
        //     .OrderByDescending(kvp => kvp.Value)
        //     .Take(10)
        //     .ToList();
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

    private void AnalyzeForFileTypes(Message message, Dictionary<string, int> fileTypesCounts)
    {

        foreach (var attachment in message.Attachments)
        {
            var fileType = GetFileType(attachment.Extension);

            if (fileTypesCounts.TryGetValue(fileType, out int count))
            {
                fileTypesCounts[fileType] = count + 1;
            }
            else
            {
                fileTypesCounts[fileType] = 1;
            }
        }
    }

    private static string GetFileType(string extension)
    {
        return extension switch
        {
            "jpg" or "jpeg" or "png" or "gif" => "image",
            "mp4" or "mov" or "avi" => "video",
            "mp3" or "wav" => "audio",
            _ => "other"
        };
    }
}