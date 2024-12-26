using OPZBackup.Extensions;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using OPZBackup.Services.Utils;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.Services.Backup;

public class BackupCompressor
{
    private readonly FileCleaner _fileCleaner;
    private readonly DirCompressor _dirCompressor;
    private readonly BackupLogger _logger;
    public readonly Timer PerformanceTimer;

    public BackupCompressor(DirCompressor dirCompressor, FileCleaner fileCleaner, Timer performanceTimer,
        BackupLogger logger)
    {
        _dirCompressor = dirCompressor;
        _fileCleaner = fileCleaner;
        PerformanceTimer = performanceTimer;
        _logger = logger;
    }
    
    public async Task CompressAsync(BackupContext context, CancellationToken cancelToken)
    {
        if (context.FileCount == 0)
            return;

        _logger.Log.Information("Compressing files");
        PerformanceTimer.StartTimer();

        var compressedSize = await _dirCompressor.CompressAsync(
            $"{App.TempPath}/{context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}",
            cancelToken
        );

        _logger.Log.Information("Files compressed in {seconds}", PerformanceTimer.Stop().Elapsed.Formatted());
        
        context.StatisticTracker.CompressedFilesSize += (ulong)compressedSize;
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }
}