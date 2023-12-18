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

    private readonly LoggingWrapper _loggingWrapper;
    private readonly BackupMessageService _backupService;
    private readonly ILogger<BackupInteractionModule> _logger;
    private readonly ResponseHandler _responseHandler;

    public BackupInteractionModule(BackupMessageService backupService, ResponseHandler responseHandler,
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
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackupCommand([Choice("ate-ultimo", 0)] [Choice("total", 1)] int choice)
    {
        await Context.Interaction.DeferAsync();
        
        var tm = await _backupService.TimeFromLastBackupAsync(Context);
        if (tm < TimeSpan.FromDays(1) && Program.RUN_WITH_COOLDOWN)
        {
            await _responseHandler.SendInvalidAttemptAsync(Context, tm);
            return;
        }

        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(MakeBackupCommand),
            choice.ToString());
        await _backupService.StartBackupAsync(Context, choice == 0);
    }

    [SlashCommand("deletar-proprio", "deletar todas as informações presentes no backup relacionadas ao usuario")]
    public async Task DeleteUserInBackupCommand()
    {
        _logger.LogCommandExecution(
            nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(DeleteUserInBackupCommand));
        await _responseHandler.SendDeleteConfirmationAsync(Context);
    }

    //DeleteUserInBackupCommand() Confirmation button interaction handlers
    [ComponentInteraction(CONFIRM_USER_DELETE_ID, true)] //TODO Can it be false?
    public async Task DeleteUserConfirm()
    {
        await _backupService.DeleteUserAsync(Context.User.Id);
        _logger.LogInformation(
            "{service}: {user} was deleted from the backup registry", nameof(BackupService), Context.User.Username);

        //TODO Put this in the approprite place
        await Context.Interaction.DeferAsync();
        await Context.Interaction.DeleteOriginalResponseAsync();
        await Context.Interaction.FollowupAsync($"***{Context.User.Username}** was deleted from the registry*");
    }

    [ComponentInteraction(CANCEL_USER_DELETE_ID, true)]
    public async Task DeleteUserCancel()
    {
        _logger.LogInformation("{service}: {user} aborted deletion", nameof(BackupService), Context.User.Username);
        await Context.Interaction.DeferAsync();
        await Context.Interaction.DeleteOriginalResponseAsync();
    }
}