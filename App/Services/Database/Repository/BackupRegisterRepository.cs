using Bot.Services.Database.Context;
using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository;

internal class BackupRegisterRepository
{
    private readonly ConsoleLogger _log = new(nameof(BackupRegisterRepository));
    private readonly MessageBackupContext _backupContext;

    public BackupRegisterRepository(DbConnection dbContext)
    {
        _backupContext = dbContext.GetConnection();
    }


    public void UpdateOnDatabase(BackupRegister backupRegisterToAdd) //update inserting first and new last message
    {
        var backupRegister = _backupContext.BackupRegisters.SingleOrDefault(b => b.Date == backupRegisterToAdd.Date)
            ?? throw new InvalidOperationException("Backup register not found on database");

        backupRegister.EndMessageId = backupRegisterToAdd.EndMessageId; //TODO: Is this really needed?

        try
        {
            _backupContext.SaveChanges();
            _log.BackupAction($"Updated on database with signed end message id: '{backupRegister.EndMessageId}'");
        }
        catch (Exception ex)
        {
            _log.Exception("Failed to update on database", ex);
        }
    }

    public void CreateOnDatabase(BackupRegister backupRegister)
    {
        if (_backupContext.BackupRegisters.Any(br => br.Date == backupRegister.Date))
            throw new InvalidOperationException("Backup register already created on database");

        try
        {
            _backupContext.BackupRegisters.Add(backupRegister);
            _backupContext.SaveChanges();
            _log.BackupAction("New Backup Register created on database");
        }
        catch (Exception ex)
        {
            _log.Exception("Failed to create new entry on database", ex);
        }
    }
}