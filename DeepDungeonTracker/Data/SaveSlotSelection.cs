using System.Collections.Generic;

namespace DeepDungeonTracker
{
    public class SaveSlotSelection
    {
        public record SaveSlotSelectionData(DeepDungeon DeepDungeon = DeepDungeon.None, int SaveSlotNumber = 0);

        private static string FileName => $"_{nameof(SaveSlotSelection)}.json";

        public IDictionary<string, SaveSlotSelectionData>? Data { get; } = SaveSlotSelection.Load();

        private static IDictionary<string, SaveSlotSelectionData>? Load() => LocalStream.Load<Dictionary<string, SaveSlotSelectionData>>(ServiceUtility.ConfigDirectory, SaveSlotSelection.FileName) ?? new();

        public async void Save() => await LocalStream.Save(ServiceUtility.ConfigDirectory, SaveSlotSelection.FileName, this.Data).ConfigureAwait(true);

        public void AddOrUpdateSelection(string key, SaveSlotSelectionData data)
        {
            if (key == null || key.Length <= 3)
                return;

            if (this.Data != null && !this.Data.TryAdd(key, data))
                this.Data[key] = data;
        }

        private static SaveSlotSelectionData Get(string key, IDictionary<string, SaveSlotSelectionData>? data) => (data?.TryGetValue(key, out var value) ?? false) ? value : new();

        public SaveSlotSelectionData GetSelection(string key) => SaveSlotSelection.Get(key, this.Data);

        public static SaveSlotSelectionData GetSelectionFromFile(string key) => SaveSlotSelection.Get(key, SaveSlotSelection.Load());
    }
}