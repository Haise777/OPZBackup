using System.IO.Compression;

namespace OPZBackup.FileManagement;

public class DirCompressor
{
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
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Update, leaveOpen: false);


            foreach (var filePath in Directory.GetFiles(channelDirPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entryName = Path.GetFileName(filePath);

                var existingEntry = zip.GetEntry(entryName);
                if (existingEntry != null)
                    existingEntry.Delete();

                zip.CreateEntryFromFile(filePath, entryName, App.CompressionLevel);
                entryNameList.Add(entryName);
            }
        });

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries.Where(e => entryNameList.Contains(e.Name)))
            compressedSize += entry.CompressedLength;

        return compressedSize;
    }
}