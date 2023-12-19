// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace OPZBot;

internal class BotConfigService
{
    public const string CONFIG_FILE_NAME = "bot.config";
    public readonly BotConfig Config;

    public BotConfigService()
    {
        if (!File.Exists($"{AppContext.BaseDirectory}{CONFIG_FILE_NAME}"))
        {
            File.Create($"{AppContext.BaseDirectory}{CONFIG_FILE_NAME}").Dispose();
            WriteConfigFile(new BotConfig());
        }

        var startupConfig = GetConfigurations();

        Config = new BotConfig
        {
            Token = startupConfig.GetValue<string?>("Token", null),
            ConnectionString = startupConfig.GetValue<string?>("ConnectionString", null),
            MainAdminRoleId = startupConfig.GetValue<ulong?>("MainAdminRoleId", null),
            RunWithCooldowns = startupConfig.GetValue("RunWithCooldowns", true),
            TestGuildId = startupConfig.GetValue<ulong?>("TestGuildId", null),
            TimezoneAdjust = startupConfig.GetValue<int?>("TimezoneAdjust", null)
        };
    }

    public void WriteConfigFile(BotConfig config)
    {
        var serializerOptions = new JsonSerializerOptions
            { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonString = JsonSerializer.Serialize(config, serializerOptions);
        File.WriteAllText($"{AppContext.BaseDirectory}{CONFIG_FILE_NAME}", jsonString);
    }

    public IConfigurationRoot GetConfigurations()
    {
        try
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(CONFIG_FILE_NAME)
                .Build();
        }
        catch (InvalidDataException)
        {
            Console.WriteLine("ERROR: Config file is corrupted\nCreating a new file");
            Console.WriteLine("[press any key]");
            Console.ReadKey();
            WriteConfigFile(new BotConfig());

            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(CONFIG_FILE_NAME)
                .Build();
        }
    }
}