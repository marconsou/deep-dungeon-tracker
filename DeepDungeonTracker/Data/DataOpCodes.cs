using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DeepDungeonTracker
{
    public class DataOpCodes
    {
        private record Root(string? Region, Lists Lists);

        private record Lists(IImmutableList<ServerZoneIpcType> ServerZoneIpcType);

        private record ServerZoneIpcType(string? Name, ushort OpCode);

        private bool IsUnknownDeepDungeonSaveDataOpCodeFound { get; set; }

        private bool IsUnknownBronzeCofferItemInfoOpCodeFound { get; set; }

        private bool IsUnknownBronzeCofferOpenOpCodeFound { get; set; }

        private IDictionary<ushort, (int, int)> UnknownDeepDungeonSaveDataOpCodes { get; } = new Dictionary<ushort, (int, int)>();

        private ICollection<ushort> KnownOpCodes { get; } = new List<ushort>(100);

        private bool IsKnownOpCode(ushort opCode) => this.KnownOpCodes.Contains(opCode);

        public async void Load(Configuration configuration)
        {
            ushort actorControl = 0;
            ushort actorControlSelf = 0;
            ushort effect = 0;
            ushort eventStart = 0;
            ushort systemLogMessage = 0;

            var result = await NetworkStream.Load<ImmutableList<Root>>("https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json");
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
                if (configuration.OpCodes.ActorControl != actorControl ||
                    configuration.OpCodes.ActorControlSelf != actorControlSelf ||
                    configuration.OpCodes.Effect != effect ||
                    configuration.OpCodes.EventStart != eventStart ||
                    configuration.OpCodes.SystemLogMessage != systemLogMessage)
                {
                    configuration.OpCodes.ActorControl = actorControl;
                    configuration.OpCodes.ActorControlSelf = actorControlSelf;
                    configuration.OpCodes.Effect = effect;
                    configuration.OpCodes.EventStart = eventStart;
                    configuration.OpCodes.SystemLogMessage = systemLogMessage;
                    configuration.Save();
                }
            }
        }

        public void FindUnknownDeepDungeonSaveDataOpCode(IntPtr dataPtr, ushort opCode, uint targetActorId)
        {
            if (this.IsUnknownDeepDungeonSaveDataOpCodeFound || this.IsKnownOpCode(opCode))
                return;

            if (targetActorId == Service.ClientState.LocalPlayer?.ObjectId && NetworkData.ExtractNumber(dataPtr, 2, 1) > 0 && NetworkData.ExtractString(dataPtr, 3, 5).All(x => x == '0'))
            {
                var aetherpoolArm = NetworkData.ExtractNumber(dataPtr, 0, 1);
                var aetherpoolArmor = NetworkData.ExtractNumber(dataPtr, 1, 1);
                this.UnknownDeepDungeonSaveDataOpCodes.TryAdd(opCode, (aetherpoolArm, aetherpoolArmor));
            }
        }

        public void CheckForUnknownDeepDungeonSaveDataOpCode(Configuration configuration)
        {
            if (this.IsUnknownDeepDungeonSaveDataOpCodeFound)
                return;

            var result = NodeUtility.AetherpoolMenu(Service.GameGui);
            if (result.Item1)
            {
                foreach (var item in this.UnknownDeepDungeonSaveDataOpCodes)
                {
                    if (item.Value.Item1 == result.Item2 && item.Value.Item2 == result.Item3)
                    {
                        this.IsUnknownDeepDungeonSaveDataOpCodeFound = true;
                        if (configuration.OpCodes.UnknownDeepDungeonSaveData != item.Key)
                        {
                            configuration.OpCodes.UnknownDeepDungeonSaveData = item.Key;
                            configuration.Save();
                        }
                        return;
                    }
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
                if (configuration.OpCodes.UnknownBronzeCofferItemInfo != opCode)
                {
                    configuration.OpCodes.UnknownBronzeCofferItemInfo = opCode;
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
                if (configuration.OpCodes.UnknownBronzeCofferOpen != opCode)
                {
                    configuration.OpCodes.UnknownBronzeCofferOpen = opCode;
                    configuration.Save();
                }
            }
        }
    }
}