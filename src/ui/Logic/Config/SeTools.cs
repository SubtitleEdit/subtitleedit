using Avalonia.Media;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeTools
{
    public SeAiReview AiReview { get; set; } = new();
    public SeAudioToText AudioToText { get; set; } = new();
    public SeConvertActors ConvertActors { get; set; } = new();
    public SeFixCommonErrors FixCommonErrors { get; set; } = new();
    public SeFixNetflixErrors FixNetflixErrors { get; set; } = new();
    public SeAdjustDisplayDurations AdjustDurations { get; set; } = new();
    public SeApplyDurationLimits ApplyDurationLimits { get; set; } = new();
    public SeBridgeGaps BridgeGaps { get; set; } = new();
    public SeChangeFormatting ChangeFormatting { get; set; } = new();
    public SeBatchConvert BatchConvert { get; set; } = new();
    public SeChangeCasing ChangeCasing { get; set; } = new();
    public SeRemoveTextForHi RemoveTextForHi { get; set; } = new();
    public SeRemoveUnicodeCharacters RemoveUnicodeCharacters { get; set; } = new();
    public SeMergeSameTimeCode MergeSameTimeCode { get; set; } = new();
    public SeMergeSameText MergeSameText { get; set; } = new();

    public string OllamaPrompt { get; set; }
    public string LlamaCppPrompt { get; set; }
    public string LmStudioPrompt { get; set; }
    public string AnthropicPrompt { get; set; }
    public string PerplexityPrompt { get; set; }
    public string GroqPrompt { get; set; }
    public string OpenRouterPrompt { get; set; }
    public string NvidiaPrompt { get; set; }
    public bool MergeKeepEndTime { get; set; }
    public bool MergeKeepEndTimeOnlyAssa { get; set; } = true;
    public bool JoinKeepTimeCodes { get; set; }
    public int JoinAppendMilliseconds { get; set; }
    public bool BinEditAppendKeepTimeCodes { get; set; }

    public string MergeTwoSubtitlesOutputFormat { get; set; }
    public string MergeTwoSubtitlesFontName1 { get; set; }
    public int MergeTwoSubtitlesFontSize1 { get; set; }
    public bool MergeTwoSubtitlesBold1 { get; set; }
    public bool MergeTwoSubtitlesItalic1 { get; set; }
    public string MergeTwoSubtitlesPrimaryColor1 { get; set; }
    public string MergeTwoSubtitlesOutlineColor1 { get; set; }
    public decimal MergeTwoSubtitlesOutlineWidth1 { get; set; }
    public decimal MergeTwoSubtitlesShadowWidth1 { get; set; }
    public bool MergeTwoSubtitlesAlignTop1 { get; set; }
    public string MergeTwoSubtitlesFontName2 { get; set; }
    public int MergeTwoSubtitlesFontSize2 { get; set; }
    public bool MergeTwoSubtitlesBold2 { get; set; }
    public bool MergeTwoSubtitlesItalic2 { get; set; }
    public string MergeTwoSubtitlesPrimaryColor2 { get; set; }
    public string MergeTwoSubtitlesOutlineColor2 { get; set; }
    public decimal MergeTwoSubtitlesOutlineWidth2 { get; set; }
    public decimal MergeTwoSubtitlesShadowWidth2 { get; set; }
    public bool MergeTwoSubtitlesAlignTop2 { get; set; }
    public int SplitNumberOfEqualParts { get; set; }
    public string SplitOutputFolder { get; set; }
    public bool SplitByLines { get; set; }
    public bool SplitByCharacters { get; set; }
    public bool SplitByTime { get; set; }
    public string SplitSubtitleFormat { get; set; }
    public string? SplitSubtitleEncoding { get; set; }
    public string SplitOddLinesAction { get; set; }
    public bool GoToLineNumberAlsoSetVideoPosition { get; set; }
    public bool GoToFirstAndLastLineAlsoSetVideoPosition { get; set; }
    public bool SplitRebalanceLongLinesSplit { get; set; }
    public bool SplitRebalanceLongLinesRebalance { get; set; }
    public bool SplitRebalanceLongLinesRebalanceOnlyTooLong { get; set; }
    public int SplitRebalanceLongLinesSingleLineMaxLength { get; set; }
    public int SplitRebalanceLongLinesMaxNumberOfLines { get; set; }
    public int SplitRebalanceLongLinesUnbreakShorterThan { get; set; }

    // Per-dialog copies of the general defaults, so a one-off run does not rewrite the app-wide
    // setting - and the dialog still opens on what it was last used with. 0 means "not saved yet";
    // every one of these is >= 1 in the UI. Same shape as the split/rebalance keys above (#13514).
    public int MergeShortLinesSingleLineMaxLength { get; set; }
    public int MergeShortLinesMaxNumberOfLines { get; set; }
    public int ApplyDurationLimitsMinDurationMs { get; set; }
    public int ApplyDurationLimitsMaxDurationMs { get; set; }

    // Two keys, not one: the Apply minimum gap box holds frames in frame mode and milliseconds
    // otherwise, so a single number came back in the wrong unit after a time-format switch.
    public int ApplyMinGapMilliseconds { get; set; }
    public int ApplyMinGapFrames { get; set; }
    public string UnicodeSymbolsToInsert { get; set; }
    public string MusicSymbol { get; set; }
    public string MusicSymbolReplace { get; set; }

    public int BinEditLeftMargin { get; set; }
    public int BinEditTopMargin { get; set; }
    public int BinEditRightMargin { get; set; }
    public int BinEditBottomMargin { get; set; }
    public string BinEditFontName { get; set; }
    public int BinEditFontSize { get; set; }
    public bool BinEditIsBold { get; set; }
    public string BinEditFontColor { get; set; }
    public string BinEditOutlineColor { get; set; }
    public string BinEditShadowColor { get; set; }
    public string BinEditBackgroundColor { get; set; }
    public decimal BinEditOutlineWidth { get; set; }
    public decimal BinEditShadowWidth { get; set; }
    public bool BinEditSelectCurrentSubtitleWhilePlaying { get; set; }
    public bool BinEditPositionMonitorActive { get; set; }
    public string BinEditPositionMonitorRatio { get; set; }
    public int BinEditPositionMonitorBarHeight { get; set; }
    public bool BinEditPositionMonitorTitleSafeOn { get; set; }
    public double BinEditPositionMonitorTitleSafePercent { get; set; }

    // Import plain text. Only the three options the dialog actually has are kept - the other
    // twelve SE4-shaped keys here were written to Settings.json and read by nothing at all.
    public string ImportTextSplitting { get; set; }
    public bool ImportTextDurationAuto { get; set; }
    public int ImportTextFixedDuration { get; set; }

    public string LastColorPickerColor { get; set; }
    public string LastColorPickerColor1 { get; set; }
    public string LastColorPickerColor2 { get; set; }
    public string LastColorPickerColor3 { get; set; }
    public string LastColorPickerColor4 { get; set; }
    public string LastColorPickerColor5 { get; set; }
    public string LastColorPickerColor6 { get; set; }
    public string LastColorPickerColor7 { get; set; }
    // Mirrored onto Configuration.Settings.Tools.RememberUseAlwaysList, which SpellCheckWordLists
    // guards every load/save of "<lang>_UseAlways.xml" with. Nothing in SE5 ever set that flag, so
    // "Change all" in spell check was a no-op that never survived the session.
    public bool SpellCheckRememberUseAlwaysList { get; set; }
    public bool SpeechToTextSelectedLinesPromptFirstTimeOnly { get; set; }
    public bool MultipleReplaceShowDotDotDotButtons { get; set; }
    public bool GridFocusTextboxAfterInsertNew { get; set; }
    public bool TextToSpeechPromptMergeContinuationLines { get; set; }
    public bool TextToSpeechPromptSkipNoiseLines { get; set; }
    public bool TextToSpeechPromptDetectSpeakers { get; set; }

    // OpenAI Compatible STT settings
    public string OpenAiCompatibleSttUrl { get; set; } = "http://localhost:8000/v1/audio/transcriptions";
    public string OpenAiCompatibleSttApiKey { get; set; } = string.Empty;
    public string OpenAiCompatibleSttModel { get; set; } = "whisper-1";
    public string OpenAiCompatibleSttExtraHeaders { get; set; } = string.Empty;
    public int OpenAiCompatibleSttTimeoutSeconds { get; set; } = 300;
    public string OpenAiCompatibleSttLanguage { get; set; } = string.Empty;
    public decimal OpenAiCompatibleSttTemperature { get; set; }
    public string OpenAiCompatibleSttPrompt { get; set; } = string.Empty;
    public bool OpenAiCompatibleSttAutoTranscribeOnAudioSelection { get; set; }
    public bool OpenAiCompatibleSttStream { get; set; }
    public string OpenAiCompatibleSttAudioFormat { get; set; } = "mp3";

    public string OpenRouterSttApiKey { get; set; } = string.Empty;
    public string OpenRouterSttModel { get; set; } = "openai/whisper-1";
    public string OpenRouterSttLanguage { get; set; } = string.Empty;
    public decimal OpenRouterSttTemperature { get; set; }
    public string OpenRouterSttPrompt { get; set; } = string.Empty;
    public int OpenRouterSttTimeoutSeconds { get; set; } = 300;

    public string DashScopeSttApiKey { get; set; } = string.Empty;
    public string DashScopeSttModel { get; set; } = "qwen3-asr-flash-filetrans";
    public string DashScopeSttLanguage { get; set; } = string.Empty;
    public string DashScopeSttRegion { get; set; } = "international";
    public bool DashScopeSttEnableWords { get; set; }
    public int DashScopeSttTimeoutSeconds { get; set; } = 3600;

    public string GoogleCloudSttKeyFile { get; set; } = string.Empty;
    public string GoogleCloudSttRegion { get; set; } = "us";
    public string GoogleCloudSttModel { get; set; } = "chirp_3";
    public string GoogleCloudSttLanguage { get; set; } = string.Empty;
    public string GoogleCloudSttBucketName { get; set; } = string.Empty;
    public int GoogleCloudSttTimeoutSeconds { get; set; } = 3600;

    /// <summary>
    /// Bills at roughly a fifth of the normal rate ($0.003 vs $0.016 per minute) in
    /// exchange for no latency guarantee, so it is off by default. Measured at 13.6x
    /// realtime on a 140 minute episode, but Google promises nothing.
    /// </summary>
    public bool GoogleCloudSttDynamicBatching { get; set; }

    public List<string> FindHistory { get; set; } = new List<string>();
    public bool AllowSingleLetterShortcutsInTextbox { get; set; }

    // Auto-break (auto br) - defaults must match libse ToolsSettings
    public bool AutoBreakLineEndingEarly { get; set; } = false;
    public bool AutoBreakCommaBreakEarly { get; set; } = false;
    public bool AutoBreakDashEarly { get; set; } = true;
    public bool AutoBreakUsePixelWidth { get; set; } = true;
    public bool AutoBreakPreferBottomHeavy { get; set; } = true;
    public double AutoBreakPreferBottomPercent { get; set; } = 5;
    public bool UseNoLineBreakAfter { get; set; } = false;
    public bool SpellCheckEnglishTreatInApostropheAsIng { get; set; } = true;
    public bool WriteToolsLog { get; set; } = false;

    public SeTools()
    {
        OllamaPrompt = string.Empty;
        LmStudioPrompt = string.Empty;
        LlamaCppPrompt = string.Empty;
        AnthropicPrompt = string.Empty;
        PerplexityPrompt = string.Empty;
        GroqPrompt = string.Empty;
        OpenRouterPrompt = string.Empty;
        NvidiaPrompt = string.Empty;
        JoinKeepTimeCodes = true;

        MergeTwoSubtitlesOutputFormat = AdvancedSubStationAlpha.NameOfFormat;
        MergeTwoSubtitlesFontName1 = "Arial";
        MergeTwoSubtitlesFontSize1 = 20;
        MergeTwoSubtitlesPrimaryColor1 = Colors.White.FromColorToHex();
        MergeTwoSubtitlesOutlineColor1 = Colors.Black.FromColorToHex();
        MergeTwoSubtitlesOutlineWidth1 = 2;
        MergeTwoSubtitlesShadowWidth1 = 1;
        MergeTwoSubtitlesAlignTop1 = true;
        MergeTwoSubtitlesFontName2 = "Arial";
        MergeTwoSubtitlesFontSize2 = 20;
        MergeTwoSubtitlesPrimaryColor2 = Colors.White.FromColorToHex();
        MergeTwoSubtitlesOutlineColor2 = Colors.Black.FromColorToHex();
        MergeTwoSubtitlesOutlineWidth2 = 2;
        MergeTwoSubtitlesShadowWidth2 = 1;
        MergeTwoSubtitlesAlignTop2 = false;
        SplitNumberOfEqualParts = 2;
        SplitByLines = true;
        SplitOutputFolder = string.Empty;
        SplitSubtitleFormat = new SubRip().Name;
        GoToLineNumberAlsoSetVideoPosition = true;
        GoToFirstAndLastLineAlsoSetVideoPosition = true;
        SplitRebalanceLongLinesSplit = true;
        SplitRebalanceLongLinesRebalance = true;
        SplitOddLinesAction = nameof(SplitOddLinesActionType.Smart);
        UnicodeSymbolsToInsert = "♪;♫;—;…;°;∙;©;®;☺;☹;♥;☮;☯;Σ;∞;≡;⇒;π";
        MusicSymbol = "♪";
        MusicSymbolReplace = "â™ª,â™«," + // ♪ + ♫ in UTF-8 opened as ANSI
                             "<s M/>,<s m/>," + // music symbols by subtitle creator
                             "#,*,¶"; // common music symbols

        BinEditLeftMargin = 10;
        BinEditTopMargin = 10;
        BinEditRightMargin = 10;
        BinEditBottomMargin = 10;
        BinEditFontName = "Arial";
        BinEditFontSize = 48;
        BinEditIsBold = false;
        BinEditFontColor = Colors.White.FromColorToHex();
        BinEditOutlineColor = Colors.Black.FromColorToHex();
        BinEditShadowColor = Colors.Black.FromColorToHex();
        BinEditBackgroundColor = Colors.Transparent.FromColorToHex();
        BinEditOutlineWidth = 2;
        BinEditShadowWidth = 1;
        BinEditPositionMonitorActive = false;
        BinEditPositionMonitorRatio = "off";
        BinEditPositionMonitorBarHeight = 0;
        BinEditPositionMonitorTitleSafeOn = true;
        BinEditPositionMonitorTitleSafePercent = 5;

        ImportTextSplitting = "auto";
        ImportTextDurationAuto = true;
        ImportTextFixedDuration = 0; // 0 = fall back to the "Adjust durations" fixed value
        SpellCheckRememberUseAlwaysList = true;
        SpeechToTextSelectedLinesPromptFirstTimeOnly = true;
        MultipleReplaceShowDotDotDotButtons = true;
        GridFocusTextboxAfterInsertNew = true;
        AllowSingleLetterShortcutsInTextbox = false;
        TextToSpeechPromptMergeContinuationLines = true;
        TextToSpeechPromptSkipNoiseLines = true;
        TextToSpeechPromptDetectSpeakers = true;

        LastColorPickerColor = Colors.Yellow.FromColorToHex();
        LastColorPickerColor1 = Colors.Red.FromColorToHex();
        LastColorPickerColor2 = Colors.Green.FromColorToHex();
        LastColorPickerColor3 = Colors.Blue.FromColorToHex();
        LastColorPickerColor4 = Colors.White.FromColorToHex();
        LastColorPickerColor5 = Colors.Black.FromColorToHex();
        LastColorPickerColor6 = Colors.Cyan.FromColorToHex();
        LastColorPickerColor7 = Colors.Orange.FromColorToHex();
    }
}
