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

    // "{\alpha&H80&}" and the per part "{\1a&H80&}" (fill), "{\3a}" (outline), "{\4a}" (shadow)
    private static readonly Regex AlphaTagRegex = new(
        @"\\(alpha|1a|3a|4a)&H([0-9A-Fa-f]{1,2})&",
        RegexOptions.Compiled);

    // "{\3c&H0000FF&}" (outline colour) and "{\4c&H0000FF&}" (shadow colour) - up to eight
    // digits, because some tools write the colour with an alpha in front (&HAABBGGRR&).
    private static readonly Regex OutlineShadowColorTagRegex = new(
        @"\\([34]c)&H([0-9A-Fa-f]{1,8})&",
        RegexOptions.Compiled);

    // "{\bord4}"/"{\shad0}", also with decimals ("{\bord2.5}"). Does not match the one axis
    // variants "\xbord"/"\ybord"/"\xshad"/"\yshad", which nothing here consumes - the
    // backslash has to sit right in front of the tag name.
    private static readonly Regex OutlineShadowWidthTagRegex = new(
        @"\\(bord|shad)(\d+(?:\.\d+)?)",
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
            // The position, fade and transparency tags have already been read off the text
            // (GetAlignment, ApplyPositionTag, ApplyTransparencyTags) - but left in, any one
            // of them makes GetFormattedText declare the whole line "too complex" and return
            // every tag untranslated, so "{\i1\pos(10,20)}Hi" kept its position and lost its
            // italic. Strip what has been consumed; the rest of the line can still convert.
            s = RemoveConsumedTags(s);

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
    /// Reads the per line outline/shadow overrides off the text and puts them on the
    /// parameter: the colours of "{\3c&amp;H..&amp;}" (outline) and "{\4c&amp;H..&amp;}"
    /// (shadow), and the widths of "{\bord..}" and "{\shad..}" - "{\bord0}" in particular is
    /// how ASSA subtitles turn the outline off for one line.
    /// <para>
    /// Call before <see cref="ApplyTransparencyTags"/>: "\3a"/"\4a" fade whatever colours are
    /// on the parameter, so the "\3c"/"\4c" colours have to be there first. The widths are in
    /// the script's own resolution, like "\pos" - pass PlayResY as
    /// <paramref name="scriptHeight"/> (see <see cref="GetScriptResolution"/>) to scale them
    /// to the canvas.
    /// </para>
    /// </summary>
    public static void ApplyStyleOverrideTags(ImageParameter ip, string? text, int scriptHeight = 0)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains("{\\", StringComparison.Ordinal))
        {
            return;
        }

        // "{\fs..}" becomes "<font size=..>" for the renderer, and like "\pos" and "\bord" its
        // value is in the script's resolution.
        ip.TagFontSizeScale = scriptHeight > 0 && ip.ScreenHeight > 0 ? ip.ScreenHeight / (float)scriptHeight : 1f;

        // Inside a "\t(..)" block these tags are an animation target, not the line's look -
        // leave the whole line alone rather than freezing it at its final state.
        if (text.Contains("\\t(", StringComparison.Ordinal))
        {
            return;
        }

        foreach (Match match in OutlineShadowColorTagRegex.Matches(text))
        {
            // ASSA colours are &HBBGGRR& - blue first. Longer values carry an alpha up front,
            // which the "\3a"/"\4a" tags own, so only the low six digits are read here.
            var bgr = Convert.ToUInt32(match.Groups[2].Value, 16) & 0xFFFFFF;
            var color = new SKColor((byte)(bgr & 0xFF), (byte)((bgr >> 8) & 0xFF), (byte)((bgr >> 16) & 0xFF));
            if (match.Groups[1].Value == "3c")
            {
                ip.OutlineColor = color.WithAlpha(ip.OutlineColor.Alpha);
            }
            else
            {
                ip.ShadowColor = color.WithAlpha(ip.ShadowColor.Alpha);
            }
        }

        // Like "\pos", the widths scale with the script resolution when exporting to a
        // different canvas size.
        var scale = scriptHeight > 0 && ip.ScreenHeight > 0 ? ip.ScreenHeight / (double)scriptHeight : 1.0;
        foreach (Match match in OutlineShadowWidthTagRegex.Matches(text))
        {
            var width = double.Parse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture) * scale;
            if (match.Groups[1].Value == "bord")
            {
                ip.OutlineWidth = width;
            }
            else
            {
                ip.ShadowWidth = width;
            }
        }
    }

    /// <summary>
    /// Reads the ASSA transparency tags off the text and puts them on the parameter: the fade
    /// curve of "{\fad(..)}"/"{\fade(..)}" (used by the Blu-ray sup writer, which can animate
    /// its palette) and the static transparency of "{\alpha&amp;H80&amp;}" and its per part
    /// "{\1a}", "{\3a}", "{\4a}" variants.
    /// <para>
    /// Has to run before the bitmap is rendered - unlike <see cref="ApplyPositionTag"/>, this
    /// changes what is drawn. A transparency that applies to text, outline and shadow alike is
    /// kept for the finished bitmap, where one blend gives the exact alpha asked for; per part
    /// transparencies go on the colours instead, so the parts can differ.
    /// </para>
    /// </summary>
    public static void ApplyTransparencyTags(ImageParameter ip, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        ip.FadeKeyframes = ExportFade.Parse(text, (long)Math.Round((ip.EndTime - ip.StartTime).TotalMilliseconds));

        if (!text.Contains("a&H", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Same reason ApplyStyleOverrideTags bails out on "\t(": inside a transition these tags are
        // an animation target, not the line's look. Freezing at the end value made the common
        // fade-out "{\t(0,300,\alpha&HFF&)}" export as a fully invisible subtitle.
        if (text.Contains("\\t(", StringComparison.Ordinal))
        {
            return;
        }

        int? all = null;
        int? primary = null;
        int? outline = null;
        int? shadow = null;
        foreach (Match match in AlphaTagRegex.Matches(text))
        {
            // ASSA counts transparency (00 = opaque, FF = invisible); opacity is the other way up.
            var opacity = 255 - int.Parse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            switch (match.Groups[1].Value)
            {
                case "alpha":
                    all = opacity;
                    break;
                case "1a":
                    primary = opacity;
                    break;
                case "3a":
                    outline = opacity;
                    break;
                default: // "4a"
                    shadow = opacity;
                    break;
            }
        }

        var primaryOpacity = primary ?? all ?? 255;
        var outlineOpacity = outline ?? all ?? 255;
        var shadowOpacity = shadow ?? all ?? 255;
        if (primaryOpacity == 255 && outlineOpacity == 255 && shadowOpacity == 255)
        {
            return;
        }

        if (primaryOpacity == outlineOpacity && primaryOpacity == shadowOpacity)
        {
            // The whole subtitle at one transparency - applying it to the drawn bitmap keeps the
            // outline from showing through the letters, which per colour alpha would do.
            ip.AlphaPercent = (int)Math.Round(primaryOpacity * 100.0 / 255.0);
            return;
        }

        ip.FontColor = Fade(ip.FontColor, primaryOpacity);
        ip.OutlineColor = Fade(ip.OutlineColor, outlineOpacity);
        ip.ShadowColor = Fade(ip.ShadowColor, shadowOpacity);
    }

    /// <summary>
    /// Removes the tags this class consumes itself - "\pos(x,y)", "\fad(..)"/"\fade(..)",
    /// the "\alpha"/"\1a"/"\3a"/"\4a" transparencies and the "\3c"/"\4c"/"\bord"/"\shad"
    /// overrides - matching exactly what <see cref="TryGetPosition"/>,
    /// <see cref="ExportFade.Parse"/>, <see cref="ApplyTransparencyTags"/> and
    /// <see cref="ApplyStyleOverrideTags"/> read, so a tag those did not understand still
    /// makes the line "too complex" instead of being dropped along with its effect.
    /// </summary>
    private static string RemoveConsumedTags(string text)
    {
        var s = PositionTagRegex.Replace(text, string.Empty);
        s = ExportFade.RemoveTags(s);
        s = AlphaTagRegex.Replace(s, string.Empty);

        // The outline/shadow overrides are only consumed outside "\t(..)" blocks (see
        // ApplyStyleOverrideTags) - with a "\t" on the line nothing is consumed, and the "\t"
        // makes the line too complex regardless.
        if (!text.Contains("\\t(", StringComparison.Ordinal))
        {
            s = OutlineShadowColorTagRegex.Replace(s, string.Empty);
            s = OutlineShadowWidthTagRegex.Replace(s, string.Empty);
        }

        // "{\pos(10,20)}Hello" is "{}Hello" now - GetFormattedText has no reason to see the
        // leftover block.
        return s.Replace("{}", string.Empty);
    }

    private static SKColor Fade(SKColor color, int opacity)
    {
        return color.WithAlpha((byte)(color.Alpha * opacity / 255));
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
