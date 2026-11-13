using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System.Globalization;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// The styles of an ASSA/SSA file, for image based export (Blu-ray sup, VobSub, BDN-XML, ...).
/// <see cref="Apply"/> makes a line's style the starting point of its image - font, colours,
/// outline, shadow, box, alignment, margins, scale, spacing and angle - in place of the export
/// window's settings, so a "Sign" style comes out yellow at the top the way a player shows it.
/// Override tags in the text still win: call <see cref="Apply"/> before
/// <see cref="ExportTextTags.ApplyStyleOverrideTags"/> and <see cref="ExportTextTags.ApplyTransparencyTags"/>.
/// </summary>
public sealed class ExportAssaStyles
{
    /// <summary>libass' resolution for a script without PlayResX/PlayResY.</summary>
    private const int DefaultScriptWidth = 384;
    private const int DefaultScriptHeight = 288;

    private readonly Dictionary<string, SsaStyle> _styles;
    private readonly SsaStyle _fallback;
    private readonly int _scriptWidth;
    private readonly int _scriptHeight;

    private ExportAssaStyles(List<SsaStyle> styles, int scriptWidth, int scriptHeight)
    {
        _styles = new Dictionary<string, SsaStyle>(StringComparer.OrdinalIgnoreCase);
        foreach (var style in styles)
        {
            _styles.TryAdd(style.Name.Trim(), style);
        }

        // A line with a style the header does not define is drawn with "Default", like libass.
        _fallback = _styles.TryGetValue("Default", out var defaultStyle) ? defaultStyle : styles[0];

        if (scriptWidth > 0 && scriptHeight > 0)
        {
            _scriptWidth = scriptWidth;
            _scriptHeight = scriptHeight;
        }
        else
        {
            _scriptWidth = DefaultScriptWidth;
            _scriptHeight = DefaultScriptHeight;
        }
    }

    /// <summary>
    /// The styles of an ASSA/SSA header, or null when it defines none.
    /// </summary>
    public static ExportAssaStyles? FromHeader(string? header)
    {
        if (string.IsNullOrEmpty(header) ||
            (!header.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase) &&
             !header.Contains("[V4 Styles]", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
        if (styles.Count == 0)
        {
            return null;
        }

        var (width, height) = ExportTextTags.GetScriptResolution(header);
        return new ExportAssaStyles(styles, width, height);
    }

    /// <summary>True when the file defines a style of that name.</summary>
    public bool HasStyle(string? name)
    {
        var key = name?.Trim().TrimStart('*');
        return !string.IsNullOrEmpty(key) && _styles.ContainsKey(key);
    }

    /// <summary>The style of that name - "*Default" is "Default" - or the fallback style.</summary>
    public SsaStyle GetStyle(string? name)
    {
        var key = name?.Trim().TrimStart('*');
        return !string.IsNullOrEmpty(key) && _styles.TryGetValue(key, out var style) ? style : _fallback;
    }

    /// <summary>
    /// Puts the line's style on the parameter, scaled from the script resolution to the canvas.
    /// <paramref name="text"/> is the line's raw text - a "{\anX}" in it wins over the style's
    /// alignment. The margins are the line's own MarginL/MarginR/MarginV; empty or 0 means the
    /// style's, as in ASSA.
    /// </summary>
    public void Apply(ImageParameter ip, string? styleName, string? text, string? marginLeft = null, string? marginRight = null, string? marginVertical = null)
    {
        var style = GetStyle(styleName);
        var scaleX = ip.ScreenWidth > 0 ? ip.ScreenWidth / (double)_scriptWidth : 1.0;
        var scaleY = ip.ScreenHeight > 0 ? ip.ScreenHeight / (double)_scriptHeight : 1.0;

        ip.FontName = style.FontName;
        ip.FontSize = (float)((double)style.FontSize * scaleY);
        ip.IsBold = style.Bold;
        ip.FontColor = style.Primary;
        ip.OutlineColor = style.Outline;
        ip.ShadowColor = style.Background;
        ip.OutlineWidth = (double)style.OutlineWidth * scaleY;
        ip.ShadowWidth = (double)style.ShadowWidth * scaleY;

        // The text starts out italic/underlined/struck - a "{\i0}" can still turn it off, and a
        // "{\r}" comes back to it.
        ip.IsItalic = style.Italic;
        ip.IsUnderline = style.Underline;
        ip.IsStrikeout = style.Strikeout;

        // BorderStyle 3 is an opaque box per line in the outline colour, the outline width being
        // its padding; libass' BorderStyle 4 is one box around all lines in the shadow colour.
        ip.BoxType = ExportBoxType.None;
        if (style.BorderStyle == "3" || style.BorderStyle == "4")
        {
            var padding = (int)Math.Round(ip.OutlineWidth, MidpointRounding.AwayFromZero);
            ip.BoxType = style.BorderStyle == "3" ? ExportBoxType.BoxPerLine : ExportBoxType.OneBox;
            ip.BackgroundColor = style.BorderStyle == "3" ? style.Outline : style.Background;
            ip.BackgroundCornerRadius = 0;
            ip.BoxPaddingLeft = padding;
            ip.BoxPaddingRight = padding;
            ip.BoxPaddingTop = padding;
            ip.BoxPaddingBottom = padding;
            ip.OutlineWidth = 0;
            ip.ShadowWidth = 0;
        }

        var styleAlignment = string.IsNullOrEmpty(style.Alignment)
            ? ExportAlignment.BottomCenter
            : ExportTextTags.FromDigit(style.Alignment[0], ExportAlignment.BottomCenter);
        ip.Alignment = ExportTextTags.GetAlignment(text, styleAlignment);
        ip.ContentAlignment = ExportContentAlignment.FromAlignment;

        var left = Margin(marginLeft, style.MarginLeft) * scaleX;
        var right = Margin(marginRight, style.MarginRight) * scaleX;
        ip.LeftRightMargin = (int)Math.Round(ip.Alignment switch
        {
            ExportAlignment.TopLeft or ExportAlignment.MiddleLeft or ExportAlignment.BottomLeft => left,
            ExportAlignment.TopRight or ExportAlignment.MiddleRight or ExportAlignment.BottomRight => right,
            _ => (left + right) / 2,
        }, MidpointRounding.AwayFromZero);
        ip.BottomTopMargin = (int)Math.Round(Margin(marginVertical, style.MarginVertical) * scaleY, MidpointRounding.AwayFromZero);

        ip.TagScaleX = (float)style.ScaleX;
        ip.TagScaleY = (float)style.ScaleY;
        ip.TagLetterSpacing = style.Spacing != 0 ? (float)((double)style.Spacing * scaleY) : null;
        ip.TagRotation = (float)style.Angle;

        // "{\fs..}" is in the script's resolution too - the same scale, also without PlayResY.
        ip.TagFontSizeScale = (float)scaleY;
    }

    private static int Margin(string? lineMargin, int styleMargin)
    {
        return int.TryParse(lineMargin, NumberStyles.Integer, CultureInfo.InvariantCulture, out var margin) && margin > 0
            ? margin
            : styleMargin;
    }

    /// <summary>The script resolution the styles and their tags are scaled from.</summary>
    public (int Width, int Height) ScriptResolution => (_scriptWidth, _scriptHeight);
}
