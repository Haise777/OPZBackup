using Discord;
using Discord.WebSocket;
using OPZBot.DataAccess.Models;

namespace OPZBot.DataAccess;

public class Mapper
{
    public User Map(IUser socketUser)
    {
        return new User
        {
            Id = socketUser.Id,
            Username = socketUser.Username
        };
    }

    public IEnumerable<User> Map(IEnumerable<IUser> socketUsers)
    {
        return socketUsers.Select(user
            => new User
            {
                Id = user.Id,
                Username = user.Username
            }).ToList();
    }

    public Channel Map(ISocketMessageChannel socketChannel)
    {
        return new Channel
        {
            Id = socketChannel.Id,
            Name = socketChannel.Name
        };
    }

    public IEnumerable<Channel> Map(IEnumerable<ISocketMessageChannel> socketChannels)
    {
        return socketChannels.Select(channel
            => new Channel
            {
                Id = channel.Id,
                Name = channel.Name
            }).ToList();
    }

    public Message Map(IMessage message, uint backupId)
    {
        return new Message
        {
            Id = message.Id,
            Content = message.Content,
            BackupId = backupId,
            AuthorId = message.Author.Id,
            ChannelId = message.Channel.Id,
            SentDate = message.Timestamp.DateTime
        };
    }

    public IEnumerable<Message> Map(IEnumerable<IMessage> messages, uint backupId)
    {
        return messages.Select(message
            => new Message
            {
                Id = message.Id,
                Content = message.Content,
                BackupId = backupId,
                AuthorId = message.Author.Id,
                ChannelId = message.Channel.Id,
                SentDate = message.Timestamp.DateTime
            }).ToList();
    }
}