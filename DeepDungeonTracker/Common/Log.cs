using Dalamud.Plugin.Services;
using System;
using System.Runtime.InteropServices;

namespace DeepDungeonTracker;

public static class Log
{
    public static void Print(IPluginLog log, string message) => log?.Information(message);

    public static void Print(IPluginLog log, IntPtr dataPtr, ushort opCode, string? message = null, bool showBytes = true, bool trimBytes = true, ushort bufferSize = 256)
    {
        var bytes = new byte[bufferSize];
        Marshal.Copy(dataPtr, bytes, 0, bytes.Length);
        log?.Information($"[{opCode:D3}, {opCode:X3}]: {message} {(showBytes ? BitConverter.ToString(bytes).Replace("-", !trimBytes ? " " : "", StringComparison.InvariantCultureIgnoreCase) : string.Empty)}");
    }
}