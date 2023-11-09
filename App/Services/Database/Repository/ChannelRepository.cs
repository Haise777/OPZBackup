using App.Services.Database.Models;
using App.Utilities;

namespace App.Services.Database.Repository
{
    internal static class ChannelRepository
    {
        public static void RegisterIfNotExists(Channel channel)
        {
            var _log = new ConsoleLogger(nameof(ChannelRepository));
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