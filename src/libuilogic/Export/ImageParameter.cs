using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

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

    /// <summary>
    /// Background of the frame-sized image made when <see cref="IsFullFrame"/> is set. Separate
    /// from <see cref="BackgroundColor"/>, which is the colour of the box behind the text.
    /// Transparent by default, so a full frame image only pads the subtitle out to the frame.
    /// </summary>
    public SKColor FullFrameBackgroundColor { get; set; } = SKColors.Transparent;

    /// <summary>
    /// Transparency of the whole rendered subtitle, 0-100, from an ASSA "{\alpha&amp;H80&amp;}"
    /// tag (see <see cref="ExportTextTags.ApplyTransparencyTags"/>). 100 - fully opaque - unless
    /// the text asks for less.
    /// </summary>
    public int AlphaPercent { get; set; } = 100;

    /// <summary>
    /// The "{\fad(..)}"/"{\fade(..)}" curve of the subtitle, or null when it has no fade tag.
    /// Only used by the Blu-ray sup writer, which can fade with palette updates; the other
    /// image formats have no way to animate a subtitle and ignore it.
    /// </summary>
    public List<ExportFadeKeyframe>? FadeKeyframes { get; set; }

    public double FramesPerSecond { get; set; }
    public bool IsRightToLeft { get; set; } = false;
    public ExportBoxType BoxType { get; set; } = ExportBoxType.None;
    public int BoxPaddingLeft { get; set; } = 0;
    public int BoxPaddingRight { get; set; } = 0;
    public int BoxPaddingTop { get; set; } = 0;
    public int BoxPaddingBottom { get; set; } = 0;

    /// <summary>
    /// Advanced text formatting (gradient fills, multiple outlines, soft shadows, glow, 3D
    /// extrude, bevel). Null renders through the classic fill/outline/shadow path; when set,
    /// <see cref="OutlineWidth"/> and <see cref="ShadowWidth"/> are ignored - the effects
    /// describe the whole look.
    /// </summary>
    public TextEffects? TextEffects { get; set; }

    public ImageParameter()
    {
        Bitmap = new SKBitmap(1, 1, true);
        Text = string.Empty;
        FontName = string.Empty;
        Buffer = [];
        Error = string.Empty;
    }

    /// <summary>
    /// <see cref="OverridePosition"/> - the bitmap's top left corner - in the shape the VobSub
    /// and DVD sup writers take it. They range check it themselves and fall back to
    /// <see cref="Alignment"/> when it falls outside the frame.
    /// </summary>
    public SKPoint? OverridePositionPoint =>
        OverridePosition.HasValue ? new SKPoint(OverridePosition.Value.X, OverridePosition.Value.Y) : null;

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
