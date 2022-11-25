using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Network;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;

namespace DeepDungeonTracker
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Deep Dungeon Tracker";

        private static string CommandName => "/ddt";

        private WindowSystem WindowSystem { get; }

        private Configuration Configuration { get; }

        private Data Data { get; }

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            this.Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(Service.PluginInterface);
            this.Data = new(this.Configuration);
            Service.CommandManager.AddHandler(Plugin.CommandName, new CommandInfo(this.OnCommand) { HelpMessage = $"Opens the [{this.Name}] configuration menu." });

            this.WindowSystem = new(this.Name.Replace(" ", string.Empty));
            this.WindowSystem.AddWindow(new ConfigurationWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new TrackerWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new FloorSetTimeWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new ScoreWindow(this.Name, this.Configuration, this.Data));
            this.WindowSystem.AddWindow(new StatisticsWindow(this.Name, this.Configuration, this.Data));

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
            Service.CommandManager.RemoveHandler(Plugin.CommandName);
            this.Data.Dispose();
        }

        private void OnCommand(string command, string args) => this.OpenConfigUi();

        private void Draw() => this.WindowSystem.Draw();

        private void OpenConfigUi() => this.WindowSystem.GetWindow(WindowEx.GetWindowId(this.Name, nameof(ConfigurationWindow)))!.Toggle();

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
    }
}