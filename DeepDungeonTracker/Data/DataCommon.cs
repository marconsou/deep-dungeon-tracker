using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DeepDungeonTracker;

public sealed class DataCommon : IDisposable
{
    private string CharacterName { get; set; } = string.Empty;

    private string ServerName { get; set; } = string.Empty;

    public bool IsTransferenceInitiated { get; private set; }

    private bool IsBronzeCofferOpened { get; set; }

    public bool IsSoloSaveSlot { get; private set; }

    public bool EnableFlyTextScore { get; private set; }

    private bool IsCairnOfPassageActivated { get; set; }

    public bool ShowFloorSetTimeValues { get; private set; }

    private bool IsBossDead { get; set; }

    private bool WasMagiciteUsed { get; set; }

    public bool ImprovedMagiciteKillsDetection { get; set; }

    private HashSet<uint> CairnOfPassageKillIds { get; set; } = new();

    private Dictionary<uint, Enemy> NearbyEnemies { get; set; } = new();

    public DeepDungeon DeepDungeon { get; private set; }

    private DutyStatus DutyStatus { get; set; }

    public SaveSlot? CurrentSaveSlot { get; private set; }

    public SaveSlotSelection SaveSlotSelection { get; } = new();

    public FloorSetTime FloorSetTime { get; private set; } = new();

    public FloorEffect FloorEffect { get; private set; } = new();

    public BossStatusTimerManager? BossStatusTimerManager { get; set; }

    public Score? Score { get; private set; }

    private string CharacterKey => $"{this.CharacterName}-{this.ServerName}";

    public bool IsInDeepDungeonRegion => this.DeepDungeon != DeepDungeon.None;

    public int ContentId => this.CurrentSaveSlot?.ContentId ?? 0;

    public int TotalScore => this.Score?.TotalScore ?? 0;

    public bool IsLastFloor => this.CurrentSaveSlot?.CurrentFloor()?.IsLastFloor() ?? false;

    private bool IsEurekaOrthosFloor99 => this.DeepDungeon == DeepDungeon.EurekaOrthos && this.CurrentSaveSlot?.CurrentFloorNumber() == 99;

    public bool IsSpecialBossFloor => this.IsEurekaOrthosFloor99;

    public bool IsBossFloor => this.IsLastFloor || this.IsSpecialBossFloor;

    private static Pomander[] SharedPomanders => new[] { Pomander.Safety, Pomander.Sight, Pomander.Strength, Pomander.Steel, Pomander.Affluence, Pomander.Flight, Pomander.Alteration, Pomander.Purity, Pomander.Fortune, Pomander.Witching, Pomander.Serenity, Pomander.Intuition, Pomander.Raising };

    public void Dispose() => this.BossStatusTimerManager?.Dispose();

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
        this.IsBossDead = false;
        this.WasMagiciteUsed = false;
        this.NearbyEnemies = new();
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

    public void EnteringCombat()
    {
        if (this.IsBossFloor)
            this.BossStatusTimerManager = this.CurrentSaveSlot?.CurrentFloorSet()?.StartBossStatusTimer(this.ExitingCombat);
    }

    public void ExitingCombat()
    {
        if (this.IsBossFloor)
            this.CurrentSaveSlot?.CurrentFloorSet()?.EndBossStatusTimer();
    }

    public static string GetSaveSlotFileName(string key, SaveSlotSelection.SaveSlotSelectionData? data) => data != null ? $"{key}-dd{(int)data.DeepDungeon}s{data.SaveSlotNumber}.json" : string.Empty;

    public static string GetLastSaveFileName(string key, SaveSlotSelection.SaveSlotSelectionData? data) => data != null ? $"{key}-dd{(int)data.DeepDungeon}s{data.SaveSlotNumber}Last.json" : string.Empty;

    public string GetSaveSlotFileName(SaveSlotSelection.SaveSlotSelectionData? data) => DataCommon.GetSaveSlotFileName(this.CharacterKey, data);

    public string GetLastSaveFileName(SaveSlotSelection.SaveSlotSelectionData? data) => DataCommon.GetLastSaveFileName(this.CharacterKey, data);

