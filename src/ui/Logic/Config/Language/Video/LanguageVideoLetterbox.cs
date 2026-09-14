namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageVideoLetterbox
{
    public string Title { get; set; }
    public string Enabled { get; set; }
    public string TopHeight { get; set; }
    public string BottomHeight { get; set; }
    public string Info { get; set; }

    public LanguageVideoLetterbox()
    {
        Title = "Letterboxing";
        Enabled = "Show letterbox bars";
        TopHeight = "Top bar height";
        BottomHeight = "Bottom bar height";
        Info = "Adds black bars over the video preview - a visual overlay only, the video file itself is never changed. Subtitles still render on top of the bars.";
    }
}
