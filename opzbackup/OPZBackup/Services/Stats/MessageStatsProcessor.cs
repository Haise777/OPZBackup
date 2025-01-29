using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public class MessageStatsProcessor
{
    // Show a embed with all of the above, plus
    // total num of mention to other users with then number of mention for each individual user
    // each file type sent with their number of sent
    // like: image: 20, video: 7, audio: 2, others: 34
    // top most common words sent inside a message
    public async Task<object> AnalyzeMessageListAsync(IEnumerable<Message> messageList)
    {
        var top10Words = GetTop10Words(messageList);

    }

    // The top most common words, total num of mentions and for whom and 
    // total files with each file type count should all be done in the same loop

    //TODO: Untested method
    private List<KeyValuePair<string, int>> GetTop10Words(IEnumerable<Message> messages)
    {
        // Define punctuation characters (using char.IsPunctuation)
        var punctuation = Enumerable.Range(0, char.MaxValue + 1)
            .Select(c => (char)c)
            .Where(c => char.IsPunctuation(c))
            .ToArray();

        var regex = new Regex(@"[\w']+"); // Matches words including apostrophes and underscores
        var wordCounts = new Dictionary<string, int>();

        foreach (var message in messages)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            var matches = regex.Matches(message.Content);
            foreach (Match match in matches)
            {
                var word = match.Value;
                var trimmed = word.Trim(punctuation);
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

        return wordCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();
    }


}