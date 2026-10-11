using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>A named batch convert setup: the function selection and settings, as SeBatchConvert JSON.</summary>
public class SeBatchConvertPreset
{
    public string Name { get; set; } = string.Empty;
    public string Settings { get; set; } = string.Empty;

    // Batch convert edits these shared tool settings directly (not via SeBatchConvert), so a
    // preset stores its own copies. Null for presets saved before they were included.
    public SeMergeSameText? MergeSameText { get; set; }
    public SeMergeSameTimeCode? MergeSameTimeCode { get; set; }
    public SeBridgeGaps? BridgeGaps { get; set; }
    public int? ApplyMinGapMilliseconds { get; set; }
    public bool? SplitRebalanceLongLinesSplit { get; set; }
    public bool? SplitRebalanceLongLinesRebalance { get; set; }
    public bool? SplitRebalanceLongLinesRebalanceOnlyTooLong { get; set; }
    public int? SplitRebalanceLongLinesSingleLineMaxLength { get; set; }
    public int? SplitRebalanceLongLinesMaxNumberOfLines { get; set; }
    public int? SplitRebalanceLongLinesUnbreakShorterThan { get; set; }
}

public class SeBatchConvert
{
    public string[] ActiveFunctions { get; set; } = [];
    public string OutputFolder { get; set; }
    public bool Overwrite { get; set; }

    /// <summary>Give output files the source file's modified/created date instead of the conversion time.</summary>
    public bool KeepSourceTimestamp { get; set; }

    /// <summary>Keep the computer from going to sleep on idle while a batch runs (#15222).</summary>
    public bool PreventSleep { get; set; }
    public string TargetFormat { get; set; }
    public string CustomTextFormatName { get; set; } = string.Empty;
    public string TargetEncoding { get; set; }
    public string OcrEngine { get; set; }
    public string TesseractLanguage { get; set; }
    public int TesseractEngineMode { get; set; }
    public string PaddleLanguage { get; set; }

    /// <summary>Apple Vision's recognition language, as its BCP-47 tag. macOS only.</summary>
    public string AppleVisionLanguage { get; set; }
    public string BinaryOcrDatabase { get; set; }
    public string NOcrBinaryOcrFallbackDatabase { get; set; }
    public string BinaryOcrNOcrFallbackDatabase { get; set; }

    /// <summary>
    /// VobSub OCR only: before recognition, rebuild each subpicture as a crisp black-on-white
    /// bitmap via histogram-based colour isolation (most frequent opaque colour = glyph fill →
    /// black, outline / anti-alias colours → white background). On by default; improves
    /// recognition on discs whose gray outlines otherwise melt adjacent characters together.
    /// See <see cref="Core.Common.VobSubColorIsolation"/>.
    /// </summary>
    public bool VobSubIsolateColors { get; set; }

    public bool FormattingRemoveAll { get; set; }
    public bool FormattingRemoveItalic { get; set; }
    public bool FormattingRemoveUnderline { get; set; }
    public bool FormattingRemoveFontTags { get; set; }
    public bool FormattingRemoveColorTags { get; set; }
    public bool FormattingRemoveBold { get; set; }
    public bool FormattingRemoveAlignmentTags { get; set; }

    public bool FormattingAddItalic { get; set; }
    public bool FormattingAddBold { get; set; }
    public bool FormattingAddUnderline { get; set; }
    public bool FormattingAddAlignmentTag { get; set; }
    public string FormattingAddAlignmentTagOption { get; set; }
    public bool FormattingAddColor { get; set; }
    public string FormattingAddColorValue { get; set; }

    public bool RemoveLineBreaksOnlyShortLines { get; set; }

    public double OffsetTimeCodesMilliseconds { get; set; }
    public bool OffsetTimeCodesForward { get; set; }

    public string AdjustVia { get; set; }
    public double AdjustDurationSeconds { get; set; }
    public int AdjustDurationPercentage { get; set; }
    public int AdjustDurationFixedMilliseconds { get; set; }
    public double AdjustOptimalCps { get; set; }
    public double AdjustMaxCps { get; set; }

    public double ChangeFrameRateFrom { get; set; }
    public double ChangeFrameRateTo { get; set; }

