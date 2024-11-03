using Discord.Interactions;
using OPZBackup.Services;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BackupService _backupService;

    public BackupModule(BackupService backupService)
    {
        _backupService = backupService;
    }
    
    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackup([Choice("ate-ultimo", 0)] [Choice("ate-inicio", 1)] int choice)
    {
        //LogCommandExecution
        
        //Checagem de Admin
        //Checagem de Ja ter backup em progresso
        
        //Tempo desde o ultimo backup (se com cooldowns)
        
        //TODO Start backup
        await _backupService.StartBackupAsync(Context, choice);
        
        throw new NotImplementedException();
    }

    [SlashCommand("cancelar", "Cancela o processo de backup atual")]
    public async Task CancelBackupProcess()
    {
        //LogCommandExecution
        
        //Checagem de Admin
        //Checagem de Ja ter backup em progresso
        
        //TODO Cancel backup
        await _backupService.CancelAsync(Context.Channel);
    }

    [SlashCommand("deletar-proprio",
        "DELETAR todas as informações presentes no backup relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInBackup()
    {
        throw new NotImplementedException();
    }
    
}