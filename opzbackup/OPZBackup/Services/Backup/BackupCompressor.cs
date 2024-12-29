using OPZBackup.Extensions;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using Timer = OPZBackup.Services.Utils.Timer;

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
        PerformanceTimer.StartTimer();

        var compressedSize = await _dirCompressor.CompressAsync(
            $"{App.TempPath}/{context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}",
            cancelToken
        );

        logger.Log.Information("Files compressed in {seconds}", PerformanceTimer.Stop().Elapsed.Formatted());

        context.StatisticTracker.CompressedFilesSize += (ulong)compressedSize;
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }
}