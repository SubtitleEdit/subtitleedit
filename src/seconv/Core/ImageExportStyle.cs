using System.Reflection;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// Resolved styling for text → image rendering (Blu-Ray sup, VobSub, BDN-XML, ...).
/// Values are resolved in this order: defaults below, then the <c>exportImages</c>
/// section of a <c>--settings</c> JSON file (base section first, then the selected
/// profile's overlay), then individual CLI flags (<c>--font-name</c>, ...).
/// Defaults match seconv's original hardcoded rendering (Arial 50, white text,
/// black outline, no box).
/// </summary>
internal sealed class ImageExportStyle
{
    public string FontName { get; set; } = "Arial";
    public float FontSize { get; set; } = 50;
    public SKColor FontColor { get; set; } = SKColors.White;
    public bool IsBold { get; set; }
    public SKColor OutlineColor { get; set; } = SKColors.Black;
    public double OutlineWidth { get; set; } = 2.5;
    public SKColor ShadowColor { get; set; } = SKColors.Black;
    public double ShadowWidth { get; set; }
    public SKColor BackgroundColor { get; set; } = SKColors.Transparent;
    public double BackgroundCornerRadius { get; set; }

    /// <summary>
    /// Null = auto: a visible <see cref="BackgroundColor"/> turns on <see cref="ExportBoxType.OneBox"/>,
    /// otherwise no box. Setting a background colour without a box type would silently
    /// render nothing, so the auto rule keeps "--background-color black" working alone.
    /// </summary>
    public ExportBoxType? BoxType { get; set; }

    public int BoxPaddingLeft { get; set; } = 5;
    public int BoxPaddingRight { get; set; } = 5;
    public int BoxPaddingTop { get; set; } = 3;
    public int BoxPaddingBottom { get; set; } = 3;

    /// <summary>Extra gap between lines as percent of line height. 0 = single spacing (matches the GUI export default).</summary>
    public int LineSpacingPercent { get; set; }

    public ExportAlignment Alignment { get; set; } = ExportAlignment.BottomCenter;
    public ExportContentAlignment ContentAlignment { get; set; } = ExportContentAlignment.Center;

    /// <summary>Vertical screen-edge margin in pixels. Null = 5% of screen height.</summary>
    public int? BottomTopMargin { get; set; }

    /// <summary>Horizontal screen-edge margin in pixels. Null = 5% of screen width.</summary>
    public int? LeftRightMargin { get; set; }

    public ExportBoxType EffectiveBoxType =>
        BoxType ?? (BackgroundColor.Alpha > 0 ? ExportBoxType.OneBox : ExportBoxType.None);

    /// <summary>
    /// Parses a colour from hex (<c>#AARRGGBB</c>, <c>#RRGGBB</c>, <c>#RGB</c>, with or
    /// without <c>#</c>) or a named SkiaSharp colour (<c>white</c>, <c>black</c>, ...).
    /// </summary>
    public static bool TryParseColor(string value, out SKColor color)
    {
        color = SKColors.Transparent;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var v = value.Trim();
        if (SKColor.TryParse(v, out color))
        {
            return true;
        }

        var field = typeof(SKColors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(f => f.FieldType == typeof(SKColor) &&
                                 f.Name.Equals(v, StringComparison.OrdinalIgnoreCase));
        if (field != null)
        {
            color = (SKColor)field.GetValue(null)!;
            return true;
        }

        return false;
    }

    public static bool TryParseBoxType(string value, out ExportBoxType boxType)
    {
        boxType = ExportBoxType.None;
        return Enum.TryParse(Normalize(value), ignoreCase: true, out boxType) && Enum.IsDefined(boxType);
    }

    public static bool TryParseAlignment(string value, out ExportAlignment alignment)
    {
        alignment = ExportAlignment.BottomCenter;
        return Enum.TryParse(Normalize(value), ignoreCase: true, out alignment) && Enum.IsDefined(alignment);
    }

    public static bool TryParseContentAlignment(string value, out ExportContentAlignment alignment)
    {
        alignment = ExportContentAlignment.Center;
        return Enum.TryParse(Normalize(value), ignoreCase: true, out alignment) && Enum.IsDefined(alignment);
    }

    // Accept "box-per-line" / "box_per_line" / "BoxPerLine" alike.
    private static string Normalize(string value)
    {
        return value.Trim().Replace("-", string.Empty).Replace("_", string.Empty);
    }
}
