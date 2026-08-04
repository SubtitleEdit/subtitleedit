using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Prepares subtitle text for image based export (Blu-ray sup, VobSub, BDN-XML, ...).
/// <para>
/// <see cref="ImageRenderer"/> only understands HTML style tags, so ASSA override tags used
/// to reach it as literal glyphs - "{\an8}Hi" was drawn as "{\an8}Hi" at the bottom of the
/// frame (issue #13025). <see cref="GetAlignment"/> and <see cref="ApplyPositionTag"/> read
/// the position out of the text and <see cref="ToRenderableText"/> turns the rest into what
/// the renderer can draw.
/// </para>
/// </summary>
public static class ExportTextTags
{
    // "{\pos(10,20)}", also inside a multi tag block and with decimals ("{\an8\pos(10.5,20)}")
    private static readonly Regex PositionTagRegex = new(
        @"\\pos\(\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*\)",
        RegexOptions.Compiled);

    /// <summary>
    /// Returns the alignment from a leading "{\anX}" tag - also inside a multi tag block
    /// like "{\an8\i1}" or "{\pos(10,20)\an8}" - or <paramref name="fallback"/> when there is none.
    /// </summary>
    public static ExportAlignment GetAlignment(string? text, ExportAlignment fallback)
    {
        if (string.IsNullOrEmpty(text))
        {
            return fallback;
        }

        var s = text.TrimStart();
        if (s.Length < 5 || s[0] != '{')
        {
            return fallback;
        }

        var blockEnd = s.IndexOf('}');
        if (blockEnd < 0)
        {
            return fallback;
        }

        var block = s.Substring(0, blockEnd);

        var idx = block.IndexOf("\\an", StringComparison.Ordinal);
        if (idx >= 0 && idx + 3 < block.Length)
        {
            return FromDigit(block[idx + 3], fallback);
        }

        // Malformed variant with the leading backslash missing ("{an8\i1}") - stripped by
        // HtmlUtil.RemoveAssAlignmentTags, so read it here too.
        if (block.StartsWith("{an", StringComparison.Ordinal) && block.Length > 3)
        {
            return FromDigit(block[3], fallback);
        }

        return fallback;
    }

    /// <summary>
    /// Converts the text to what <see cref="ImageRenderer"/> can draw: alignment tags are
    /// removed (already read by <see cref="GetAlignment"/>), ASSA italic/bold/color/font tags
    /// become their HTML equivalents, and anything left over is dropped rather than drawn.
    /// </summary>
    public static string ToRenderableText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var s = HtmlUtil.RemoveAssAlignmentTags(text);

        // Only ASSA input pays for this - and only ASSA input may lose a lone "\n"/"\h" to
        // RemoveSsaTags below, which would be wrong for e.g. a plain "C:\new" in an SRT.
        // Tested on the original text: removing the alignment tags can be what takes the
        // last "{\" away.
        if (text.Contains("{\\", StringComparison.Ordinal))
        {
            // "{\i1}" -> "<i>", "{\b1}" -> "<b>", "{\c&H0000FF&}" -> "<font color=\"#ff0000\">", ...
            s = AdvancedSubStationAlpha.GetFormattedText(s);

            // Whatever GetFormattedText left behind: tags it does not translate, and every tag
            // on a line it considers too complex (\pos, \fad, \k, \clip, drawings) - it returns
            // those unchanged, and drawn as text they are worse than not drawn at all.
            s = Utilities.RemoveSsaTags(s);

            // "{\an8\an8}" leaves an empty block behind, which RemoveSsaTags keeps (no backslash).
            s = s.Replace("{}", string.Empty);
        }

