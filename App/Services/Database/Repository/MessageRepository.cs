using App.Services.Database.Models;

namespace App.Services.Database.Repository
{
    internal static class MessageRepository
    {
        public static bool CheckIfExists(ulong id)
        {
            var context = DbConnection.GetConnection();
            return context.Messages.Any(m => m.Id == id);
        }

        public static void SaveToDatabase(List<Message> messagesToSave)
        {
            var log = new ConsoleLogger(nameof(MessageRepository));
            var context = DbConnection.GetConnection();

            try
            {
                context.Messages.AddRange(messagesToSave);
                context.SaveChanges();
                log.BackupAction($"Saved {messagesToSave.Count} messages to database");
            }
            catch (Exception ex)
            {
                log.Exception("Failed to save message batch to database", ex);
                throw;
            }
        }
    }
}
