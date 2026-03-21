namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageTransparentVideo
{
    public string Title { get; set; }
    public string InfoAssaOff { get; set; }
    public string InfoAssaOn { get; set; }

    public LanguageTransparentVideo()
    {
        Title = "Generate transparent video with subtitles";
        InfoAssaOff = "Note: Advanced SubStation Alpha styling supported.";
        InfoAssaOn = "Note: Advanced SubStation Alpha styling will be used :)";
    }
}