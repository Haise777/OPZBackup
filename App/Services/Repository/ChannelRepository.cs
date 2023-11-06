using App.Services.Context;
using App.Services.Models;
using Discord.WebSocket;

namespace App.Services.Repository
{
    internal class ChannelRepository
    {


        public static void RegisterIfNotExists(ISocketMessageChannel channel)
        {
            var context = new MessageBackupContext();
            if (context.Channels.Any(c => c.Id == channel.Id))
            {
                return;
            }

            context.Add(
                new Channel()
                {
                    Id = channel.Id,
                    Name = channel.Name,
                });

            context.SaveChanges();
        }

    }
}