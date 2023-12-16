using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;
using OPZBot.Services.MessageBackup;

namespace OPZBot.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LoggingWrapper _loggingWrapper;
    private readonly BackupMessageService _service;
    private readonly ILogger<BackupInteractionModule> _logger;
    private readonly ResponseHandler _responseHandler;

    public BackupInteractionModule(BackupMessageService service, ResponseHandler responseHandler,
        ILogger<BackupInteractionModule> logger, LoggingWrapper loggingWrapper)
    {
        _responseHandler = responseHandler;
        _service = service;
        _logger = logger;
        _loggingWrapper = loggingWrapper;

        _service.StartedBackupProcess += _responseHandler.SendStartNotificationAsync;
        _service.StartedBackupProcess += _loggingWrapper.LogStart;
        _service.FinishedBatch += _responseHandler.SendBatchFinishedAsync;
        _service.FinishedBatch += _loggingWrapper.LogBatchFinished;
        _service.CompletedBackupProcess += _responseHandler.SendCompletedAsync;
        _service.CompletedBackupProcess += _loggingWrapper.LogCompleted;
        _service.ProcessHasFailed += _responseHandler.SendFailedAsync;
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackupCommand([Choice("ate-ultimo", 0)] [Choice("total", 1)] int choice)
    {
        try
        {
            _logger.LogCommandExecution(
                nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(MakeBackupCommand), choice.ToString());
            await _service.StartBackupAsync(Context, choice == 0);
        }
        catch (Exception ex)
        {
            await _logger.RichLogErrorAsync(ex, nameof(MakeBackupCommand));
            throw;
        }
    }

    [SlashCommand("deletar-proprio", "deletar todas as informações presentes no backup relacionadas ao usuario")]
    public async Task DeleteUserInBackupCommand()
    {
        throw new NotImplementedException();
    }
}