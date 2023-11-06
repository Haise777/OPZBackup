using App.Services.Context;

namespace App.Services.Models
{
    internal class BackupBuilder
    {
        private DateTime _startDate;
        private ulong _authorId;
        private ulong _startMessageId;
        private ulong _endMessageId;

        public BackupBuilder(DateTime startDate, ulong authorId, ulong startMessageId)
        {
            _startDate = startDate;
            _authorId = authorId;
            _startMessageId = startMessageId;
        }

        public void SaveOnDatabase(ulong endMessageId)
        {
            using var context = new MessageBackupContext();
            var currentBackup = context.Backups.SingleOrDefault(b => b.Date == _startDate);

            if (currentBackup == null)
            {
                currentBackup = new Backup()
                {
                    Date = _startDate,
                    Author = _authorId,
                    YoungestMessage = _startMessageId,
                };
                context.Backups.Add(currentBackup);
            }

            currentBackup.OldestMessage = _endMessageId;
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("toImplementErrorLog :: Error trying to save to DB, on BackupBuilder");
                throw;
            }
        }
    }
}
