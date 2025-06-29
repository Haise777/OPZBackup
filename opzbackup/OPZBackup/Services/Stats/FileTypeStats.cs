namespace OPZBackup.Services.Stats;

public class FileTypeStats
{
    public int ImageCount { get; private set; }
    public int VideoCount { get; private set; }
    public int AudioCount { get; private set; }
    public int OtherCount { get; private set; }

    public void Increment(string ext)
    {
        switch (GetFileType(ext))
        {
            case FileType.Image: ImageCount++; break;
            case FileType.Video: VideoCount++; break;
            case FileType.Audio: AudioCount++; break;
            default: OtherCount++; break;
        }
    }

    private static FileType GetFileType(string extension)
        => extension.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" or "png" or "gif" => FileType.Image,
            "mp4" or "mov" or "avi" => FileType.Video,
            "mp3" or "wav" => FileType.Audio,
            _ => FileType.Other,
        };
}

public enum FileType { Image, Video, Audio, Other }