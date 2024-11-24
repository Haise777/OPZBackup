using System.Text.RegularExpressions;

namespace OPZBackup.Data.Dto;

public record OnlineFile
{
    private static readonly Regex MatchFileExtension = new(@"\.([^\.]+?)(?=\?ex)");
    public readonly ulong SenderId; 
    public readonly string FileExtension;
    public readonly string FileName;
    public readonly string Url;

    public OnlineFile(string url, string fileName, ulong senderId)
    {
        Url = url;
        FileName = fileName;
        SenderId = senderId;
        FileExtension = GetExtension(url);
    }

    public string FullFileName => $"{FileName}.{FileExtension}";

    private static string GetExtension(string fileUrl)
    {
        var matched = MatchFileExtension.Match(fileUrl).Value;
        var index = matched.IndexOf('.') + 1;
        var extension = matched.Substring(index);

        return extension.Length > 8 ? "" : extension;
    }
}