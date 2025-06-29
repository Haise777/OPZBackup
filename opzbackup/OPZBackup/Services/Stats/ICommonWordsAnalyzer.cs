using OPZBackup.Data.Models;

namespace OPZBackup.Services.Stats;

public interface ICommonWordsAnalyzer
{
    Dictionary<string, int> WordCounter { get; }
    void AnalyzeForCommonWords(Message message);
}