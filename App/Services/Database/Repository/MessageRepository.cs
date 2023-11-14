using Bot.Services.Database.Context;
using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository
{
    internal class MessageRepository
    {
        private readonly ConsoleLogger _log = new(nameof(MessageRepository));
        private readonly MessageBackupContext _backupContext;

        public MessageRepository(DbConnection dbContext)
        {
            _backupContext = dbContext.GetConnection();
        }

        public bool CheckIfExists(ulong id)
            => _backupContext.Messages.Any(m => m.Id == id);


        public void SaveToDatabase(List<Message> messagesToSave)
        {
            try
            {
                _backupContext.Messages.AddRange(messagesToSave);
                _backupContext.SaveChanges();
                _log.BackupAction($"Saved {messagesToSave.Count} messages to database");
            }
            catch (Exception ex)
            {
                _log.Exception("Failed to save message batch to database", ex);
                throw;
            }
        }
    }
}
