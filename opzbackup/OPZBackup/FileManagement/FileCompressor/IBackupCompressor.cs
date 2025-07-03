namespace OPZBackup.FileManagement.FileCompressor;


public interface IBackupCompressor
{
    public Task<CompressionResult> CompressAsync(string dirPath, string outputPath);

}