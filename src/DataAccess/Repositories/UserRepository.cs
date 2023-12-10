using OPZBot.Core.Contracts;
using OPZBot.DataAccess.Context;

namespace OPZBot.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MyDbContext _context;

    public UserRepository(MyDbContext context)
    {
        _context = context;
    }
}