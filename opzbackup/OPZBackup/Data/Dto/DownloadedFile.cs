namespace OPZBackup.Data.Dto;

public record DownloadedFile(
    byte[] FileBytes,
    ulong SenderId,
    string FileName,
    string FileExtension
)
{
    public string FullFileName => $"{FileName}.{FileExtension}";
}