using Bot.Services.Database.Context;
using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository
{
    internal class ChannelRepository
    {
        private readonly ConsoleLogger _log = new(nameof(ChannelRepository));
        private readonly MessageBackupContext _backupContext;

        public ChannelRepository(DbConnection dbConnection)
        {
            _backupContext = dbConnection.GetConnection();
        }

        public void RegisterIfNotExists(Channel channel)
        {
            if (_backupContext.Channels.Any(c => c.Id == channel.Id))
            {
                _log.BackupAction($"Channel '{channel.Name}' already has been added");
                return;
            }

            try
            {
                _backupContext.Channels.Add(channel);
                _backupContext.SaveChanges();
                _log.BackupAction($"Added new channel: '{channel.Name}'");
            }
            catch (Exception ex)
            {
                _log.Exception("Failed to save new channel entry", ex);
                throw;
            }
        }
    }
}