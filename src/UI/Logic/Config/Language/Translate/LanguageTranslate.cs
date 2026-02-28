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
    }
}