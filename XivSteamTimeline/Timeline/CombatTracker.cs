using System;
using Dalamud.Game.ClientState.Conditions;

namespace XivSteamTimeline.Timeline;

public static class CombatTracker
{
    public static Instant? CombatStart { get; private set; }
    public static TimeSpan? LastCombatDuration { get; private set; }

    public static event EventHandler<TimeSpan> OnCombatEnd = delegate { };

    public static void Initialise()
    {
        Service.Condition.ConditionChange += OnCondition;
    }

    public static void Destroy()
    {
        Service.Condition.ConditionChange -= OnCondition;
    }

    private static void OnCondition(ConditionFlag flag, bool value)
    {
        switch (flag)
        {
            case ConditionFlag.InCombat:
                if (value)
                {
                    CombatStart = Instant.Now;
                }
                else
                {
                    var elapsed = CombatStart!.Elapsed();
                    LastCombatDuration = elapsed;
                    OnCombatEnd(null, elapsed);
                    CombatStart = null;
                }
                break;
        }
    }
}
