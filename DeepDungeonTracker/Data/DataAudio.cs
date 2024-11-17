using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Globalization;

namespace DeepDungeonTracker;

public class DataAudio
{
    public void PlaySound(SoundIndex id) => UIGlobals.PlaySoundEffect(Convert.ToUInt32(id, CultureInfo.InvariantCulture));
}