using AnsiStyles;
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
        _filePath = $"logs/backup/backup_{date}.txt"; //TODO centralize log paths configuration in one place
        
        Log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Async(f => f.File(_filePath,
                outputTemplate: OutputTemplate.SplitDefaultTemplate("Backup")))
            .WriteTo.Console(outputTemplate: OutputTemplate.DefaultTemplate(OutputTemplate.ColorText("Backup", 63)))
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