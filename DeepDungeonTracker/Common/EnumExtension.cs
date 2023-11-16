using System;
using System.ComponentModel;

namespace DeepDungeonTracker;

public static class EnumExtension
{
    public static string GetDescription<T>(this T value) where T : Enum
    {
        var description = value.ToString();
        var attributes = value.GetType().GetField(description)?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes?.Length > 0 ? ((attributes[0] as DescriptionAttribute)?.Description ?? description) : description;
    }

    public static string[] GetNames<T>(this T value) where T : Enum
    {
        var methodInfo = typeof(EnumExtension).GetMethod(nameof(EnumExtension.GetDescription))?.MakeGenericMethod(value.GetType());
        var values = Enum.GetValues(typeof(T));
        var names = new string[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            if (methodInfo?.Invoke(null, new[] { values.GetValue(i) }) is string description)
                names[i] = description;
        }
        return names;
    }
}