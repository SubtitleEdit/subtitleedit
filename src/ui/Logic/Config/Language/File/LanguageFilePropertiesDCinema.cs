namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageFilePropertiesDCinema
{
    public string Title { get; internal set; }
    public string GenerateIdAuto { get; internal set; }
    public string SubtitleId { get; internal set; }
    public string GenerateId { get; internal set; }
    public string MovieTitle { get; internal set; }
    public string ReelNumber { get; internal set; }
    public string IssueDate { get; internal set; }
    public string Now { get; internal set; }
    public string EditRate { get; internal set; }
    public string TimeCodeRate { get; internal set; }
    public string StartTime { get; internal set; }
    public string Font { get; internal set; }
    public string FontId { get; internal set; }
    public string FontUri { get; internal set; }
    public string Generate { get; internal set; }
    public string FontColor { get; internal set; }
    public string ChooseColor { get; internal set; }
    public string FontEffect { get; internal set; }
    public string EffectColor { get; internal set; }
    public string FontSize { get; internal set; }
    public string TopBottomMargin { get; internal set; }
    public string FadeUpTime { get; internal set; }
    public string FadeDownTime { get; internal set; }
    public string Frames { get; internal set; }
    public string Export { get; internal set; }

    public LanguageFilePropertiesDCinema()
    {
        Title = "D-Cinema SMPTE Properties";
        GenerateIdAuto = "Generate new ID on save";
        SubtitleId = "Subtitle ID";
        GenerateId = "Generate ID";
        MovieTitle = "Movie title";
        ReelNumber = "Reel number";
        IssueDate = "Issue date";
        Now = "Now";
        EditRate = "Edit rate";
        TimeCodeRate = "Time code rate";
        StartTime = "Start time";
        Font = "Font";
        FontId = "Font ID";
        FontUri = "Font URI";
        Generate = "Generate";
        FontColor = "Font color";
        ChooseColor = "Choose color";
        FontEffect = "Font effect";
        EffectColor = "Effect color";
        FontSize = "Font size";
        TopBottomMargin = "Top/bottom margin";
        FadeUpTime = "Fade up time";
        FadeDownTime = "Fade down time";
        Frames = "Frames";
        Export = "Export...";
    }

}