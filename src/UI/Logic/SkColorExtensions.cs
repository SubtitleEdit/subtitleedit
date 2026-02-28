using System;
using System.Globalization;
using Avalonia.Media;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Logic;

public static class SkColorExtensions
{
    /// <summary>
    /// Converts an SKColor to a hex string. Default is ARGB (#AARRGGBB).
    /// Set includeAlpha to false for RGB (#RRGGBB).
    /// </summary>
    public static string ToHex(this SKColor color, bool includeAlpha = true)
    {
        return includeAlpha
            ? $"#{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2}" // ARGB
            : $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";               // RGB
    }

    public static Color ToAvaloniaColor(this SKColor color)
    {
        return new Color(color.Alpha, color.Red, color.Green, color.Blue);
    }

    /// <summary>
    /// Converts a hex string (e.g., "#RRGGBB" or "#AARRGGBB") to an SKColor.
    /// </summary>
    public static SKColor FromHex(this string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Invalid hex string.");

        hex = hex.TrimStart('#');

        byte a = 255, r, g, b;

        if (hex.Length == 6)
        {
            // Format: RRGGBB
            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            // Format: AARRGGBB
            a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
        }
        else
        {
            throw new FormatException("Hex string must be 6 (RRGGBB) or 8 (AARRGGBB) characters long.");
        }

        return new SKColor(r, g, b, a);
    }
}