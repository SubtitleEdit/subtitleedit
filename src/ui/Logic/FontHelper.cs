using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class FontHelper
{
    private static readonly string[] FontFileExtensions = { ".ttf", ".otf", ".ttc", ".otc" };

    /// <summary>
    /// Enumerates the font files (.ttf/.otf/.ttc/.otc) in a folder, recursively.
    /// Missing or inaccessible folders yield nothing.
    /// </summary>
    public static IEnumerable<string> EnumerateFontFiles(string folder)
    {
        if (!Directory.Exists(folder))
        {
            yield break;
        }

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(folder, "*.*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
            });
        }
        catch
        {
            yield break;
        }

        foreach (var file in files)
        {
            if (FontFileExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            {
                yield return file;
            }
        }
    }

    /// <summary>
    /// Returns the family names of the fonts collected in SE's own Fonts folder
    /// (<see cref="Se.FontsFolder"/>) - both the libass-compatible face name and the
    /// typographic family name, so the names match what either renderer would use.
    /// Collections (.ttc/.otc) are enumerated per face.
    /// </summary>
    public static List<string> GetFontsFolderFontNames()
    {
        var names = new List<string>();
        foreach (var file in EnumerateFontFiles(Se.FontsFolder))
        {
            for (var index = 0; index < 30; index++)
            {
                using var typeface = SKTypeface.FromFile(file, index);
                if (typeface == null)
                {
                    break;
                }

                foreach (var name in new[] { GetLibAssaFontName(typeface), typeface.FamilyName })
                {
                    if (!string.IsNullOrEmpty(name) && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        names.Add(name);
                    }
                }
            }
        }

        names.Sort(StringComparer.OrdinalIgnoreCase);
        return names;
    }

    public static List<string> GetSystemFonts()
    {
        return FontManager.Current.SystemFonts.Select(p => p.Name).OrderBy(f => f).ToList();
    }

    /// <summary>
    /// Returns font family names compatible with libass.
    /// Unlike Avalonia/Skia, which surfaces the typographic family name (e.g. "Copperplate Gothic"),
    /// libass on Windows matches against the Win32/GDI family name stored in name ID 1
    /// (e.g. "Copperplate Gothic Bold", "Copperplate Gothic Light").
    /// Implemented by <see cref="FontFaces"/> (libuilogic), which reads name ID 1 directly from
    /// each typeface's OpenType 'name' table so the result is correct on Windows, Linux, and
    /// macOS. Result is cached after the first call.
    /// </summary>
    public static List<string> GetLibAssaFonts() => FontFaces.GetFontFaces();

    /// <summary>
    /// Returns the libass-compatible (Win32/GDI, name ID 1) family name for a single typeface.
    /// Falls back to <see cref="SKTypeface.FamilyName"/> when the name table cannot be read.
    /// </summary>
    public static string GetLibAssaFontName(SKTypeface typeface) => FontFaces.GetFaceName(typeface);

    /// <summary>
    /// Given a libass-compatible (Win32/GDI, name ID 1) font family name,
    /// returns the Skia typographic family name (<see cref="SKTypeface.FamilyName"/>)
    /// that <see cref="SKFontManager"/> recognises for the same typeface.
    /// Falls back to <paramref name="libAssaFontName"/> when no match is found.
    /// Result is cached after the first call.
    /// </summary>
    public static string GetSkiaFontNameFromLibAssaFontName(string libAssaFontName) =>
        FontFaces.GetSkiaFamilyName(libAssaFontName);
}
