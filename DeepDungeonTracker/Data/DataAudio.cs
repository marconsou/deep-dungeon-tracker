using System.Runtime.InteropServices;

namespace DeepDungeonTracker;

public class DataAudio
{
    private delegate ulong PlaySoundDelegate(byte id, ulong unk1, ulong unk2);

    private PlaySoundDelegate PlaySound_ { get; }

    public DataAudio() => this.PlaySound_ = Marshal.GetDelegateForFunctionPointer<PlaySoundDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 4D 39 BE"));

    public void PlaySound(SoundIndex id) => this.PlaySound_.Invoke((byte)id, 0, 0);
}