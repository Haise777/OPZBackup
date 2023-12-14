using Discord;

namespace OPZBot.Services.MessageBackup;

public class MessageDataBatch
{
    public List<IUser> Users { get; set; } = new();
    public List<IMessage> Messages { get; set; } = new();
}