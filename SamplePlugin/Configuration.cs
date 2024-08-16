using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public TimelineConfigEvent CombatStart { get; set; } =
        new TimelineConfigEvent("Combat Start", "steam_attack", true, 1);

    public sealed class TimelineConfigEvent
    {
        public string Name { get; set; }
        public string TimelineIcon { get; set; }
        public bool Enabled { get; set; }
        public uint Priority { get; set; }

        public TimelineConfigEvent(string name, string timelineIcon = "steam_marker", bool enabled = true, uint priority = 1)
        {
            Name = name;
            TimelineIcon = timelineIcon;
            this.Enabled = enabled;
            this.Priority = priority;
        }
    }


    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
