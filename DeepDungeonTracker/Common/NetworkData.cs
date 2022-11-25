using System;
using System.Runtime.InteropServices;

namespace DeepDungeonTracker
{
    public static class NetworkData
    {
        public static int ExtractNumber(IntPtr dataPtr, int offset, int size)
        {
            var bytes = new byte[4];
            Marshal.Copy(dataPtr + offset, bytes, 0, size);
            return BitConverter.ToInt32(bytes);
        }

        public static string ExtractString(IntPtr dataPtr, int offset, int size, bool reverseBytes = false)
        {
            var bytes = new byte[size];
            Marshal.Copy(dataPtr + offset, bytes, 0, size);

            if (reverseBytes)
                Array.Reverse(bytes);

            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}