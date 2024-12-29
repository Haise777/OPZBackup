using OPZBackup.Extensions;
using OPZBackup.Services.Backup;
using Serilog;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.Logger;

public class BackupLogger : IAsyncDisposable
{
    private readonly string _filePath;
    private readonly BackupContext _context;

    public BackupLogger(BackupContext context)
    {
        _context = context;

        var date = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss");
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

    public void BatchFinished(Timer timer, int batchNumber)
    {
        Log.Information("Batch '{n}' finished in {elapsed} | {mean}",
            batchNumber, timer.Elapsed.Formatted(), timer.Mean.Formatted());
        StatisticLogger.Information("Batch '{n}' finished in: {seconds} / avg: {mean}",
            batchNumber, timer.Elapsed.Formatted(), timer.Mean.Formatted());
    }

    public void FilesDownloaded(Timer timer)
    {
        Log.Information("Download finished in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Downloading attachments took: {seconds} / avg: {mean}",
            timer.Elapsed.Formatted(), timer.Mean.Formatted());
    }

    public void BatchSaved(Timer timer)
    {
        Log.Information("Batch saved in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Completing batch took: {seconds} / avg: {mean}",
            timer.Elapsed.Formatted(), timer.Mean.Formatted());
    }

    public void MessagesSaved(Timer timer)
    {
        Log.Verbose("Messages saved in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Saving messages took: {seconds} / avg: {mean}",
            timer.Elapsed.Formatted(), timer.Mean.Formatted());
    }

    public void MessagesProcessed(Timer timer)
    {
        Log.Verbose("Processed messages in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Processing messages took: {seconds} / avg: {mean}",
            timer.Elapsed.Formatted(), timer.Mean.Formatted());
    }

    public void MessagesFetched(Timer timer)
    {
        Log.Verbose("Fetched messages in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Fetching messages took: {seconds} / avg: {mean}",
            timer.Elapsed.Formatted(), timer.Mean.Formatted());
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
}