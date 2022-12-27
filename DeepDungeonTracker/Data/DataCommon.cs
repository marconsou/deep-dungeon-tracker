using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeepDungeonTracker
{
    public class DataCommon
    {
        private string CharacterName { get; set; } = string.Empty;

        private string ServerName { get; set; } = string.Empty;

        public bool IsTransferenceInitiated { get; private set; }

        private bool IsBronzeCofferOpened { get; set; }

        public bool IsSoloSaveSlot { get; private set; }

        public bool EnableFlyTextScore { get; private set; }

        private bool IsCairnOfPassageActivated { get; set; }

        private HashSet<uint> CairnOfPassageKillIds { get; set; } = new();

        public DeepDungeon DeepDungeon { get; private set; }

        private DutyStatus DutyStatus { get; set; }

        public SaveSlot? CurrentSaveSlot { get; private set; }

        public SaveSlotSelection SaveSlotSelection { get; } = new();

        public FloorSetTime FloorSetTime { get; private set; } = new();

        public FloorEffect FloorEffect { get; private set; } = new();

        public Score? Score { get; private set; }

        private string CharacterKey => $"{this.CharacterName}-{this.ServerName}";

        public bool IsInDeepDungeonRegion => this.DeepDungeon != DeepDungeon.None;

        public int ContentId => this.CurrentSaveSlot?.ContentId ?? 0;

        public bool IsLastFloor => this.CurrentSaveSlot?.CurrentFloor()?.IsLastFloor() ?? false;

        public int TotalScore => this.Score?.TotalScore ?? 0;

        public void ResetCharacterData()
        {
            this.CharacterName = string.Empty;
            this.ServerName = string.Empty;
            this.FloorSetTime = new();
            this.FloorEffect = new();
        }

        public void EnteringDeepDungeon()
        {
            this.IsTransferenceInitiated = false;
            this.IsBronzeCofferOpened = false;
            this.IsSoloSaveSlot = true;
            this.EnableFlyTextScore = false;
            this.IsCairnOfPassageActivated = false;
            this.CairnOfPassageKillIds = new();
            this.DutyStatus = DutyStatus.None;
            this.CurrentSaveSlot?.ContentIdUpdate(0);
        }

        public void ExitingDeepDungeon()
        {
            this.EnableFlyTextScore = false;

            if (this.DutyStatus != DutyStatus.Complete)
                this.CurrentSaveSlot?.KOed();

            if (this.DutyStatus == DutyStatus.None)
            {
                this.FloorSetTime.Pause();
                this.CurrentSaveSlot?.CurrentFloor()?.TimeUpdate(this.FloorSetTime.CurrentFloorTime);
                this.FloorScoreUpdate();
            }
            else
                this.FloorScoreUpdate(this.CurrentSaveSlot?.CurrentFloor()?.Score);

            this.SaveDeepDungeonData();
        }

        public static string GetSaveSlotFileName(string key, SaveSlotSelection.SaveSlotSelectionData data) => data != null ? $"{key}-dd{(int)data.DeepDungeon}s{data.SaveSlotNumber}.json" : string.Empty;

        public string GetSaveSlotFileName(SaveSlotSelection.SaveSlotSelectionData? data)
        {
            data ??= SaveSlotSelection.Get(this.CharacterKey);
            return DataCommon.GetSaveSlotFileName(this.CharacterKey, data);
        }

        private async void SaveDeepDungeonData()
        {
            if (this.IsSoloSaveSlot)
            {
                var data = SaveSlotSelection.Get(this.CharacterKey);
                var fileName = DataCommon.GetSaveSlotFileName(this.CharacterKey, data);
                await LocalStream.Save(ServiceUtility.ConfigDirectory, fileName, this.CurrentSaveSlot).ConfigureAwait(true);
                Log.Print($"Save: {fileName}");
            }
        }

        public SaveSlot? LoadDeepDungeonData(string fileName)
        {
            Log.Print($"Load: {fileName}");
            this.CurrentSaveSlot = LocalStream.Load<SaveSlot>(ServiceUtility.ConfigDirectory, fileName);
            return this.CurrentSaveSlot;
        }

        public void LoadDeepDungeonData(string key, SaveSlotSelection.SaveSlotSelectionData data)
        {
            var fileName = DataCommon.GetSaveSlotFileName(key, data);
            this.LoadDeepDungeonData(fileName);
        }

        public void LoadDeepDungeonData(bool ignoreDeepDungeonRegion = false)
        {
            var data = SaveSlotSelection.Get(this.CharacterKey);
            var fileName = DataCommon.GetSaveSlotFileName(this.CharacterKey, data);
            this.CurrentSaveSlot = ((!ignoreDeepDungeonRegion && (data.DeepDungeon == this.DeepDungeon)) || ignoreDeepDungeonRegion) && (data.SaveSlotNumber != 0) ? this.LoadDeepDungeonData(fileName) : new();
        }

        public void CheckForSaveSlotSelection()
        {
            var saveSlotNumber = NodeUtility.SaveSlotNumber(Service.GameGui);
            if (saveSlotNumber != 0)
                this.SaveSlotSelection.AddOrUpdate(this.CharacterKey, new(this.DeepDungeon, saveSlotNumber));
        }

        public void ResetSaveSlotSelection() => this.SaveSlotSelection.AddOrUpdate(this.CharacterKey, new(this.DeepDungeon, 0));

        public void CharacterUpdate()
        {
            var characterName = this.CharacterName;
            if (string.IsNullOrWhiteSpace(this.CharacterName))
                this.CharacterName = Service.ClientState.LocalPlayer?.Name.ToString() ?? string.Empty;

            var serverName = this.ServerName;
            if (string.IsNullOrWhiteSpace(this.ServerName))
                this.ServerName = Service.ClientState.LocalPlayer?.HomeWorld.GameData?.Name.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(characterName) && !string.IsNullOrWhiteSpace(this.CharacterName) && string.IsNullOrWhiteSpace(serverName) && !string.IsNullOrWhiteSpace(this.ServerName))
            {
                this.LoadDeepDungeonData();
                if (!this.SaveSlotSelection.DataList.ContainsKey(this.CharacterKey) && this.DeepDungeon != DeepDungeon.None)
                {
                    this.SaveSlotSelection.AddOrUpdate(this.CharacterKey, new(this.DeepDungeon));
                    this.SaveSlotSelection.Save();
                }
            }
        }

        public void CheckForSolo()
        {
            if (!this.IsSoloSaveSlot)
                return;

            this.IsSoloSaveSlot = ServiceUtility.IsSolo;
            if (!this.IsSoloSaveSlot)
                this.ResetSaveSlotSelection();
        }

        public void CheckForCharacterStats()
        {
            this.CurrentSaveSlot?.CurrentLevelUpdate();
            var result = NodeUtility.AetherpoolStatus(Service.GameGui);
            if (result.Item1)
                this.CurrentSaveSlot?.AetherpoolUpdate(result.Item2, result.Item3);
        }

        public void CheckForBossKilled(DataText dataText)
        {
            if (!this.IsLastFloor || this.DutyStatus != DutyStatus.None)
                return;

            foreach (var enemy in Service.ObjectTable)
            {
                var character = enemy as Character;
                if (character == null)
                    continue;

                if (character.IsDead && (character.ObjectKind == ObjectKind.BattleNpc) && character.StatusFlags.HasFlag(StatusFlags.Hostile))
                {
                    if (dataText?.IsBoss(character.Name.TextValue).Item1 ?? false)
                    {
                        this.CurrentSaveSlot?.CurrentFloor()?.EnemyKilled();
                        this.DutyCompleted();
                        break;
                    }
                }
            }
        }

        public void CheckForMapReveal()
        {
            var currentFloor = this.CurrentSaveSlot?.CurrentFloor();

            if (this.IsLastFloor || (currentFloor?.Map ?? false))
                return;

            if (MapUtility.IsMapFullyRevealed(currentFloor?.MapData ?? new()))
                currentFloor?.MapFullyRevealed();
        }

        public void CheckForTimeBonus()
        {
            if (this.DutyStatus != DutyStatus.None)
                return;

            this.CurrentSaveSlot?.CurrentFloorSet()?.CheckForTimeBonus(this.FloorSetTime.TotalTime);
        }

        public void CheckForCairnOfPassageActivation(DataText dataText)
        {
            if (this.IsCairnOfPassageActivated || (this.CurrentSaveSlot?.CurrentFloor()?.IsLastFloor() ?? false))
                return;

            foreach (var enemy in Service.ObjectTable)
            {
                var character = enemy as Character;
                if (character == null)
                    continue;

                if (character.IsDead && (character.ObjectKind == ObjectKind.BattleNpc) && (character.StatusFlags.HasFlag(StatusFlags.Hostile) || (dataText?.IsMandragora(character.Name.TextValue).Item1 ?? false)))
                {
                    if (!this.IsCairnOfPassageActivated && this.CairnOfPassageKillIds.Add(character.ObjectId))
                    {
                        this.CurrentSaveSlot?.CurrentFloor()?.CairnOfPassageShines();
                        break;
                    }
                }
            }

            this.IsCairnOfPassageActivated = NodeUtility.CairnOfPassageActivation(Service.GameGui);
        }

        public void CheckForSaveSlotDeletion()
        {
            var result = NodeUtility.SaveSlotDeletion(Service.GameGui);
            var deleteSaveSlots = new bool[] { result.Item1, result.Item2 };
            for (var i = 0; i < deleteSaveSlots.Length; i++)
            {
                if (deleteSaveSlots[i])
                {
                    var saveSlotNumber = i + 1;
                    if (LocalStream.Delete(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName(new(this.DeepDungeon, saveSlotNumber))))
                    {
                        var data = SaveSlotSelection.Get(this.CharacterKey);
                        if (data.DeepDungeon == this.DeepDungeon && data.SaveSlotNumber == saveSlotNumber)
                        {
                            this.ResetSaveSlotSelection();
                            this.SaveSlotSelection.Save();
                        }
                    }
                }
            }
        }

        public void DeepDungeonUpdate(DataText dataText, ushort territoryType)
        {
            var deepDungeon = this.DeepDungeon;
            if (dataText?.IsPalaceOfTheDeadRegion(territoryType) ?? false)
                this.DeepDungeon = DeepDungeon.PalaceOfTheDead;
            else if (dataText?.IsHeavenOnHighRegion(territoryType) ?? false)
                this.DeepDungeon = DeepDungeon.HeavenOnHigh;
            else if (dataText?.IsEurekaOrthosRegion(territoryType) ?? false)
                this.DeepDungeon = DeepDungeon.EurekaOrthos;
            else
                this.DeepDungeon = DeepDungeon.None;

            if (this.DeepDungeon != DeepDungeon.None && this.DeepDungeon != deepDungeon)
                this.LoadDeepDungeonData();
        }

        public TimeSpan GetRespawnTime()
        {
            var floorNumber = this.CurrentSaveSlot?.CurrentFloorNumber();

            if (this.IsLastFloor)
                return default;

            if (this.DeepDungeon == DeepDungeon.PalaceOfTheDead)
            {
                if (floorNumber >= 1 && floorNumber <= 9)
                    return TimeSpan.FromSeconds(40);
                else if (floorNumber >= 11 && floorNumber <= 39)
                    return TimeSpan.FromMinutes(1);
                else if (floorNumber >= 41 && floorNumber <= 49)
                    return TimeSpan.FromMinutes(2);
                else if (floorNumber >= 51 && floorNumber <= 89)
                    return TimeSpan.FromMinutes(1);
                else if (floorNumber >= 91 && floorNumber <= 99)
                    return TimeSpan.FromMinutes(2);
                else if (floorNumber >= 101 && floorNumber <= 149)
                    return TimeSpan.FromMinutes(1.5);
                else if (floorNumber >= 151 && floorNumber <= 199)
                    return TimeSpan.FromMinutes(5);
            }
            else if (this.DeepDungeon == DeepDungeon.HeavenOnHigh)
            {
                if (floorNumber >= 1 && floorNumber <= 29)
                    return TimeSpan.FromMinutes(1);
                else if (floorNumber >= 31 && floorNumber <= 99)
                    return TimeSpan.FromMinutes(10);
            }
            else if (this.DeepDungeon == DeepDungeon.EurekaOrthos)
            {
                if (floorNumber >= 1 && floorNumber <= 29)
                    return TimeSpan.FromMinutes(1);
                else if (floorNumber >= 31 && floorNumber <= 99)
                    return TimeSpan.FromMinutes(10);
            }
            return default;
        }

        public void EnchantmentMessageReceived(DataText dataText, string message)
        {
            var result = dataText?.IsEnchantment(message) ?? new();
            if (result.Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.EnchantmentAffected((Enchantment)(result.Item2! - TextIndex.BlindnessEnchantment));
        }

        public void TrapMessageReceived(DataText dataText, string message)
        {
            var result = dataText?.IsTrap(message) ?? new();
            if (result.Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.TrapTriggered((Trap)(result.Item2! - TextIndex.LandmineTrap));
        }

        public void CheckForEnemyKilled(DataText dataText, string name)
        {
            var currentFloor = this.CurrentSaveSlot?.CurrentFloor();

            currentFloor?.EnemyKilled();
            if (dataText?.IsMimic(name).Item1 ?? false)
                currentFloor?.MimicKilled();
            else if (dataText?.IsMandragora(name).Item1 ?? false)
                currentFloor?.MandragoraKilled();
            else if (dataText?.IsNPC(name).Item1 ?? false)
                currentFloor?.NPCKilled();
        }

        public void CheckForPlayerKilled(Character character)
        {
            if (ServiceUtility.IsSolo)
            {
                if (string.Equals(character?.Name.ToString(), this.CharacterName, StringComparison.OrdinalIgnoreCase) && character?.CurrentHp == 0)
                    this.CurrentSaveSlot?.CurrentFloor()?.PlayerKilled();
            }
            else
            {
                foreach (var item in Service.PartyList)
                {
                    if (string.Equals(character?.Name.ToString(), item.Name.ToString(), StringComparison.OrdinalIgnoreCase) && character?.CurrentHp == 0)
                    {
                        this.CurrentSaveSlot?.CurrentFloor()?.PlayerKilled();
                        break;
                    }
                }
            }
        }

        private void FloorScoreUpdate(int? additional = null) => this.CurrentSaveSlot?.CurrentFloor()?.ScoreUpdate(this.TotalScore - this.CurrentSaveSlot.Score() + (additional ?? 0));

        public void CalculateScore(bool includeFloorCompletion)
        {
            this.Score = ScoreCreator.Create(this.CurrentSaveSlot ?? new(), true);
            this.Score?.TotalScoreCalculation(ServiceUtility.IsSolo, includeFloorCompletion);
        }

        public void StartFirstFloor(int contentId)
        {
            void CreateSaveSlot(int floorNumber)
            {
                this.CurrentSaveSlot = new(this.DeepDungeon, contentId, Service.ClientState?.LocalPlayer?.ClassJob.Id ?? 0);
                this.CurrentSaveSlot.AddFloorSet(floorNumber);
            }

            if (this.ContentId == 0)
            {
                this.FloorEffect = new();
                this.FloorSetTime.Start();
                this.SaveSlotSelection.Save();

                var result = NodeUtility.MapFloorNumber(Service.GameGui);
                var floorNumber = result.Item1 ? result.Item2 : 0;

                if (this.IsSoloSaveSlot)
                {
                    Task.Delay(1000).ContinueWith(x => this.EnableFlyTextScore = true, TaskScheduler.Default);
                    if (!LocalStream.Exists(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName(null)))
                        CreateSaveSlot(floorNumber);
                    else
                    {
                        this.LoadDeepDungeonData();
                        if (this.CurrentSaveSlot?.ContentId != contentId)
                            this.CurrentSaveSlot?.AddFloorSet(floorNumber);
                        else
                            this.CurrentSaveSlot.ResetFloorSet();
                        this.CurrentSaveSlot?.ContentIdUpdate(contentId);
                    }
                }
                else
                    CreateSaveSlot(floorNumber);
            }
        }

        public void StartNextFloor()
        {
            if (this.ContentId != 0 && this.IsTransferenceInitiated)
            {
                this.IsTransferenceInitiated = false;
                this.IsCairnOfPassageActivated = false;
                this.CairnOfPassageKillIds = new();
                var time = this.FloorSetTime.AddFloor();
                this.CurrentSaveSlot?.CurrentFloor()?.TimeUpdate(time);
                this.FloorScoreUpdate();
                this.CurrentSaveSlot?.AddFloor();

                var floorEffect = new FloorEffect
                {
                    ShowPomanderOfAffluence = this.FloorEffect.IsPomanderOfAffluenceUsed,
                    ShowPomanderOfFlight = this.FloorEffect.IsPomanderOfFlightUsed,
                    ShowPomanderOfAlteration = this.FloorEffect.IsPomanderOfAlterationUsed
                };
                this.FloorEffect = floorEffect;
            }
        }

        public void DutyCompleted()
        {
            this.DutyStatus = DutyStatus.Complete;
            this.FloorSetTime.Pause();
            this.CurrentSaveSlot?.CurrentFloor()?.TimeUpdate(this.FloorSetTime.CurrentFloorTime);
            this.FloorScoreUpdate();
            this.SaveDeepDungeonData();
        }

        public void DutyFailed(int contentId, int dutyFlag)
        {
            var dutyFailed = 2;
            if (this.ContentId == contentId && dutyFlag == dutyFailed)
            {
                this.DutyStatus = DutyStatus.Failed;
                this.FloorSetTime.Pause();
                this.CurrentSaveSlot?.CurrentFloor()?.TimeUpdate(this.FloorSetTime.CurrentFloorTime);
                this.FloorScoreUpdate();
                this.CurrentSaveSlot?.CurrentFloorSet()?.NoTimeBonus();
                this.SaveDeepDungeonData();
            }
        }

        public void RegenPotionConsumed() => this.CurrentSaveSlot?.CurrentFloor()?.RegenPotionConsumed();

        public void GoldCofferPomander(int itemId) => this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened((Coffer)(itemId - 1));

        public void SilverCofferPomander(int itemId) => this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(itemId - 1 + Coffer.InfernoMagicite);

        public void SilverCofferAetherpool()
        {
            if (!this.IsLastFloor)
                this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(Coffer.Aetherpool);
        }

        public void PomanderUsed(int itemId)
        {
            itemId--;
            if (!Enum.IsDefined(typeof(Pomander), itemId))
                return;

            var pomander = (Pomander)itemId;

            if (pomander == Pomander.Safety)
                this.FloorEffect.ShowPomanderOfSafety = true;
            else if (pomander == Pomander.Affluence)
                this.FloorEffect.IsPomanderOfAffluenceUsed = true;
            else if (pomander == Pomander.Flight)
                this.FloorEffect.IsPomanderOfFlightUsed = true;
            else if (pomander == Pomander.Alteration)
                this.FloorEffect.IsPomanderOfAlterationUsed = true;

            this.CurrentSaveSlot?.CurrentFloor()?.PomanderUsed(pomander);
        }

        public void MagiciteUsed(int itemId)
        {
            itemId = itemId - 1 + (int)Coffer.InfernoMagicite;
            if (!Enum.IsDefined(typeof(Pomander), itemId))
                return;

            var pomander = (Pomander)itemId;
            this.CurrentSaveSlot?.CurrentFloor()?.PomanderUsed(pomander);
        }

        public void TransferenceInitiated() => this.IsTransferenceInitiated = true;

        public void BronzeCofferUpdate(DataText dataText, int itemId)
        {
            if (this.IsBronzeCofferOpened)
            {
                this.IsBronzeCofferOpened = false;
                var result = dataText?.IsPotsherd((uint)itemId) ?? new();
                if (result.Item1)
                    this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(Coffer.Potsherd);
                else
                    this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(Coffer.Medicine);
            }
        }

        public void BronzeCofferOpened() => this.IsBronzeCofferOpened = true;
    }
}