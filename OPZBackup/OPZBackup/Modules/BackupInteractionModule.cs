// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OPZBackup.Services.MessageBackup;
using OPZBackup.Logging;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    public const string CONFIRM_USER_DELETE_ID = "DLT_CONF_CONFIRM";
    public const string CANCEL_USER_DELETE_ID = "DLT_CONF_CANCEL";
    private static IMessageBackupService? _currentBackupService;

    private static readonly SemaphoreSlim LockPreCommand = new(1, 1);
    private static readonly SemaphoreSlim Lock = new(1, 1);
    private readonly ILogger<BackupInteractionModule> _logger;
    private readonly LoggingWrapper _loggingWrapper;

    private readonly IMessageBackupService _messageBackupService;
    private readonly IBackupResponseHandler _responseHandler;

    public BackupInteractionModule(IMessageBackupService messageBackupService, IBackupResponseHandler responseHandler,
        ILogger<BackupInteractionModule> logger, LoggingWrapper loggingWrapper)
    {
        _responseHandler = responseHandler;
        _messageBackupService = messageBackupService;
        _logger = logger;
        _loggingWrapper = loggingWrapper;

        SubscribeEvents();
    }

    public static bool IsBackupInProgress => Lock.CurrentCount < 1;

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackup([Choice("ate-ultimo", 0)] [Choice("total", 1)] int choice)
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(MakeBackup), choice.ToString());
        await DeferAsync();

        if (!await CheckForAdminRole()) return;
        if (await CheckIfBackupInProcess()) return;
        try
        {
            _currentBackupService = _messageBackupService;

            var tm = await _messageBackupService.TimeFromLastBackupAsync(Context);
            if (tm > TimeSpan.FromHours(0) && oldProgram.RunWithCooldowns)
            {
                _logger.LogInformation(
                    "{service}: Backup is still in cooldown for this channel", nameof(BackupService));
                await _responseHandler.SendInvalidAttemptAsync(Context, tm);
                return;
            }

            await _messageBackupService.StartBackupAsync(Context, choice == 0);
        }
        finally
        {
            _currentBackupService = null;
            Lock.Release();
        }
    }

    [SlashCommand("cancelar", "Cancela o processo de backup atual")]
    public async Task CancelBackupProcess()
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(CancelBackupProcess));
        await DeferAsync();

        if (!await CheckForAdminRole()) return;
        if (_currentBackupService is null)
        {
            await _responseHandler.SendProcessToCancelAsync(Context, true);
            return;
        }

        await _currentBackupService.CancelSource.CancelAsync();
        await _responseHandler.SendProcessToCancelAsync(Context);
    }

    [SlashCommand("deletar-proprio",
        "DELETAR todas as informações presentes no backup relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInBackup()
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(DeleteUserInBackup));
        
        await _responseHandler.SendDeleteConfirmationAsync(Context);
    }

    //DeleteUserInBackupCommand() Confirmation button interaction handlers
    [ComponentInteraction(CONFIRM_USER_DELETE_ID, true)]
    public async Task DeleteUserConfirm()
    {
        await _messageBackupService.DeleteUserAsync(Context);
        _logger.LogInformation(
            "{service}: {user} was deleted from the backup registry", nameof(BackupService), Context.User.Username);

        await _responseHandler.SendUserDeletionResultAsync(Context, true);
    }

    [ComponentInteraction(CANCEL_USER_DELETE_ID, true)]
    public async Task DeleteUserCancel()
    {
        _logger.LogInformation("{service}: {user} aborted deletion", nameof(BackupService), Context.User.Username);
        await _responseHandler.SendUserDeletionResultAsync(Context, false);
    }

    private async Task<bool> CheckIfBackupInProcess()
    {
        await LockPreCommand.WaitAsync();
        try
        {
            if (IsBackupInProgress)
            {
                await _responseHandler.SendAlreadyInProgressAsync(Context);
                return true;
            }

            await Lock.WaitAsync();
            return false;
        }
        finally
        {
            LockPreCommand.Release();
        }
    }

    private async Task<bool> CheckForAdminRole()
    {
        var user = Context.User as SocketGuildUser;

        if (user!.Roles.Any(x => x.Id == oldProgram.MainAdminRoleId)) return true;

        await _responseHandler.SendNotRightPermissionAsync(Context);
        return false;
    }

    private void SubscribeEvents()
    {
        _messageBackupService.StartedBackupProcess += _responseHandler.SendStartNotificationAsync;
        _messageBackupService.StartedBackupProcess += _loggingWrapper.LogStart;
        _messageBackupService.FinishedBatch += _responseHandler.SendBatchFinishedAsync;
        _messageBackupService.FinishedBatch += _loggingWrapper.LogBatchFinished;
        _messageBackupService.CompletedBackupProcess += _responseHandler.SendCompletedAsync;
        _messageBackupService.CompletedBackupProcess += _loggingWrapper.LogCompleted;
        _messageBackupService.ProcessFailed += _responseHandler.SendFailedAsync;
        _messageBackupService.EmptyBackupAttempt += _responseHandler.SendEmptyMessageBackupAsync;
        _messageBackupService.EmptyBackupAttempt += _loggingWrapper.LogEmptyMessageBackupAttempt;
        _messageBackupService.ProcessCanceled += _responseHandler.SendProcessCancelledAsync;
        _messageBackupService.ProcessCanceled += _loggingWrapper.LogBackupCancelled;
    }
}