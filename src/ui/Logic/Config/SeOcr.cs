using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeOcr
{
    public string Engine { get; set; }
    public string NOcrDatabase { get; set; }
    public string NOcrBinaryOcrFallbackDatabase { get; set; }
    public string BinaryOcrNOcrFallbackDatabase { get; set; }
    public int NOcrMaxWrongPixels { get; set; }
    public int NOcrPixelsAreSpace { get; set; }
    public bool NOcrDrawUnknownText { get; set; }
    public int BinaryOcrPixelsAreSpace { get; set; }
    public double BinaryOcrMaxErrorPercent { get; set; }
    public List<string> OllamaModels { get; set; }
    public string OllamaModel { get; set; }
    public string OllamaUrl { get; set; }
    public string OllamaLanguage { get; set; }
    public int OllamaOcrTimeoutMinutes { get; set; }
    public string LlamaCppUrl { get; set; }
    public string LlamaCppOcrModel { get; set; }
    public string LlamaCppOcrPrompt { get; set; }
    public int LlamaCppOcrTimeoutMinutes { get; set; }
    public string CrispEmbedBackend { get; set; }
    public string CrispEmbedModel { get; set; }
    public int CrispEmbedOcrTimeoutMinutes { get; set; }
    public string GoogleVisionApiKey { get; set; }
    public string GoogleVisionLanguage { get; set; }
    public string MistralApiKey { get; set; }
    public bool IsNewLetterItalic { get; set; }
    public bool SubmitOnFirstLetter { get; set; }
    public bool PromptForBlankOcrText { get; set; }
    public int NOcrNoOfLinesToAutoDraw { get; set; }
    public int NOcrZoomFactor { get; set; }
    public string NOcrLineAlgorithm { get; set; }
    public string PaddleOcrMode { get; set; }
    public string PaddleOcrLastLanguage { get; set; }
    public string TesseractLastLanguage { get; set; }
    public int TesseractEngineMode { get; set; }
    public string GoogleVisionOcrLastLanguage { get; set; }
    public string GoogleLensOcrLastLanguage { get; set; }
    public bool DoTryToGuessUnknownWords { get; set; }
    public bool DoPromptForUnknownWords { get; set; }
    public bool DoAutoBreak { get; set; }
    public bool CaptureAssaPosition { get; set; }
    public bool DoFixOcrErrors { get; set; }
    public string LastLanguageDictionaryFile { get; set; }
    public decimal TextBoxFontSize { get; set; }
    public bool TextBoxFontBold { get; set; }
    public string TextBoxFontName { get; set; }
    public bool UseWordSplitList { get; set; }
    public bool VobSubUseCustomColors { get; set; }
    public string VobSubColorBackground { get; set; }
    public string VobSubColorPattern { get; set; }
    public string VobSubColorEmphasis1 { get; set; }
    public string VobSubColorEmphasis2 { get; set; }

    public SeOcr()
    {
        Engine = "nOCR";
        DoFixOcrErrors = true;
        DoTryToGuessUnknownWords = true;

        NOcrDatabase = "Latin";
        NOcrBinaryOcrFallbackDatabase = string.Empty;
        BinaryOcrNOcrFallbackDatabase = string.Empty;
        NOcrMaxWrongPixels = 25;
        NOcrPixelsAreSpace = 12;
        NOcrDrawUnknownText = true;
        NOcrNoOfLinesToAutoDraw = 60;
        NOcrZoomFactor = 4;
        NOcrLineAlgorithm = "Random";

        BinaryOcrPixelsAreSpace = 12;
        BinaryOcrMaxErrorPercent = 7.5;

        OllamaModels = ["glm-ocr"];
        OllamaLanguage = "English";
        OllamaModel = OllamaModels.First();
        OllamaUrl = "http://localhost:11434/api/chat";
        OllamaOcrTimeoutMinutes = 5;

        LlamaCppUrl = "http://127.0.0.1:8080/v1/chat/completions";
        LlamaCppOcrModel = string.Empty;
        LlamaCppOcrPrompt = "Extract all text exactly as written. The language is {language}. Preserve line breaks.";
        LlamaCppOcrTimeoutMinutes = 5;

        CrispEmbedBackend = "GLM-OCR";
        CrispEmbedModel = "glm-ocr-q8_0.gguf"; // the CrispEmbed registry's default GLM-OCR quant
        CrispEmbedOcrTimeoutMinutes = 5;

        GoogleVisionApiKey = string.Empty;
        GoogleVisionLanguage = "en";

        PaddleOcrMode = "mobile";
        PaddleOcrLastLanguage = "en";
        TesseractLastLanguage = "eng";
        TesseractEngineMode = 3; // Default, based on what is available (tesseract --oem)

        GoogleVisionOcrLastLanguage = "en";
        GoogleLensOcrLastLanguage = "en";

        MistralApiKey = string.Empty;
        
        LastLanguageDictionaryFile = string.Empty;

        TextBoxFontSize = 14;
        TextBoxFontName = string.Empty;

        UseWordSplitList = true;

        CaptureAssaPosition = false;

        PromptForBlankOcrText = true;

        VobSubUseCustomColors = false;
        VobSubColorBackground = "#00000000";
        VobSubColorPattern = "#FF000000";
        VobSubColorEmphasis1 = "#FFFFFFFF";
        VobSubColorEmphasis2 = "#FF000000";
    }
}