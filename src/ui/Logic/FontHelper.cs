using Avalonia.Media;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class FontHelper
{
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
