namespace OPZBot;

internal class BotConfig
{
    public string? Token { get; set; }
    public string? ConnectionString { get; set; }
    public ulong? MainAdminRoleId { get; set; }
    public bool RunWithCooldowns { get; set; } = true;
    public ulong? TestGuildId { get; set; }
    public int? TimezoneAdjust { get; set; }
}