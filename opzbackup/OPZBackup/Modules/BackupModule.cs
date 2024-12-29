using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Logger;
using OPZBackup.ResponseHandlers.Backup;
using BackupProcess = OPZBackup.Services.Backup.BackupProcess;
using ILogger = Serilog.ILogger;

namespace OPZBackup.Modules;

[Group("backup", "utilizar a função de backup")]
public class BackupModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger _logger;

    private readonly ModuleResponseHandler _responseHandler;
    private readonly BackupService _backupService;
    
    #region constructor

    public BackupModule(
        BackupService backupService,
        ILogger logger,
        ModuleResponseHandler responseHandler)
    {
        _responseHandler = responseHandler;
        _backupService = backupService;
        _logger = logger.ForContext("System", LoggerUtils.ColorText("BackupModule", 12));
    }

    #endregion

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

        await _backupService.ExecuteBackupAsync(Context, choice);
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
        
        await _backupService.CancelAsync(Context);
    }

    [SlashCommand("deletar-proprio",
        "DELETAR todas as informações presentes no armazenamento relacionadas ao usuario PERMANENTEMENTE")]
    public async Task DeleteUserInStorage()
    {
        //TODO: Implement DeleteUserInStorage
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