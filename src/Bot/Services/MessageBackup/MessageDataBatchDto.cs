using Discord;

namespace OPZBot.Services.MessageBackup;

public class MessageDataBatchDto
{
    public IEnumerable<IUser> Users { get; }
    public IEnumerable<IMessage> Messages { get; }

    public MessageDataBatchDto(IEnumerable<IUser> users, IEnumerable<IMessage> messages)
    {
        Users = users;
        Messages = messages;
    }
}