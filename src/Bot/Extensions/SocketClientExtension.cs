using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace OPZBot.Extensions;

public static class SocketClientExtension
{
    public static void ValidateConfigIds(this DiscordSocketClient client, IConfigurationRoot config)
    {
        try
        {
            _ = config.GetValue<ulong?>("MainAdminRoleId") is not null
                ? client.Guilds.First().GetRole(config.GetValue<ulong>("MainAdminRoleId"))
                  ?? throw new ApplicationException("'MainAdminRoleId' invalid config value")
                : null;
        }
        catch (ApplicationException ex)
        {
            Log.Fatal(ex, "Invalid config value");
            Environment.Exit(1);
        }
    }
}