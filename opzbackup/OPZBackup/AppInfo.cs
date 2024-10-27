using Microsoft.Extensions.Configuration;

namespace OPZBackup;

public static class AppInfo
{
    public static readonly IConfiguration Configuration;
    private const string _configPrefix = "app";

    static AppInfo()
    {
        Configuration = GetConfigurationFromFile();
    }


    public const string APP_VER = "2.0.0-alpha1";
    public static DateTime SessionTime { get; } = DateTime.Now;
    public static string FileBackupPath { get; } = $"{AppContext.BaseDirectory}Backup/Files";

    public static bool RunWithCooldowns { get; private set; } =
        Configuration.GetValue<bool>($"{_configPrefix}:{nameof(RunWithCooldowns)}");

    public static int TimezoneAdjust { get; private set; } =
        Configuration.GetValue<int>($"{_configPrefix}:{nameof(TimezoneAdjust)}");

    public static ulong? MainAdminRoleId { get; private set; } =
        Configuration.GetValue<ulong?>($"{_configPrefix}:{nameof(MainAdminRoleId)}");

    public static ulong TestGuildId { get; set; } =
        Configuration.GetValue<ulong>($"{_configPrefix}:{nameof(TestGuildId)}");

    public static string Token { get; set; } =
        Configuration.GetValue<string>($"{_configPrefix}:{nameof(Token)}");

    public static ulong BotUserId { get; private set; }

    public static void SetBotUserId(ulong botUserId)
    {
        if (BotUserId != 0)
            throw new InvalidOperationException(
                $"The value of {nameof(BotUserId)} can only be set once per application startup.");

        BotUserId = botUserId;
    }

    private static IConfiguration GetConfigurationFromFile()
    {
        try
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine("FATAL ERROR: Config file is corrupted");
            throw;
        }
    }
}