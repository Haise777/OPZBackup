using OPZBot.Core.Contracts;
using OPZBot.DataAccess.Context;

namespace OPZBot.DataAccess.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly MyDbContext _context;

    public MessageRepository(MyDbContext context)
    {
        _context = context;
    }
}