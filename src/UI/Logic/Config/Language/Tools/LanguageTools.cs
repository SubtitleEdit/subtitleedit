namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageTools
{
    public LanguageFixCommonErrors FixCommonErrors { get; set; } = new();
    public LanguageAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public LanguageApplyDurationLimits ApplyDurationLimits { get; set; } = new();
    public LanguageApplyMinGaps ApplyMinGaps { get; set; } = new();
    public LanguageBeautifyTimeCodes BeautifyTimeCodes { get; set; } = new();
    public LanguageBridgeGaps BridgeGaps { get; set; } = new();
    public LanguageSortBy SortBy { get; set; } = new();
    public LanguageBatchConvert BatchConvert { get; set; } = new();
    public LanguageChangeCasing ChangeCasing { get; set; } = new();
    public LanguageChangeFormatting ChangeFormatting { get; set; } = new();
    public LanguageJoinSubtitles JoinSubtitles { get; set; } = new();
    public LanguageSplitSubtitle SplitSubtitle { get; set; } = new();
    public LanguageSplitBreakLongLines SplitBreakLongLines { get; set; } = new();
    public LanguageMergeShortLines MergeShortLines { get; set; } = new();
    public LanguageMergeLineswithSameText MergeLinesWithSameText { get; set; } = new();
    public LanguageMergeLineswithSameTimeCodes MergeLinesWithSameTimeCodes { get; set; } = new();
    public LanguageNetflixCheckAndFix NetflixCheckAndFix { get; set; } = new();
    public LanguageImageBasedEdit ImageBasedEdit { get; set; } = new();
    public string PickAlignmentTitle { get; set; }
    public string PickFontNameTitle { get; set; }
    public string ColorPickerTitle { get; set; }
    public string FilterLayersTitle { get; set; }
    public string FilterLayersHideFromWaveform { get; set; }
    public string FilterLayersHideFromSubtitleGrid { get; set; }
    public string FilterLayersHideFromVideoPreview { get; set; }
    public string PickSubtitleFormat { get; set; }
    public string PickLayerTitle { get; set; }
    public string RecentColors { get; set; }

    public LanguageTools()
    {
        PickAlignmentTitle = "Choose alignment";
        PickFontNameTitle = "Choose font name";
        ColorPickerTitle = "Choose color";
        FilterLayersTitle = "Filter layers for display";
        FilterLayersHideFromWaveform = "Hide from waveform/spectrogram";
        FilterLayersHideFromSubtitleGrid = "Hide from subtitle grid";
        FilterLayersHideFromVideoPreview = "Hide from video preview";
        PickSubtitleFormat = "Choose subtitle format";
        PickLayerTitle = "Set layer";
        RecentColors = "Recent colors";
    }
}