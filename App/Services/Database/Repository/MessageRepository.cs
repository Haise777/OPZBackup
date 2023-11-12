using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository
{
    internal class MessageRepository
    {
        private readonly ConsoleLogger _log = new(nameof(MessageRepository));

        public static bool CheckIfExists(ulong id)
        {
            var context = DbConnection.GetConnection();
            return context.Messages.Any(m => m.Id == id);
        }

        public void SaveToDatabase(List<Message> messagesToSave)
        {

            var context = DbConnection.GetConnection();

            try
            {
                context.Messages.AddRange(messagesToSave);
                context.SaveChanges();
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
