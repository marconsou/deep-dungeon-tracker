using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Globalization;

namespace DeepDungeonTracker;

public class DataAudio
{
    public void PlaySound(SoundIndex id) => UIModule.PlaySound(Convert.ToUInt32(id, CultureInfo.InvariantCulture));
}