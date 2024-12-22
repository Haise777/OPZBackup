using OPZBackup.Extensions;
using Serilog;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.Logger;

public class BackupLogger : IAsyncDisposable
{
    private static bool _first = true;
    private readonly string _filePath;

    public BackupLogger()
    {
        var date = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss");
        _filePath = $"{LoggerConfig.LogFilePath}/backup/backup_{date}.txt";

        var newLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Async(f => f.File(_filePath,
                outputTemplate: OutputTemplate.DefaultTemplateSplitted("Backup")))
            .WriteTo.Console(outputTemplate: OutputTemplate.DefaultTemplate("Backup"))
            .CreateLogger();

        Log = (Serilog.Core.Logger)newLogger.ForContext("Backup", LoggerUtils.ColorText("Backup", 63));

        if (_first)
        {
            _first = false;
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
    
    public Serilog.Core.Logger Log { get; set; }

    

        
    public void BatchFinished(Timer timer, int batchNumber)
    {
        Log.Information("Batch '{n}' finished in {elapsed} | {mean}",
            batchNumber, timer.Elapsed.Formatted(), timer.Mean.Formatted());
    }
    
    public void FilesDownloaded(Timer timer)
    {
        Log.Information("Download finished in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
    }
    
    public void BatchSaved(Timer timer)
    {
        Log.Information("Batch saved in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
    }

    public async ValueTask DisposeAsync()
    {
        await Log.DisposeAsync();
        var fileInfo = new FileInfo(_filePath);
        if (fileInfo.Length == 0)
        {
            //TODO: File.Delete(_filePath);
        }

        GC.SuppressFinalize(this);
    }
}