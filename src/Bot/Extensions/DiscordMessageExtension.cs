using Discord;

namespace OPZBot.Extensions;

public static class DiscordMessageExtension
{
    public static DateTime TimestampWithFixedTimezone(this IMessage message)
    {
        return message.Timestamp.DateTime.AddHours(Program.TimezoneAdjust);
    }
}