using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using PrintMessageDelegate = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureLogModule.Delegates.PrintMessage;
namespace DeepDungeonTracker.Hook
{
    public unsafe class PrintMessageHook : IDisposable
    {
        private readonly Hook<PrintMessageDelegate>? _getLogMessageHook;
        
        public PrintMessageHook()
        {
            _getLogMessageHook = Service.GameInteropProvider.HookFromAddress<PrintMessageDelegate>(
                RaptureLogModule.MemberFunctionPointers.PrintMessage,
                DetourPrintMessage);
            //_getLogMessageHook.Enable();
        }

        public void Dispose()
        {
            _getLogMessageHook?.Dispose();
        }

        private uint DetourPrintMessage(RaptureLogModule* self, ushort logKindId, Utf8String* senderName, Utf8String* message, int timestamp, bool silent = false)
        {
            Service.ChatGui.Print($"Log message! ({message->ToString()})");
            return _getLogMessageHook.Original(self, logKindId, senderName, message, timestamp, silent);
        }
    }
}