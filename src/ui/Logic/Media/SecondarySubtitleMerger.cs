using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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
    /// </summary>
    public static void AddSecondarySubtitle(Subtitle subtitle, Subtitle? subtitleSecondary)
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
        foreach (var p in subtitleSecondary.Paragraphs)
        {
            subtitle.Paragraphs.Add(p);
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
