using Dalamud;
using Dalamud.Utility;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DeepDungeonTracker;

public class DataText
{
    private IDictionary<TextIndex, (uint, string)> Texts { get; } = new Dictionary<TextIndex, (uint, string)>();

    private IImmutableList<TerritoryType> Territories { get; }

    public DataText()
    {
        var language = DataText.GetLanguage();
        this.LoadItems(language);
        this.LoadEnemies(language);
        this.LoadEnchantments(language);
        this.LoadTraps(language);
        this.Territories = Service.DataManager.GetExcelSheet<TerritoryType>(Service.ClientState.ClientLanguage)!.ToImmutableList();
    }

    private static Language GetLanguage()
    {
        return Service.ClientState.ClientLanguage switch
        {
            ClientLanguage.Japanese => Language.Japanese,
            ClientLanguage.English => Language.English,
            ClientLanguage.German => Language.German,
            ClientLanguage.French => Language.French,
            _ => Language.English,
        };
    }

    private void AddText(TextIndex key, uint id, SeString seString) => this.Texts.Add(key, (id, seString.ToDalamudString().ToString()));

    private void LoadItems(Language language)
    {
        var sheet = Service.DataManager.GameData.Excel.GetSheet<Item>(language);
        var indices = new uint[] { 15422, 23164, 38941 };

        for (var i = 0; i < indices.Length; i++)
        {
            var id = indices[i];
            this.AddText(TextIndex.GelmorranPotsherd + i, id, sheet!.GetRow(id)!.Singular);
        }
    }

    private void LoadEnemies(Language language)
    {
        var sheet = Service.DataManager.Excel.GetSheet<BNpcName>(language);
        var indices = new uint[] { 2566, 6880, 5041, 7610, 10309,
            4986, 4999, 5012, 5025, 5038, 5309, 5321, 5333, 5345, 5356, 5371, 5384, 5397, 5410, 5424, 5438, 5449, 5461, 5471,
            7480, 7481, 7478, 7483, 7485, 7487, 7489, 7490, 7493,
            12240, 12261, 12242, 12263, 12265, 12267, 12246, 12247, 12102, 12100,
            5046, 5047, 5048, 5049, 5050, 5051, 5052, 5053, 5283, 5284, 5285, 5286, 5287, 5288, 5289, 5290, 5291, 5292, 5293, 5294, 5295, 5296, 5297, 5298,
            12322, 12323, 12324};

        for (var i = 0; i < indices.Length; i++)
        {
            var id = indices[i];
            this.AddText(TextIndex.Mimic + i, id, sheet!.GetRow(id)!.Singular);
        }
    }

    private void LoadEnchantments(Language language)
    {
        var sheet = Service.DataManager.GameData.Excel.GetSheet<LogMessage>(language);
        var indices = new uint[] { 7230, 7231, 7232, 7233, 7234, 7235, 7236, 7237, 7238, 7239, 7240, 9211, 9212, 10302 };

        for (var i = 0; i < indices.Length; i++)
        {
            var id = indices[i];
            this.AddText(TextIndex.BlindnessEnchantment + i, id, sheet!.GetRow(id)!.Text);
        }
    }

    private void LoadTraps(Language language)
    {
        var sheet = Service.DataManager.GameData.Excel.GetSheet<LogMessage>(language);
        var indices = new uint[] { 7224, 7225, 7226, 7227, 7228, 9210, 10278 };

        for (var i = 0; i < indices.Length; i++)
        {
            var id = indices[i];
            this.AddText(TextIndex.LandmineTrap + i, id, sheet!.GetRow(id)!.Text);
        }
    }

    private (bool, TextIndex?) IsText(TextIndex start, TextIndex end, string? name, uint? index)
    {
        for (var i = start; i <= end; i++)
        {
            if ((name != null && string.Equals(name, this.Texts[i].Item2, StringComparison.OrdinalIgnoreCase)) || (index != null && this.Texts[i].Item1 == index))
                return (true, i);
        }
        return (false, null);
    }

    public (bool, TextIndex?) IsPotsherd(uint index) => this.IsText(TextIndex.GelmorranPotsherd, TextIndex.OrthosAetherpoolFragment, null, index);

    public (bool, TextIndex?) IsMimic(string name) => this.IsText(TextIndex.Mimic, TextIndex.QuiveringCoffer, name, null);

    public (bool, TextIndex?) IsMandragora(string name) => this.IsText(TextIndex.Pygmaioi, TextIndex.OrthosKorrigan, name, null);

    public (bool, TextIndex?) IsBoss(string name) => this.IsText(TextIndex.PalaceDeathgaze, TextIndex.Excalibur, name, null);

    public (bool, TextIndex?) IsNPC(string name) => this.IsText(TextIndex.DuskwightLancer, TextIndex.NecroseKnight, name, null);

    public (bool, TextIndex?) IsDreadBeast(string name) => this.IsText(TextIndex.LamiaQueen, TextIndex.DemiCochma, name, null);

    public (bool, TextIndex?) IsEnchantment(string name) => this.IsText(TextIndex.BlindnessEnchantment, TextIndex.DemiclonePenaltyEnchantment, name, null);

    public (bool, TextIndex?) IsTrap(string name) => this.IsText(TextIndex.LandmineTrap, TextIndex.OwletTrap, name, null);

    public bool IsPalaceOfTheDeadRegion(uint territoryType) => new uint[] { 56, 1793 }.Contains(this.RegionId(territoryType));

    public bool IsHeavenOnHighRegion(uint territoryType) => new uint[] { 2409, 2775 }.Contains(this.RegionId(territoryType));

    public bool IsEurekaOrthosRegion(uint territoryType) => new uint[] { 67, 2529 }.Contains(this.RegionId(territoryType));

    private uint RegionId(uint territoryType) => this.Territories.FirstOrDefault(x => x.RowId == territoryType)!.PlaceName.Row;
}