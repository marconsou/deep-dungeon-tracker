using System;
using System.Collections.Immutable;
using System.Linq;

namespace DeepDungeonTracker
{
    public static class MapUtility
    {
        private sealed record Room(float X = 0.0f, float Y = 0.0f, float Width = 0.0f, float Height = 0.0f, RoomOpening RoomOpening = RoomOpening.Special);

        public static bool IsMapFullyRevealed(MapData mapData)
        {
            var nodes = NodeUtility.MapRoom(Service.GameGui);
            IImmutableList<Room> rooms;

            if (nodes != null && nodes.Count > 0)
            {
                rooms = ImmutableArray.Create<Room>();
                mapData.Reset();

                foreach (var node in nodes)
                {
                    if (node.PartCount == 16)
                    {
                        rooms = rooms.Add(new(node.X, node.Y, node.Width, node.Height, (RoomOpening)node.PartId));
                        mapData.Update((int)(node.X / node.Width), (int)(node.Y / node.Height), node.PartId, FloorType.Normal);
                    }
                    else if (node.PartCount == 9)
                    {
                        rooms = rooms.Add(new());
                        mapData.Update((int)(node.X / node.Width), (int)(node.Y / node.Height), node.PartId, FloorType.HallOfFallacies);
                    }
                }
            }
            else
                return false;

            if (rooms.Count == 0)
                return false;

            if (mapData.FloorType == FloorType.Normal)
            {
                foreach (var item in rooms)
                {
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Top)!))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X) && (i.Y == item.Y - i.Height)) == null)
                            return false;
                    }
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Right)!))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X + item.Width) && (i.Y == item.Y)) == null)
                            return false;
                    }
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Bottom)!))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X) && (i.Y == item.Y + item.Height)) == null)
                            return false;
                    }
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Left)!))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X - i.Width) && (i.Y == item.Y)) == null)
                            return false;
                    }
                }
                return true;
            }
            if (mapData.FloorType == FloorType.HallOfFallacies)
            {
                if (rooms.Count == 12)
                    return true;
            }
            return false;
        }
    }
}