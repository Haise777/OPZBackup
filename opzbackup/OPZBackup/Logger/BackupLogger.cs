using Serilog;

namespace OPZBackup.Logger;

public class BackupLogger : IAsyncDisposable
{
    private static bool _first = true;
    private readonly string _filePath;

    public BackupLogger()
    {
        var date = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss");
        _filePath = $"{LoggerConfig.LogFilePath}/backup/backup_{date}.txt";

        Log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Async(f => f.File(_filePath,
                outputTemplate: OutputTemplate.SplitDefaultTemplate("Backup")))
            .WriteTo.Console(outputTemplate: OutputTemplate.DefaultTemplate(LoggerUtils.ColorText("Backup", 63)))
            .CreateLogger();

        if (_first)
        {
            _first = false;
            DisposeAsync().GetAwaiter().GetResult();
        }
    }

    public Serilog.Core.Logger Log { get; set; }

    public async ValueTask DisposeAsync()
    {
        await Log.DisposeAsync();
        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.Length == 0)
            File.Delete(_filePath);
        GC.SuppressFinalize(this);
    }
}