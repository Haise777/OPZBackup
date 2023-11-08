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

            context.Messages.AddRange(messagesToSave);
            context.SaveChanges();
        }
    }
}
