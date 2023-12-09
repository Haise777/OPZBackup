using Data.Contracts.Entities;

namespace OPZBot;

public class ProcessedMessage
{
    public List<User> Users { get; set; }
    public List<Message> Messages { get; set; }
}