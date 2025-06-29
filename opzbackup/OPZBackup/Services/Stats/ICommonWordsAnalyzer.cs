using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public interface ICommonWordsAnalyzer
{
    Dictionary<string, int> AnalyzeForCommonWords(Message message);
    string ProcessWord(string word);
    bool ShouldWordBeIgnored(string word);
    void IncrementOrAddWord(string word, Dictionary<string, int> wordCounts);
}