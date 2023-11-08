using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository
{
    internal static class ChannelRepository
    {
        public static Channel RegisterIfNotExists(Channel channel)
        {
            var context = new MessageBackupContext();
            var theChannel = context.Channels.SingleOrDefault(c => c.Id == channel.Id);
            if (theChannel is not null)
            {
                return theChannel;
            }


            context.Channels.Add(channel);
            context.SaveChanges();
            return channel;
        }
    }
}