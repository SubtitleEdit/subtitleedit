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
    public string PaddleLanguage { get; set; }

    public bool FormattingRemoveAll { get; set; }
    public bool FormattingRemoveItalic { get; set; }
    public bool FormattingRemoveUnderline { get; set; }
    public bool FormattingRemoveFontTags { get; set; }
    public bool FormattingRemoveColorTags { get; set; }
    public bool FormattingRemoveBold { get; set; }
    public bool FormattingRemoveAlignmentTags { get; set; }

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

    public SeBatchConvert()
    {
        OutputFolder = string.Empty;
        SaveInSourceFolder = true;
        TargetFormat = string.Empty;
        TargetEncoding = string.Empty;
        OcrEngine = "Tesseract";
        TesseractLanguage = "eng";
        PaddleLanguage = "en";
        OffsetTimeCodesForward = true;
        AdjustVia = "Seconds"; 
        AdjustDurationSeconds = 0.1;
        AdjustDurationPercentage = 100;
        AdjustDurationFixedMilliseconds = 3000;
        ChangeFrameRateFrom = 23.976;
        ChangeFrameRateTo = 24;
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
    }
}