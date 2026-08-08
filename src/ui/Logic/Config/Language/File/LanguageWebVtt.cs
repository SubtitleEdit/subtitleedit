namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageWebVtt
{
    public string SetStylesForSelectedLinesTitle { get; set; }
    public string Voice { get; set; }
    public string ShowVoiceColumn { get; set; }
    public string Voices { get; set; }
    public string NewVoiceDotDotDot { get; set; }
    public string RemoveVoices { get; set; }
    public string VoiceName { get; set; }
    public string BrowserPreview { get; set; }
    public string ImportStylesTitle { get; set; }
    public string ExportStylesTitle { get; set; }
    public string OpenStyleFileTitle { get; set; }
    public string NoStylesToImport { get; set; }
    public string DuplicateStyleNamesX { get; set; }

    public LanguageWebVtt()
    {
        SetStylesForSelectedLinesTitle = "Set WebVTT styles for selected lines";
        Voice = "Voice";
        ShowVoiceColumn = "Show \"Voice\" column";
        Voices = "Voices";
        NewVoiceDotDotDot = "New voice...";
        RemoveVoices = "Remove voices";
        VoiceName = "Voice name";
        BrowserPreview = "Browser preview";
        ImportStylesTitle = "Import WebVTT styles";
        ExportStylesTitle = "Export WebVTT styles";
        OpenStyleFileTitle = "Open WebVTT file to import styles from";
        NoStylesToImport = "No styles found in the chosen file";
        DuplicateStyleNamesX = "Duplicate style names: {0}";
    }
}
