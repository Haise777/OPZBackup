using System.Text.RegularExpressions;

namespace OPZBackup.Services.Utils;

public class Attachment
{
    public readonly string Url;
    public readonly string FileName;
    public readonly string FileExtension;
    public readonly string FilePath;
    
    private static readonly Regex MatchFileExtension = new(@"\.([^\.]+?)(?=\?ex)");

    public Attachment(string url, string fileName, string filePath)
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
        var extension = MatchFileExtension.Match(fileUrl).Value;
        return extension.Length > 8 ? "" : extension;
    }
}