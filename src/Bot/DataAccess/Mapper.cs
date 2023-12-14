using Discord;
using Discord.WebSocket;
using OPZBot.DataAccess.Models;

namespace OPZBot.DataAccess;

public class Mapper
{
    public User Map(IUser socketUser)
    {
        return new User()
        {
            Id = socketUser.Id,
            Username = socketUser.Username
        };
    }
    
    public IEnumerable<User> Map(IEnumerable<IUser> socketUsers)
    {
        var toReturn = new List<User>();
        foreach (var user in socketUsers)
        {
            toReturn.Add(new User()
            {
                Id = user.Id,
                Username = user.Username
            });
        }

        return toReturn;
    }
    
    public Channel Map(ISocketMessageChannel socketChannel)
    {
        return new Channel()
        {
            Id = socketChannel.Id,
            Name = socketChannel.Name,
        };
    }
    
    public IEnumerable<Channel> Map(IEnumerable<ISocketMessageChannel> socketChannels)
    {
        var toReturn = new List<Channel>();
        foreach (var channel in socketChannels)
        {
            toReturn.Add(new Channel()
            {
                Id = channel.Id,
                Name = channel.Name
            });
        }

        return toReturn;
    }
    
    public Message Map(IMessage message)
    {
        return new Message()
        {
            Id = message.Id,
            Content = message.Content,
            AuthorId = message.Author.Id,
            ChannelId = message.Channel.Id,
            SentDate = message.Timestamp.DateTime
        };
    }
    
    public IEnumerable<Message> Map(IEnumerable<IMessage> messages, uint backupId)
    {
        var toReturn = new List<Message>();
        foreach (var message in messages)
        {
            toReturn.Add(new Message()
            {
                Id = message.Id,
                Content = message.Content,
                BackupId = backupId,
                AuthorId = message.Author.Id,
                ChannelId = message.Channel.Id,
                SentDate = message.Timestamp.DateTime
            });
        }

        return toReturn;
    }
}