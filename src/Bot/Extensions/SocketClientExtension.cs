// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

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
            var adminRole = client.Guilds.First().GetRole(config.GetValue<ulong>("MainAdminRoleId"))
                            ?? throw new ApplicationException("'MainAdminRoleId' invalid config value");
#if DEBUG
            var testGuild = client.Guilds.First().GetRole(config.GetValue<ulong>("TestGuildId"))
                            ?? throw new ApplicationException("'TestGuildId' invalid config value");
#endif
        }
        catch (ApplicationException ex)
        {
            Log.Fatal(ex, "Invalid config value");
            Environment.Exit(1);
        }
    }
}