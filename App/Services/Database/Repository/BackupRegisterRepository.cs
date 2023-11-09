using App.Services.Database.Models;

namespace App.Services.Database.Repository;

internal class BackupRegisterRepository
{
    private readonly ulong _authorId;
    private readonly ulong _channelId;
    private readonly DateTime _startDate;
    private ulong _startMessage;
    private BackupRegister? _backupRegister;
    private bool _firstUpdate = true;
    private readonly ConsoleLogger _log = new(nameof(BackupRegisterRepository));

    public BackupRegisterRepository(DateTime startDate, ulong authorId, ulong channelId)
    {
        _startDate = startDate;
        _authorId = authorId;
        _channelId = channelId;
    }

    public void UpdateOnDatabase(ulong lastMessageId) //update inserting first and new last message
    {
        var context = DbConnection.GetConnection();

        var x = context.BackupRegisters.First().Date;

        _backupRegister = context.BackupRegisters.SingleOrDefault(b => b.Date == _startDate);
        if (_backupRegister == null)
            throw new InvalidOperationException("Backup register not found on database");

        if (_firstUpdate)
        {
            _backupRegister.StartMessageId = _startMessage;
            _firstUpdate = false;
        }

        _backupRegister.EndMessageId = lastMessageId;
        try
        {
            context.SaveChanges();
            _log.BackupAction($"Updated on database with signed end message id: '{lastMessageId}'");
        }
        catch (Exception ex)
        {
            _log.Exception("Failed to update on database", ex);
            throw;
        }
    }

    public void CreateOnDatabase()
    {
        var context = DbConnection.GetConnection();

        if (context.BackupRegisters.Any(br => br.Date == _startDate))
            throw new InvalidOperationException("Backup register already created on database");

        _backupRegister = new BackupRegister
        {
            Date = _startDate,
            AuthorId = _authorId,
            ChannelId = _channelId,
            StartMessageId = null,
            EndMessageId = null
        };

        try
        {
            context.BackupRegisters.Add(_backupRegister);
            context.SaveChanges();
            _log.BackupAction("New Backup Register created on database");
        }
        catch (Exception ex)
        {
            _log.Exception("Failed to create new entry on database", ex);
            throw;
        }
    }

    public void InsertStartMessage(ulong startMessageId)
    {
        if (_backupRegister is null)
            throw new InvalidOperationException("The backup register has not been created yet");
        if (_backupRegister.StartMessageId is not null)
            throw new InvalidOperationException("The start message is already defined");

        _startMessage = startMessageId;
        _log.BackupAction($"Start message id '{startMessageId}' inserted");
    }

    public static ulong GetOldestMessageId(ulong currentMessageId)
    {
        var context = DbConnection.GetConnection();

        ConsoleLogger.GenericBackupAction("Getting end message id", $"current message id: {currentMessageId}");
        var currentMessageBackupDate = context.Messages.Single(m => m.Id == currentMessageId).BackupDate;

        var existingBackup = context.BackupRegisters.Single(b => b.Date == currentMessageBackupDate);

        return existingBackup.EndMessageId ?? throw new InvalidOperationException("Invalid older backup found");
    }
}