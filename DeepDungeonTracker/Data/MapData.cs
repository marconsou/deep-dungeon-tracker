using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker
{
    public class MapData
    {
        [JsonInclude]
        public FloorType FloorType { get; private set; }

        [JsonInclude]
        public IList<int?> RoomIds { get; private set; } = Enumerable.Repeat<int?>(null, MapData.Length * MapData.Length).ToList();

        [JsonIgnore]
        public static int Length => 5;

        private static int GetIndex(int x, int y)
        {
            var min = 0;
            var max = MapData.Length - 1;
            x = Math.Clamp(x, min, max);
            y = Math.Clamp(y, min, max);
            return x + (y * MapData.Length);
        }

        public int? GetId(int x, int y) => this.RoomIds[MapData.GetIndex(x, y)];

        public void Reset()
        {
            for (var i = 0; i < this.RoomIds.Count; i++)
                this.RoomIds[i] = null;
            this.FloorType = FloorType.None;
        }

        public void Update(int x, int y, int? id, FloorType floorType)
        {
            this.RoomIds[MapData.GetIndex(x, y)] = id;
            this.FloorType = floorType;
        }
    }
}