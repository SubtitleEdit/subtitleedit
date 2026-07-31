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
    /// Returns the fonts collected in SE's own Fonts folder (<see cref="Se.FontsFolder"/>) -
    /// each name (both the libass-compatible face name and the typographic family name, so
    /// the names match what either renderer would use) with the file and face index it came
    /// from, so a collected font can be rendered without being installed.
    /// Collections (.ttc/.otc) are enumerated per face.
    /// </summary>
    public static List<CollectedFont> GetFontsFolderFonts()
    {
        var fonts = new List<CollectedFont>();
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
                    if (!string.IsNullOrEmpty(name) &&
                        !fonts.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        fonts.Add(new CollectedFont(name, file, index));
                    }
                }
            }
        }

        fonts.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name));
        return fonts;
    }

    /// <summary>Family names of the fonts collected in SE's own Fonts folder, sorted.</summary>
    public static List<string> GetFontsFolderFontNames() => GetFontsFolderFonts().Select(f => f.Name).ToList();

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

/// <summary>A font from SE's Fonts folder: a family/face name plus the file (and face index
/// within a .ttc/.otc collection) it was read from.</summary>
public sealed record CollectedFont(string Name, string FilePath, int FaceIndex);
