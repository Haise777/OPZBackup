using OPZBot.Core.Entities;

namespace OPZBot.Bot.Services.MessageBackup;

public class ProcessedMessageData
{
    public List<User> Users { get; set; }
    public List<Message> Messages { get; set; }
}