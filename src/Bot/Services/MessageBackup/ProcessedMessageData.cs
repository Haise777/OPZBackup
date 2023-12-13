using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class ProcessedMessageData
{
    public List<User> Users { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
}