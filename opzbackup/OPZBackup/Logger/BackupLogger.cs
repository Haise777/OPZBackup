using System.Collections.Immutable;
using OPZBackup.Extensions;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;
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

    public void BatchFinished(Timer timer, int batchNumber)
    {
        Log.Information("Batch '{n}' finished in {elapsed} | {mean}",
            batchNumber, timer.Elapsed.Formatted(), timer.Mean.Formatted());
        StatisticLogger.Information("Batch '{n}' finished in: {seconds} / avg: {mean} \n\n",
            batchNumber, timer.Elapsed.TotalSeconds, timer.Mean.TotalSeconds);
    }

    public void FilesDownloaded(Timer timer)
    {
        Log.Information("Download finished in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Downloading attachments took: {seconds} / avg: {mean}",
            timer.Elapsed.TotalSeconds, timer.Mean.TotalSeconds);
    }

    public void BatchSaved(Timer timer)
    {
        Log.Verbose("Batch saved in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Completing batch took: {seconds} / avg: {mean}",
            timer.Elapsed.TotalSeconds, timer.Mean.TotalSeconds);
    }

    public void MessagesSaved(Timer timer)
    {
        Log.Verbose("Messages saved in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Saving messages took: {seconds} / avg: {mean}",
            timer.Elapsed.TotalSeconds, timer.Mean.TotalSeconds);
    }

    public void MessagesProcessed(Timer timer)
    {
        Log.Verbose("Processed messages in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Processing messages took: {seconds} / avg: {mean}",
            timer.Elapsed.TotalSeconds, timer.Mean.TotalSeconds);
    }

    public void MessagesFetched(Timer timer)
    {
        Log.Verbose("Fetched messages in {seconds} | {mean}", timer.Elapsed.Formatted(),
            timer.Mean.Formatted());
        StatisticLogger.Information("Fetching messages took: {seconds} / avg: {mean}",
            timer.Elapsed.TotalSeconds, timer.Mean.TotalSeconds);
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

    public void BackupFinished(TimeValue batchTimer, ImmutableDictionary<string, TimeValue> performanceTimers)
    {
        Log.Information("Backup {id} finished in {time}\n" +
                        " | Occupying {compressedTotal} in saved attachments",
            _context.BackupRegistry.Id,
            batchTimer.Total.Formatted(),
            _context.StatisticTracker.CompressedFilesSize.ToFormattedString()
        );

        LogStatisticalPerformance(batchTimer, performanceTimers);
    }

    private void LogStatisticalPerformance(TimeValue batchTimer, ImmutableDictionary<string, TimeValue> performanceTimers)
    {
        var totalStatistics = _context.StatisticTracker.GetTotalStatistics();
        var fetchPerformance = performanceTimers[BatchManager.FetchTimerId];
        var processPerformance = performanceTimers[BatchManager.ProcessTimerId];
        var savePerformance = performanceTimers[BatchManager.SaveMessagesId];
        var downloadPerformance = performanceTimers[BatchManager.DownloadTimerId];

        StatisticLogger.Information(
            """


            ---- Backup process finished

            Channel: {channelId}
            Total time: {totalTime}
            Messages: {messagesCount}
            Files: {fileCount} | {fileSize} bytes

            --- Batch performance
            N of batches: {batchNumber}
            Total time: {batchTime} (s)
            Mean time: {batchMean} (s)

            --- Fetch performance
            N of messages per fetch: {fetchCount}
            Total time: {fetchTime} (s)
            Mean time: {fetchMean} (s)

            --- Process performance
            Total time: {processTime} (s)
            Mean time: {processMean} (s)

            --- Message saving performance
            Total time: {savingTime} (s)
            Mean time: {savingMean} (s)

            --- Download performance
            N max allowed downloads in parallel: {maxParallelDownloads}
            Total downloaded: {fileCount}
            Total bytes: {fileSize} bytes
            Total time: {downloadTime} (s)
            Mean time: {downloadMean} (s)

            """,
            _context.BackupRegistry.ChannelId,
            batchTimer.Total.Formatted(),
            _context.MessageCount,
            _context.FileCount, totalStatistics.ByteSize,
            _context.BatchNumber,
            batchTimer.Total.TotalSeconds,
            batchTimer.Mean.TotalSeconds,
            App.MaxMessagesPerBatch,
            fetchPerformance.Total.TotalSeconds,
            fetchPerformance.Mean.TotalSeconds,
            processPerformance.Total.TotalSeconds,
            processPerformance.Mean.TotalSeconds,
            savePerformance.Total.TotalSeconds,
            savePerformance.Mean.TotalSeconds,
            50,
            _context.FileCount, totalStatistics.ByteSize,
            downloadPerformance.Total.TotalSeconds,
            downloadPerformance.Mean.TotalSeconds);
    }
}