using System.Text.RegularExpressions;
using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public class CommonWordsAnalyzer : ICommonWordsAnalyzer
{
    private readonly Regex _wordRegex = new(@"[\w']+");
    private readonly Regex _mentionRegex = new(@"<@!?(\d+)>");
    
    private readonly char[] _punctuationChars;
    private List<string> _wordsToIgnore;
    private Dictionary<string, int> _wordCounter = new();
    public Dictionary<string, int> WordCounter => _wordCounter;

    public CommonWordsAnalyzer()
    {
        _punctuationChars = Enumerable.Range(0, char.MaxValue + 1)
            .Select(c => (char)c)
            .Where(char.IsPunctuation)
            .ToArray();

        ReadAndParseWordList(); //TODO mudar dps
    }
    
    public void AnalyzeForCommonWords(Message message)
    {
        var wordMatches = _wordRegex.Matches(message.Content!);
        foreach (Match match in wordMatches)
        {
            var processedWord = ProcessWord(match.Value);

            if (string.IsNullOrEmpty(processedWord) || ShouldWordBeIgnored(processedWord))
                continue;

            IncrementOrAddWord(processedWord);
        }
    }

    private string ProcessWord(string word)
    {
        var trimmed = word.Trim(_punctuationChars);
        var lowerWord = trimmed.ToLowerInvariant();

        return lowerWord;
    }

    private bool ShouldWordBeIgnored(string word)
    {
        if (!_wordsToIgnore.Any())
            return false;
        
        return _wordsToIgnore.Contains(word);
    }

    private void IncrementOrAddWord(string word)
    {
        if (_wordCounter.TryGetValue(word, out int count))
        {
            _wordCounter[word] = count + 1;
        }
        else
        {
            _wordCounter[word] = 1;
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