using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Globalization;

namespace DeepDungeonTracker;

public class DataAudio
{
#pragma warning disable CA1822
    public void PlaySound(SoundIndex id) => UIModule.PlaySound(Convert.ToUInt32(id, CultureInfo.InvariantCulture));
#pragma warning restore CA1822
}