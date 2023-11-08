using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository
{
    internal class BackupRegisterRepository
    {
        private DateTime _startDate;
        private ulong _authorId;
        private ulong? _startMessageId;
        private ulong _channelId;
        private BackupRegister? backup_register;

        public BackupRegisterRepository(DateTime startDate, ulong authorId, ulong channelId)
        {
            _startDate = startDate;
            _authorId = authorId;
            _channelId = channelId;
        }


        public void UpdateOnDatabase(ulong lastMessageId) //update inserting first and new last message
        {
            using var context = new MessageBackupContext();

            backup_register = context.BackupRegisters.SingleOrDefault(b => b.Date == _startDate);
            if (backup_register == null)
                throw new InvalidOperationException("Backup register not found on database");

            backup_register.OldestMessage = lastMessageId;
            context.SaveChanges();
        }

        public void CreateOnDatabase()
        {
            using var context = new MessageBackupContext();

            if (context.BackupRegisters.Any(br => br.Date == _startDate))
                throw new InvalidOperationException("Backup register already created on database");

            backup_register = new BackupRegister()
            {
                Date = _startDate,
                Author = _authorId,
                ChannelId = _channelId,
                YoungestMessage = null,
                OldestMessage = null
            };

            context.BackupRegisters.Add(backup_register);
            context.SaveChanges();
        }


        public void InsertStartMessage(ulong startMessageId)
        {
            if (_startMessageId is not null)
                throw new InvalidOperationException("The start message is already defined");
            if (backup_register is null)
                throw new InvalidOperationException("The backup register has not been created yet");

            backup_register.YoungestMessage = startMessageId;
        }
    }
}

