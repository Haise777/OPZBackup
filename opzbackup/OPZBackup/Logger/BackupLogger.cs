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

        var c = StringStyle.Foreground[63];
        var r = StringStyle.Reset;
        var serviceName = $"{c}Backup{r}"; //TODO
        
        Log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Async(f => f.File(_filePath,
                outputTemplate:"[{Timestamp:HH:mm:ss} {Level:u3}]{NewLine} - {Message}{NewLine}{Exception}"))
            .WriteTo.Console(outputTemplate:$"[{{Timestamp:HH:mm:ss}} {{Level:u3}} {serviceName}] {{Message}}{{NewLine}}{{Exception}}")
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