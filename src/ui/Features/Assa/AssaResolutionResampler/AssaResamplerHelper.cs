using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;

public static class AssaResamplerHelper
{
    /// <summary>
    /// Font sizes at or below this are the built-in ASSA default (authored against the 288-high
    /// default resolution) rather than a size anyone picked - see <see cref="ScaleDefaultFontSizes"/>.
    /// </summary>
    private const decimal MaxDefaultFontSize = 25;

    /// <summary>
    /// Lifts the small default font sizes of a header that declares no PlayResX/PlayResY up to the
    /// video height, the way SE 4 did when it set the resolution of a new subtitle.
    /// <para>
    /// Such a header is the built-in one (or the default style storage written into it), which is
    /// authored against ASSA's 288-high default - a 20pt font there is 7% of the picture height and
    /// would shrink to 2% the moment PlayResY becomes 1080. Only sizes at or below
    /// <see cref="MaxDefaultFontSize"/> are touched: a larger size is one the user chose, and
    /// margins, outline and shadow are never touched at all. Full resampling stays for headers that
    /// do declare a resolution, where the file really was authored for another picture size
    /// (issue #13799 - OCR results took the user's stored style and inflated every value in it).
    /// </para>
    /// </summary>
    public static void ScaleDefaultFontSizes(Subtitle subtitle, decimal targetHeight)
    {
        if (string.IsNullOrEmpty(subtitle.Header) || targetHeight <= 0)
        {
            return;
        }

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        var changed = false;
        foreach (var style in styles)
        {
            if (style.FontSize > 0 && style.FontSize <= MaxDefaultFontSize)
            {
                style.FontSize = AssaResampler.Resample(AdvancedSubStationAlpha.DefaultHeight, targetHeight, style.FontSize);
                changed = true;
            }
        }

        if (changed)
        {
            subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(subtitle.Header, styles);
        }
    }

    public static void ApplyResampling(
        Subtitle subtitle,
        decimal sourceWidth,
        decimal sourceHeight,
        decimal targetWidth,
        decimal targetHeight,
        bool changeMargins = true,
        bool changeFontSize = true,
        bool changeDrawing = true,
        bool changePositions = true
        )
    {
        if (string.IsNullOrEmpty(subtitle.Header))
        {
            subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Resample styles
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        foreach (var style in styles)
        {
            if (changeMargins)
            {
                style.MarginLeft = AssaResampler.Resample(sourceWidth, targetWidth, style.MarginLeft);
                style.MarginRight = AssaResampler.Resample(sourceWidth, targetWidth, style.MarginRight);
                style.MarginVertical = AssaResampler.Resample(sourceHeight, targetHeight, style.MarginVertical);
            }

            if (changeFontSize)
            {
                style.FontSize = AssaResampler.Resample(sourceHeight, targetHeight, style.FontSize);
            }

            if (changeFontSize || changeDrawing)
            {
                style.OutlineWidth = AssaResampler.Resample(sourceHeight, targetHeight, style.OutlineWidth);
                style.ShadowWidth = AssaResampler.Resample(sourceHeight, targetHeight, style.ShadowWidth);
                style.Spacing = AssaResampler.Resample(sourceWidth, targetWidth, style.Spacing);
            }
        }

        subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(subtitle.Header, styles);

        // Update PlayRes in header
        subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + targetWidth.ToString(CultureInfo.InvariantCulture), "[Script Info]", subtitle.Header);
        subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + targetHeight.ToString(CultureInfo.InvariantCulture), "[Script Info]", subtitle.Header);

        // Resample paragraphs
        foreach (var p in subtitle.Paragraphs)
        {
            p.Text = ResampleText(p.Text, sourceWidth, sourceHeight, targetWidth, targetHeight, changeFontSize, changePositions, changeDrawing);
        }
    }

    /// <summary>
    /// Scales the override tags of one line's text between two script resolutions: font-related
    /// tags, position tags (\pos, \move, \org, rectangular \clip) and drawings / vector clips.
    /// </summary>
    public static string ResampleText(
        string text,
        decimal sourceWidth,
        decimal sourceHeight,
        decimal targetWidth,
        decimal targetHeight,
        bool changeFontSize = true,
        bool changePositions = true,
        bool changeDrawing = true)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        if (changeFontSize)
        {
            text = AssaResampler.ResampleOverrideTagsFont(sourceWidth, targetWidth, sourceHeight, targetHeight, text);
        }

        if (changePositions)
        {
            text = AssaResampler.ResampleOverrideTagsPosition(sourceWidth, targetWidth, sourceHeight, targetHeight, text);
        }

        if (changeDrawing)
        {
            text = AssaResampler.ResampleOverrideTagsDrawing(sourceWidth, targetWidth, sourceHeight, targetHeight, text, null);
        }

        return text;
    }
}