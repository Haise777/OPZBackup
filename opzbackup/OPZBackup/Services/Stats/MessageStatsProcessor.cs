using System.Text.RegularExpressions;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public class MessageStatsProcessor
{
    private readonly Regex _wordRegex = new(@"[\w']+");
    private readonly Regex _mentionRegex = new(@"<@!?(\d+)>");
    private readonly ICommonWordsAnalyzer _commonWordsAnalyzer;
    private readonly FileTypeStats _fileTypeStats;

    public MessageStatsProcessor(FileTypeStats fileTypeStats, ICommonWordsAnalyzer commonWordsAnalyzer)
    {
        _fileTypeStats = fileTypeStats;
        _commonWordsAnalyzer = commonWordsAnalyzer;
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
        Dictionary<string, int>? wordCounts = null;
        var mentionCounts = new Dictionary<ulong, int>();

        foreach (var message in messageList)
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                AnalyzeForMentions(message, mentionCounts);
                wordCounts = _commonWordsAnalyzer.AnalyzeForCommonWords(message);
            }

            if (message.HasFile)
                AnalyzeForFileTypes(message);
        }

        var wordCountArray = wordCounts is not null
            ? wordCounts.OrderByDescending(kv => kv.Value).Chunk(10).ToArray()
            : [];

        return new UserStats(
            null,
            mentionCounts.Chunk(10).ToArray(),
            wordCountArray,
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

    private void AnalyzeForFileTypes(Message message)
    {
        foreach (var attach in message.Attachments)
            _fileTypeStats.Increment(attach.Extension);
    }
}