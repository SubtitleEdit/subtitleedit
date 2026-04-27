namespace Nikse.SubtitleEdit.Logic.Config;

public class SeDCinemaSmpte
{
    public string DCinemaFontFile { get; set; }
    public string DCinemaLoadFontResource { get; set; }
    public int DCinemaFontSize { get; set; }
    public int DCinemaBottomMargin { get; set; }
    public double DCinemaZPosition { get; set; }
    public int DCinemaFadeUpTime { get; set; }
    public int DCinemaFadeDownTime { get; set; }
    public bool DCinemaAutoGenerateSubtitleId { get; set; }

    public string CurrentDCinemaSubtitleId { get; set; }
    public string CurrentDCinemaMovieTitle { get; set; }
    public string CurrentDCinemaReelNumber { get; set; }
    public string CurrentDCinemaIssueDate { get; set; }
    public string CurrentDCinemaLanguage { get; set; }
    public string CurrentDCinemaEditRate { get; set; }
    public string CurrentDCinemaTimeCodeRate { get; set; }
    public string CurrentDCinemaStartTime { get; set; }
    public string CurrentDCinemaFontId { get; set; }
    public string CurrentDCinemaFontUri { get; set; }
    public string CurrentDCinemaFontColor { get; set; }
    public string CurrentDCinemaFontEffect { get; set; }
    public string CurrentDCinemaFontEffectColor { get; set; }
    public int CurrentDCinemaFontSize { get; set; }

    public SeDCinemaSmpte()
    {
        DCinemaFontFile = "Arial.ttf";
        DCinemaLoadFontResource = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
        DCinemaFontSize = 42;
        DCinemaBottomMargin = 8;
        DCinemaZPosition = 0;
        DCinemaFadeUpTime = 0;
        DCinemaFadeDownTime = 0;
        DCinemaAutoGenerateSubtitleId = true;

        CurrentDCinemaSubtitleId = string.Empty;
        CurrentDCinemaMovieTitle = string.Empty;
        CurrentDCinemaReelNumber = string.Empty;
        CurrentDCinemaIssueDate = string.Empty;
        CurrentDCinemaLanguage = string.Empty;
        CurrentDCinemaEditRate = string.Empty;
        CurrentDCinemaTimeCodeRate = string.Empty;
        CurrentDCinemaStartTime = string.Empty;
        CurrentDCinemaFontId = string.Empty;
        CurrentDCinemaFontUri = string.Empty;
        CurrentDCinemaFontColor = string.Empty;
        CurrentDCinemaFontEffect = string.Empty;
        CurrentDCinemaFontEffectColor = string.Empty;
        CurrentDCinemaFontSize = 0;
    }
}