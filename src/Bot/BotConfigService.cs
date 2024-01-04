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
        CreateFilesIfNotExists();
        var config = GetConfigFile();

        Config = new BotConfig
        {
            Token = config.GetValue<string?>("Token", null),
            MainAdminRoleId = config.GetValue<ulong?>("MainAdminRoleId", null),
            RunWithCooldowns = config.GetValue("RunWithCooldowns", true),
            TestGuildId = config.GetValue<ulong?>("TestGuildId", null),
            TimezoneAdjust = config.GetValue<int?>("TimezoneAdjust", null)
        };
    }

    public void WriteConfigFile(BotConfig config)
    {
        var serializerOptions = new JsonSerializerOptions
            { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonString = JsonSerializer.Serialize(config, serializerOptions);
        File.WriteAllText($"{AppContext.BaseDirectory}{CONFIG_FILE_NAME}", jsonString);
    }

    public IConfigurationRoot GetConfigFile()
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
            Console.WriteLine("ERROR: Config file is corrupted\nCreating a new config file");
            Console.WriteLine("[press any key]");
            Console.ReadKey();
            WriteConfigFile(new BotConfig());

            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(CONFIG_FILE_NAME)
                .Build();
        }
    }

    private void CreateFilesIfNotExists()
    {
        if (!File.Exists($"{AppContext.BaseDirectory}{CONFIG_FILE_NAME}"))
        {
            File.Create($"{AppContext.BaseDirectory}{CONFIG_FILE_NAME}").Dispose();
            WriteConfigFile(new BotConfig());
        }

        if (!Directory.Exists(@$"{AppContext.BaseDirectory}/Backup"))
            Directory.CreateDirectory(@$"{AppContext.BaseDirectory}/Backup");
    }
}