namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeTwoSubtitles
{
    public string Title { get; set; }
    public string Subtitle1 { get; set; }
    public string Subtitle2 { get; set; }
    public string LoadFromCurrentText { get; set; }
    public string LoadFromCurrentTranslation { get; set; }
    public string LoadFromFile { get; set; }
    public string OutputFormat { get; set; }
    public string OutputFormatSubRip { get; set; }
    public string OutputFormatAssa { get; set; }
    public string Style1 { get; set; }
    public string Style2 { get; set; }
    public string Merge { get; set; }

    public LanguageMergeTwoSubtitles()
    {
        Title = "Merge two subtitles";
        Subtitle1 = "Subtitle 1";
        Subtitle2 = "Subtitle 2";
        LoadFromCurrentText = "Load from current (text)";
        LoadFromCurrentTranslation = "Load from current (translation)";
        LoadFromFile = "Load from file...";
        OutputFormat = "Output format";
        OutputFormatSubRip = "SubRip (.srt)";
        OutputFormatAssa = "Advanced Sub Station Alpha (.ass)";
        Style1 = "Style 1";
        Style2 = "Style 2";
        Merge = "_Merge";
    }
}
