using System.IO.Compression;
using OPZBackup.Extensions;
using OPZBackup.FileManagement;
using OPZBackup.Logger;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.FileManagement.FileCompressor;

public class DirCompressor : IBackupCompressor
{
    private readonly FileCleaner _fileCleaner;

    public DirCompressor(FileCleaner fileCleaner)
    {
        _fileCleaner = fileCleaner;
    }

    public async Task<CompressionResult> CompressAsync(string dirPath, string outputPath)
    {
        var compressedSize = await CompressFilesFromDir(dirPath,outputPath);

        return new CompressionResult(
                    (ulong)compressedSize
        );
    }

    private async Task<long> CompressFilesFromDir(string channelDirPath, string targetDirPath)
    {
        var fileName = Path.GetFileName(channelDirPath.TrimEnd(Path.DirectorySeparatorChar));
        var zipPath = Path.Combine(targetDirPath, $"{fileName}.zip");
        var fileMode = File.Exists(zipPath) ? FileMode.Open : FileMode.Create;
        var entryNameList = new List<string>();
        long compressedSize = 0;

        await Task.Run(() =>
        {
            using var fileStream = new FileStream(zipPath, fileMode);
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Update, false);

            var filePaths = Directory.GetFiles(channelDirPath, "*", SearchOption.AllDirectories);
            var basePathLength = channelDirPath.TrimEnd(Path.DirectorySeparatorChar).Length + 1;

            foreach (var filePath in filePaths)
            {
                // cancellationToken.ThrowIfCancellationRequested();

                var relativePath = filePath.Substring(basePathLength);

                var existingEntry = zip.GetEntry(relativePath);
                if (existingEntry != null)
                    existingEntry.Delete();

                zip.CreateEntryFromFile(filePath, relativePath, App.CompressionLevel);
                entryNameList.Add(relativePath);
            }
        });

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries.Where(e => entryNameList.Contains(e.Name)))
            compressedSize += entry.CompressedLength;

        return compressedSize;
    }
}