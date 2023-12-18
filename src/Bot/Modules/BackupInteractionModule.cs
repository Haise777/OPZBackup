using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;
using OPZBot.Services.MessageBackup;

namespace OPZBot.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
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

    [ComponentInteraction("DELETE_CONF_CONFIRM", true)]
    public async Task DeleteUserConfirm()
    {
        await _backupService.DeleteUserAsync(Context.User.Id);

        _logger.LogInformation(
            "{service}: {user} was deleted from the backup registry", nameof(BackupService), Context.User.Username);
    }

    [ComponentInteraction("DELETE_CONF_CANCEL", true)]
    public async Task DeleteUserCancel()
    {
        await Context.Interaction.DeleteOriginalResponseAsync();
    }
}