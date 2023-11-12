using Bot.Services.Database.Models;
using Bot.Utilities;

namespace Bot.Services.Database.Repository
{
    internal class ChannelRepository
    {
        private readonly ConsoleLogger _log = new(nameof(ChannelRepository));

        public void RegisterIfNotExists(Channel channel)
        {
            var context = DbConnection.GetConnection();

            if (context.Channels.Any(c => c.Id == channel.Id))
            {
                _log.BackupAction($"Channel '{channel.Name}' already has been added");
                return;
            }

            try
            {
                context.Channels.Add(channel);
                context.SaveChanges();
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