        // The renderer has no underline, so "<u>" would be drawn as literal text.
        return HtmlUtil.RemoveOpenCloseTags(s, HtmlUtil.TagUnderline);
    }

    /// <summary>
    /// Reads the anchor point of a "{\pos(x,y)}" tag, in script coordinates.
    /// </summary>
    public static bool TryGetPosition(string? text, out float x, out float y)
    {
        x = 0;
        y = 0;

        if (string.IsNullOrEmpty(text) || !text.Contains("\\pos(", StringComparison.Ordinal))
        {
            return false;
        }

        var match = PositionTagRegex.Match(text);
        return match.Success
               && float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x)
               && float.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
    }

    /// <summary>
    /// Sets <see cref="ImageParameter.OverridePosition"/> from a "{\pos(x,y)}" tag, if there is one.
    /// <para>
    /// Call this after the bitmap has been rendered: "\pos" gives the anchor point of the text
    /// for the current alignment ("{\an8\pos(x,y)}" puts x,y at the top center of the text), so
    /// the bitmap's top left corner can only be worked out once its size is known.
    /// </para>
    /// <para>
    /// The coordinates are in the script's own resolution - pass PlayResX/PlayResY as
    /// <paramref name="scriptWidth"/>/<paramref name="scriptHeight"/> (see
    /// <see cref="GetScriptResolution"/>) when exporting to a different canvas size.
    /// </para>
    /// </summary>
    public static void ApplyPositionTag(ImageParameter ip, string? text, int scriptWidth = 0, int scriptHeight = 0)
    {
        if (!TryGetPosition(text, out var x, out var y) || ip.ScreenWidth <= 0 || ip.ScreenHeight <= 0)
        {
            return;
        }

        if (scriptWidth > 0 && scriptHeight > 0)
        {
            x = x * ip.ScreenWidth / scriptWidth;
            y = y * ip.ScreenHeight / scriptHeight;
        }

        var width = ip.Bitmap?.Width ?? 0;
        var height = ip.Bitmap?.Height ?? 0;

        var left = ip.Alignment switch
        {
            ExportAlignment.TopLeft or ExportAlignment.MiddleLeft or ExportAlignment.BottomLeft => x,
            ExportAlignment.TopRight or ExportAlignment.MiddleRight or ExportAlignment.BottomRight => x - width,
            _ => x - width / 2f,
        };

        var top = ip.Alignment switch
        {
            ExportAlignment.TopLeft or ExportAlignment.TopCenter or ExportAlignment.TopRight => y,
            ExportAlignment.MiddleLeft or ExportAlignment.MiddleCenter or ExportAlignment.MiddleRight => y - height / 2f,
            _ => y - height,
        };

        // Every handler ignores an override position that falls outside the frame, and a
        // partly outside image would be cut off, so keep the whole bitmap on screen.
        var maxLeft = Math.Max(0, ip.ScreenWidth - width);
        var maxTop = Math.Max(0, ip.ScreenHeight - height);

        ip.OverridePosition = new SKPointI(
            (int)Math.Round(Math.Clamp(left, 0, maxLeft), MidpointRounding.AwayFromZero),
            (int)Math.Round(Math.Clamp(top, 0, maxTop), MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// PlayResX/PlayResY from an ASSA/SSA header, or (0,0) when the header has none - "\pos"
    /// coordinates are relative to those.
    /// </summary>
    public static (int Width, int Height) GetScriptResolution(string? header)
    {
        if (string.IsNullOrEmpty(header))
        {
            return (0, 0);
        }

        var w = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", header);
        var h = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", header);

        if (int.TryParse(w, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) && width > 0 &&
            int.TryParse(h, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) && height > 0)
        {
            return (width, height);
        }

        return (0, 0);
    }

    private static ExportAlignment FromDigit(char digit, ExportAlignment fallback)
    {
        return digit switch
        {
            '1' => ExportAlignment.BottomLeft,
            '2' => ExportAlignment.BottomCenter,
            '3' => ExportAlignment.BottomRight,
            '4' => ExportAlignment.MiddleLeft,
            '5' => ExportAlignment.MiddleCenter,
            '6' => ExportAlignment.MiddleRight,
            '7' => ExportAlignment.TopLeft,
            '8' => ExportAlignment.TopCenter,
            '9' => ExportAlignment.TopRight,
            _ => fallback,
        };
    }
}
