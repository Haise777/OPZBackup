namespace OPZBackup.Data.Dto;

public record DownloadedFile(
    byte[] FileBytes,
    string FileName,
    string FileExtension
)
{
    public string FullFileName => $"{FileName}.{FileExtension}";
}