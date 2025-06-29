using System.Text.RegularExpressions;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public class CommonWordsAnalyzer : ICommonWordsAnalyzer
{
    private readonly Regex _wordRegex = new(@"[\w']+");
    private readonly Regex _mentionRegex = new(@"<@!?(\d+)>");
    
    private readonly char[] _punctuationChars;
    private List<string> _wordsToIgnore;

    public CommonWordsAnalyzer()
    {
        _punctuationChars = Enumerable.Range(0, char.MaxValue + 1)
            .Select(c => (char)c)
            .Where(c => char.IsPunctuation(c))
            .ToArray();

        ReadAndParseWordList(); //TODO mudar dps
    }
    
    public Dictionary<string, int> AnalyzeForCommonWords(Message message)
    {
        var wordCounts = new Dictionary<string, int>();
        var wordMatches = _wordRegex.Matches(message.Content!);
        foreach (Match match in wordMatches)
        {
            var processedWord = ProcessWord(match.Value);

            if (string.IsNullOrEmpty(processedWord) || ShouldWordBeIgnored(processedWord))
                continue;

            IncrementOrAddWord(processedWord, wordCounts);
        }
        
        return wordCounts;
    }

    public string ProcessWord(string word)
    {
        var trimmed = word.Trim(_punctuationChars);
        var lowerWord = trimmed.ToLowerInvariant();

        return lowerWord;
    }

    public bool ShouldWordBeIgnored(string word)
    {
        if (!_wordsToIgnore.Any())
            return false;
        
        return _wordsToIgnore.Contains(word);
    }

    public void IncrementOrAddWord(string word, Dictionary<string, int> wordCounts)
    {
        if (wordCounts.TryGetValue(word, out int count))
        {
            wordCounts[word] = count + 1;
        }
        else
        {
            wordCounts[word] = 1;
        }
    }

    private void ReadAndParseWordList()
    {
        var wordList = new List<string>();
        using (var sr = new StreamReader("./ignored_words.txt"))
        {
            var line = sr.ReadLine();
            if (line != null)
                wordList = line.Split(',').ToList();
        }

        _wordsToIgnore = wordList;
    }
}