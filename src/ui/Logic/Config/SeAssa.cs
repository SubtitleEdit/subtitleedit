using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAssa
{
    public bool AutoSetResolution { get; set; }
    public bool AutoSetResolutionConvert { get; set; }

    /// <summary>
    /// When an opened video's picture size differs from the script's PlayResX/PlayResY, show the
    /// resolution resampler pre-filled with both instead of resampling silently (#14367).
    /// </summary>
    public bool AutoSetResolutionPrompt { get; set; }

    public List<SeAssaStyle> StoredStyles { get; set; }
    public string LastOverrideTag { get; set; }
    public List<string> LastOverrideTags { get; set; }
    public int ProgressBarHeight { get; set; }
    public string ProgressBarForegroundColor { get; set; }
    public string ProgressBarBackgroundColor { get; set; }
    public int ProgressBarCornerStyleIndex { get; set; }
    public int BackgroundBoxesPaddingLeft { get; set; }
    public int BackgroundBoxesPaddingRight { get; set; }
    public int BackgroundBoxesPaddingTop { get; set; }
    public int BackgroundBoxesPaddingBottom { get; set; }
    public bool BackgroundBoxesFillWidth { get; set; }
    public int BackgroundBoxesFillWidthMarginLeft { get; set; }
    public int BackgroundBoxesFillWidthMarginRight { get; set; }
    public int BackgroundBoxesStyleIndex { get; set; }
    public int BackgroundBoxesStyleRadius { get; set; }
    public string BackgroundBoxesBoxColor { get; set; }
    public string BackgroundBoxesShadowColor { get; set; }
    public string BackgroundBoxesOutlineColor { get; set; }
    public bool HideLayersFromWaveform { get; set; }

    /// <summary>
    /// ASSA draw: opacity of the background (video frame or image) behind the drawing, 0.1-1.
    /// </summary>
    public double DrawBackgroundOpacity { get; set; }

    /// <summary>
    /// ASSA draw: stretch a background image to the canvas instead of fitting it with its aspect ratio.
    /// </summary>
    public bool DrawBackgroundStretch { get; set; }

    /// <summary>
    /// ASSA draw: shapes the user saved to the shape library ("My shapes").
    /// </summary>
    public List<SeAssaDrawShape> DrawShapeLibrary { get; set; }
    public bool HideLayersFromSubtitleGrid { get; set; }
    public bool HideLayersFromVideoPreview { get; set; }
    public bool FontCollectorTrimFonts { get; set; }

    public SeAssa()
    {
        AutoSetResolution = true;
        AutoSetResolutionConvert = true;
        AutoSetResolutionPrompt = true;

        // Seed the storage so a fresh install (and a settings reset) has a default style to
        // apply to new/converted ASSA subtitles instead of an empty "Styles saved" list.
        StoredStyles = new List<SeAssaStyle> { SeAssaStyle.MakeStorageDefault() };
        LastOverrideTag = OverrideTagDisplay.List().First().Tag;
        LastOverrideTags = new List<string>();

        ProgressBarHeight = 40;
        ProgressBarForegroundColor = "#FF0000";
        ProgressBarBackgroundColor = "#80000000";
        ProgressBarCornerStyleIndex = 0;

        BackgroundBoxesPaddingLeft = 25;
        BackgroundBoxesPaddingRight = 25;
        BackgroundBoxesPaddingTop = 27;
        BackgroundBoxesPaddingBottom = 25;
        BackgroundBoxesFillWidth = false;
        BackgroundBoxesFillWidthMarginLeft = 0;
        BackgroundBoxesFillWidthMarginRight = 0;

        BackgroundBoxesStyleIndex = 1;
        BackgroundBoxesStyleRadius = 10;
        BackgroundBoxesBoxColor = Color.FromArgb(50, 0,0,0).FromColorToHex();
        BackgroundBoxesShadowColor = Color.FromArgb(50, 0,0,0).FromColorToHex();    
        BackgroundBoxesOutlineColor = Color.FromArgb(50, 200,200,200).FromColorToHex();

        HideLayersFromWaveform = true;
        DrawBackgroundOpacity = 1.0;
        DrawShapeLibrary = new List<SeAssaDrawShape>();
        HideLayersFromSubtitleGrid = false;
        HideLayersFromVideoPreview = false;
    }
}