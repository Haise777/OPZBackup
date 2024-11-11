using Discord;

namespace OPZBackup.Extensions;

public static class DiscordMessageExtension
{
    public static DateTime TimestampWithFixedTimezone(this IMessage message)
    {
        return message.Timestamp.DateTime.AddHours(App.TimezoneAdjust);
    }
}