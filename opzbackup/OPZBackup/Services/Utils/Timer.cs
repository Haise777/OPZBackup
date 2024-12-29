using System.Diagnostics;

namespace OPZBackup.Services.Utils;

public class Timer
{
    private readonly Stopwatch _stopWatch = new();
    private int _totalFrames;

    public TimeSpan Mean => Total / _totalFrames;
    public TimeSpan Total { get; private set; } = TimeSpan.Zero;

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
        _totalFrames++;

        return this;
    }
}