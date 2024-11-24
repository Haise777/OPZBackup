namespace OPZBackup.Extensions;

public static class TimeSpanExtension
{
    public static string Formatted(this TimeSpan timeSpan)
    {
        if (timeSpan.Seconds < 60)
            if (timeSpan.Seconds < 10)
                return timeSpan.ToString("s'.'fff's'");
            else
                return timeSpan.ToString("ss'.'fff's'");

        if (timeSpan.Minutes < 60)
            return timeSpan.ToString("mm':'ss");

        return timeSpan.ToString("hh':'mm':'ss");
    }
}