using System.Diagnostics;

namespace OPZBackup.Services.Utils;

public class Timer : TimeValue
{
    private readonly Stopwatch _stopWatch = new();
    public TimeSpan Elapsed => _stopWatch.Elapsed;

    public Timer StartTimer()
    {
        _stopWatch.Restart();
        return this;
    }

    public Timer Stop()
    {
        _stopWatch.Stop();
        Total += _stopWatch.Elapsed;
        TotalFrames++;

        return this;
    }
}