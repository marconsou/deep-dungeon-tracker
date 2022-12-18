using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DeepDungeonTracker
{
    public class DataOpCodes
    {
        private sealed record Root(string? Region, Lists Lists);

        private sealed record Lists(IImmutableList<ServerZoneIpcType> ServerZoneIpcType);

        private sealed record ServerZoneIpcType(string? Name, ushort OpCode);

        private bool IsUnknownBronzeCofferItemInfoOpCodeFound { get; set; }

        private bool IsUnknownBronzeCofferOpenOpCodeFound { get; set; }

        private ICollection<ushort> KnownOpCodes { get; } = new List<ushort>(100);

        private bool IsKnownOpCode(ushort opCode) => this.KnownOpCodes.Contains(opCode);

        public async void Load(Configuration configuration)
        {
            ushort actorControl = 0;
            ushort actorControlSelf = 0;
            ushort effect = 0;
            ushort eventStart = 0;
            ushort systemLogMessage = 0;

            var result = await NetworkStream.Load<ImmutableList<Root>>(new("https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json"));
            var opCodes = result?.Find(x => x.Region == "Global")?.Lists.ServerZoneIpcType;

            for (var i = 0; i < opCodes?.Count; i++)
            {
                var item = opCodes[i];

                if (item.Name == "ActorControl")
                    actorControl = item.OpCode;
                else if (item.Name == "ActorControlSelf")
                    actorControlSelf = item.OpCode;
                else if (item.Name == "Effect")
                    effect = item.OpCode;
                else if (item.Name == "EventStart")
                    eventStart = item.OpCode;
                else if (item.Name == "SystemLogMessage")
                    systemLogMessage = item.OpCode;

                this.KnownOpCodes.Add(item.OpCode);
            }

            if (actorControl != 0 && actorControlSelf != 0 && effect != 0 && eventStart != 0 && systemLogMessage != 0)
            {
                if (configuration?.OpCodes.ActorControl != actorControl ||
                    configuration.OpCodes.ActorControlSelf != actorControlSelf ||
                    configuration.OpCodes.Effect != effect ||
                    configuration.OpCodes.EventStart != eventStart ||
                    configuration.OpCodes.SystemLogMessage != systemLogMessage)
                {
                    configuration!.OpCodes.ActorControl = actorControl;
                    configuration.OpCodes.ActorControlSelf = actorControlSelf;
                    configuration.OpCodes.Effect = effect;
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

            if (new int[] { 3, 8 }.Contains(NetworkData.ExtractNumber(dataPtr, 0, 1)) && NetworkData.ExtractNumber(dataPtr, 4, 4) == targetActorId && this.IsUnknownBronzeCofferOpenOpCodeFound)
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
}