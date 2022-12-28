using System.Collections.Generic;

namespace DeepDungeonTracker
{
    public class SaveSlotSelection
    {
        public record SaveSlotSelectionData(DeepDungeon DeepDungeon = DeepDungeon.None, int SaveSlotNumber = 0);

        private static string FileName => $"_{nameof(SaveSlotSelection)}.json";

        private IDictionary<string, SaveSlotSelectionData> Data { get; set; } = SaveSlotSelection.Load();

        public IDictionary<string, SaveSlotSelectionData> DataList => new Dictionary<string, SaveSlotSelectionData>(this.Data);

        public void Reload() => this.Data = SaveSlotSelection.Load();

        private static IDictionary<string, SaveSlotSelectionData> Load() => LocalStream.Load<Dictionary<string, SaveSlotSelectionData>>(ServiceUtility.ConfigDirectory, SaveSlotSelection.FileName) ?? new();

        public static SaveSlotSelectionData Get(string key) => SaveSlotSelection.Load().TryGetValue(key, out var value) ? value : new();

        public void Save() => LocalStream.Save(ServiceUtility.ConfigDirectory, SaveSlotSelection.FileName, this.Data).ConfigureAwait(true);

        public void AddOrUpdate(string key, SaveSlotSelectionData data)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length <= 3 || data == null || data.DeepDungeon == DeepDungeon.None)
                return;

            if (!this.Data.TryAdd(key, data))
                this.Data[key] = data;
        }
    }
}