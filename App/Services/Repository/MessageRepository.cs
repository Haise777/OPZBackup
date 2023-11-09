using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository
{
    internal static class MessageRepository
    {
        public static bool CheckIfExists(ulong id)
        {
            using var context = new MessageBackupContext();
            return context.Messages.Any(m => m.Id == id);
        }

        public static void SaveToDatabase(List<Message> messagesToSave)
        {
            var log = new ConsoleLogger(nameof(MessageRepository));
            using var context = new MessageBackupContext();

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
