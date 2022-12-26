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
                mapData?.Reset();

                var startX = float.MaxValue;
                var startY = float.MaxValue;
                var endX = float.MinValue;
                var endY = float.MinValue;

                foreach (var node in nodes)
                {
                    startX = Math.Min(startX, node.X);
                    startY = Math.Min(startY, node.Y);
                    endX = Math.Max(endX, node.X + node.Width);
                    endY = Math.Max(endY, node.Y + node.Height);
                }

                foreach (var node in nodes)
                {
                    if (node.PartCount == 16)
                    {
                        int x = (int)(((node.X - startX) + (((node.Width * MapData.Length) / 2.0f) - ((endX - startX) / 2.0f))) / node.Width);
                        int y = (int)(((node.Y - startY) + (((node.Height * MapData.Length) / 2.0f) - ((endY - startY) / 2.0f))) / node.Height);

                        rooms = rooms.Add(new(node.X, node.Y, node.Width, node.Height, (RoomOpening)node.PartId));
                        mapData?.Update(x, y, node.PartId, FloorType.Normal);
                    }
                    else if (node.PartCount == 9)
                    {
                        rooms = rooms.Add(new());
                        mapData?.Update((int)(node.X / node.Width), (int)(node.Y / node.Height), node.PartId, FloorType.HallOfFallacies);
                    }
                }
            }
            else
                return false;

            if (rooms.Count == 0)
                return false;

            if (mapData?.FloorType == FloorType.Normal)
            {
                foreach (var item in rooms)
                {
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Top)!, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X) && (i.Y == item.Y - i.Height)) == null)
                            return false;
                    }
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Right)!, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X + item.Width) && (i.Y == item.Y)) == null)
                            return false;
                    }
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Bottom)!, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X) && (i.Y == item.Y + item.Height)) == null)
                            return false;
                    }
                    if (Enum.GetName(item.RoomOpening)!.Contains(Enum.GetName(RoomOpening.Left)!, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (rooms.FirstOrDefault(i => (i.X == item.X - i.Width) && (i.Y == item.Y)) == null)
                            return false;
                    }
                }
                return true;
            }
            if (mapData?.FloorType == FloorType.HallOfFallacies)
            {
                if (rooms.Count == 12)
                    return true;
            }
            return false;
        }
    }
}