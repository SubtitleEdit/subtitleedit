using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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

    /// <summary>
    /// The folders font files are searched in: SE's own Fonts folder first, so a collected
    /// font counts as found even when it is not installed, then the platform font folders.
    /// </summary>
    public static List<string> GetFontFolders()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var folders = new List<string> { Se.FontsFolder };

        if (OperatingSystem.IsWindows())
        {
            folders.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folders.Add(Path.Combine(localAppData, "Microsoft", "Windows", "Fonts"));
        }
        else if (OperatingSystem.IsMacOS())
        {
            folders.Add("/System/Library/Fonts");
            folders.Add("/Library/Fonts");
            folders.Add(Path.Combine(home, "Library", "Fonts"));
        }
        else
        {
            folders.Add("/usr/share/fonts");
            folders.Add("/usr/local/share/fonts");
            folders.Add(Path.Combine(home, ".fonts"));
            folders.Add(Path.Combine(home, ".local", "share", "fonts"));
        }

        return folders;
    }

    /// <summary>
    /// Scans font folders and matches each font file's family/face names against
    /// <paramref name="fontNames"/>. Skia has no file-path API, so files are read directly;
    /// both the typographic family name and the Win32/GDI face name (what libass matches)
    /// are checked. Returns a map from font name to the matching files; on a completed scan
    /// every requested name has an entry (empty when not found), on cancellation only the
    /// names matched so far do. <paramref name="onFound"/> fires once per (name, file) match,
    /// on the scanning thread. <paramref name="folders"/> defaults to <see cref="GetFontFolders"/>.
    /// </summary>
    public static Dictionary<string, List<string>> FindFontFiles(
        ICollection<string> fontNames,
        CancellationToken cancellationToken,
        Action<string, string>? onFound = null,
        IEnumerable<string>? folders = null)
    {
        var wanted = new HashSet<string>(fontNames, StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var fontFile in (folders ?? GetFontFolders()).SelectMany(EnumerateFontFiles))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return result; // incomplete - no negative entries for names not scanned to the end
            }

            // A collection (.ttc/.otc) holds several faces; plain files have one at index 0.
            for (var index = 0; index < 30; index++)
            {
                using var typeface = SKTypeface.FromFile(fontFile, index);
                if (typeface == null)
                {
                    break;
                }

                foreach (var name in new[] { typeface.FamilyName, GetLibAssaFontName(typeface) })
                {
                    if (string.IsNullOrEmpty(name) || !wanted.Contains(name))
                    {
                        continue;
                    }

                    if (!result.TryGetValue(name, out var files))
                    {
                        files = new List<string>();
                        result[name] = files;
                    }

                    if (!files.Contains(fontFile, StringComparer.OrdinalIgnoreCase))
                    {
                        files.Add(fontFile);
                        onFound?.Invoke(name, fontFile);
                    }
                }
            }
        }

        foreach (var name in wanted)
        {
            if (!result.ContainsKey(name))
            {
                result[name] = new List<string>();
            }
        }

        return result;
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

/// <summary>A font from SE's Fonts folder: a family/face name plus the file (and face index
/// within a .ttc/.otc collection) it was read from.</summary>
public sealed record CollectedFont(string Name, string FilePath, int FaceIndex);
