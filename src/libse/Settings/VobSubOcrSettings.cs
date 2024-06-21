namespace Nikse.SubtitleEdit.Core.Settings
{
    public class VobSubOcrSettings
    {
        public int XOrMorePixelsMakesSpace { get; set; }
        public double AllowDifferenceInPercent { get; set; }
        public double BlurayAllowDifferenceInPercent { get; set; }
        public string LastImageCompareFolder { get; set; }
        public int LastModiLanguageId { get; set; }
        public string LastOcrMethod { get; set; }
        public string TesseractLastLanguage { get; set; }
        public bool UseTesseractFallback { get; set; }
        public bool UseItalicsInTesseract { get; set; }
        public int TesseractEngineMode { get; set; }
        public bool UseMusicSymbolsInTesseract { get; set; }
        public bool RightToLeft { get; set; }
        public bool TopToBottom { get; set; }
        public int DefaultMillisecondsForUnknownDurations { get; set; }
        public bool FixOcrErrors { get; set; }
        public bool PromptForUnknownWords { get; set; }
        public bool GuessUnknownWords { get; set; }
        public bool AutoBreakSubtitleIfMoreThanTwoLines { get; set; }
        public double ItalicFactor { get; set; }

        public bool LineOcrDraw { get; set; }
        public int LineOcrMinHeightSplit { get; set; }
        public bool LineOcrAdvancedItalic { get; set; }
        public string LineOcrLastLanguages { get; set; }
        public string LineOcrLastSpellCheck { get; set; }
        public int LineOcrLinesToAutoGuess { get; set; }
        public int LineOcrMinLineHeight { get; set; }
        public int LineOcrMaxLineHeight { get; set; }
        public int LineOcrMaxErrorPixels { get; set; }
        public string LastBinaryImageCompareDb { get; set; }
        public string LastBinaryImageSpellCheck { get; set; }
        public bool BinaryAutoDetectBestDb { get; set; }
        public string LastTesseractSpellCheck { get; set; }
        public bool CaptureTopAlign { get; set; }
        public int UnfocusedAttentionBlinkCount { get; set; }
        public int UnfocusedAttentionPlaySoundCount { get; set; }
        public string CloudVisionApiKey { get; set; }
        public string CloudVisionLanguage { get; set; }
        public bool CloudVisionSendOriginalImages { get; set; }

        public VobSubOcrSettings()
        {
            XOrMorePixelsMakesSpace = 8;
            AllowDifferenceInPercent = 1.0;
            BlurayAllowDifferenceInPercent = 7.5;
            LastImageCompareFolder = "English";
            LastModiLanguageId = 9;
            LastOcrMethod = "Tesseract";
            UseItalicsInTesseract = true;
            TesseractEngineMode = 3; // Default, based on what is available (T4 docs)
            UseMusicSymbolsInTesseract = true;
            UseTesseractFallback = true;
            RightToLeft = false;
            TopToBottom = true;
            DefaultMillisecondsForUnknownDurations = 5000;
            FixOcrErrors = true;
            PromptForUnknownWords = true;
            GuessUnknownWords = true;
            AutoBreakSubtitleIfMoreThanTwoLines = true;
            ItalicFactor = 0.2f;
            LineOcrLinesToAutoGuess = 100;
            LineOcrMaxErrorPixels = 45;
            LastBinaryImageCompareDb = "Latin+Latin";
            BinaryAutoDetectBestDb = true;
            CaptureTopAlign = false;
            UnfocusedAttentionBlinkCount = 50;
            UnfocusedAttentionPlaySoundCount = 1;
            CloudVisionApiKey = string.Empty;
            CloudVisionLanguage = "en";
            CloudVisionSendOriginalImages = false;
        }
    }
}