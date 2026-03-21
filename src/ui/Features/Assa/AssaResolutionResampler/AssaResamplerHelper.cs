using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;

public static class AssaResamplerHelper
{
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
            if (changeFontSize)
            {
                p.Text = AssaResampler.ResampleOverrideTagsFont(sourceWidth, targetWidth, sourceHeight, targetHeight, p.Text);
            }

            if (changePositions)
            {
                p.Text = AssaResampler.ResampleOverrideTagsPosition(sourceWidth, targetWidth, sourceHeight, targetHeight, p.Text);
            }

            if (changeDrawing)
            {
                p.Text = AssaResampler.ResampleOverrideTagsDrawing(sourceWidth, targetWidth, sourceHeight, targetHeight, p.Text, null);
            }
        }
    }
}