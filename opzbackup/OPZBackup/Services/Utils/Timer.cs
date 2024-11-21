using System.Diagnostics;

namespace OPZBackup.Services.Utils;

public class Timer
{
    private readonly Stopwatch _stopWatch = new();
    public TimeSpan TotalElapsed = TimeSpan.Zero;
    private int _totalFrames;

    public void StartTimer()
    {
        _stopWatch.Restart();
    }

    public TimeSpan Stop()
    {
        _stopWatch.Stop();
        TotalElapsed += _stopWatch.Elapsed;
        _totalFrames++;
        
        return _stopWatch.Elapsed;
    }

    public TimeSpan Mean()
    {
        return TotalElapsed / _totalFrames;
    }

    public TimeSpan Total() => TotalElapsed;
}