    public double ChangeSpeedPercent { get; set; }

    public int DeleteXFirstLines { get; set; }
    public int DeleteXLastLines { get; set; }
    public string DeleteLinesContains { get; set; }
    public string DeleteActorsOrStyles { get; set; }

    public int AssaChangeResolutionTargetWidth { get; set; }
    public int AssaChangeResolutionTargetHeight { get; set; }
    public bool AssaChangeResolutionChangeMargins { get; set; }
    public bool AssaChangeResolutionChangeFontSize { get; set; }
    public bool AssaChangeResolutionChangePosition { get; set; }
    public bool AssaChangeResolutionChangeDrawing { get; set; }

    public string AssaChangeStyleFromStyle { get; set; }
    public string AssaChangeStyleToStyle { get; set; }
    public bool AssaChangeStyleTrimUnusedStyles { get; set; }

    public bool AssaChangeStylePropertiesSetSpacing { get; set; }
    public decimal AssaChangeStylePropertiesSpacing { get; set; }
    public bool AssaChangeStylePropertiesSetAlignment { get; set; }
    public string AssaChangeStylePropertiesAlignment { get; set; }

    public bool SaveInSourceFolder { get; set; }

    public string AutoTranslateEngine { get; set; }
    public string AutoTranslateSourceLanguage { get; set; }
    public string AutoTranslateTargetLanguage { get; set; }
    /// <summary>Codes of the extra languages to translate into besides the "To" language, comma separated.</summary>
    public string AutoTranslateExtraTargetLanguages { get; set; }

    /// <summary>
    /// Batch convert's own "use external server" switch for the llama.cpp engines - independent of
    /// the Auto-translate window's <see cref="SeAutoTranslate.LlamaCppUseRemoteServer"/> (#14005).
    /// The server URL itself is shared via <see cref="SeAutoTranslate.LlamaCppApiUrl"/>.
    /// </summary>
    public bool LlamaCppUseRemoteServer { get; set; }

    public string ChangeCasingType { get; set; }
    public bool NormalCasingFixNames { get; set; }
    public bool NormalCasingOnlyUpper { get; set; }

    public string FixRtlMode { get; set; }
    public string LastFilterItem { get; set; }
    public string LanguagePostFix { get; set; }
    public bool AssaUseSourceStylesIfPossible { get; set; }
    public string AssaHeader { get; set; }
    public string AssaFooter { get; set; }
    /// <summary>When the ASSA header template replaces a source file's header, keep the source's embedded fonts (footer).</summary>
    public bool AssaKeepSourceEmbeddedFonts { get; set; }

    public bool AssaEmbedFontsTrim { get; set; }

    /// <summary>The EBU STL header (GSI block) chosen in the EBU STL settings, empty = defaults.</summary>
    public string EbuHeader { get; set; } = string.Empty;
    public int EbuJustificationCode { get; set; } = 2;

    // Cavena 890 header fields - batch convert's own, so a run never picks up the title of
    // whatever file was last exported from the main window. An empty title = file name.
    public string Cavena890TranslatedTitle { get; set; } = string.Empty;
    public string Cavena890OriginalTitle { get; set; } = string.Empty;
    public string Cavena890Translator { get; set; } = string.Empty;
    public string Cavena890Comment { get; set; } = string.Empty;

    /// <summary>Start of programme in milliseconds, 0 = the format's default (10:00:00:00).</summary>
    public double Cavena890StartOfProgrammeMs { get; set; }

    public int MergeShortLinesMaxCharacters { get; set; }
    public int MergeShortLinesMaxMillisecondsBetweenLines { get; set; }
    public bool MergeShortLinesOnlyContinuationLines { get; set; }

    public bool ApplyDurationLimitsFixMinDuration { get; set; }
    public int ApplyDurationLimitsMinDurationMs { get; set; }
    public bool ApplyDurationLimitsFixMaxDuration { get; set; }
    public int ApplyDurationLimitsMaxDurationMs { get; set; }

    public string SortBy { get; set; }
    public bool SortByDescending { get; set; }

    public bool BeautifyTimeCodesSnapToShotChanges { get; set; }
    public bool BeautifyTimeCodesUseFixedFrameRate { get; set; }
    public double BeautifyTimeCodesFixedFrameRate { get; set; }

