namespace OPZBackup.FileManagement;

public static class FileCleaner
{
    public static async Task<bool> DeleteDirAsync(string channelDirPath)
    {
        //TODO-3 Should have a logger here too
        if (!Directory.Exists(channelDirPath))
            return false;

        await Task.Run(() => Directory.Delete(channelDirPath, recursive: true));
        return true;
    }
}