using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;
using OPZBot.Services.MessageBackup;

namespace OPZBot.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly MessageBackupService _backupService;
    private readonly BackupResponseHandler _responseHandler;
    private readonly ILogger<BackupInteractionModule> _logger;
    private readonly BackupLogging _backupLogging;

    public BackupInteractionModule(MessageBackupService backupService, BackupResponseHandler responseHandler,
        ILogger<BackupInteractionModule> logger, BackupLogging backupLogging)
    {
        _responseHandler = responseHandler;
        _backupService = backupService;
        _logger = logger;
        _backupLogging = backupLogging;


        _backupService.StartedBackupProcess += _responseHandler.SendStartNotification;
        _backupService.StartedBackupProcess += _backupLogging.LogBackupStart;
        
        _backupService.FinishedBatch += _responseHandler.SendBatchFinished;
        _backupService.FinishedBatch += _backupLogging.LogBatchFinished;
        
        _backupService.CompletedBackupProcess += _responseHandler.SendBackupCompleted;
        _backupService.CompletedBackupProcess += _backupLogging.LogBackupCompleted;
        
        _backupService.ProcessHasFailed += _responseHandler.SendBackupFailed;
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackupCommand([Choice("ate-ultimo", 0)] [Choice("total", 1)] int choice)
    {
        try
        {
            _logger.LogCommandExecution(
                nameof(BackupService), Context.User.Username, Context.Channel.Name, nameof(MakeBackupCommand));

            await _backupService.StartBackupAsync(Context, choice < 1);
        }
        catch (Exception ex)
        {
            await _logger.RichLogErrorAsync(ex, "MakeBackupCommand");
            throw;
        }
    }

    [SlashCommand("deletar-proprio", "deletar todas as informações presentes no backup relacionadas ao usuario")]
    public async Task DeleteUserInBackupCommand()
    {
        throw new NotImplementedException();
    }
}