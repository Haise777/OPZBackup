using Discord;

namespace OPZBot.Services.MessageBackup;

public class MessageDataBatchDto
{
    public MessageDataBatchDto(IEnumerable<IUser> users, IEnumerable<IMessage> messages)
    {
        Users = users;
        Messages = messages;
    }

    public IEnumerable<IUser> Users { get; }
    public IEnumerable<IMessage> Messages { get; }
}