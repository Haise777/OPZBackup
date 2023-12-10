using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPZBot.DataAccess.Context;

namespace OPZBot.DataAccess;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MyDbContext>(options
            => options.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.34-mysql")));

        return services;
    }
}