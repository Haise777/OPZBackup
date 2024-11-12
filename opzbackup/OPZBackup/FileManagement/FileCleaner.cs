using Microsoft.Extensions.Logging;
using OPZBackup.Logger;
using Serilog;

namespace OPZBackup.FileManagement;

public class FileCleaner
{
    private readonly ILogger<FileCleaner> _logger;

    public FileCleaner(ILogger<FileCleaner> logger)
    {
        _logger = logger;
    }

    public async Task<bool> DeleteDirAsync(string? channelDirPath)
    {
        var task = Task.Run(() =>
        {
            if (channelDirPath is null || !Directory.Exists(channelDirPath))
                return false;

            Directory.Delete(channelDirPath, true);
            _logger.LogInformation("Directory '{channelDirPath}' deleted", channelDirPath);

            return true;
        });

        return await task;
    }

    public static bool DeleteDir(string? channelDirPath) 
    {
        if (channelDirPath is null || !Directory.Exists(channelDirPath))
            return false;

        Directory.Delete(channelDirPath, true);
        return true;
    }
}