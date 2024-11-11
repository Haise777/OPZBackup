using System.IO.Compression;

namespace OPZBackup.FileManagement;

public class DirCompressor
{
    public async Task CompressAsync(string? channelDirPath)
    {
        if (channelDirPath is null)
            return;

        var placeName = Path.GetFileName(channelDirPath.TrimEnd(Path.DirectorySeparatorChar));
        var zipPath = Path.Combine(App.FileBackupPath, $"{placeName}.zip");
        var fileMode = File.Exists(zipPath) ? FileMode.Open : FileMode.Create;

        await Task.Run(() =>
        {
            using var fileStream = new FileStream(zipPath, fileMode);
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Update);

            foreach (var filePath in Directory.GetFiles(channelDirPath))
            {
                var entryName = Path.GetFileName(filePath);

                var existingEntry = zip.GetEntry(entryName);
                if (existingEntry != null)
                    existingEntry.Delete();

                zip.CreateEntryFromFile(filePath, entryName, CompressionLevel.Fastest);
            }
        });
    }
}