    private void SaveDeepDungeonData()
    {
        if (this.IsSoloSaveSlot)
        {
            var data = this.SaveSlotSelection.GetSelectionData(this.CharacterKey);
            var fileName = DataCommon.GetSaveSlotFileName(this.CharacterKey, data);
            LocalStream.Save(ServiceUtility.ConfigDirectory, fileName, this.CurrentSaveSlot).ConfigureAwait(true);
        }
        else
        {
            this.CurrentSaveSlot = new();
            this.FloorSetTime = new();
        }
    }

    public SaveSlot? LoadDeepDungeonData(bool showFloorSetTimeValues, string fileName)
    {
        this.ShowFloorSetTimeValues = showFloorSetTimeValues;
        this.CurrentSaveSlot = LocalStream.Load<SaveSlot>(ServiceUtility.ConfigDirectory, fileName);
        return this.CurrentSaveSlot;
    }

    public void LoadDeepDungeonData(bool showFloorSetTimeValues, bool ignoreDeepDungeonRegion = false)
    {
        var data = this.SaveSlotSelection.GetSelectionData(this.CharacterKey);
        var fileName = DataCommon.GetSaveSlotFileName(this.CharacterKey, data);
        this.CurrentSaveSlot = ((!ignoreDeepDungeonRegion && (data?.DeepDungeon == this.DeepDungeon)) || ignoreDeepDungeonRegion) && (data?.SaveSlotNumber != 0) ? this.LoadDeepDungeonData(showFloorSetTimeValues, fileName) : new();
    }

    public void CheckForSaveSlotSelection()
    {
        var saveSlotNumber = NodeUtility.SaveSlotNumber(Service.GameGui);
        if (saveSlotNumber != 0)
            this.SaveSlotSelection.SetSelectionData(this.DeepDungeon, saveSlotNumber);
    }

