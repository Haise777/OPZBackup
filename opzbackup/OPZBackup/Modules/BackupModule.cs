using Discord.Interactions;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackup([Choice("ate-ultimo", 0)] [Choice("total", 1)] int choice)
    {
        throw new NotImplementedException();
    }

    [SlashCommand("cancelar", "Cancela o processo de backup atual")]
    public async Task CancelBackupProcess()
    {
        throw new NotImplementedException();
    }

    [SlashCommand("deletar-proprio",
        "DELETAR todas as informações presentes no backup relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInBackup()
    {
        throw new NotImplementedException();
    }
    
}