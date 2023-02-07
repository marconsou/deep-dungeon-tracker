using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Network;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Linq;

namespace DeepDungeonTracker
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Deep Dungeon Tracker";

        private Commands Commands { get; }

        private WindowSystem WindowSystem { get; }

        private ConfigurationWindow ConfigurationWindow { get; }

        private Configuration Configuration { get; }

        private Data Data { get; }

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface?.Create<Service>();

            this.Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(Service.PluginInterface);

            this.Data = new(this.Configuration);
            this.Commands = new(this.Name, this.OnConfigCommand, this.OnTrackerCommand, this.OnTimeCommand, this.OnScoreCommand, this.OnLoadCommand);

            this.WindowSystem = new(this.Name.Replace(" ", string.Empty, StringComparison.InvariantCultureIgnoreCase));
#pragma warning disable CA2000
            this.ConfigurationWindow = new ConfigurationWindow(this.Name, this.Configuration, this.Data, this.OpenStatisticsWindow);
            this.WindowSystem.AddWindow(this.ConfigurationWindow);
            this.WindowSystem.AddWindow(new TrackerWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new FloorSetTimeWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new ScoreWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new StatisticsWindow(this.Name, this.Configuration, this.Data));
#pragma warning restore CA2000

            Service.PluginInterface.UiBuilder.DisableAutomaticUiHide = false;
            Service.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
            Service.PluginInterface.UiBuilder.DisableGposeUiHide = false;
            Service.PluginInterface.UiBuilder.DisableUserUiHide = false;

            Service.PluginInterface.UiBuilder.Draw += this.Draw;
            Service.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;

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
            if (!this.IsStatisticsWindowOpen())
            {
                if (!this.Data.IsInsideDeepDungeon)
                    this.Data.Common.LoadDeepDungeonData(false, true);

                this.Data.Statistics.Load(this.Data.Common.CurrentSaveSlot, this.OpenStatisticsWindow);
            }
            else
                this.CloseStatisticsWindow();
        }

        private void Draw() => this.WindowSystem.Draw();

        private void OpenConfigUi() => this.ConfigurationWindow.Toggle();

        private void Update(Framework framework)
        {
            Service.PluginInterface.UiBuilder.OverrideGameCursor = !this.Configuration.General.UseInGameCursor;
            this.Data.Update(this.Configuration);
        }

        private void Login(object? sender, EventArgs e) => this.Data.Login();

        private void TerritoryChanged(object? sender, ushort territoryType) => this.Data.TerritoryChanged(territoryType);

        private void ConditionChange(ConditionFlag flag, bool value) => this.Data.ConditionChange(flag, value);

        private void ChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled) => this.Data.ChatMessage(message.TextValue);

        private void NetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (direction == NetworkMessageDirection.ZoneDown)
                this.Data.NetworkMessage(dataPtr, opCode, targetActorId, this.Configuration);
        }

        public bool IsStatisticsWindowOpen() => this.WindowSystem.Windows.FirstOrDefault(x => x is StatisticsWindow)?.IsOpen ?? false;

        public void OpenStatisticsWindow()
        {
            var window = this.WindowSystem.Windows.FirstOrDefault(x => x is StatisticsWindow);
            if (window != null)
                window.IsOpen = true;
        }

        public void CloseStatisticsWindow()
        {
            var window = this.WindowSystem.Windows.FirstOrDefault(x => x is StatisticsWindow);
            if (window != null)
                window.IsOpen = false;
        }
    }
}