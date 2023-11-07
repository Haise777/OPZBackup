using App.Services.Context;
using App.Services.Models;
using Discord.WebSocket;

namespace App.Services.Repository
{
    internal class ChannelRepository
    {


        public static Channel RegisterIfNotExists(ISocketMessageChannel channel)
        {
            var context = new MessageBackupContext();
            var theChannel = context.Channels.SingleOrDefault(c => c.Id == channel.Id);
            if (theChannel is not null)
            {
                return theChannel;
            }

            var newChannel = new Channel()
            {
                Id = channel.Id,
                Name = channel.Name,
            };

            context.Channels.Add(newChannel);
            context.SaveChanges();
            return newChannel;
        }

        public void SaveOnDatabase()
        {

        }
    }
}