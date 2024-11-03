using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Services;
using BackupService = OPZBackup.Services.BackupService;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BackupService _backupService;
    private readonly BackupResponseHandler _backupResponseHandler;
    private static BackupService? _currentBackup;
    private static readonly SemaphoreSlim CommandLock = new(1, 1);

    public BackupModule(BackupService backupService, BackupResponseHandler backupResponseHandler)
    {
        _backupService = backupService;
        _backupResponseHandler = backupResponseHandler;
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackup([Choice("ate-ultimo", 0)] [Choice("ate-inicio", 1)] int choice)
    {
        //TODO LogCommandExecution

        if (!await CheckForAdminRole())
        {
            await _backupResponseHandler.SendForbiddenAsync(Context);
            return;
        }

        if (CommandLock.CurrentCount < 1)
        {
            await _backupResponseHandler.SendAlreadyInProgressAsync(Context);
            return;
        }

        //TODO Tempo desde o ultimo backup (se com cooldowns)

        await CommandLock.WaitAsync();
        try
        {
            _currentBackup = _backupService;
            await _currentBackup.StartBackupAsync(Context, choice == 0);
        }
        finally
        {
            _currentBackup = null;
            CommandLock.Release();
        }
    }

    [SlashCommand("cancelar", "Cancela o processo de backup atual")]
    public async Task CancelBackupProcess()
    {
        //LogCommandExecution

        if (!await CheckForAdminRole())
        {
            await _backupResponseHandler.SendForbiddenAsync(Context);
            return;
        }

        if (!(CommandLock.CurrentCount < 1) || _currentBackup == null)
        {
            await _backupResponseHandler.SendNoBackupInProgressAsync(Context);
            return;
        }

        _currentBackup.Cancel();
    }

    [SlashCommand("deletar-proprio",
        "DELETAR todas as informações presentes no backup relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInBackup()
    {
        throw new NotImplementedException();
    }

    private async Task<bool> CheckForAdminRole()
    {
        var user = Context.User as SocketGuildUser;

        if (user!.Roles.Any(x => x.Id == AppInfo.MainAdminRoleId))
            return true;

        return false;
    }
}