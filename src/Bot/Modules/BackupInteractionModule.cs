using Discord.Interactions;
using OPZBot.Services.MessageBackup;

namespace OPZBot.Modules;

[Group("backup","utilizar a função de backup")]
public class BackupInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BackupService _backupService;

    public BackupInteractionModule(BackupService backupService)
    {
        _backupService = backupService;
        
    }
    
    [SlashCommand("fazer", "efetua backup deste canal")]
    public async Task MakeBackupCommand([Choice("ate-ultimo", 0), Choice("total", 1)] int choice)
    {
        await _backupService.StartBackupAsync(Context, choice < 1);
    }

    [SlashCommand("deletar-proprio", "deleta todas as informações presentes no backup relacionadas ao usuario")]
    public async Task DeleteUserInBackupCommand()
    {
        throw new NotImplementedException();
    }
}