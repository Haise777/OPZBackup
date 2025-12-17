namespace OPZBackup.FileManagement.FileCompressor;


public interface IDirCompressor
{
    public Task<CompressionResult> CompressAsync(string dirPath, string outputPath);

}