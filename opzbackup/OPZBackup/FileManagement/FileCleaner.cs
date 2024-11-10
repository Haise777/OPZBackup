namespace OPZBackup.FileManagement;

public static class FileCleaner
{
    public static async Task DeleteDirAsync(string channelDirPath)
    {
        await Task.Run(() => Directory.Delete(channelDirPath, recursive: true));
    }
}