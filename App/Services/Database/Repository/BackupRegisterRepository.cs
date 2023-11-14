using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository;

internal class BackupRegisterRepository
{
    private readonly ConsoleLogger _log = new(nameof(BackupRegisterRepository));
    private readonly DbConnection _connection;

    public BackupRegisterRepository(DbConnection dbConnection)
    {
        _connection = dbConnection;
    }

    public void UpdateOnDatabase(BackupRegister backupRegisterToAdd) //update inserting first and new last message
    {
        var context = _connection.GetConnection();
        var backupRegister = context.BackupRegisters.SingleOrDefault(b => b.Date == backupRegisterToAdd.Date)
            ?? throw new InvalidOperationException("Backup register not found on database");

        backupRegister.EndMessageId = backupRegisterToAdd.EndMessageId; //TODO: Is this really needed?

        context.SaveChanges();
        _log.BackupAction($"Updated on database with signed end message id: '{backupRegister.EndMessageId}'");
    }

    public void CreateOnDatabase(BackupRegister backupRegister)
    {
        var context = _connection.GetConnection();
        if (context.BackupRegisters.Any(br => br.Date == backupRegister.Date))
            throw new InvalidOperationException("Backup register already created on database");

        context.BackupRegisters.Add(backupRegister);
        context.SaveChanges();
        _log.BackupAction("New Backup Register created on database");
    }
}