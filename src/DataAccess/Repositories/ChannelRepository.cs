using OPZBot.Core.Contracts;
using OPZBot.DataAccess.Context;

namespace OPZBot.DataAccess.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly MyDbContext _context;

    public ChannelRepository(MyDbContext context)
    {
        _context = context;
    }
}