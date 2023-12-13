using Discord;
using Discord.WebSocket;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services;

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
    
    public Channel Map(ISocketMessageChannel socketChannel)
    {
        return new Channel()
        {
            Id = socketChannel.Id,
            Name = socketChannel.Name,
        };
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
}