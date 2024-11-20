using AnsiStyles;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers.Backup;
using BackupService = OPZBackup.Services.Backup.BackupService;
using ILogger = Serilog.ILogger;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupModule : InteractionModuleBase<SocketInteractionContext>
{
    private static BackupService? _currentBackup;
    private static readonly SemaphoreSlim CommandLock = new(1, 1);
    private readonly BackupService _backupService;
    private readonly ILogger _logger;
    private readonly ServiceResponseHandlerFactory _responseHandlerFactory;

    private readonly ModuleResponseHandler _responseHandler;

    public BackupModule(
        BackupService backupService,
        ILogger logger,
        ModuleResponseHandler responseHandler,
        ServiceResponseHandlerFactory responseHandlerFactory)
    {
        _responseHandler = responseHandler;
        _backupService = backupService;
        _responseHandlerFactory = responseHandlerFactory;
        
        _logger = logger.ForContext("System", LoggerUtils.ColorText("BackupModule", 12));
    }

    [SlashCommand("fazer", "efetuar backup deste canal")]
    public async Task MakeBackup([Choice("ate-ultimo", 0)] [Choice("ate-inicio", 1)] int choice)
    {
        await Context.Interaction.DeferAsync();
        //_logger.LogCommandExecution(Context, nameof(MakeBackup), choice.ToString());
        _logger.Information("Fazer");
        if (!IsInAdminRole())
        {
            await ForbiddenAsync(nameof(MakeBackup));
            return;
        }

        if (CommandLock.CurrentCount < 1)
        {
            // _logger.LogInformation("There is already a backup in progress.");
            _logger.Information("Fazer");
            await _responseHandler.SendAlreadyInProgressAsync(Context);
            return;
        }

        //TODO-2 Tempo desde o ultimo backup (se com cooldowns)

        await CommandLock.WaitAsync();
        try
        {
            _currentBackup = _backupService;
            var serviceResponseHandler = _responseHandlerFactory.Create(Context);
            await _currentBackup.StartBackupAsync(Context, serviceResponseHandler, choice == 0);
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
        //_logger.LogCommandExecution(Context, nameof(CancelBackupProcess));

        if (!IsInAdminRole())
        {
            await ForbiddenAsync(nameof(CancelBackupProcess));
            return;
        }

        if (!(CommandLock.CurrentCount < 1) || _currentBackup == null)
        {
            // _logger.LogInformation("There is no backup in progress.");
            await _responseHandler.SendNoBackupInProgressAsync(Context);
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
        // _logger.LogInformation(
        //     "{User} does not have permission to execute {command}", Context.User.Username, commandName);

        await _responseHandler.SendForbiddenAsync(Context);
    }

    private bool IsInAdminRole()
    {
        var user = Context.User as SocketGuildUser;

        if (user!.Roles.Any(x => x.Id == App.MainAdminRoleId))
            return true;

        return false;
    }
}