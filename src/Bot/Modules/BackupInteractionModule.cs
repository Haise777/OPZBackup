using Discord.Interactions;
using OPZBot.Services.MessageBackup;

namespace OPZBot.Modules;

[Group("backup","utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly MessageBackupService _backupService;
    private readonly BackupResponseHandler _responseHandler;

    public BackupInteractionModule(MessageBackupService backupService, BackupResponseHandler responseHandler)
    {
        _responseHandler = responseHandler;
        _backupService = backupService;

        _backupService.StartedBackupProcess += _responseHandler.SendStartNotification;
        _backupService.FinishedBatch += _responseHandler.SendBatchFinished;
        _backupService.CompletedBackupProcess += _responseHandler.SendBackupCompleted;
        _backupService.ProcessHasFailed += _responseHandler.SendBackupFailed;
    }
    
    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackupCommand([Choice("ate-ultimo", 0), Choice("total", 1)] int choice)
    {
        await _backupService.StartBackupAsync(Context, choice < 1);
    }

    [SlashCommand("deletar-proprio", "deletar todas as informações presentes no backup relacionadas ao usuario")]
    public async Task DeleteUserInBackupCommand()
    {
        throw new NotImplementedException();
    }
}