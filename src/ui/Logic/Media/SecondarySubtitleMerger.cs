using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class SecondarySubtitleMerger
{
    /// <summary>
    /// Adds the secondary subtitle's paragraphs and style to a preview subtitle about
    /// to be pushed to the video player.
    /// The secondary style is sized against its own header's PlayRes (the real video
    /// dimensions), while the target header may use a different - or no - PlayRes
    /// (libass defaults to 384x288 when absent). Grafting the style line verbatim made
    /// the secondary subtitle tiny for WebVTT mains (PlayResY = video height) and huge
    /// for ASSA mains with a small PlayResY (issue #13425), so resample it to the
    /// target's scale first.
    /// The secondary subtitle is shared between refreshes and is only ever read, so its
    /// paragraphs are added by reference - except in SMPTE mode, where the stretch the main
    /// subtitle already got has to apply to them too, and except for a line carrying a baked-in
    /// "\pos" (from justified lines, which position themselves in absolute PlayRes pixels rather
    /// than through the style's alignment/margins), which also needs its own copy to rescale.
    /// </summary>
    public static void AddSecondarySubtitle(Subtitle subtitle, Subtitle? subtitleSecondary, bool smpteMode)
    {
        if (subtitleSecondary == null)
        {
            return;
        }

        var styleName = subtitleSecondary.Paragraphs.FirstOrDefault()?.Extra ?? "Secondary";
        var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitleSecondary.Header);

        var sourceWidth = GetPlayRes(subtitleSecondary.Header, "PlayResX", 384);
        var sourceHeight = GetPlayRes(subtitleSecondary.Header, "PlayResY", 288);
        var targetWidth = GetPlayRes(subtitle.Header, "PlayResX", 384);
        var targetHeight = GetPlayRes(subtitle.Header, "PlayResY", 288);

        if (sourceHeight != targetHeight)
        {
            style.FontSize = AssaResampler.Resample(sourceHeight, targetHeight, style.FontSize);
            style.OutlineWidth = AssaResampler.Resample(sourceHeight, targetHeight, style.OutlineWidth);
            style.ShadowWidth = AssaResampler.Resample(sourceHeight, targetHeight, style.ShadowWidth);
            style.MarginVertical = AssaResampler.Resample(sourceHeight, targetHeight, style.MarginVertical);
        }

        if (sourceWidth != targetWidth)
        {
            style.MarginLeft = AssaResampler.Resample(sourceWidth, targetWidth, style.MarginLeft);
            style.MarginRight = AssaResampler.Resample(sourceWidth, targetWidth, style.MarginRight);
        }

        subtitle.Header = AdvancedSubStationAlpha.AddSsaStyle(style, subtitle.Header);

        var playResChanged = sourceWidth != targetWidth || sourceHeight != targetHeight;
        foreach (var p in subtitleSecondary.Paragraphs)
        {
            var paragraph = smpteMode ? SmptePreviewStretch.Stretched(p) : p;
            if (playResChanged && paragraph.Text.IndexOf("\\pos", StringComparison.Ordinal) >= 0)
            {
                if (!smpteMode)
                {
                    paragraph = new Paragraph(paragraph);
                }

                paragraph.Text = AssaResampler.ResampleOverrideTagsPosition(sourceWidth, targetWidth, sourceHeight, targetHeight, paragraph.Text);
            }

            subtitle.Paragraphs.Add(paragraph);
        }
    }

    private static decimal GetPlayRes(string? header, string tagName, int defaultValue)
    {
        if (string.IsNullOrEmpty(header))
        {
            return defaultValue;
        }

        var value = AdvancedSubStationAlpha.GetTagValueFromHeader(tagName, "[Script Info]", header);
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number > 0)
        {
            return number;
        }

        return defaultValue;
    }
}
