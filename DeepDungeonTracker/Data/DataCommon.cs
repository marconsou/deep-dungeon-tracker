using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using System;
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

        public int Score { get; set; }

        public DeepDungeon DeepDungeon { get; private set; }

        private DutyStatus DutyStatus { get; set; }

        public SaveSlot? CurrentSaveSlot { get; private set; }

        public SaveSlotSelection SaveSlotSelection { get; } = new();

        public FloorSetTime FloorSetTime { get; private set; } = new();

        public FloorEffect FloorEffect { get; private set; } = new();

        private string CharacterKey => $"{this.CharacterName}-{this.ServerName}";

        public bool IsInDeepDungeonRegion => this.DeepDungeon != DeepDungeon.None;

        public int ContentId => this.CurrentSaveSlot?.ContentId ?? 0;

        public bool IsLastFloor => this.CurrentSaveSlot?.CurrentFloor()?.IsLastFloor() ?? false;

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
                this.CurrentSaveSlot?.CurrentFloor()?.ScoreUpdate(this.Score - this.CurrentSaveSlot.Score());
            }

            this.SaveCurrentDeepDungeonData();
        }

        public static string GetSaveSlotFileName(string key, SaveSlotSelection.SaveSlotSelectionData data) => $"{key}-dd{(int)data.DeepDungeon}s{data.SaveSlotNumber}.json";

        private string GetSaveSlotFileName(SaveSlotSelection.SaveSlotSelectionData? data = null)
        {
            data ??= this.SaveSlotSelection.GetSelection(this.CharacterKey);
            return DataCommon.GetSaveSlotFileName(this.CharacterKey, data);
        }

        private async void SaveCurrentDeepDungeonData()
        {
            if (this.IsSoloSaveSlot)
                await LocalStream.Save(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName(), this.CurrentSaveSlot);
        }

        private void LoadLastDeepDungeonData()
        {
            var data = this.SaveSlotSelection.GetSelection(this.CharacterKey);
            if (data.DeepDungeon == this.DeepDungeon)
                this.CurrentSaveSlot = LocalStream.Load<SaveSlot>(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName());
        }

        public void CheckForSaveSlotSelection()
        {
            var saveSlotNumber = NodeUtility.SaveSlotNumber(Service.GameGui);
            if (saveSlotNumber != 0)
                this.SaveSlotSelection.AddOrUpdateSelection(this.CharacterKey, new(this.DeepDungeon, saveSlotNumber));
        }

        public void ResetSaveSlotSelection() => this.SaveSlotSelection.AddOrUpdateSelection(this.CharacterKey, new(this.DeepDungeon, 0));

        public void CharacterUpdate()
        {
            var characterName = this.CharacterName;
            if (this.CharacterName.IsNullOrEmpty())
                this.CharacterName = Service.ClientState.LocalPlayer?.Name.ToString() ?? string.Empty;

            var serverName = this.ServerName;
            if (this.ServerName.IsNullOrEmpty())
                this.ServerName = Service.ClientState.LocalPlayer?.HomeWorld.GameData?.Name.ToString() ?? string.Empty;

            if (characterName.IsNullOrEmpty() && !this.CharacterName.IsNullOrEmpty() && serverName.IsNullOrEmpty() && !this.ServerName.IsNullOrEmpty())
                this.LoadLastDeepDungeonData();
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

        public void CheckForMapReveal()
        {
            var currentFloor = this.CurrentSaveSlot?.CurrentFloor();
            if (MapUtility.IsMapFullyRevealed(currentFloor?.MapData ?? new()))
                currentFloor?.MapFullyRevealed();
        }

        public void CheckForTimeBonus()
        {
            if (this.DutyStatus != DutyStatus.None)
                return;

            this.CurrentSaveSlot?.CurrentFloorSet()?.CheckForTimeBonus(this.FloorSetTime.TotalTime);
        }

        public void DeepDungeonUpdate(DataText dataText, ushort territoryType)
        {
            if (dataText.IsThePalaceOfTheDeadRegion(territoryType))
                this.DeepDungeon = DeepDungeon.ThePalaceOfTheDead;
            else if (dataText.IsHeavenOnHighRegion(territoryType))
                this.DeepDungeon = DeepDungeon.HeavenOnHigh;
            else if (dataText.IsEurekaOrthosRegion(territoryType))
                this.DeepDungeon = DeepDungeon.EurekaOrthos;
            else
                this.DeepDungeon = DeepDungeon.None;
        }

        public TimeSpan GetRespawnTime()
        {
            var floorNumber = this.CurrentSaveSlot?.CurrentFloorNumber();

            if (this.IsLastFloor)
                return default;

            if (this.DeepDungeon == DeepDungeon.ThePalaceOfTheDead)
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
            var result = dataText.IsEnchantment(message);
            if (result.Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.Enchantments.Add((Enchantment)(result.Item2! - TextIndex.BlindnessEnchantment));
        }

        public void TrapMessageReceived(DataText dataText, string message)
        {
            var result = dataText.IsTrap(message);
            if (result.Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.Traps.Add((Trap)(result.Item2! - TextIndex.LandmineTrap));
        }

        public void CheckForEnemyKilled(DataText dataText, string name)
        {
            this.CurrentSaveSlot?.CurrentFloor()?.EnemyKilled();
            if (dataText.IsMimic(name).Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.MimicKilled();
            else if (dataText.IsMandragora(name).Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.MandragoraKilled();
            else if (dataText.IsNPC(name).Item1)
                this.CurrentSaveSlot?.CurrentFloor()?.NPCKilled();
        }

        public void CheckForBossKilled(DataText dataText, string name)
        {
            if (dataText.IsBoss(name).Item1)
            {
                this.CurrentSaveSlot?.CurrentFloor()?.EnemyKilled();
                this.DutyCompleted();
            }
        }

        public void CheckForPlayerKilled(Character character)
        {
            if (ServiceUtility.IsSolo)
            {
                if (character.Name.ToString().ToLower() == this.CharacterName.ToLower() && character.CurrentHp == 0)
                    this.CurrentSaveSlot?.CurrentFloor()?.PlayerKilled();
            }
            else
            {
                foreach (var item in Service.PartyList)
                {
                    if (character.Name.ToString().ToLower() == item.Name.ToString().ToLower() && character.CurrentHp == 0)
                    {
                        this.CurrentSaveSlot?.CurrentFloor()?.PlayerKilled();
                        break;
                    }
                }
            }
        }

        public void StartFirstFloor(int contentId)
        {
            void CreateSaveSlot(int floorNumber)
            {
                this.CurrentSaveSlot = new(contentId);
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
                    Task.Delay(1000).ContinueWith(x => this.EnableFlyTextScore = true);
                    if (!LocalStream.Exists(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName()))
                        CreateSaveSlot(floorNumber);
                    else
                    {
                        this.LoadLastDeepDungeonData();
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
                var time = this.FloorSetTime.AddFloor();
                this.CurrentSaveSlot?.CurrentFloor()?.TimeUpdate(time);
                this.CurrentSaveSlot?.CurrentFloor()?.ScoreUpdate(this.Score - this.CurrentSaveSlot.Score());
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
            this.CurrentSaveSlot?.CurrentFloor()?.ScoreUpdate(this.Score - this.CurrentSaveSlot.Score());
            this.SaveCurrentDeepDungeonData();
        }

        public void DutyFailed(int contentId, int dutyFlag)
        {
            var dutyFailed = 2;
            if (this.ContentId == contentId && dutyFlag == dutyFailed)
            {
                this.DutyStatus = DutyStatus.Failed;
                this.FloorSetTime.Pause();
                this.CurrentSaveSlot?.CurrentFloor()?.TimeUpdate(this.FloorSetTime.CurrentFloorTime);
                this.CurrentSaveSlot?.CurrentFloor()?.ScoreUpdate(this.Score - this.CurrentSaveSlot.Score());
                this.CurrentSaveSlot?.CurrentFloorSet()?.NoTimeBonus();
                this.SaveCurrentDeepDungeonData();
            }
        }

        public void RegenPotionConsumed() => this.CurrentSaveSlot?.CurrentFloor()?.RegenPotionConsumed();

        public void GoldCofferPomander(int itemId) => this.CurrentSaveSlot?.CurrentFloor()?.Coffers.Add((Coffer)(itemId - 1));

        public void SilverCofferPomander(int itemId) => this.CurrentSaveSlot?.CurrentFloor()?.Coffers.Add(itemId - 1 + Coffer.InfernoMagicite);

        public void SilverCofferAetherpool()
        {
            if (!this.IsLastFloor)
                this.CurrentSaveSlot?.CurrentFloor()?.Coffers.Add(Coffer.Aetherpool);
        }

        public void PomanderUsed(int itemId)
        {
            var coffer = (Coffer)(itemId - 1);
            if (coffer == Coffer.PomanderOfSafety)
                this.FloorEffect.ShowPomanderOfSafety = true;
            else if (coffer == Coffer.PomanderOfAffluence)
                this.FloorEffect.IsPomanderOfAffluenceUsed = true;
            else if (coffer == Coffer.PomanderOfFlight)
                this.FloorEffect.IsPomanderOfFlightUsed = true;
            else if (coffer == Coffer.PomanderOfAlteration)
                this.FloorEffect.IsPomanderOfAlterationUsed = true;
            else if (coffer == Coffer.PomanderOfSerenity)
                this.CurrentSaveSlot?.CurrentFloor()?.Enchantments.Clear();
        }

        public void TransferenceInitiated() => this.IsTransferenceInitiated = true;

        public void DeleteDeepDungeonSaveData(int saveSlot1Id, int saveSlot2Id)
        {
            void SaveSlotSelectUpdate(int saveSlotNumber)
            {
                var data = SaveSlotSelection.GetSelectionFromFile(this.CharacterKey);
                if (data.DeepDungeon == this.DeepDungeon && data.SaveSlotNumber == saveSlotNumber)
                {
                    this.ResetSaveSlotSelection();
                    this.SaveSlotSelection.Save();
                }
            }

            if (saveSlot1Id == 0)
            {
                LocalStream.Delete(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName(new(this.DeepDungeon, 1)));
                SaveSlotSelectUpdate(1);
            }
            if (saveSlot2Id == 0)
            {
                LocalStream.Delete(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName(new(this.DeepDungeon, 2)));
                SaveSlotSelectUpdate(2);
            }
        }

        public void BronzeCofferUpdate(DataText dataText, int itemId)
        {
            if (this.IsBronzeCofferOpened)
            {
                this.IsBronzeCofferOpened = false;
                var result = dataText.IsPotsherd((uint)itemId);
                if (result.Item1)
                    this.CurrentSaveSlot?.CurrentFloor()?.Coffers.Add(Coffer.Potsherd);
                else
                    this.CurrentSaveSlot?.CurrentFloor()?.Coffers.Add(Coffer.Medicine);
            }
        }

        public void BronzeCofferOpened() => this.IsBronzeCofferOpened = true;
    }
}