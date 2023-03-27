using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;

namespace DeepDungeonTracker;

public static class ScreenStream
{
    public static (bool, string) Screenshot(Vector2 position, Vector2 size, string directory, string fileName)
    {
        try
        {
            var rect = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            using var bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen((int)position.X, (int)position.Y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

            if (!LocalStream.Exists(directory))
                Directory.CreateDirectory(directory);

            bitmap.Save(Path.Combine(directory, fileName), ImageFormat.Png);
            return (true, "Screenshot saved!");
        }
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return (false, "Screenshot failed!");
        }
    }
}