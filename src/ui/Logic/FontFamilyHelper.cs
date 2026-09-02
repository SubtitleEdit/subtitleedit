using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Turns a font name from the settings into a <see cref="FontFamily"/>.
/// Kept out of <see cref="UiUtil"/> on purpose: the subtitle grid's syntax highlighting converter
/// needs this, and UiUtil's static state pulls in enough of Avalonia to abort a headless test run.
/// </summary>
public static class FontFamilyHelper
{
    /// <summary>
    /// "Default" is a placeholder in the appearance settings, not a font family that exists on any
    /// platform - it is what the font drop-downs show for "whatever the system uses". Handing it to
    /// <see cref="FontFamily"/> asks Avalonia for a family nothing can resolve, and that unresolvable
    /// name then travels into its glyph-fallback lookups: on Windows the subtitle grid rendered Arabic
    /// as empty boxes while the same text was fine in the text box, waveform and video (issue #14150).
    /// Map the placeholder (and an empty name) to the platform default instead.
    /// ".AppleSystemUIFont" is deliberately not mapped - on macOS that is the real family name stored
    /// when the user picks "System Font", and the platform default there is Helvetica Neue (#12009).
    /// </summary>
    public static FontFamily Make(string? fontName)
    {
        return string.IsNullOrWhiteSpace(fontName) || fontName == "Default"
            ? FontFamily.Default
            : new FontFamily(fontName);
    }
}
