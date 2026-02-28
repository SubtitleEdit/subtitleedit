using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAssa
{
    public bool AutoSetResolution { get; set; }
    public bool AutoSetResolutionConvert { get; set; }
    public List<SeAssaStyle> StoredStyles { get; set; }
    public string LastOverrideTag { get; internal set; }
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
    public bool HideLayersFromSubtitleGrid { get; set; }
    public bool HideLayersFromVideoPreview { get; set; }

    public SeAssa()
    {
        AutoSetResolution = true;
        AutoSetResolutionConvert = true;

        StoredStyles = new List<SeAssaStyle>();
        LastOverrideTag = OverrideTagDisplay.List().First().Tag;
        
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
        HideLayersFromSubtitleGrid = false;
        HideLayersFromVideoPreview = false;
    }
}