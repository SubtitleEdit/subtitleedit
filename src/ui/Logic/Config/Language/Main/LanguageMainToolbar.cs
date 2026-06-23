namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMainToolbar
{
    public string NewHint { get; set; }
    public string OpenHint { get; set; }
    public string OpenVideoHint { get; set; }
    public string SaveHint { get; set; }
    public string SaveAsHint { get; set; }
    public string FindHint { get; set; }
    public string ReplaceHint { get; set; }
    public string SpellCheckHint { get; set; }
    public string FixCommonErrorsHint { get; set; }
    public string RemoveTextForHiHint { get; set; }
    public string VisualSyncHint { get; set; }
    public string BeautifyTimeCodesHint { get; set; }
    public string BurnInHint { get; set; }
    public string SettingsHint { get; set; }
    public string LayoutHint { get; set; }
    public string HelpHint { get; set; }
    public string AutoBreakHint { get; set; }
    public string UnbreakHint { get; set; }
    public string AssaStylesHint { get; set; }
    public string AssaPropertiesHint { get; set; }
    public string AssaAttachmentsHint { get; set; }
    public string AssaDrawHint { get; set; }
    public string SsaStylesHint { get; set; }
    public string SsaPropertiesHint { get; set; }
    public string SsaAttachmentsHint { get; set; }


    public LanguageMainToolbar()
    {
        NewHint = "Start a new subtitle file {0}";
        OpenHint = "Open an existing subtitle file {0}";
        OpenVideoHint = "Open a video file {0}";
        SaveHint = "Save the current subtitle {0}";
        SaveAsHint = "Save subtitle with a new name {0}";
        FindHint = "Find text in subtitles {0}";
        ReplaceHint = "Find and replace text {0}";
        SpellCheckHint = "Check subtitles for spelling errors {0}";
        FixCommonErrorsHint = "Fix common subtitle errors {0}";
        RemoveTextForHiHint = "Remove text for hearing impaired {0}";
        VisualSyncHint = "Visual sync {0}";
        BeautifyTimeCodesHint = "Beautify time codes {0}";
        BurnInHint = "Burn subtitles into a video file {0}";
        SettingsHint = "Adjust program settings and preferences {0}";
        LayoutHint = "Change toolbar and panel layout {0}";
        HelpHint = "Open help website {0}";
        AutoBreakHint = "Automatically break long lines {0}";
        UnbreakHint = "Merge multi-line subtitles into one line {0}";
        AssaStylesHint = "Advanced Sub Station Alpha styles";
        AssaPropertiesHint = "Advanced Sub Station Alpha properties";
        AssaAttachmentsHint = "Advanced Sub Station Alpha attachments";
        AssaDrawHint = "Advanced Sub Station Alpha draw shapes";
        SsaStylesHint = "Sub Station Alpha styles";
        SsaPropertiesHint = "Sub Station Alpha properties";
        SsaAttachmentsHint = "Sub Station Alpha attachments";
    }
}