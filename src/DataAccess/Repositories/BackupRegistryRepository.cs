using OPZBot.Core.Contracts;
using OPZBot.DataAccess.Context;

namespace OPZBot.DataAccess.Repositories;

public class BackupRegistryRepository : IBackupRegistryRepository
{
    private readonly MyDbContext _context;

    public BackupRegistryRepository(MyDbContext context)
    {
        _context = context;
    }
}