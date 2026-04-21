namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageTranslate
{
    public string TranslateViaCopyPaste { get; set; }
    public string MaxBlockSize { get; set; }
    public string LineSeparator { get; set; }
    public string BlockCopyInfo { get; set; }
    public object BlockCopyGetFromClipboard { get; set; }
    public string ReCopyTextToClipboard { get; set; }
    public string BlockXOfY { get; set; }
    public string NoTextInClipboard { get; set; }
    public string TextInClipboardIsSameAsSourceText { get; set; }
    public string LineMerge { get; set; }
    public string DelayInSecondsBetweenRequests { get; set; }
    public string MaxBytesPerRequest { get; set; }
    public string PromptText { get; set; }
    public string TranslateEachLineSeparately { get; set; }
    public string TranslationError { get; set; }
    public string TranslationFailedMessage { get; set; }
    public string TranslationFailedHint { get; set; }
    public string ShowTechnicalDetails { get; set; }
    public string HideTechnicalDetails { get; set; }
    public string TechnicalDetails { get; set; }
    public string PleaseSelectATargetLanguage { get; set; }

    public LanguageTranslate()
    {
        TranslateViaCopyPaste = "Auto-translate via copy/paste";
        MaxBlockSize = "Max block size";
        LineSeparator = "Line separator";
        BlockCopyInfo = "Go to translator/AI and paste text (already in clipboard), copy result back to clipboard and click the button below.";
        BlockCopyGetFromClipboard = "Get text from clipboard (from translator/AI)";
        ReCopyTextToClipboard = "Re-copy text to clipboard (for translator/AI)";
        BlockXOfY = "Block {0} of {1}";
        NoTextInClipboard = "No text in clipboard";
        TextInClipboardIsSameAsSourceText = "The text in clipboard is the same as the source text, please try again.";
        LineMerge = "Line merge";
        DelayInSecondsBetweenRequests = "Delay in seconds between requests";
        MaxBytesPerRequest = "Max bytes per request";
        PromptText = "Prompt text";
        TranslateEachLineSeparately = "Translate each line separately";
        TranslationError = "Translation error";
        TranslationFailedMessage = "{0} translation failed.";
        TranslationFailedHint = "This may be due to a network issue, an invalid API key, or API usage limits.\nPlease check your connection and API settings, then try again.";
        ShowTechnicalDetails = "Show technical details";
        HideTechnicalDetails = "Hide technical details";
        TechnicalDetails = "Technical details";
        PleaseSelectATargetLanguage = "Please select a target language";
    }
}