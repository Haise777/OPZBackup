using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository
{
    internal class ChannelRepository
    {
        private readonly ConsoleLogger _log = new(nameof(ChannelRepository));
        private readonly DbConnection _connection;

        public ChannelRepository(DbConnection dbConnection)
        {
            _connection = dbConnection;
        }

        public void RegisterIfNotExists(Channel channel)
        {
            var context = _connection.GetConnection();
            if (context.Channels.Any(c => c.Id == channel.Id))
            {
                _log.BackupAction($"Channel '{channel.Name}' already has been added");
                return;
            }

            context.Channels.Add(channel);
            context.SaveChanges();
            _log.BackupAction($"Added new channel: '{channel.Name}'");
        }
    }
}