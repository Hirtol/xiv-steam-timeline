using System;

namespace XivSteamTimeline.Timeline;

public class DutyTracker
{
    public static DutyTracker Instance { get; private set; } = new DutyTracker();

    public uint CurrentDuty { get; private set; }
    public Instant? DutyStart { get; private set; }
    public Instant? PullStart { get; private set; }

    private DutyTracker()
    {
        CurrentDuty = 0;
    }

    public void SetCurrentDuty(uint duty)
    {
        if (CurrentDuty != duty)
        {
            CurrentDuty = duty;
            DutyStart = Instant.Now;
            Service.ChatGui.Print($"Started new duty {duty}");
        }
    }

    public void StartNewPull()
    {
        PullStart = Instant.Now;
    }

    /// <summary>
    /// Marks the current pull as ended and return the elapsed time in seconds
    /// </summary>
    /// <returns></returns>
    public double EndPull()
    {
        var elapsedSeconds = PullStart?.Refresh();

        return elapsedSeconds?.TotalSeconds ?? 0.0;
    }

    public double EndDuty()
    {
        var elapsedSeconds = DutyStart?.Refresh();
        CurrentDuty = 0;

        return elapsedSeconds?.TotalSeconds ?? 0.0;
    }
}
