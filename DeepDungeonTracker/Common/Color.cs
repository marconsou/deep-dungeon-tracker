using System.Numerics;

namespace DeepDungeonTracker;

public static class Color
{
    public static Vector4 Black => new(0.0f, 0.0f, 0.0f, 1.0f);

    public static Vector4 Blue => new(0.0f, 0.0f, 1.0f, 1.0f);

    public static Vector4 Bronze => new(0.80078125f, 0.49609375f, 0.1953125f, 1.0f);

    public static Vector4 Cyan => new(0.0f, 1.0f, 1.0f, 1.0f);

    public static Vector4 Default => new(1.0f, 1.0f, 1.0f, 1.0f);

    public static Vector4 Gold => new(1.0f, 0.843137324f, 0.0f, 1.0f);

    public static Vector4 Gray => new(0.501960814f, 0.501960814f, 0.501960814f, 1.0f);

    public static Vector4 Green => new(0.0f, 1.0f, 0.0f, 1.0f);

    public static Vector4 Magenta => new(1.0f, 0.0f, 1.0f, 1.0f);

    public static Vector4 Orange => new(1.0f, 0.647058845f, 0.0f, 1.0f);

    public static Vector4 Platinum => new(0.89453125f, 0.890625f, 0.8828125f, 1.0f);

    public static Vector4 Red => new(1.0f, 0.0f, 0.0f, 1.0f);

    public static Vector4 Silver => new(0.752941251f, 0.752941251f, 0.752941251f, 1.0f);

    public static Vector4 Tan => new(0.823529482f, 0.705882370f, 0.549019635f, 1.0f);

    public static Vector4 Transparent => new(0.0f, 0.0f, 0.0f, 0.0f);

    public static Vector4 Turquoise => new(0.250980407f, 0.878431439f, 0.815686345f, 1.0f);

    public static Vector4 White => new(1.0f, 1.0f, 1.0f, 1.0f);

    public static Vector4 Yellow => new(1.0f, 1.0f, 0.0f, 1.0f);
}