using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers;
using BackupService = OPZBackup.Services.BackupService;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BackupService _backupService;
    private readonly ILogger<BackupModule> _logger;
    private readonly BackupResponseHandlerFactory _responseHandlerFactory;
    
    private BackupResponseHandler _responseHandler;
    private static BackupService? _currentBackup;
    private static readonly SemaphoreSlim CommandLock = new(1, 1);

    public BackupModule(BackupService backupService, ILogger<BackupModule> logger,
        BackupResponseHandlerFactory responseHandlerFactory)
    {
        _backupService = backupService;
        _logger = logger;
        _responseHandlerFactory = responseHandlerFactory;
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackup([Choice("ate-ultimo", 0)] [Choice("ate-inicio", 1)] int choice)
    {
        await Context.Interaction.DeferAsync();
        _responseHandler = _responseHandlerFactory.Create(Context);
        _logger.LogCommandExecution(Context, nameof(MakeBackup), choice.ToString());

        if (!IsInAdminRole())
        {
            await ForbiddenAsync(nameof(MakeBackup));
            return;
        }

        if (CommandLock.CurrentCount < 1)
        {
            _logger.LogInformation("There is already a backup in progress.");
            await _responseHandler.SendAlreadyInProgressAsync();
            return;
        }

        //TODO Tempo desde o ultimo backup (se com cooldowns)

        await CommandLock.WaitAsync();
        try
        {
            _currentBackup = _backupService;
            await _currentBackup.StartBackupAsync(Context, _responseHandler, choice == 0);
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
        await Context.Interaction.DeferAsync();
        _responseHandler = _responseHandlerFactory.Create(Context);
        _logger.LogCommandExecution(Context, nameof(CancelBackupProcess));

        if (!IsInAdminRole())
        {
            await ForbiddenAsync(nameof(CancelBackupProcess));
            return;
        }

        if (!(CommandLock.CurrentCount < 1) || _currentBackup == null)
        {
            _logger.LogInformation("There is no backup in progress.");
            await _responseHandler.SendNoBackupInProgressAsync();
            return;
        }

        _currentBackup.Cancel();
    }

    [SlashCommand("deletar-proprio",
        "DELETAR todas as informações presentes no armazenamento relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInStorage()
    {
        //TODO-3 Implement DeleteUserInStorage
        throw new NotImplementedException();
    }

    private async Task ForbiddenAsync(string commandName)
    {
        _logger.LogInformation(
            "{User} does not have permission to execute {command}", Context.User.Username, commandName);

        await _responseHandler.SendForbiddenAsync();
    }

    private bool IsInAdminRole()
    {
        var user = Context.User as SocketGuildUser;

        if (user!.Roles.Any(x => x.Id == AppInfo.MainAdminRoleId))
            return true;

        return false;
    }
}