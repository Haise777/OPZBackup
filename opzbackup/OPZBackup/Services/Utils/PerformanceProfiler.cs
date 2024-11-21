namespace OPZBackup.Services.Utils;

public class PerformanceProfiler
{
    public readonly Dictionary<string, Timer> Timers = new();

    public Timer Subscribe(string timerName)
    {
        var profiler = new Timer();
        Timers.Add(timerName, profiler);

        return profiler;
    }

    public TimeSpan TotalElapsed()
    {
        var total = TimeSpan.Zero;

        foreach (var timer in Timers.Values)
            total += timer.Total();
        
        return total;
    }
}