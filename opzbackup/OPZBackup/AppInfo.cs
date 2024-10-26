using Microsoft.Extensions.Configuration;

namespace OPZBackup;

public static class AppInfo
{
    private static readonly IConfiguration _configuration;
    private const string _configPrefix = "app";

    static AppInfo()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }


    public const string APP_VER = "2.0.0-alpha1";
    public static DateTime SessionTime { get; } = DateTime.Now;
    public static string FileBackupPath { get; } = $"{AppContext.BaseDirectory}Backup/Files";

    public static bool RunWithCooldowns { get; private set; } =
        _configuration.GetValue<bool>($"{_configPrefix}:{nameof(RunWithCooldowns)}");

    public static int TimezoneAdjust { get; private set; } =
        _configuration.GetValue<int>($"{_configPrefix}:{nameof(TimezoneAdjust)}");

    public static ulong? MainAdminRoleId { get; private set; } =
        _configuration.GetValue<ulong?>($"{_configPrefix}:{nameof(MainAdminRoleId)}");
    
    public static ulong TestGuildId { get; set; } =
        _configuration.GetValue<ulong>($"{_configPrefix}:{nameof(TestGuildId)}");
    public static string Token { get; set; } =
        _configuration.GetValue<string>($"{_configPrefix}:{nameof(Token)}");

    public static ulong BotUserId { get; private set; }
    public static void SetBotUserId(ulong botUserId)
    {
        if (BotUserId != 0)
            throw new InvalidOperationException($"The value of {nameof(BotUserId)} can only be set once per application startup.");
        
        BotUserId = botUserId;
    }
}