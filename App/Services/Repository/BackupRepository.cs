using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository
{
    internal class BackupRepository
    {
        private DateTime _startDate;
        private ulong _authorId;
        private ulong _startMessageId;
        private ulong _channelId;
        private BackupRegister? backup;

        public BackupRepository(DateTime startDate, ulong authorId, ulong channelId)
        {
            _startDate = startDate;
            _authorId = authorId;
            _channelId = channelId;
        }


        public void UpdateOnDatabase(ulong lastMessageId) //update inserting first and new last message
        {
            using var context = new MessageBackupContext();

            backup = context.BackupRegisters.SingleOrDefault(b => b.Date == _startDate);
            if (backup == null)
                throw new InvalidOperationException("Backup not found on database");

            backup.YoungestMessage = _startMessageId;
            backup.OldestMessage = lastMessageId;
            context.SaveChanges();
        }

        public void InsertStartMessage(ulong startMessageId)
        {
            if (_startMessageId == 0)
                _startMessageId = startMessageId;
        }
    }
}

