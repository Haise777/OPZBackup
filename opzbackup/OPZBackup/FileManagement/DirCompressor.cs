using System.IO.Compression;

namespace OPZBackup.FileManagement;

public class DirCompressor
{
    //BUG: Fixed-Needs testing: Its skipping over the folder files when compressing
    public virtual async Task<long> CompressAsync(string channelDirPath, string targetDirPath,
        CancellationToken cancellationToken)
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
                cancellationToken.ThrowIfCancellationRequested();

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