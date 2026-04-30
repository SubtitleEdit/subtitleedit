using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
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

                foreach (var win32Name in ReadWin32FamilyNames(typeface).Select(p => p.Name))
                {
                    if (!string.IsNullOrEmpty(win32Name) && !map.ContainsKey(win32Name))
                    {
                        map[win32Name] = typeface.FamilyName;
                    }
                }

                if (!string.IsNullOrEmpty(typeface.FamilyName) && !map.ContainsKey(typeface.FamilyName))
                {
                    map[typeface.FamilyName] = typeface.FamilyName;
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
        var names = ReadWin32FamilyNames(typeface);
        if (names.Count == 0)
        {
            return null;
        }

        var culture = CultureInfo.CurrentUICulture;
        var exactCultureName = names.FirstOrDefault(p => p.LanguageId == culture.LCID).Name;
        if (!string.IsNullOrEmpty(exactCultureName))
        {
            return exactCultureName;
        }

        var neutralCultureName = names.FirstOrDefault(p => IsSameNeutralLanguage(p.LanguageId, culture.TwoLetterISOLanguageName)).Name;
        if (!string.IsNullOrEmpty(neutralCultureName))
        {
            return neutralCultureName;
        }

        var englishName = names.FirstOrDefault(p => p.LanguageId == 0x0409).Name;
        if (!string.IsNullOrEmpty(englishName))
        {
            return englishName;
        }

        return names[0].Name;
    }

    private static bool IsSameNeutralLanguage(int languageId, string neutralLanguage)
    {
        try
        {
            return CultureInfo.GetCultureInfo(languageId).TwoLetterISOLanguageName == neutralLanguage;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private static List<(string Name, int LanguageId)> ReadWin32FamilyNames(SKTypeface typeface)
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
            return new List<(string Name, int LanguageId)>();
        }

        if (data == null || data.Length < 6)
        {
            return new List<(string Name, int LanguageId)>();
        }

        // name table header: format(2) count(2) stringOffset(2)
        var count = (data[2] << 8) | data[3];
        var stringOffset = (data[4] << 8) | data[5];
        var names = new List<(string Name, int LanguageId)>();

        for (var i = 0; i < count; i++)
        {
            var r = 6 + i * 12;
            if (r + 12 > data.Length)
            {
                break;
            }

            var platformId = (data[r] << 8) | data[r + 1]; // 3 = Windows
            var encodingId = (data[r + 2] << 8) | data[r + 3]; // 1 = Unicode BMP
            var languageId = (data[r + 4] << 8) | data[r + 5];
            var nameId = (data[r + 6] << 8) | data[r + 7]; // 1 = Win32 Family Name
            var length = (data[r + 8] << 8) | data[r + 9];
            var strOffset = (data[r + 10] << 8) | data[r + 11];

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
            if (!names.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                names.Add((name, languageId));
            }
        }

        return names;
    }
}
