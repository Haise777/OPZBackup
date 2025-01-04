using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using OPZBackup.Data;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers.Backup;
using OPZBackup.Services.Backup;
using Serilog;

namespace OPZBackup.Services.Backup;

public class BackupService
{
    private static BackupProcess? _currentBackup;
    private static readonly SemaphoreSlim CommandLock = new(1, 1);
    private readonly BackupProcess _backupProcess;
    private readonly ILogger _logger;
    private readonly ModuleResponseHandler _responseHandler;
    private readonly ServiceResponseHandlerFactory _responseHandlerFactory;

    public BackupService(BackupProcess backupProcess, ILogger logger, ModuleResponseHandler responseHandler,
        ServiceResponseHandlerFactory responseHandlerFactory)
    {
        _backupProcess = backupProcess;
        _responseHandler = responseHandler;
        _responseHandlerFactory = responseHandlerFactory;

        _logger = logger.ForContext("System", LoggerUtils.ColorText("BackupService", 12));
    }

    public async Task ExecuteBackupAsync(SocketInteractionContext context, int choice)
    {
        if (CommandLock.CurrentCount < 1)
        {
            _logger.Information("There is already a backup in progress.");
            await _responseHandler.SendAlreadyInProgressAsync(context);
            return;
        }

        //TODO: Tempo desde o ultimo backup (se com cooldowns)

        await AttemptBackup(context, choice);
    }

    public async Task CancelAsync(SocketInteractionContext context)
    {
        if (!(CommandLock.CurrentCount < 1) || _currentBackup == null)
        {
            _logger.Information("There is no backup in progress.");
            await _responseHandler.SendNoBackupInProgressAsync(context);
            return;
        }

        await _currentBackup.CancelAsync();
    }

    private async Task AttemptBackup(SocketInteractionContext context, int choice)
    {
        await CommandLock.WaitAsync();
        try
        {
            _currentBackup = _backupProcess;
            var serviceResponseHandler = _responseHandlerFactory.Create(context);
            await _currentBackup.StartBackupAsync(context, serviceResponseHandler, choice == 0);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Backup attempt failed");
            throw;
        }
        finally
        {
            _currentBackup = null;
            CommandLock.Release();
        }
    }
}