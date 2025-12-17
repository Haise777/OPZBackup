using OPZBackup.Extensions;
using OPZBackup.FileManagement;
using OPZBackup.FileManagement.FileCompressor;
using OPZBackup.Logger;

namespace OPZBackup.Services.Backup;

public class BackupCompressor
{
    private readonly DirCompressor _dirCompressor;
    private readonly FileCleaner _fileCleaner;
    public readonly Timer PerformanceTimer;

    public BackupCompressor(DirCompressor dirCompressor, FileCleaner fileCleaner, Timer performanceTimer)
    {
        _dirCompressor = dirCompressor;
        _fileCleaner = fileCleaner;
        PerformanceTimer = performanceTimer;
    }

    public async Task CompressAsync(BackupContext context, CancellationToken cancelToken, BackupLogger logger)
    {
        if (context.FileCount == 0)
            return;

        logger.Log.Information("Compressing files");

        var compressedSize = await _dirCompressor.CompressAsync(
            $"{App.TempPath}/{context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}"
        );

        logger.Log.Information("Files compressed");

        context.StatisticTracker.CompressedFilesSize += compressedSize.compressedSize;
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }
}