namespace Nikse.SubtitleEdit.UiLogic.BatchConvert;

/// <summary>
/// Batch convert's Transport Stream output settings (SE4's "TS settings..." dialog): how
/// DVB image tracks are placed when exported to an image based format, the file name
/// ending template for each extracted track, and the teletext-only filter.
/// </summary>
public class TransportStreamExportSettings
{
    public const string HAlignLeft = "left";
    public const string HAlignCenter = "center";
    public const string HAlignRight = "right";

    public const string PlaceholderTwoLetter = "{two-letter-country-code}";
    public const string PlaceholderTwoLetterUppercase = "{two-letter-country-code-uppercase}";
    public const string PlaceholderThreeLetter = "{three-letter-country-code}";
    public const string PlaceholderThreeLetterUppercase = "{three-letter-country-code-uppercase}";

    /// <summary>Replace the DVB subtitle's own X with a horizontal alignment + margin.</summary>
    public bool OverrideXPosition { get; set; }

    /// <summary>"left", "center" or "right" (see the HAlign* constants).</summary>
    public string HAlign { get; set; } = HAlignCenter;

    /// <summary>Left/right margin in percent of the screen width (used for left/right alignment).</summary>
    public int HMarginPercent { get; set; } = 5;

    /// <summary>Replace the DVB subtitle's own Y with a bottom margin.</summary>
    public bool OverrideYPosition { get; set; }

    /// <summary>Bottom margin in percent of the screen height.</summary>
    public int BottomMarginPercent { get; set; } = 5;

    /// <summary>Scale the DVB subtitle's screen size (and bitmaps + positions) to ScreenWidth x ScreenHeight.</summary>
    public bool OverrideScreenSize { get; set; }
    public int ScreenWidth { get; set; } = 1920;
    public int ScreenHeight { get; set; } = 1080;

    /// <summary>
    /// Appended to the base file name (before the extension) for every track pulled out of a
    /// Transport Stream. Supports the Placeholder* tokens. Empty = batch convert's regular
    /// language post fix applies instead.
    /// </summary>
    public string FileNameAppend { get; set; } = "." + PlaceholderTwoLetter;

    /// <summary>Skip DVB image tracks - only teletext (and other text) tracks are converted.</summary>
    public bool OnlyTeletext { get; set; }
}
