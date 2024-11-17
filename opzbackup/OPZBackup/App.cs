using System.IO.Compression;
using Microsoft.Extensions.Configuration;

namespace OPZBackup;

public static class App
{
    private const string _configPrefix = "app";


    public const string Version = "0.1.0";
    public static readonly IConfiguration Configuration = GetConfigurationFromFile();

    static App()
    {
    }

    public static DateTime SessionDate { get; } = DateTime.Now;

    public static string BaseBackupPath { get; } = $"{AppContext.BaseDirectory.Replace('\\', '/')}backup";

    public static string BackupPath { get; } = $"{BaseBackupPath}/files";

    public static string TempPath { get; } = $"{BackupPath}/temp";

    public static bool RunWithCooldowns { get; private set; } =
        Configuration.GetValue<bool>($"{_configPrefix}:{nameof(RunWithCooldowns)}");

    public static int TimezoneAdjust { get; private set; } =
        Configuration.GetValue<int>($"{_configPrefix}:{nameof(TimezoneAdjust)}");

    public static ulong? MainAdminRoleId { get; private set; } =
        Configuration.GetValue<ulong?>($"{_configPrefix}:{nameof(MainAdminRoleId)}");

    public static string Token { get; private set; } =
        Configuration.GetValue<string>($"{_configPrefix}:{nameof(Token)}");

    public static int MaxMessagesPerBatch { get; private set; } =
        Configuration.GetValue<int>($"{_configPrefix}:{nameof(MaxMessagesPerBatch)}");

    public static CompressionLevel CompressionLevel { get; private set; } = GetCompressionLevel();

    public static ulong BotUserId { get; private set; }

    public static void SetBotUserId(ulong botUserId)
    {
        if (BotUserId != 0)
            throw new InvalidOperationException(
                $"The value of {nameof(BotUserId)} can only be set once per application startup.");

        BotUserId = botUserId;
    }

    public static IConfiguration GetConfigurationFromFile()
    {
        try
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .Build();
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine("FATAL ERROR: Config file is corrupted");
            throw;
        }
    }

    private static CompressionLevel GetCompressionLevel()
    {
        var configValue = Configuration.GetValue<string>($"{_configPrefix}:{nameof(CompressionLevel)}");

        return configValue switch
        {
            "optimal" => CompressionLevel.Optimal,
            "fastest" => CompressionLevel.Fastest,
            "hardest" => CompressionLevel.SmallestSize,
            "off" => CompressionLevel.NoCompression,
            _ => throw new ApplicationException($"Unknown compression level: {configValue}")
        };
    }
}