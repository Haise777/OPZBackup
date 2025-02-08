namespace OPZBackup.Extensions;

public static class TimeSpanExtension
{
    public static string Formatted(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 60)
            if (timeSpan.TotalSeconds < 10)
                return timeSpan.ToString("s'.'fff's'");
            else
                return timeSpan.ToString("ss'.'fff's'");

        if (timeSpan.TotalMinutes < 60)
            return timeSpan.ToString("mm':'ss");

        return timeSpan.ToString("hh':'mm':'ss");
    }
}