using System.Diagnostics;

namespace OPZBackup.Services.Utils;

public class Timer
{
    private readonly Stopwatch _stopWatch = new();
    private int _totalFrames;

    public TimeSpan Mean => Total / _totalFrames;
    public TimeSpan Total { get; private set; } = TimeSpan.Zero;

    public TimeSpan Elapsed => _stopWatch.Elapsed;

    public void StartTimer()
    {
        _stopWatch.Restart();
    }

    public TimeSpan Stop()
    {
        _stopWatch.Stop();
        Total += _stopWatch.Elapsed;
        _totalFrames++;

        return _stopWatch.Elapsed;
    }
}