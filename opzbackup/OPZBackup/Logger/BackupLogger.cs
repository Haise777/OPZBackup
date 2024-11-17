using Serilog;

namespace OPZBackup.Logger;

public class BackupLogger : IAsyncDisposable
{
    public Serilog.Core.Logger Log { get; set; }
    private readonly string _filePath;
    private static bool _first = true;

    public BackupLogger()
    {
        var date = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss");
        _filePath = $"logs/backup_{date}.txt";

        Log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(_filePath)
            .WriteTo.Console()
            .CreateLogger();

        if (_first)
        {
            _first = false;
            DisposeAsync().GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Log.DisposeAsync();
        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.Length == 0)
            File.Delete(_filePath);
        GC.SuppressFinalize(this);
    }
}