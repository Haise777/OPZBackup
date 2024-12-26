using OPZBackup.FileManagement;
using OPZBackup.Logger;

namespace OPZBackup.Services.Backup;


public class BackupCompressor : DirCompressor
{
    private readonly FileCleaner _fileCleaner;


    public async Task CompressAsync(BackupContext context, CancellationToken cancelToken)
    {
        if (context.FileCount == 0)
            return;

        var compressedSize = await base.CompressAsync(
            $"{App.TempPath}/{context.BackupRegistry.ChannelId}",
            $"{App.BackupPath}",
            cancelToken
        );
        
        context.StatisticTracker.CompressedFilesSize += (ulong)compressedSize;
        await _fileCleaner.DeleteDirAsync(App.TempPath);
    }


}