    public void ResetSaveSlotSelection() => this.SaveSlotSelection.SetSelectionData(this.DeepDungeon, 0);

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
            if (this.IsInDeepDungeonRegion)
            {
                this.SaveSlotSelection.ResetSelectionData();
                if (!this.SaveSlotSelection.GetData().ContainsKey(this.CharacterKey))
                {
                    this.SaveSlotSelection.SetSelectionData(this.DeepDungeon, 0);
                    this.SaveSlotSelection.Save(this.CharacterKey);
                }
                this.LoadDeepDungeonData(false);
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
        if (this.IsBossFloor && !this.IsBossDead)
        {
            foreach (var enemy in Service.ObjectTable)
            {
                var character = enemy as Character;
                if (character == null)
                    continue;

                if (character.IsDead && character.ObjectKind == ObjectKind.BattleNpc && character.StatusFlags.HasFlag(StatusFlags.Hostile))
                {
                    if (dataText?.IsBoss(character.Name.TextValue).Item1 ?? false)
                    {
                        this.IsBossDead = true;
                        this.CurrentSaveSlot?.CurrentFloor()?.EnemyKilled();
                        if (this.IsLastFloor)
                            this.DutyCompleted();
                        break;
                    }
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

    public void CheckForBossStatusTimer(bool inCombat)
    {
        if (inCombat && this.IsBossFloor)
        {
            unsafe
            {
                var enemy = Service.ObjectTable.Where(x => ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)x.Address)->GetIsTargetable()).MaxBy(x => (x as Character)?.MaxHp) as BattleChara;
                this.BossStatusTimerManager?.Update(enemy);
            }
        }
    }

    public void CheckForNearbyEnemies(DataText dataText)
    {
        if (this.DeepDungeon != DeepDungeon.HeavenOnHigh || !this.ImprovedMagiciteKillsDetection || this.IsLastFloor)
            return;

        foreach (var enemy in Service.ObjectTable)
        {
            var character = enemy as Character;
            if (character == null)
                continue;

            var name = character.Name.TextValue;
            if ((character.ObjectKind == ObjectKind.BattleNpc) && (character.StatusFlags.HasFlag(StatusFlags.Hostile) || (dataText?.IsMandragora(name).Item1 ?? false)))
                this.NearbyEnemies.TryAdd(character.ObjectId, new() { Name = name, IsDead = character.IsDead });
        }
    }

    private bool CheckForMagiciteKills(DataText dataText, uint id)
    {
        if (this.DeepDungeon != DeepDungeon.HeavenOnHigh || !this.ImprovedMagiciteKillsDetection || this.IsLastFloor)
            return false;

        if (this.WasMagiciteUsed)
        {
            var currentFloor = this.CurrentSaveSlot?.CurrentFloor();

            foreach (var item in this.NearbyEnemies)
            {
                var enemy = item.Value;
                if (!enemy.IsDead)
                {
                    enemy.IsDead = true;
                    this.NearbyEnemies[item.Key] = enemy;

                    currentFloor?.EnemyKilled();
                    if (dataText?.IsMimic(enemy.Name).Item1 ?? false)
                        currentFloor?.MimicKilled();
                    else if (dataText?.IsMandragora(enemy.Name).Item1 ?? false)
                        currentFloor?.MandragoraKilled();
                }
            }
            this.WasMagiciteUsed = false;
            return true;
        }
        else
        {
            if (this.NearbyEnemies.TryGetValue(id, out var enemy))
            {
                if (!enemy.IsDead)
                {
                    enemy.IsDead = true;
                    this.NearbyEnemies[id] = enemy;
                }
                else
                    return true;
            }
        }
        return false;
    }

    public void CheckForSaveSlotDeletion()
    {
        var result = NodeUtility.SaveSlotDeletion(Service.GameGui);
        var moveSaveSlots = new bool[] { result.Item1, result.Item2 };
        for (var i = 0; i < moveSaveSlots.Length; i++)
        {
            if (moveSaveSlots[i])
            {
                var saveSlotNumber = i + 1;
                var saveSlotFileName = this.GetSaveSlotFileName(new(this.DeepDungeon, saveSlotNumber));
                if (LocalStream.Exists(ServiceUtility.ConfigDirectory, saveSlotFileName))
                {
                    if (LocalStream.Move(ServiceUtility.ConfigDirectory, ServiceUtility.ConfigDirectory, saveSlotFileName, this.GetLastSaveFileName(new(this.DeepDungeon, saveSlotNumber))))
                    {
                        var data = this.SaveSlotSelection.GetSelectionData(this.CharacterKey);
                        if (data?.DeepDungeon == this.DeepDungeon && data.SaveSlotNumber == saveSlotNumber)
                        {
                            this.ResetSaveSlotSelection();
                            this.SaveSlotSelection.Save(this.CharacterKey);
                        }
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

        if (this.IsInDeepDungeonRegion && this.DeepDungeon != deepDungeon)
            this.LoadDeepDungeonData(false);
    }

    public TimeSpan GetRespawnTime()
    {
        var floorNumber = this.CurrentSaveSlot?.CurrentFloorNumber();

        if (this.IsBossFloor)
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
            else if (floorNumber >= 31 && floorNumber <= 98)
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

    public void CheckForEnemyKilled(DataText dataText, string name, uint id)
    {
        if (this.CheckForMagiciteKills(dataText, id))
            return;

        var currentFloor = this.CurrentSaveSlot?.CurrentFloor();

        currentFloor?.EnemyKilled();
        if (dataText?.IsMimic(name).Item1 ?? false)
            currentFloor?.MimicKilled();
        else if (dataText?.IsMandragora(name).Item1 ?? false)
            currentFloor?.MandragoraKilled();
        else if (dataText?.IsNPC(name).Item1 ?? false)
            currentFloor?.NPCKilled();
        else if (dataText?.IsDreadBeast(name).Item1 ?? false)
            currentFloor?.DreadBeastKilled();
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

    public void CalculateScore(ScoreCalculationType scoreCalculationType)
    {
        this.Score = ScoreCreator.Create(this.CurrentSaveSlot ?? new(), true);
        this.Score?.TotalScoreCalculation(ServiceUtility.IsSolo, scoreCalculationType);
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
            this.ShowFloorSetTimeValues = true;
            this.FloorSetTime.Start();
            this.SaveSlotSelection.Save(this.CharacterKey);

            var result = NodeUtility.MapFloorNumber(Service.GameGui);
            var floorNumber = result.Item1 ? result.Item2 : 0;

            if (this.IsSoloSaveSlot)
            {
                var saveSlotSelection = this.SaveSlotSelection.GetSelectionData(this.CharacterKey);
                if (!LocalStream.Exists(ServiceUtility.ConfigDirectory, this.GetSaveSlotFileName(saveSlotSelection)))
                    CreateSaveSlot(floorNumber);
                else
                {
                    this.LoadDeepDungeonData(true);
                    if (this.CurrentSaveSlot?.ContentId != contentId)
                    {
                        if (this.CurrentSaveSlot?.CurrentFloorNumber() + 1 == floorNumber)
                            this.CurrentSaveSlot?.AddFloorSet(floorNumber);
                        else
                            CreateSaveSlot(floorNumber);
                    }
                    else
                        this.CurrentSaveSlot.ResetFloorSet();
                    this.CurrentSaveSlot?.ContentIdUpdate(contentId);
                }
                Task.Delay(2000).ContinueWith(x => this.EnableFlyTextScore = true, TaskScheduler.Default);
            }
            else
                CreateSaveSlot(floorNumber);
        }
    }

    public void StartNextFloor()
    {
        if (this.ContentId != 0 && this.IsTransferenceInitiated)
        {
            this.SpecificFloors();
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

    private void SpecificFloors()
    {
        if (this.IsEurekaOrthosFloor99)
        {
            var floor = this.CurrentSaveSlot?.CurrentFloor();
            if (floor?.Kills == 0)
                floor.EnemyKilled();
            if (floor?.CairnOfPassageKills == 0)
                floor.CairnOfPassageShines();
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

    public void PomanderObtained(int itemId)
    {
        Coffer pomander = default;
        if (this.DeepDungeon == DeepDungeon.PalaceOfTheDead || this.DeepDungeon == DeepDungeon.HeavenOnHigh)
        {
            pomander = (Coffer)(itemId - 1);
        }
        else if (this.DeepDungeon == DeepDungeon.EurekaOrthos)
        {
            var totalProtomanders = (int)(Pomander.Dread + 1);
            var index = itemId - totalProtomanders - 1;
            pomander = index >= 0 ? (Coffer)DataCommon.SharedPomanders[itemId - totalProtomanders - 1] : (Coffer)itemId - 1;
        }
        this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(pomander);
    }

    public void AetherpoolObtained()
    {
        if (!this.IsLastFloor)
            this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(Coffer.Aetherpool);
    }

    public void MagiciteObtained(int itemId) => this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(itemId - 1 + Coffer.InfernoMagicite);

    public void DemicloneObtained(int itemId) => this.CurrentSaveSlot?.CurrentFloor()?.CofferOpened(itemId - 1 + Coffer.UneiDemiclone);

    public void PomanderUsed(int itemId)
    {
        itemId--;
        var pomander = (Pomander)itemId;

        if (this.DeepDungeon == DeepDungeon.EurekaOrthos)
        {
            var totalProtomanders = (int)(Pomander.Dread + 1);
            var index = itemId - totalProtomanders;
            pomander = index >= 0 ? DataCommon.SharedPomanders[itemId - totalProtomanders] : (Pomander)itemId;
        }

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
        this.SpecialPomanderUsed(Coffer.InfernoMagicite, itemId);
        this.WasMagiciteUsed = true;
    }

    public void DemicloneUsed(int itemId) => this.SpecialPomanderUsed(Coffer.UneiDemiclone, itemId);

    private void SpecialPomanderUsed(Coffer baseSpecialPomander, int itemId)
    {
        itemId = itemId - 1 + (int)baseSpecialPomander;

        var pomander = (Pomander)itemId;
        this.CurrentSaveSlot?.CurrentFloor()?.PomanderUsed(pomander);
    }

    public void TransferenceInitiated()
    {
        this.WasMagiciteUsed = false;
        this.NearbyEnemies = new();
        this.IsTransferenceInitiated = true;
    }

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