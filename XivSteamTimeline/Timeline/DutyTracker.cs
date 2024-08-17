using System;

namespace XivSteamTimeline.Timeline;

public class DutyTracker
{
    public static DutyTracker Instance { get; private set; } = new DutyTracker();

    public uint CurrentDuty { get; private set; }
    public DateTime? DutyStartTime { get; private set; }
    public DateTime? PullStartTime { get; private set; }

    private DutyTracker()
    {
        CurrentDuty = 0;
    }

    public void SetCurrentDuty(uint duty)
    {
        if (CurrentDuty != duty)
        {
            CurrentDuty = duty;
            DutyStartTime = DateTime.Now;
        }
    }

    public void StartNewPull()
    {
        PullStartTime = DateTime.Now;
    }

    /// <summary>
    /// Marks the current pull as ended and return the elapsed time in seconds
    /// </summary>
    /// <returns></returns>
    public double EndPull()
    {
        var elapsedSeconds = DateTime.Now - PullStartTime;
        PullStartTime = null;

        return elapsedSeconds?.TotalSeconds ?? 0.0;
    }

    public double EndDuty()
    {
        var elapsedSeconds = DateTime.Now - DutyStartTime;
        DutyStartTime = null;
        CurrentDuty = 0;

        return elapsedSeconds?.TotalSeconds ?? 0.0;
    }
}
