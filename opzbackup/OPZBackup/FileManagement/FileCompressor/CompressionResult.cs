using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.FileManagement.FileCompressor;

//TODO: Only this for now, for future, maybe it would be nice to enrich this response with more info
public record CompressionResult(
    ulong compressedSize
);