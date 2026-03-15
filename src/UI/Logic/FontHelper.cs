using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

public static class FontHelper
{
    public static List<string> GetSystemFonts()
    {
        return FontManager.Current.SystemFonts.Select(p => p.Name).OrderBy(f => f).ToList();
    }

    private static List<string>? _libAssaFontsCache;
    private static Dictionary<string, string>? _libAssaToSkiaMapCache;

    /// <summary>
    /// Returns font family names compatible with libass.
    /// Unlike Avalonia/Skia, which surfaces the typographic family name (e.g. "Copperplate Gothic"),
    /// libass on Windows matches against the Win32/GDI family name stored in name ID 1
    /// (e.g. "Copperplate Gothic Bold", "Copperplate Gothic Light").
    /// This method reads name ID 1 directly from each typeface's OpenType 'name' table so
    /// the result is correct on Windows, Linux, and macOS.
    /// Result is cached after the first call.
    /// </summary>
    public static List<string> GetLibAssaFonts() => _libAssaFontsCache ??= BuildLibAssaFonts();

    private static List<string> BuildLibAssaFonts()
    {
        var fonts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skManager = SKFontManager.Default;

        foreach (var familyName in skManager.FontFamilies)
        {
            using var styleSet = skManager.GetFontStyles(familyName);
            for (var i = 0; i < styleSet.Count; i++)
            {
                using var typeface = styleSet.CreateTypeface(i);
                if (typeface == null)
                {
                    continue;
                }

                var name = ReadWin32FamilyName(typeface) ?? typeface.FamilyName;
                if (!string.IsNullOrEmpty(name))
                {
                    fonts.Add(name);
                }
            }
        }

        return fonts.OrderBy(f => f).ToList();
    }

    /// <summary>
    /// Returns the libass-compatible (Win32/GDI, name ID 1) family name for a single typeface.
    /// Falls back to <see cref="SKTypeface.FamilyName"/> when the name table cannot be read.
    /// </summary>
    public static string GetLibAssaFontName(SKTypeface typeface) =>
        ReadWin32FamilyName(typeface) ?? typeface.FamilyName;

    /// <summary>
    /// Given a libass-compatible (Win32/GDI, name ID 1) font family name,
    /// returns the Skia typographic family name (<see cref="SKTypeface.FamilyName"/>)
    /// that <see cref="SKFontManager"/> recognises for the same typeface.
    /// Falls back to <paramref name="libAssaFontName"/> when no match is found.
    /// Result is cached after the first call.
    /// </summary>
    public static string GetSkiaFontNameFromLibAssaFontName(string libAssaFontName)
    {
        if (string.IsNullOrEmpty(libAssaFontName))
        {
            return libAssaFontName;
        }

        return GetLibAssaToSkiaMap().TryGetValue(libAssaFontName, out var skiaName) ? skiaName : libAssaFontName;
    }

    private static Dictionary<string, string> GetLibAssaToSkiaMap() =>
        _libAssaToSkiaMapCache ??= BuildLibAssaToSkiaMap();

    private static Dictionary<string, string> BuildLibAssaToSkiaMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var skManager = SKFontManager.Default;

        foreach (var familyName in skManager.FontFamilies)
        {
            using var styleSet = skManager.GetFontStyles(familyName);
            for (var i = 0; i < styleSet.Count; i++)
            {
                using var typeface = styleSet.CreateTypeface(i);
                if (typeface == null)
                {
                    continue;
                }

                var win32Name = ReadWin32FamilyName(typeface);
                if (!string.IsNullOrEmpty(win32Name) && !map.ContainsKey(win32Name))
                {
                    map[win32Name] = typeface.FamilyName;
                }
            }
        }

        return map;
    }

    /// <summary>
    /// Reads name ID 1 (Win32/GDI family name) from the OpenType 'name' table.
    /// Returns null when the table cannot be read (e.g. for Type1/PostScript fonts
    /// that lack OpenType tables).
    /// </summary>
    private static string? ReadWin32FamilyName(SKTypeface typeface)
    {
        byte[]? data;
        try
        {
            data = typeface.GetTableData(0x6E616D65u); // 'name' table tag
        }
        catch
        {
            // Type1 (.pfb) and other non-OpenType fonts do not have a 'name' table;
            // SkiaSharp throws instead of returning null for these typefaces.
            return null;
        }

        if (data == null || data.Length < 6)
        {
            return null;
        }

        // name table header: format(2) count(2) stringOffset(2)
        var count = (data[2] << 8) | data[3];
        var stringOffset = (data[4] << 8) | data[5];
        string? fallback = null;

        for (var i = 0; i < count; i++)
        {
            var r = 6 + i * 12;
            if (r + 12 > data.Length)
            {
                break;
            }

            int platformId = (data[r] << 8) | data[r + 1];  // 3 = Windows
            int encodingId = (data[r + 2] << 8) | data[r + 3];  // 1 = Unicode BMP
            int languageId = (data[r + 4] << 8) | data[r + 5];
            int nameId = (data[r + 6] << 8) | data[r + 7];  // 1 = Win32 Family Name
            int length = (data[r + 8] << 8) | data[r + 9];
            int strOffset = (data[r + 10] << 8) | data[r + 11];

            if (platformId != 3 || encodingId != 1 || nameId != 1)
            {
                continue;
            }

            var pos = stringOffset + strOffset;
            if (pos + length > data.Length)
            {
                continue;
            }

            var name = Encoding.BigEndianUnicode.GetString(data, pos, length);
            if (languageId == 0x0409)  // English (US) — prefer this over other languages
            {
                return name;
            }

            fallback ??= name;
        }

        return fallback;
    }
}
