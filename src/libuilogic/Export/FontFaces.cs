using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Enumerates installed fonts as individual faces the way Win32/GDI (and SE4) did:
/// one entry per named face ("Segoe UI", "Segoe UI Semibold", "Arial Black", ...), read
/// from name ID 1 of each typeface's OpenType 'name' table. libass matches these names,
/// so the ASSA style editor uses this list - and the image-based export dialog uses it
/// too so every installed face is selectable (issue #12537). Skia itself groups faces
/// under typographic family names, so resolving a face back to a typeface goes through
/// the face map built here (family + weight/width/slant), never through the name alone.
/// </summary>
public static class FontFaces
{
    private readonly record struct FaceInfo(string SkiaFamily, int Weight, int Width, SKFontStyleSlant Slant);

    private static List<string>? _facesCache;
    private static Dictionary<string, FaceInfo>? _faceMapCache;
    private static readonly object Gate = new();

    /// <summary>All installed font faces by their Win32/GDI (name ID 1) names, sorted.</summary>
    public static List<string> GetFontFaces()
    {
        if (_facesCache == null)
        {
            lock (Gate)
            {
                _facesCache ??= GetFaceMap().Keys.OrderBy(f => f).ToList();
            }
        }

        return _facesCache;
    }

    /// <summary>
    /// The Skia typographic family name for a face name, e.g. "Segoe UI Semibold" -> "Segoe UI".
    /// Falls back to <paramref name="faceName"/> when unknown.
    /// </summary>
    public static string GetSkiaFamilyName(string faceName)
    {
        if (string.IsNullOrEmpty(faceName))
        {
            return faceName;
        }

        return GetFaceMap().TryGetValue(faceName, out var face) ? face.SkiaFamily : faceName;
    }

    /// <summary>The Win32/GDI (name ID 1) face name of a typeface; falls back to the Skia family name.</summary>
    public static string GetFaceName(SKTypeface typeface) =>
        ReadWin32FamilyName(typeface) ?? typeface.FamilyName;

    /// <summary>
    /// Resolves a face name to a typeface, applying bold/italic relative to the face's own
    /// style: "Segoe UI Semibold" + bold matches the family at bold weight or heavier,
    /// "Arial Black" + bold stays black, and italic keeps the face's weight. Unknown names
    /// (an uninstalled font, or a plain family name from an old settings profile) fall back
    /// to Skia's own name matching; returns null only when Skia has no match at all.
    /// </summary>
    public static SKTypeface? CreateTypeface(string faceName, bool bold, bool italic)
    {
        if (!string.IsNullOrWhiteSpace(faceName) && GetFaceMap().TryGetValue(faceName, out var face))
        {
            var weight = bold ? Math.Max(face.Weight, (int)SKFontStyleWeight.Bold) : face.Weight;
            var slant = italic ? SKFontStyleSlant.Italic : face.Slant;
            var typeface = SKFontManager.Default.MatchFamily(face.SkiaFamily, new SKFontStyle(weight, face.Width, slant));
            if (typeface != null)
            {
                return typeface;
            }
        }

        return SKTypeface.FromFamilyName(faceName, new SKFontStyle(
            bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright));
    }

    private static Dictionary<string, FaceInfo> GetFaceMap()
    {
        if (_faceMapCache == null)
        {
            lock (Gate)
            {
                _faceMapCache ??= BuildFaceMap();
            }
        }

        return _faceMapCache;
    }

    private static Dictionary<string, FaceInfo> BuildFaceMap()
    {
        var map = new Dictionary<string, FaceInfo>(StringComparer.OrdinalIgnoreCase);
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
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var candidate = new FaceInfo(typeface.FamilyName, typeface.FontStyle.Weight, typeface.FontStyle.Width, typeface.FontStyle.Slant);

                // Several typefaces share one face name (e.g. "Segoe UI" regular, italic and
                // bold all carry name ID 1 "Segoe UI") - keep the most regular one so the
                // face's base style is what the name implies.
                if (!map.TryGetValue(name, out var existing) || Score(candidate) < Score(existing))
                {
                    map[name] = candidate;
                }
            }
        }

        return map;
    }

    // Distance from "regular": upright before italic, weight near 400, width near normal (5).
    private static int Score(FaceInfo face) =>
        (face.Slant == SKFontStyleSlant.Upright ? 0 : 10_000) +
        Math.Abs(face.Weight - (int)SKFontStyleWeight.Normal) +
        Math.Abs(face.Width - (int)SKFontStyleWidth.Normal) * 100;

    /// <summary>
    /// Reads name ID 1 (Win32/GDI family name) from the OpenType 'name' table.
    /// Returns null when the table cannot be read (e.g. for Type1/PostScript fonts
    /// that lack OpenType tables).
    /// </summary>
    internal static string? ReadWin32FamilyName(SKTypeface typeface)
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
