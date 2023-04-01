using Dalamud.Game.Command;
using System;
using static Dalamud.Game.Command.CommandInfo;

namespace DeepDungeonTracker;

public sealed class Commands : IDisposable
{
    private static string ConfigCommand => "/ddt";

    private static string MainCommand => $"{Commands.ConfigCommand}main";

    private static string TrackerCommand => $"{Commands.ConfigCommand}tracker";

    private static string TimeCommand => $"{Commands.ConfigCommand}time";

    private static string ScoreCommand => $"{Commands.ConfigCommand}score";

    private static string LoadCommand => $"{Commands.ConfigCommand}load";

    public Commands(string pluginName, HandlerDelegate onConfigCommand, HandlerDelegate onMainCommand, HandlerDelegate onTrackerCommand, HandlerDelegate onTimeCommand, HandlerDelegate onScoreCommand, HandlerDelegate onLoadCommand)
    {
        Service.CommandManager.AddHandler(Commands.ConfigCommand, new CommandInfo(onConfigCommand) { HelpMessage = $"Opens the {pluginName} configuration menu." });
        Service.CommandManager.AddHandler(Commands.MainCommand, new CommandInfo(onMainCommand) { HelpMessage = "Opens the Main Window showing saved files and backups." });
        Service.CommandManager.AddHandler(Commands.TrackerCommand, new CommandInfo(onTrackerCommand) { HelpMessage = "Toggles the Tracker Window visibility." });
        Service.CommandManager.AddHandler(Commands.TimeCommand, new CommandInfo(onTimeCommand) { HelpMessage = "Toggles the Floor Set Time Window visibility." });
        Service.CommandManager.AddHandler(Commands.ScoreCommand, new CommandInfo(onScoreCommand) { HelpMessage = "Toggles the Score Window visibility." });
        Service.CommandManager.AddHandler(Commands.LoadCommand, new CommandInfo(onLoadCommand) { HelpMessage = "Loads the last saved slot and opens the Statistics Window." });
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(Commands.ConfigCommand);
        Service.CommandManager.RemoveHandler(Commands.MainCommand);
        Service.CommandManager.RemoveHandler(Commands.TrackerCommand);
        Service.CommandManager.RemoveHandler(Commands.TimeCommand);
        Service.CommandManager.RemoveHandler(Commands.ScoreCommand);
        Service.CommandManager.RemoveHandler(Commands.LoadCommand);
    }
}