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
            using var context = new MessageBackupContext();

            try
            {
                context.Messages.AddRange(messagesToSave);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                ConsoleLogger.GenericException($"{nameof(MessageRepository)}-{nameof(SaveToDatabase)}", ex);
                throw;
            }
        }
    }
}
