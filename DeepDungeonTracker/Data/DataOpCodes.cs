using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace DeepDungeonTracker;

public class DataOpCodes
{
#pragma warning disable CA1812
    private sealed record Root(string? Region, Lists Lists);

    private sealed record Lists(IImmutableList<ServerZoneIpcType> ServerZoneIpcType);

    private sealed record ServerZoneIpcType(string? Name, ushort OpCode);
#pragma warning restore CA1812

    private bool IsUnknownBronzeCofferItemInfoOpCodeFound { get; set; }

    private bool IsUnknownBronzeCofferOpenOpCodeFound { get; set; }

    private List<ushort> KnownOpCodes { get; } = new List<ushort>(100);

    private static readonly int[] BronzeCofferItemInfoOpCodeData = [3, 8];

    private bool IsKnownOpCode(ushort opCode) => this.KnownOpCodes.Contains(opCode);

    public async Task Load(Configuration configuration)
    {
        ushort actorControl = 0;
        ushort actorControlSelf = 0;
        ushort effectResult = 0;
        ushort eventStart = 0;
        ushort systemLogMessage = 0;

        var uri = "https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json";
        var result = await NetworkStream.Load<ImmutableList<Root>>(new(uri)).ConfigureAwait(true);
        var opCodes = result?.Find(x => x.Region == "Global")?.Lists.ServerZoneIpcType;

        for (var i = 0; i < opCodes?.Count; i++)
        {
            var item = opCodes[i];

            if (item.Name == "ActorControl")
                actorControl = item.OpCode;
            else if (item.Name == "ActorControlSelf")
                actorControlSelf = item.OpCode;
            else if (item.Name == "EffectResult")
                effectResult = item.OpCode;
            else if (item.Name == "EventStart")
                eventStart = item.OpCode;
            else if (item.Name == "SystemLogMessage")
                systemLogMessage = item.OpCode;

            this.KnownOpCodes.Add(item.OpCode);
        }

        if (actorControl != 0 && actorControlSelf != 0 && effectResult != 0 && eventStart != 0 && systemLogMessage != 0)
        {
            if (configuration?.OpCodes.ActorControl != actorControl ||
                configuration.OpCodes.ActorControlSelf != actorControlSelf ||
                configuration.OpCodes.EffectResult != effectResult ||
                configuration.OpCodes.EventStart != eventStart ||
                configuration.OpCodes.SystemLogMessage != systemLogMessage)
            {
                configuration!.OpCodes.ActorControl = actorControl;
                configuration.OpCodes.ActorControlSelf = actorControlSelf;
                configuration.OpCodes.EffectResult = effectResult;
                configuration.OpCodes.EventStart = eventStart;
                configuration.OpCodes.SystemLogMessage = systemLogMessage;
                configuration.Save();
            }
        }
    }

    public void FindUnknownBronzeCofferItemInfoOpCode(IntPtr dataPtr, ushort opCode, uint targetActorId, Configuration configuration)
    {
        if (this.IsUnknownBronzeCofferItemInfoOpCodeFound || this.IsKnownOpCode(opCode))
            return;

        if (DataOpCodes.BronzeCofferItemInfoOpCodeData.Contains(NetworkData.ExtractNumber(dataPtr, 0, 1)) && NetworkData.ExtractNumber(dataPtr, 4, 4) == targetActorId && this.IsUnknownBronzeCofferOpenOpCodeFound)
        {
            this.IsUnknownBronzeCofferItemInfoOpCodeFound = true;
            if (configuration?.OpCodes.UnknownBronzeCofferItemInfo != opCode)
            {
                configuration!.OpCodes.UnknownBronzeCofferItemInfo = opCode;
                configuration.Save();
            }
        }
    }

    public void FindUnknownBronzeCofferOpenOpCode(IntPtr dataPtr, ushort opCode, uint targetActorId, Configuration configuration)
    {
        if (this.IsUnknownBronzeCofferOpenOpCodeFound || this.IsKnownOpCode(opCode))
            return;

        var obj = Service.ObjectTable.SearchById(targetActorId);
        if (obj?.ObjectKind == ObjectKind.Treasure && NetworkData.ExtractString(dataPtr, 6, 2) == "9643")
        {
            this.IsUnknownBronzeCofferOpenOpCodeFound = true;
            if (configuration?.OpCodes.UnknownBronzeCofferOpen != opCode)
            {
                configuration!.OpCodes.UnknownBronzeCofferOpen = opCode;
                configuration.Save();
            }
        }
    }
}