namespace OPZBackup.Services.Utils;

public class PerformanceProfiler
{
    public readonly Dictionary<string, Timer> Timers = new();

    public Timer Subscribe(string timerName)
    {
        var timer = new Timer();

        Timers.Add(timerName, timer);
        return timer;
    }

    public TimeSpan TotalElapsed(bool exclude = false, params string[] args)
    {
        var total = TimeSpan.Zero;

        foreach (var timer in Timers)
            if (!args.Any())
                total += timer.Value.Total;

            else if (!exclude && args.Contains(timer.Key))
                total += timer.Value.Total;

            else if (!args.Contains(timer.Key))
                total += timer.Value.Total;

        return total;
    }
}