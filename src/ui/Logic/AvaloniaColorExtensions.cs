using System;
using System.Globalization;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Logic;

public static class AvaloniaColorExtensions
{
    /// <summary>
    /// Converts an Color to a hex string. Default is ARGB (#AARRGGBB).
    /// Set includeAlpha to false for RGB (#RRGGBB).
    /// </summary>
    public static string FromColorToHex(this Avalonia.Media.Color color, bool includeAlpha = true)
    {
        return includeAlpha
            ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}" // ARGB
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}";            // RGB
    }

    /// <summary>
    /// Converts a hex string (e.g., "#RRGGBB" or "#AARRGGBB") to a Color.
    /// </summary>
    public static Avalonia.Media.Color FromHexToColor(this string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Invalid hex string.");
        }

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

        return new Avalonia.Media.Color(a, r, g, b);
    }

    /// <summary>
    /// Converts an Avalonia Color to an SKColor.
    /// </summary>
    public static SKColor ToSkColor(this Avalonia.Media.Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}