using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.SteamApi;
using Lumina.Excel.GeneratedSheets2;
using XivSteamTimeline.Timeline;
using XivSteamTimeline.Windows;
using Steamworks;

namespace XivSteamTimeline;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/pmycommand";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("XivSteamTimeline");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin(IDalamudPluginInterface dalamud, IDataManager dataManager)
    {
        dalamud.Create<Service>();
        Service.LogHandler = (string msg) => Service.Logger.Debug(msg);
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = WindowSystem;
        Service.Condition.ConditionChange += OnConditionChanged;

        Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Service.Config = Configuration;

        var good = SteamAPI.Init();
        if (!good)
        {
            Service.ChatGui.Print(
                "Steam API Initialisation failed, are you using an up-to-date steam_api64.dll? Use the steam-compat-api!");
        }

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        Service.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        Service.PluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;

        // Adds another button that is doing the same but for the main ui of the plugin
        Service.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;


        Service.ClientState.TerritoryChanged += OnTerritoryChanged;
        Service.ClientState.Login += OnLogin;
        Service.DutyState.DutyStarted += OnDutyStart;
        Service.DutyState.DutyWiped += OnDutyWipe;
        Service.DutyState.DutyCompleted += OnDutyComplete;


        if (Service.ClientState.IsLoggedIn)
        {
            SteamTimeline.SetTimelineGameMode(ETimelineGameMode.k_ETimelineGameMode_Playing);
        }
        else
        {
            SteamTimeline.SetTimelineGameMode(ETimelineGameMode.k_ETimelineGameMode_Staging);
        }
    }

    public void Dispose()
    {
        Service.Condition.ConditionChange -= OnConditionChanged;
        Service.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= ConfigWindow.Toggle;
        Service.PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;

        Service.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Service.ClientState.Login -= OnLogin;
        Service.DutyState.DutyStarted -= OnDutyStart;
        Service.DutyState.DutyWiped -= OnDutyWipe;
        Service.DutyState.DutyCompleted -= OnDutyComplete;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        Service.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();

        if (SteamAPI.IsSteamRunning())
        {
            unsafe
            {
                Steamworks.SteamTimeline.AddTimelineEvent("Test Description", "steam_attack", "Test Title 2", 1, -20f,
                                                          5,
                                                          ETimelineEventClipPriority
                                                              .k_ETimelineEventClipPriority_Featured);
            }
        }
    }

    public void OnTimelineEvent(TimelineEventType eType)
    {
        switch (eType)
        {
            case TimelineEventType.CombatStart:
                var ev = Service.Config.CombatStart;
                if (ev.Enabled)
                {
                    AddTimelineItem(ev.TimelineIcon, ev.Name, "Combat Started", ev.Priority, 0, 0,
                                    ETimelineEventClipPriority.k_ETimelineEventClipPriority_None);
                }

                break;
            case TimelineEventType.DutyStart:
                DutyTracker.Instance.StartNewPull();
                break;
            case TimelineEventType.DutyWipe:
                float wipeSecondsPassed = (float)DutyTracker.Instance.EndPull();
                AddTimelineItem("steam_checkmark", "Wipe", "Full party wipe", 2, 0, 0,
                                ETimelineEventClipPriority.k_ETimelineEventClipPriority_None);
                AddTimelineItem("steam_bolt", "Pulled", "Pull start", 3, -wipeSecondsPassed,
                                wipeSecondsPassed + 5,
                                ETimelineEventClipPriority.k_ETimelineEventClipPriority_Featured);
                break;
            case TimelineEventType.DutyCompleted:
                float secondsInThePastStart = (float)DutyTracker.Instance.EndPull();
                AddTimelineItem("steam_checkmark", "Finish Duty", "Killed the boss", 2, 0, 0,
                                ETimelineEventClipPriority.k_ETimelineEventClipPriority_None);
                AddTimelineItem("steam_bolt", "Pulled", "Pull start", 3, -secondsInThePastStart,
                                secondsInThePastStart + 5,
                                ETimelineEventClipPriority.k_ETimelineEventClipPriority_Featured);
                break;
            case TimelineEventType.DutyEnd:
                DutyTracker.Instance.EndDuty();
                AddTimelineItem("steam_minus", "End Duty", "End of duty", 1, 0, 0,
                                ETimelineEventClipPriority.k_ETimelineEventClipPriority_None);
                break;
            case TimelineEventType.LoadingStart:
                // SteamTimeline.SetTimelineGameMode(ETimelineGameMode.k_ETimelineGameMode_LoadingScreen);
                break;
            case TimelineEventType.LoadingEnd:
                // SteamTimeline.SetTimelineGameMode(ETimelineGameMode.k_ETimelineGameMode_Playing);
                break;
            default:
                break;
        }
    }

    private void OnTerritoryChanged(ushort typeId)
    {
        var newTerritory = Service.LuminaRow<TerritoryType>(typeId)?.PlaceName.Value
                                  ?.NameNoArticle.ToString();
        SteamTimeline.SetTimelineStateDescription(newTerritory, 0);

        if (DutyTracker.Instance.CurrentDuty != 0)
        {
            OnTimelineEvent(TimelineEventType.DutyEnd);
        }
    }

    private void OnLogin()
    {
        OnTimelineEvent(TimelineEventType.LoadingEnd);
    }

    private void OnConditionChanged(ConditionFlag flag, bool value)
    {
        // Service.ChatGui.Print($"Condition change: {flag}={value}");

        switch (flag)
        {
            case ConditionFlag.InCombat:
                OnTimelineEvent(value ? TimelineEventType.CombatStart : TimelineEventType.CombatEnd);
                break;
            case ConditionFlag.BetweenAreas:
                OnTimelineEvent(value ? TimelineEventType.LoadingStart : TimelineEventType.LoadingEnd);
                break;
            default:
                break;
        }
    }

    private void OnDutyStart(object? sender, ushort dutyId)
    {
        DutyTracker.Instance.SetCurrentDuty(dutyId);
        OnTimelineEvent(TimelineEventType.DutyStart);
    }

    private void OnDutyWipe(object? sender, ushort dutyId)
    {
        OnTimelineEvent(TimelineEventType.DutyWipe);
    }

    private void OnDutyComplete(object? sender, ushort dutyId)
    {
        OnTimelineEvent(TimelineEventType.DutyCompleted);
    }

    private void AddTimelineItem(
        string icon, string title, string description, uint priority, float startSecondsOffset, float duration,
        ETimelineEventClipPriority possibleClip)
    {
        SteamTimeline.AddTimelineEvent(description, icon, title, priority, startSecondsOffset,
                                       duration,
                                       possibleClip);
    }

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
