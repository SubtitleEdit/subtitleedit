using Nikse.SubtitleEdit.Core.AutoTranslate;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeBatchConvert
{
    public string[] ActiveFunctions { get; set; } = [];
    public string OutputFolder { get; set; }
    public bool Overwrite { get; set; }
    public string TargetFormat { get; set; }
    public string TargetEncoding { get; set; }
    public string OcrEngine { get; set; }
    public string TesseractLanguage { get; set; }
    public int TesseractEngineMode { get; set; }
    public string PaddleLanguage { get; set; }
    public string BinaryOcrDatabase { get; set; }
    public string NOcrBinaryOcrFallbackDatabase { get; set; }
    public string BinaryOcrNOcrFallbackDatabase { get; set; }

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

    public bool SaveInSourceFolder { get; set; }

    public string AutoTranslateEngine { get; set; }
    public string AutoTranslateSourceLanguage { get; set; }
    public string AutoTranslateTargetLanguage { get; set; }

    public string ChangeCasingType { get; set; }
    public bool NormalCasingFixNames { get; set; }
    public bool NormalCasingOnlyUpper { get; set; }

    public string FixRtlMode { get; set; }
    public string LastFilterItem { get; set; }
    public string LanguagePostFix { get; set; }
    public bool AssaUseSourceStylesIfPossible { get; set; }
    public string AssaHeader { get; set; }
    public string AssaFooter { get; set; }

    public int MergeShortLinesMaxCharacters { get; set; }
    public int MergeShortLinesMaxMillisecondsBetweenLines { get; set; }
    public bool MergeShortLinesOnlyContinuationLines { get; set; }

    public bool ApplyDurationLimitsFixMinDuration { get; set; }
    public int ApplyDurationLimitsMinDurationMs { get; set; }
    public bool ApplyDurationLimitsFixMaxDuration { get; set; }
    public int ApplyDurationLimitsMaxDurationMs { get; set; }

    public string SortBy { get; set; }
    public bool SortByDescending { get; set; }

    public SeBatchConvert()
    {
        OutputFolder = string.Empty;
        SaveInSourceFolder = true;
        TargetFormat = string.Empty;
        TargetEncoding = string.Empty;
        OcrEngine = "Tesseract";
        TesseractLanguage = "eng";
        TesseractEngineMode = 3; // Default, based on what is available (tesseract --oem)
        PaddleLanguage = "en";
        BinaryOcrDatabase = "Latin";
        NOcrBinaryOcrFallbackDatabase = string.Empty;
        BinaryOcrNOcrFallbackDatabase = string.Empty;
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
        AutoTranslateEngine = new OllamaTranslate().Name;
        AutoTranslateSourceLanguage = "auto";
        AutoTranslateTargetLanguage = "en";
        ChangeCasingType = "Normal";
        NormalCasingFixNames = true;
        FixRtlMode = "ReverseStartEnd";
        LastFilterItem = string.Empty;
        LanguagePostFix = Se.Language.General.TwoLetterLanguageCode;
        AssaUseSourceStylesIfPossible = true;
        AssaHeader = string.Empty;
        AssaFooter = string.Empty;

        MergeShortLinesMaxCharacters = 55;
        MergeShortLinesMaxMillisecondsBetweenLines = 250;
        MergeShortLinesOnlyContinuationLines = true;

        ApplyDurationLimitsFixMinDuration = true;
        ApplyDurationLimitsMinDurationMs = 1000;
        ApplyDurationLimitsFixMaxDuration = true;
        ApplyDurationLimitsMaxDurationMs = 8000;

        SortBy = "Number";
        SortByDescending = false;
    }
}