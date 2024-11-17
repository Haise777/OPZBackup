using System.Text.RegularExpressions;

namespace OPZBackup.Data.Dto;

public record OnlineFile
{
    private static readonly Regex MatchFileExtension = new(@"\.([^\.]+?)(?=\?ex)");
    public readonly string FileExtension;
    public readonly string FileName;
    public readonly string Url;
    public string FullFileName => $"{FileName}.{FileExtension}";

    public OnlineFile(string url, string fileName)
    {
        Url = url;
        FileName = fileName;
        FileExtension = GetExtension(url);
    }

    private static string GetExtension(string fileUrl)
    {
        var matched = MatchFileExtension.Match(fileUrl).Value;
        var index = matched.IndexOf('.') + 1;
        var extension = matched.Substring(index);

        return extension.Length > 8 ? "" : extension;
    }
}