using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Network;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Linq;

namespace DeepDungeonTracker;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "Deep Dungeon Tracker";

    private Commands Commands { get; }

    private WindowSystem WindowSystem { get; }

    private Configuration Configuration { get; }

    private Data Data { get; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface?.Create<Service>();

        this.Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(Service.PluginInterface);

        this.Data = new(pluginInterface?.UiBuilder!, this.Configuration);
        this.Commands = new(Plugin.Name, this.OnConfigCommand, this.OnMainCommandd, this.OnTrackerCommand, this.OnTimeCommand, this.OnScoreCommand, this.OnLoadCommand);

        this.WindowSystem = new(Plugin.Name.Replace(" ", string.Empty, StringComparison.InvariantCultureIgnoreCase));
#pragma warning disable CA2000
        this.WindowSystem.AddWindow(new ConfigurationWindow(Plugin.Name, this.Configuration, this.MainWindowToggleVisibility));
        this.WindowSystem.AddWindow(new MainWindow(Plugin.Name, this.Configuration, this.Data, this.OpenWindow<StatisticsWindow>));
        this.WindowSystem.AddWindow(new TrackerWindow(Plugin.Name, this.Configuration, this.Data));
        this.WindowSystem.AddWindow(new FloorSetTimeWindow(Plugin.Name, this.Configuration, this.Data));
        this.WindowSystem.AddWindow(new ScoreWindow(Plugin.Name, this.Configuration, this.Data));
        this.WindowSystem.AddWindow(new StatisticsWindow(Plugin.Name, this.Configuration, this.Data, this.MainWindowToggleVisibility, this.BossStatusTimerWindowToggleVisibility));
        this.WindowSystem.AddWindow(new BossStatusTimerWindow(Plugin.Name, this.Configuration, this.Data));
#pragma warning restore CA2000

        Service.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
        Service.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
        Service.PluginInterface.UiBuilder.DisableGposeUiHide = false;
        Service.PluginInterface.UiBuilder.DisableUserUiHide = false;

        Service.PluginInterface.UiBuilder.Draw += this.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
        Service.PluginInterface.UiBuilder.OpenMainUi += this.OpenMainUi;

        Service.Framework.Update += this.Update;
        Service.ClientState.Login += this.Login;
        Service.ClientState.TerritoryChanged += this.TerritoryChanged;
        Service.Condition.ConditionChange += this.ConditionChange;
        Service.ChatGui.ChatMessage += this.ChatMessage;
        Service.GameNetwork.NetworkMessage += this.NetworkMessage;
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= this.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
        Service.PluginInterface.UiBuilder.OpenMainUi -= this.OpenMainUi;

        Service.Framework.Update -= this.Update;
        Service.ClientState.Login -= this.Login;
        Service.ClientState.TerritoryChanged -= this.TerritoryChanged;
        Service.Condition.ConditionChange -= this.ConditionChange;
        Service.ChatGui.ChatMessage -= this.ChatMessage;
        Service.GameNetwork.NetworkMessage -= this.NetworkMessage;

        WindowEx.DisposeWindows(this.WindowSystem.Windows);
        this.WindowSystem.RemoveAllWindows();

        this.Commands.Dispose();
        this.Data.Dispose();
    }

    private void OnConfigCommand(string command, string args) => this.OpenConfigUi();

    private void OnMainCommandd(string command, string args) => this.MainWindowToggleVisibility();

    private void OnTrackerCommand(string command, string args)
    {
        this.Configuration.Tracker.Show = !this.Configuration.Tracker.Show;
        this.Configuration.Save();
    }

    private void OnTimeCommand(string command, string args)
    {
        this.Configuration.FloorSetTime.Show = !this.Configuration.FloorSetTime.Show;
        this.Configuration.Save();
    }

    private void OnScoreCommand(string command, string args)
    {
        this.Configuration.Score.Show = !this.Configuration.Score.Show;
        this.Configuration.Save();
    }

    private void OnLoadCommand(string command, string args)
    {
        if (!this.IsWindowOpen<StatisticsWindow>())
        {
            if (!this.Data.IsInsideDeepDungeon)
                this.Data.Common.LoadDeepDungeonData(false, true);

            this.Data.Statistics.Load(this.Data.Common.CurrentSaveSlot, this.OpenWindow<StatisticsWindow>);
        }
        else
            this.CloseWindow<StatisticsWindow>();
    }

    private void MainWindowToggleVisibility()
    {
        if (!this.IsWindowOpen<MainWindow>())
            this.OpenWindow<MainWindow>();
        else
            this.CloseWindow<MainWindow>();
    }

    private void BossStatusTimerWindowToggleVisibility()
    {
        if (!this.IsWindowOpen<BossStatusTimerWindow>())
            this.OpenWindow<BossStatusTimerWindow>();
        else
            this.CloseWindow<BossStatusTimerWindow>();
    }

    private void Draw() => this.WindowSystem.Draw();

    private void OpenConfigUi() => this.WindowSystem.Windows.FirstOrDefault(x => x is ConfigurationWindow)!.Toggle();

    private void OpenMainUi() => this.MainWindowToggleVisibility();

    private void Update(IFramework framework) => this.Data.Update(this.Configuration);

    private void Login() => this.Data.Login();

    private void TerritoryChanged(ushort territoryType) => this.Data.TerritoryChanged(territoryType);

    private void ConditionChange(ConditionFlag flag, bool value) => this.Data.ConditionChange(flag, value);

    private void ChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled) => this.Data.ChatMessage(message.TextValue);

    private void NetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
    {
        if (direction == NetworkMessageDirection.ZoneDown)
            this.Data.NetworkMessage(dataPtr, opCode, targetActorId, this.Configuration);
    }

    private void OpenWindow<T>() where T : Window => this.WindowSystem.Windows.FirstOrDefault(x => x is T)!.IsOpen = true;

    private void CloseWindow<T>() where T : Window => this.WindowSystem.Windows.FirstOrDefault(x => x is T)!.IsOpen = false;

    private bool IsWindowOpen<T>() where T : Window => this.WindowSystem.Windows.FirstOrDefault(x => x is T)!.IsOpen;
}