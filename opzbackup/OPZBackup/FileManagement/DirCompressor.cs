using System.IO.Compression;
using System.Text;

namespace OPZBackup.FileManagement;

public class DirCompressor
{
    public async Task CompressAsync(string channelDirPath)
    {
        var placeName = Path.GetFileName(channelDirPath.TrimEnd(Path.DirectorySeparatorChar));
        var zipPath = Path.Combine(AppInfo.FileBackupPath, $"{placeName}.zip");
        FileMode fileMode;
        ZipArchiveMode zipMode;

        if (File.Exists(zipPath))
        {
            fileMode = FileMode.Open;
            zipMode = ZipArchiveMode.Update;
        }
        else
        {
            fileMode = FileMode.Create;
            zipMode = ZipArchiveMode.Create;
        }


        await Task.Run(() =>
        {
            using (var fileStream = new FileStream(zipPath, fileMode))
            {
                using (var zip = new ZipArchive(fileStream, zipMode))
                {
                    foreach (var filePath in Directory.GetFiles(channelDirPath))
                    {
                        var entryName = Path.GetFileName(filePath);

                        var existingEntry = zip.GetEntry(entryName);
                        if (existingEntry != null)
                            existingEntry.Delete();

                        zip.CreateEntryFromFile(filePath, entryName, CompressionLevel.Fastest);
                    }
                }
            }
        });
    }
}