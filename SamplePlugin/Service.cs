using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SamplePlugin;

public sealed class Service
{
    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    public static IPluginLog Logger { get; private set; } = null!;

    [PluginService]
    public static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    public static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    public static IGameGui GameGui { get; private set; } = null!;

    [PluginService]
    public static IGameConfig GameConfig { get; private set; } = null!;

    [PluginService]
    public static IGameInteropProvider Hook { get; private set; } = null!;

    [PluginService]
    public static ISigScanner SigScanner { get; private set; } = null!;

    [PluginService]
    public static ICondition Condition { get; private set; } = null!;

    [PluginService]
    public static IFramework Framework { get; private set; } = null!;

    [PluginService]
    public static ITextureProvider Texture { get; private set; } = null!;

    [PluginService]
    public static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    public static IDtrBar DtrBar { get; private set; } = null!;

    [PluginService]
    public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    
    [PluginService]
    public static IClientState ClientState { get; private set; } = null!;
    [PluginService]
    public static IDutyState DutyState { get; private set; } = null!;

    [PluginService]
    public static IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    public static ITargetManager TargetManager { get; private set; } = null!;

    [PluginService]
    public static IKeyState KeyState { get; private set; } = null!;
    
    public static Action<string>? LogHandler;
    public static void Log(string msg) => LogHandler?.Invoke(msg);

    public static object LuminaRSVLock = new(); // TODO: replace with System.Threading.Lock
    public static Lumina.GameData? LuminaGameData;
    public static Lumina.Excel.ExcelSheet<T>? LuminaSheet<T>() where T : Lumina.Excel.ExcelRow => LuminaGameData?.GetExcelSheet<T>(Lumina.Data.Language.English);
    public static T? LuminaRow<T>(uint row) where T : Lumina.Excel.ExcelRow => LuminaSheet<T>()?.GetRow(row);

    public static WindowSystem? WindowSystem;

    public static Configuration Config = null!;
}
