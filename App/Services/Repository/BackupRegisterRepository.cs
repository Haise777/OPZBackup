using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository;

internal class BackupRegisterRepository
{
    private readonly ulong _authorId;
    private readonly ulong _channelId;
    private readonly DateTime _startDate;
    private BackupRegister? _backupRegister;

    public BackupRegisterRepository(DateTime startDate, ulong authorId, ulong channelId)
    {
        _startDate = startDate;
        _authorId = authorId;
        _channelId = channelId;
    }


    public void UpdateOnDatabase(ulong lastMessageId) //update inserting first and new last message
    {
        using var context = new MessageBackupContext();

        _backupRegister = context.BackupRegisters.SingleOrDefault(b => b.Date == _startDate);
        if (_backupRegister == null)
            throw new InvalidOperationException("Backup register not found on database");

        _backupRegister.OldestMessage = lastMessageId;
        context.SaveChanges();
    }

    public void CreateOnDatabase()
    {
        using var context = new MessageBackupContext();

        if (context.BackupRegisters.Any(br => br.Date == _startDate))
            throw new InvalidOperationException("Backup register already created on database");

        _backupRegister = new BackupRegister
        {
            Date = _startDate,
            Author = _authorId,
            ChannelId = _channelId,
            YoungestMessage = null,
            OldestMessage = null
        };

        context.BackupRegisters.Add(_backupRegister);
        context.SaveChanges();
    }


    public void InsertStartMessage(ulong startMessageId)
    {
        if (_backupRegister is null)
            throw new InvalidOperationException("The backup register has not been created yet");
        if (_backupRegister.YoungestMessage is not null)
            throw new InvalidOperationException("The start message is already defined");

        _backupRegister.YoungestMessage = startMessageId;
    }
}