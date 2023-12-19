// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;
using OPZBot.Services.MessageBackup;

// ReSharper disable UnusedMember.Global

namespace OPZBot.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    public const string CONFIRM_USER_DELETE_ID = "DLT_CONF_CONFIRM";
    public const string CANCEL_USER_DELETE_ID = "DLT_CONF_CANCEL";
    private readonly IBackupMessageService _backupService;
    private readonly ILogger<BackupInteractionModule> _logger;
    private static readonly SemaphoreSlim Lock = new(1,1);

    private readonly IResponseHandler _responseHandler;
    private readonly LoggingWrapper _loggingWrapper;

    public BackupInteractionModule(IBackupMessageService backupService, IResponseHandler responseHandler,
        ILogger<BackupInteractionModule> logger, LoggingWrapper loggingWrapper)
    {
        _responseHandler = responseHandler;
        _backupService = backupService;
        _logger = logger;
        _loggingWrapper = loggingWrapper;

        _backupService.StartedBackupProcess += _responseHandler.SendStartNotificationAsync;
        _backupService.StartedBackupProcess += _loggingWrapper.LogStart;
        _backupService.FinishedBatch += _responseHandler.SendBatchFinishedAsync;
        _backupService.FinishedBatch += _loggingWrapper.LogBatchFinished;
        _backupService.CompletedBackupProcess += _responseHandler.SendCompletedAsync;
        _backupService.CompletedBackupProcess += _loggingWrapper.LogCompleted;
        _backupService.ProcessHasFailed += _responseHandler.SendFailedAsync;
        _backupService.EmptyBackupAttempt += _responseHandler.SendEmptyBackupAsync;
        _backupService.EmptyBackupAttempt += _loggingWrapper.LogEmptyBackupAttempt;
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackupCommand([Choice("ate-ultimo", 0)] [Choice("total", 1)] int choice)
    {
        if (Lock.CurrentCount < 1)
        {
            await Context.Interaction.RespondAsync("*Por limitações do Discord, não é possivel efetuar mais de um processo de backup simutaneamente*");
            await Task.Delay(7000);
            await Context.Interaction.DeleteOriginalResponseAsync();
            return;
        }
        await Lock.WaitAsync();
        try
        {
            await Context.Interaction.DeferAsync();

            var tm = await _backupService.TimeFromLastBackupAsync(Context);
            if (tm < TimeSpan.FromDays(1) && Program.RunWithCooldowns)
            {
                await _responseHandler.SendInvalidAttemptAsync(Context, tm);
                return;
            }

            _logger.LogCommandExecution(
                nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(MakeBackupCommand),
                choice.ToString());
            await _backupService.StartBackupAsync(Context, choice == 0);
        }
        finally
        {
            Lock.Release();
        }
        
    }

    [SlashCommand("deletar-proprio", "DELETAR todas as informações presentes no backup relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInBackupCommand()
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(DeleteUserInBackupCommand));
        await _responseHandler.SendDeleteConfirmationAsync(Context);
    }

    //DeleteUserInBackupCommand() Confirmation button interaction handlers
    [ComponentInteraction(CONFIRM_USER_DELETE_ID, true)]
    public async Task DeleteUserConfirm()
    {
        await _backupService.DeleteUserAsync(Context.User.Id);
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
}