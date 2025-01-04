namespace OPZBackup.Services.Utils;

public class TimeValue
{
    public TimeValue()
    {
    }

    // public TimeValue(int frames, TimeSpan totalTime)
    // {
    //     TotalFrames = frames;
    //     Total = totalTime;
    // }
    
    protected int TotalFrames;
    public TimeSpan Mean => Total / TotalFrames;
    public TimeSpan Total { get; protected set; } = TimeSpan.Zero;
    
}