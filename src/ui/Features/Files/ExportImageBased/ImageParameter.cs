using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ImageParameter
{
    public ExportAlignment Alignment { get; set; }
    public ExportContentAlignment ContentAlignment { get; set; }
    public int PaddingLeftRight { get; set; }
    public int PaddingTopBottom { get; set; }
    public SKBitmap Bitmap { get; set; }
    public string Text { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Index { get; set; }
    public SKColor FontColor { get; set; }
    public string FontName { get; set; }
    public float FontSize { get; set; }
    public bool IsBold { get; set; }
    public SKColor OutlineColor { get; set; }
    public double OutlineWidth { get; set; }
    public SKColor ShadowColor { get; set; }
    public double ShadowWidth { get; set; }
    public SKColor BackgroundColor { get; set; }
    public double BackgroundCornerRadius { get; set; }
    public int LineSpacingPercent { get; set; }
    public byte[] Buffer { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public int BottomTopMargin { get; set; }
    public int LeftRightMargin { get; set; }
    public SKPointI? OverridePosition { get; set; }
    public string Error { get; set; }
    public bool IsForced { get; set; }
    public bool IsFullFrame { get; set; }
    public double FramesPerSecond { get; set; }
    public bool IsRightToLeft { get; set; } = false;

    public ImageParameter()
    {
        Bitmap = new SKBitmap(1, 1, true);
        Text = string.Empty;
        FontName = string.Empty;
        Buffer = [];
        Error = string.Empty;
    }

    public BluRayContentAlignment BluRayContentAlignment => Alignment switch
    {
        ExportAlignment.TopLeft => BluRayContentAlignment.TopLeft,
        ExportAlignment.TopCenter => BluRayContentAlignment.TopCenter,
        ExportAlignment.TopRight => BluRayContentAlignment.TopRight,
        ExportAlignment.MiddleLeft => BluRayContentAlignment.MiddleLeft,
        ExportAlignment.MiddleCenter => BluRayContentAlignment.MiddleCenter,
        ExportAlignment.MiddleRight => BluRayContentAlignment.MiddleRight,
        ExportAlignment.BottomLeft => BluRayContentAlignment.BottomLeft,
        ExportAlignment.BottomCenter => BluRayContentAlignment.BottomCenter,
        ExportAlignment.BottomRight => BluRayContentAlignment.BottomRight,
        _ => BluRayContentAlignment.BottomCenter,
    };
}
