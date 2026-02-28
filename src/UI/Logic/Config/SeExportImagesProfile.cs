using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeExportImagesProfile
{
    public string ProfileName { get; set; } = "Default";
    public ExportAlignment Alignment { get; set; }
    public ExportContentAlignment ContentAlignment { get; set; }
    public string Text { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Index { get; set; }
    public string FontColor { get; set; }
    public string FontName { get; set; }
    public float FontSize { get; set; }
    public bool IsBold { get; set; }
    public string OutlineColor { get; set; }
    public double OutlineWidth { get; set; }
    public string ShadowColor { get; set; }
    public double ShadowWidth { get; set; }
    public string BackgroundColor { get; set; }
    public double BackgroundCornerRadius { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public int BottomTopMargin { get; set; }
    public int LeftRightMargin { get; set; }
    public string OverridePosition { get; set; }
    public string Error { get; set; }
    public bool IsForced { get; set; }
    public bool IsFullFrame { get; set; }
    public double FramesPerSecond { get; set; }
    public int PaddingLeftRight { get; set; }
    public int PaddingTopBottom { get; set; }
    public int LineSpacingPercent { get; set; }

    public SeExportImagesProfile()
    {
        ProfileName = "Default";
        Alignment = ExportAlignment.BottomCenter;
        Text = string.Empty;
        FontColor = SKColors.White.ToHex(true);
        FontName = string.Empty;
        FontSize = 26;
        OutlineColor = SKColors.Black.ToHex(true);
        OutlineWidth = 2;
        ShadowColor = SKColors.Black.ToHex(true);
        ShadowWidth = 2;
        BackgroundColor = SKColors.Transparent.ToHex(true);
        BackgroundCornerRadius = 0;
        ScreenWidth = 1920;
        ScreenHeight = 1080;
        BottomTopMargin = 10;
        LeftRightMargin = 10;
        Error = string.Empty;
        FramesPerSecond = 25;
        OverridePosition = string.Empty;
        LineSpacingPercent = 0;
        PaddingLeftRight = 2;
        PaddingTopBottom = 2;
    }

    public override string ToString()
    {
        return ProfileName;
    }
}