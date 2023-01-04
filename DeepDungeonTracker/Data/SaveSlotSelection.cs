using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeepDungeonTracker
{
    public class SaveSlotSelection
    {
        public record SaveSlotSelectionData(DeepDungeon DeepDungeon = DeepDungeon.None, int SaveSlotNumber = 0);

        private static string FileName => $"_{nameof(SaveSlotSelection)}.json";

        private IDictionary<string, SaveSlotSelectionData> Data { get; }

        private SaveSlotSelectionData? CurrrentSelectionData { get; set; }

        public SaveSlotSelection() => this.Data = LocalStream.Load<Dictionary<string, SaveSlotSelectionData>>(ServiceUtility.ConfigDirectory, SaveSlotSelection.FileName) ?? new();

        public void ResetSelectionData() => this.CurrrentSelectionData = null;

        public void SetSelectionData(DeepDungeon deepDungeon, int saveSlotNumber) => this.CurrrentSelectionData = new(deepDungeon, saveSlotNumber);

        public SaveSlotSelectionData? GetSelectionData(string key)
        {
            if (this.Data.TryGetValue(key, out var value))
            {
                if (this.CurrrentSelectionData == null)
                    this.CurrrentSelectionData = new(value.DeepDungeon, value.SaveSlotNumber);
                return value;
            }
            return null;
        }

#pragma warning disable CA1024
        public IReadOnlyDictionary<string, SaveSlotSelectionData> GetData() => new ReadOnlyDictionary<string, SaveSlotSelectionData>(this.Data);
#pragma warning restore CA1024

        public void Save(string key)
        {
            if (!string.IsNullOrWhiteSpace(key) && key.Length > 3 && this.CurrrentSelectionData != null && this.CurrrentSelectionData.DeepDungeon != DeepDungeon.None)
            {
                if (!this.Data.TryAdd(key, this.CurrrentSelectionData))
                    this.Data[key] = this.CurrrentSelectionData;
            }
            LocalStream.Save(ServiceUtility.ConfigDirectory, SaveSlotSelection.FileName, this.Data).ConfigureAwait(true);
        }
    }
}