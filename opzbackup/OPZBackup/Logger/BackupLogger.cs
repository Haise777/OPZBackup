using System.Collections.Immutable;
using OPZBackup.Extensions;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;
using Serilog;

namespace OPZBackup.Logger;

public class BackupLogger : IAsyncDisposable
{
    private readonly string _filePath;
    private readonly BackupContext _context;

    public BackupLogger(BackupContext context)
    {
        _context = context;

        var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var basePath = $"{LoggerConfig.LogFilePath}/backups/{_context.BackupRegistry.Id}_backup_{date}";
        var statisticPath = $"{basePath}/performance.txt";
        _filePath = $"{basePath}/log_history.txt";


        var newLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Async(f => f.File(_filePath,
                outputTemplate: OutputTemplate.DefaultTemplateSplitted("Backup")))
            .WriteTo.Console(outputTemplate: OutputTemplate.DefaultTemplate("Backup"))
            .CreateLogger();

        Log = (Serilog.Core.Logger)newLogger.ForContext("Backup",
            LoggerUtils.ColorText($"Backup [{_context.BackupRegistry.Id}]", 63));

        StatisticLogger = new LoggerConfiguration()
            .WriteTo.Async(f => f.File(statisticPath, outputTemplate: "{Message}{NewLine}"))
            .CreateLogger();
    }

    public async ValueTask DisposeAsync()
    {
        await Log.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public Serilog.Core.Logger Log { get; set; }
    public Serilog.Core.Logger StatisticLogger { get; set; }

    public void BatchFinished(int batchNumber)
    {
        Log.Information("Batch '{n}' finished",
            batchNumber);
        StatisticLogger.Information("Batch '{n}' finished \n\n",
            batchNumber);
    }

    public void FilesDownloaded()
    {
        Log.Information("Download finished");
        StatisticLogger.Information("Downloading attachments"
        );
    }

    public void BatchSaved()
    {
        Log.Verbose("Batch saved");
        StatisticLogger.Information("Completing batch"
        );
    }

    public void MessagesSaved()
    {
        Log.Verbose("Messages saved");
        StatisticLogger.Information("Saving messages"
        );
    }

    public void MessagesProcessed()
    {
        Log.Verbose("Processed messages");
        StatisticLogger.Information("Processing messages"
        );
    }

    public void MessagesFetched()
    {
        Log.Verbose("Fetched messages");
        StatisticLogger.Information("Fetching messages"
        );
    }

    public void BackupCancelled()
    {
        Log.Information("Backup was cancelled in Batch '{n}, with {messageCount} messages'",
            _context.BatchNumber, _context.MessageCount);
    }

    public void BackupFailed(Exception exception)
    {
        Log.Error(exception, "Backup failed at Batch '{n}' after {messageCount} messages",
            _context.BatchNumber, _context.MessageCount);
    }

    public void BackupFinished()
    {
        Log.Information("Backup {id} finished\n" +
                        " | Occupying {compressedTotal} in saved attachments",
            _context.BackupRegistry.Id,
            _context.StatisticTracker.CompressedFilesSize.ToFormattedString()
        );

        LogStatisticalPerformance();
    }

    //TODO: Retirar o BackupContext desta classe, fazer com q ela fique independente dele
    //Também se possível, mover a instancia deste logger para dentro da BackupContext.

    public void EmptyBackup()
    {
        Log.Information("Empty Backup attempt, backup was cancelled'");
    }

    private void LogStatisticalPerformance()
    {

        StatisticLogger.Information(
            """


                        ---- Backup process finished

            """);
    }
}