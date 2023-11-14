using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository
{
    internal class MessageRepository
    {
        private readonly ConsoleLogger _log = new(nameof(MessageRepository));
        private readonly DbConnection _connection;

        public MessageRepository(DbConnection dbConnection)
        {
            _connection = dbConnection;
        }

        public bool CheckIfExists(ulong id)
        {
            var context = _connection.GetConnection();
            return context.Messages.Any(m => m.Id == id);
        }

        public void SaveToDatabase(List<Message> messagesToSave)
        {
            var context = _connection.GetConnection();

            context.Messages.AddRange(messagesToSave);
            context.SaveChanges();
            _log.BackupAction($"Saved {messagesToSave.Count} messages to database");
        }
    }
}
