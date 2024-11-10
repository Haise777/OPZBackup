using System.Text.RegularExpressions;

namespace OPZBackup.Data.Dto;

public class OnlineAttachment
{
    public readonly string Url;
    public readonly string FileName;
    public readonly string FileExtension;
    public readonly string FilePath;
    
    private static readonly Regex MatchFileExtension = new(@"\.([^\.]+?)(?=\?ex)");

    public OnlineAttachment(string url, string fileName, string filePath)
    {
        Url = url;
        FileName = fileName;
        FileExtension = GetExtension(url);
        FilePath = filePath;
    }

    public string GetFullPath()
    {
        return $"{FilePath}/{FileName}.{FileExtension}";
    }
    
    private static string GetExtension(string fileUrl)
    {
        var matched = MatchFileExtension.Match(fileUrl).Value;
        var index = matched.IndexOf('.') + 1;
        var extension = matched.Substring(index);
        
        return extension.Length > 8 ? "" : extension;
    }
}