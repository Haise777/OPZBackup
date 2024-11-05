namespace OPZBackup.FileManagement;

public static class FileCleaner
{
    public static async Task DeleteFilesAsync(IEnumerable<string> filePaths)
    {
        var concurrentDeletion = new List<Task>();
        foreach (var path in filePaths)
            concurrentDeletion.Add(DeleteFiles(path));

        var deletionInProgress = Task.WhenAll(concurrentDeletion); 

        try
        {
            await deletionInProgress;
        }
        catch (Exception) //Test if it actually catches thrown exception
        {
            if (deletionInProgress.Exception is not null)
                throw deletionInProgress.Exception;
        }
    }

    private static Task DeleteFiles(string filePath)
    {
        if (Path.GetExtension(filePath) == string.Empty)
            if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(filePath, true);
                return Task.CompletedTask;
            }

        File.Delete(filePath);
        return Task.CompletedTask;
    }
}