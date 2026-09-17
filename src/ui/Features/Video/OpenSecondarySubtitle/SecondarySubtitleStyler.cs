using Avalonia.Media;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

/// <summary>
/// Turns a parsed second subtitle into the styled ASSA subtitle the video player shows. Shared by
/// the "Second subtitle file" dialog and by opening a second subtitle with the dialog turned off.
/// </summary>
public static class SecondarySubtitleStyler
{
    public static SsaStyle MakeStyle(string styleName, int fontSize, bool bold, Color color, FontBoxType boxType, string alignment)
    {
        var style = new SsaStyle
        {
            Name = styleName,
            // Follow the main preview's font (issue #13492): a hardcoded Arial showed a
            // sans-serif secondary under a serif main whenever the preview font was changed.
            FontName = Se.Settings.Video.MpvPreviewFontName,
            FontSize = fontSize,
            Bold = bold,
            Primary = color.ToSKColor(),
            Outline = Colors.Black.ToSKColor(),
            Background = Colors.Black.ToSKColor(),
            Secondary = Colors.Yellow.ToSKColor(),
            Alignment = alignment,
            OutlineWidth = 2,
            ShadowWidth = 1,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
            BorderStyle = GetBorderStyle(boxType),
            ScaleX = 100,
            ScaleY = 100,
        };

        style.Outline = new SkiaSharp.SKColor(style.Outline.Red, style.Outline.Green, style.Outline.Blue, color.A);
        style.Background = new SkiaSharp.SKColor(style.Background.Red, style.Background.Green, style.Background.Blue, color.A);
        return style;
    }

    /// <summary>
    /// Sets an ASSA header holding just <paramref name="style"/>, with PlayRes at the video size
    /// the style's font size is meant for.
    /// </summary>
    public static void SetHeader(Subtitle subtitle, SsaStyle style, int width, int height)
    {
        subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            new List<SsaStyle> { style });
        subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + width.ToString(CultureInfo.InvariantCulture), "[Script Info]", subtitle.Header);
        subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + height.ToString(CultureInfo.InvariantCulture), "[Script Info]", subtitle.Header);
    }

    public static Subtitle Build(Subtitle secondarySubtitle, SsaStyle style, int width, int height)
    {
        var result = new Subtitle(secondarySubtitle);
        SetHeader(result, style, width, height);
        foreach (var p in result.Paragraphs)
        {
            p.Extra = style.Name;
        }

        return result;
    }

    /// <summary>
    /// Builds the second subtitle from the style saved in <see cref="SeVideo"/>, for opening it
    /// without the dialog.
    /// </summary>
    public static Subtitle BuildFromSettings(Subtitle secondarySubtitle, FfmpegMediaInfo2? mediaInfo)
    {
        var video = Se.Settings.Video;
        var width = mediaInfo?.Dimension.Width ?? 1920;
        var height = mediaInfo?.Dimension.Height ?? 1080;
        var style = MakeStyle(
            "Style" + Guid.NewGuid().ToString().Replace("-", string.Empty),
            GetFontSizeFromSettings(height),
            video.SecondarySubtitleFontBold,
            video.SecondarySubtitleColor.FromHexToColor(),
            video.SecondarySubtitleBoxType,
            video.SecondarySubtitleAlignment);
        return Build(secondarySubtitle, style, width, height);
    }

    public static int GetFontSizeFromSettings(int videoHeight)
    {
        var fontSize = Se.Settings.Video.SecondarySubtitleFontSize * videoHeight / AdvancedSubStationAlpha.DefaultHeight;
        return Math.Max(1, (int)Math.Round(fontSize, MidpointRounding.AwayFromZero));
    }

    public static void SaveToSettings(int fontSize, int videoHeight, bool bold, Color color, FontBoxType boxType, string alignment)
    {
        var video = Se.Settings.Video;
        // Not AssaResampler: it rounds to one decimal, which can drift the size by a pixel on a
        // 4K video when it is scaled back.
        video.SecondarySubtitleFontSize = Math.Round((decimal)fontSize * AdvancedSubStationAlpha.DefaultHeight / videoHeight, 4);
        video.SecondarySubtitleFontBold = bold;
        video.SecondarySubtitleColor = color.FromColorToHex();
        video.SecondarySubtitleBoxType = boxType;
        video.SecondarySubtitleAlignment = alignment;
    }

    private static string GetBorderStyle(FontBoxType boxType)
    {
        return boxType switch
        {
            FontBoxType.OneBox => "4",
            FontBoxType.BoxPerLine => "3",
            _ => "1",
        };
    }
}
