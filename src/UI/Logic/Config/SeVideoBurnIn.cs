using System;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideoBurnIn
{
    public string FontName { get; set; }
    public bool FontBold { get; set; }
    public decimal OutlineWidth { get; set; }
    public decimal ShadowWidth { get; set; }
    public double FontFactor { get; set; }
    public string Encoding { get; set; }
    public string Preset { get; set; }
    public string PixelFormat { get; set; }
    public string Crf { get; set; }
    public string Tune { get; set; }
    public string AudioEncoding { get; set; }
    public bool AudioForceStereo { get; set; }
    public string AudioSampleRate { get; set; }
    public bool TargetFileSize { get; set; }
    public bool NonAssaBox { get; set; }
    public bool GenTransparentVideoNonAssaBox { get; set; }
    public bool GenTransparentVideoNonAssaBoxPerLine { get; set; }
    public string GenTransparentVideoExtension { get; set; }
    public string NonAssaBoxColor { get; set; }
    public string NonAssaTextColor { get; set; }
    public string NonAssaShadowColor { get; set; }
    public string NonAssaOutlineColor { get; set; }
    public string NonAssaAlignment { get; set; }
    public bool NonAssaFixRtlUnicode { get; set; }
    public decimal NonAssaMarginVertical { get; set; }
    public decimal NonAssaMarginHorizontal { get; set; }
    public string EmbedOutputExt { get; set; }
    public string EmbedOutputSuffix { get; set; }
    public string EmbedOutputReplace { get; set; }
    public bool DeleteInputVideoFile { get; set; }
    public bool UseOutputFolder { get; set; }
    public string OutputFolder { get; set; }
    public string BurnInSuffix { get; set; }
    public bool UseSourceResolution { get; set; }
    public string Effects { get; set; }

    public SeVideoBurnIn()
    {
        FontName = "Arial";
        PixelFormat = string.Empty;
        EmbedOutputExt = ".mkv";
        OutputFolder = string.Empty;
        FontFactor = 0.52;
        Encoding = "libx264";
        Preset = "medium";
        Crf = "23";
        Tune = "film";
        AudioEncoding = "copy";
        AudioForceStereo = true;
        AudioSampleRate = "48000";
        FontBold = true;
        OutlineWidth = 6;
        ShadowWidth = 3;
        NonAssaBox = true;
        NonAssaBoxColor = SKColors.Black.ToHex();
        NonAssaTextColor = SKColors.White.ToHex();
        NonAssaShadowColor = SKColors.Black.ToHex();
        NonAssaOutlineColor = SKColors.Black.ToHex();
        EmbedOutputSuffix = "embed";
        EmbedOutputReplace = "embed" + Environment.NewLine + "SoftSub" + Environment.NewLine + "SoftSubbed";
        BurnInSuffix = "_new";
        GenTransparentVideoExtension = ".mkv";
        NonAssaAlignment = "2";
        Effects = string.Empty;
    }
}