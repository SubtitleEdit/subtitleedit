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
        var (width, height) = GetVideoSize(mediaInfo);
        var style = MakeStyle(
            "Style" + Guid.NewGuid().ToString().Replace("-", string.Empty),
            GetFontSizeFromSettings(height),
            video.SecondarySubtitleFontBold,
            video.SecondarySubtitleColor.FromHexToColor(),
            video.SecondarySubtitleBoxType,
            video.SecondarySubtitleAlignment);
        return Build(secondarySubtitle, style, width, height);
    }

    /// <summary>
    /// Builds a remembered second subtitle without the dialog (#15044): the saved style when
    /// "Remember these settings" is on, else the defaults the dialog starts from.
    /// </summary>
    public static Subtitle BuildRemembered(Subtitle secondarySubtitle, FfmpegMediaInfo2? mediaInfo)
    {
        if (Se.Settings.Video.SecondarySubtitleOverrideStyle)
        {
            return BuildFromSettings(secondarySubtitle, mediaInfo);
        }

        var (width, height) = GetVideoSize(mediaInfo);
        var style = MakeStyle(
            "Style" + Guid.NewGuid().ToString().Replace("-", string.Empty),
            AssaResampler.Resample(AdvancedSubStationAlpha.DefaultHeight, height, Se.Settings.Video.MpvPreviewFontSize),
            Se.Settings.Video.MpvPreviewFontBold,
            Colors.White,
            FontBoxType.None,
            "8"); // Top-center
        return Build(secondarySubtitle, style, width, height);
    }

    /// <summary>
    /// The video's size, or 1920x1080 when there is none - also for an audio file, where the
    /// media info is there but its dimension is 0x0 (a zero height made "Remember these
    /// settings" divide by zero, and gave PlayResY 0 with font size 1).
    /// </summary>
    public static (int Width, int Height) GetVideoSize(FfmpegMediaInfo2? mediaInfo)
    {
        var dimension = mediaInfo?.Dimension;
        return dimension is { Width: > 0, Height: > 0 }
            ? (dimension.Value.Width, dimension.Value.Height)
            : (1920, 1080);
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