    public bool SnapTimeCodesToFramesUseFixedFrameRate { get; set; }
    public double SnapTimeCodesToFramesFixedFrameRate { get; set; }

    public bool ConvertColorsToDialogRemoveColorTags { get; set; }
    public bool ConvertColorsToDialogAddNewLines { get; set; }
    public bool ConvertColorsToDialogReBreakLines { get; set; }

    public int RenumberStartNumber { get; set; }

    public int MergeContinuationLinesMaxGapMs { get; set; }
    public int MergeContinuationLinesMaxCharacters { get; set; }

    public string ConvertActorsFromType { get; set; }
    public string ConvertActorsToType { get; set; }
    public bool ConvertActorsSetColor { get; set; }
    public string ConvertActorsColor { get; set; }
    public bool ConvertActorsChangeCasing { get; set; }
    public int ConvertActorsCasingType { get; set; }
    public bool ConvertActorsOnlyNames { get; set; }

    /// <summary>Named function selections + settings. A preset's own stored copy has this list empty.</summary>
    public List<SeBatchConvertPreset> Presets { get; set; } = new();

    /// <summary>
    /// "Add folder" (and dropping a folder on the file list) also picks up files in subfolders.
    /// Off by default - a recursive scan of a big tree or a network share can take a while.
    /// </summary>
    public bool ScanFolderRecursive { get; set; }

    /// <summary>
    /// What "Add folder" (and dropping a folder on the file list) does with video files
    /// (.mkv/.mp4/.ts/...) it finds: <see cref="ScanFolderVideoFilesAsk"/> prompts each time,
    /// <see cref="ScanFolderVideoFilesInclude"/> adds their embedded subtitle tracks,
    /// <see cref="ScanFolderVideoFilesSkip"/> leaves them out like SE 4 did (#15742). Video files
    /// added or dropped one by one are always added.
    /// </summary>
    public string ScanFolderVideoFiles { get; set; } = ScanFolderVideoFilesAsk;

    public const string ScanFolderVideoFilesAsk = "Ask";
    public const string ScanFolderVideoFilesInclude = "Include";
    public const string ScanFolderVideoFilesSkip = "Skip";

    public bool ImageAdjustBrightnessOn { get; set; }
    public double ImageAdjustBrightness { get; set; }
    public double ImageAdjustContrast { get; set; }
    public double ImageAdjustGamma { get; set; } = 100;
    public bool ImageAdjustAlphaOn { get; set; }
    public double ImageAdjustAlpha { get; set; }
    public double ImageAdjustAlphaThreshold { get; set; }
    public bool ImageAdjustColorOn { get; set; }
    public string ImageAdjustColorValue { get; set; } = "#FFFFFFFF";

    // Transport Stream output settings (SE4's "TS settings..."), see TransportStreamExportSettings.
    public bool TsOverrideXPosition { get; set; }
    public string TsOverrideHAlign { get; set; } = TransportStreamExportSettings.HAlignCenter;
    public int TsOverrideHMargin { get; set; } = 5; // percent
    public bool TsOverrideYPosition { get; set; }
    public int TsOverrideBottomMargin { get; set; } = 5; // percent
    public bool TsOverrideScreenSize { get; set; }
    public int TsScreenWidth { get; set; } = 1920;
    public int TsScreenHeight { get; set; } = 1080;
    public string TsFileNameAppend { get; set; } = "." + TransportStreamExportSettings.PlaceholderTwoLetter;
    public bool TsOnlyTeletext { get; set; }

    public TransportStreamExportSettings GetTransportStreamExportSettings()
    {
        return new TransportStreamExportSettings
        {
            OverrideXPosition = TsOverrideXPosition,
            HAlign = TsOverrideHAlign ?? TransportStreamExportSettings.HAlignCenter,
            HMarginPercent = TsOverrideHMargin,
            OverrideYPosition = TsOverrideYPosition,
            BottomMarginPercent = TsOverrideBottomMargin,
            OverrideScreenSize = TsOverrideScreenSize,
            ScreenWidth = TsScreenWidth,
            ScreenHeight = TsScreenHeight,
            FileNameAppend = TsFileNameAppend ?? string.Empty,
            OnlyTeletext = TsOnlyTeletext,
        };
    }

