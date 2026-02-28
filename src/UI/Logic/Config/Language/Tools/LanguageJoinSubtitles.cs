namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageJoinSubtitles
{
    public string Title { get; set; }
    public string KeepTimeCodes { get; set; }
    public string AppendTimeCodes { get; set; }
    public string AddMsAfterEachFile { get; set; }
    public string Join { get; set; }

    public LanguageJoinSubtitles()
    {
        Title = "Join subtitles";
        Join = "_Join";
        KeepTimeCodes = "Keep time codes";
        AppendTimeCodes = "Append time codes";
        AddMsAfterEachFile = "Add milliseconds after each file";
    }
}