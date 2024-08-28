using System;

namespace XivSteamTimeline.Timeline;

public class Instant
{

    public static Instant Now => new(DateTime.Now);
    public DateTime Start { get; private set; }
    
    private Instant(DateTime start)
    {
        Start = start;
    }

    public TimeSpan Elapsed()
    {
        return DateTime.Now - Start;
    }

    public TimeSpan Refresh()
    {
        var elapsed = Elapsed();
        Start = DateTime.Now;
        return elapsed;
    }
}
