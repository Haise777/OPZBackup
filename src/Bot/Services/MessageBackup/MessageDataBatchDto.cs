using Discord;

namespace OPZBot.Services.MessageBackup;

public class MessageDataBatchDto
{
    public IEnumerable<IUser> Users { get; init; }
    public IEnumerable<IMessage> Messages { get; init; }

    public MessageDataBatchDto(List<IUser> users, List<IMessage> messages)
    {
        Users = users;
        Messages = messages;
    }
}