    public SeBatchConvert()
    {
        OutputFolder = string.Empty;
        SaveInSourceFolder = true;
        TargetFormat = string.Empty;
        TargetEncoding = string.Empty;
        // See SeOcr.Engine: on macOS the built-in recognizer needs no install, while the
        // Tesseract default would send a fresh Mac user to Homebrew before the first batch run.
        OcrEngine = System.OperatingSystem.IsMacOS()
            ? Features.Ocr.Engines.AppleVisionOcr.StaticName
            : "Tesseract";
        TesseractLanguage = "eng";
        TesseractEngineMode = 3; // Default, based on what is available (tesseract --oem)
        PaddleLanguage = "en";
        AppleVisionLanguage = "en-US";
        BinaryOcrDatabase = "Latin";
        NOcrBinaryOcrFallbackDatabase = string.Empty;
        BinaryOcrNOcrFallbackDatabase = string.Empty;
        VobSubIsolateColors = true;
        PreventSleep = true;
        OffsetTimeCodesForward = true;
        AdjustVia = "Seconds";
        AdjustDurationSeconds = 0.1;
        AdjustDurationPercentage = 100;
        AdjustDurationFixedMilliseconds = 3000;
        ChangeFrameRateFrom = 23.976;
        ChangeFrameRateTo = 24;
        ChangeSpeedPercent = 100;
        DeleteLinesContains = string.Empty;
        DeleteActorsOrStyles = string.Empty;
        FormattingAddAlignmentTagOption = "an2";
        FormattingAddColorValue = "#FFFFFFFF";
        AssaChangeResolutionTargetWidth = 1920;
        AssaChangeResolutionTargetHeight = 1080;
        AssaChangeResolutionChangeMargins = true;
        AssaChangeResolutionChangeFontSize = true;
        AssaChangeResolutionChangePosition = true;
        AssaChangeResolutionChangeDrawing = true;
        AssaChangeStyleFromStyle = string.Empty;
        AssaChangeStyleToStyle = string.Empty;
        AssaChangeStyleTrimUnusedStyles = false;
        AssaChangeStylePropertiesSetSpacing = true;
        AssaChangeStylePropertiesSpacing = 0;
        AssaChangeStylePropertiesSetAlignment = false;
        AssaChangeStylePropertiesAlignment = "an2";
        AutoTranslateEngine = new OllamaTranslate().Name;
        AutoTranslateSourceLanguage = "auto";
        AutoTranslateTargetLanguage = "en";
        AutoTranslateExtraTargetLanguages = string.Empty;
        ChangeCasingType = "Normal";
        NormalCasingFixNames = true;
        FixRtlMode = "ReverseStartEnd";
        LastFilterItem = string.Empty;
        LanguagePostFix = Se.Language.General.TwoLetterLanguageCode;
        AssaUseSourceStylesIfPossible = true;
        AssaHeader = string.Empty;
        AssaFooter = string.Empty;
        AssaKeepSourceEmbeddedFonts = false;

        MergeShortLinesMaxCharacters = 55;
        MergeShortLinesMaxMillisecondsBetweenLines = 250;
        MergeShortLinesOnlyContinuationLines = true;

        ApplyDurationLimitsFixMinDuration = true;
        ApplyDurationLimitsMinDurationMs = 1000;
        ApplyDurationLimitsFixMaxDuration = true;
        ApplyDurationLimitsMaxDurationMs = 8000;

        SortBy = "Number";
        SortByDescending = false;

        BeautifyTimeCodesSnapToShotChanges = true;
        BeautifyTimeCodesFixedFrameRate = 23.976;

        SnapTimeCodesToFramesFixedFrameRate = 23.976;

        ConvertColorsToDialogRemoveColorTags = true;

        RenumberStartNumber = 1;

        MergeContinuationLinesMaxGapMs = 250;
        MergeContinuationLinesMaxCharacters = 0; // 0 = max line length x max number of lines

        ConvertActorsFromType = "InlineSquareBrackets";
        ConvertActorsToType = "Actor";
        ConvertActorsColor = Avalonia.Media.Colors.Yellow.FromColorToHex();
    }
}