using System;
using Lumina.Excel.GeneratedSheets;

namespace XivSteamTimeline.Timeline;

public class DutyTracker
{
    public static DutyTracker Instance { get; private set; } = new DutyTracker();

    public uint CurrentDuty { get; private set; }
    public string CurrentDutyName => Service.LuminaRow<TerritoryType>(CurrentDuty)?.PlaceName?.Value?.Name?.ToString() ?? "UNKNOWN";
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
