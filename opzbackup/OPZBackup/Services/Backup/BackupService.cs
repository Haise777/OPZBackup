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
    private readonly ModuleResponseHandler _responseHandler;
    private readonly ServiceResponseHandlerFactory _responseHandlerFactory;
    private readonly MyDbContext _dbContext;
    private readonly ILogger _logger;

    public BackupService(BackupProcess backupProcess, ILogger logger, ModuleResponseHandler responseHandler,
        ServiceResponseHandlerFactory responseHandlerFactory, MyDbContext dbContext)
    {
        _backupProcess = backupProcess;
        _responseHandler = responseHandler;
        _responseHandlerFactory = responseHandlerFactory;
        _dbContext = dbContext;

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

        //TODO: change this cooldown tracking
        if (App.RunWithCooldowns)
        {
            var isInCooldown = await CheckIfInCooldown(context.Channel);
            if (isInCooldown.isInCooldown)
            {
                _logger.Information("Backup in this channel is on cooldown.");
                await _responseHandler.SendInvalidAttemptAsync(context, isInCooldown.timeDifference);
                return;
            }
        }

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

    private record CoolddownCheckResult(bool isInCooldown, TimeSpan timeDifference);

    private async Task<CoolddownCheckResult> CheckIfInCooldown(ISocketMessageChannel channel)
    {
        var channelId = channel.Id;
        var lastBackupInThisChannel = await _dbContext.BackupRegistries //TODO: Make a better query for this
            .OrderByDescending(x => x.Date)
            .FirstOrDefaultAsync(x => x.ChannelId == channelId);

        if (lastBackupInThisChannel == null)
            return new CoolddownCheckResult(false, TimeSpan.Zero);

        var timeDifference = DateTime.Now.Subtract(lastBackupInThisChannel.Date);
        var isLongerThanADay = timeDifference.Duration() > TimeSpan.FromDays(1);

        return new CoolddownCheckResult(!isLongerThanADay, timeDifference);
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