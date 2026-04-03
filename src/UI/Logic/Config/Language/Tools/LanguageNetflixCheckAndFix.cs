namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageNetflixCheckAndFix
{
    public string Title { get; set; }
    public string GenerateReport { get; set; }
    public string NothingToReport { get; set; }
    public string SaveNetflixQualityReport { get; set; }
    public string NetflixReportSaved { get; set; }
    public string NetFlixQualityReportSavedToX { get; set; }
    public string DialogHyphenSpace { get; set; }
    public string EllipsesNotThreeDots { get; set; }
    public string OnlyAllowedGlyphs { get; set; }
    public string Italics { get; set; }
    public string MaxCharsSec { get; set; }
    public string MaxDuration { get; set; }
    public string MaxLineLength { get; set; }
    public string MinDuration { get; set; }
    public string MaxNumberOfLines { get; set; }
    public string OneToTenSpellOut { get; set; }
    public string ShotChanges { get; set; }
    public string StartNumberSpellOut { get; set; }
    public string TextforHiUseBrackets { get; set; }
    public string FrameRate { get; set; }
    public string TwoFrameGrap { get; set; }
    public string WhiteSpace { get; set; }

    public LanguageNetflixCheckAndFix()
    {
        Title = "Netflix check and fix errors";
        GenerateReport = "Generate report";
        NothingToReport = "No issues found.";
        SaveNetflixQualityReport = "Save Netflix quality report";
        NetflixReportSaved = "Netflix quality report saved";
        NetFlixQualityReportSavedToX = "Netflix quality report saved to:\n {0}";
        DialogHyphenSpace = "Dialog hyphen space";
        EllipsesNotThreeDots = "Use elipses (not three dots)";
        OnlyAllowedGlyphs = "Only allowed glyphs";
        Italics = "Italics";
        MaxCharsSec = "Max chars/sec";
        MaxDuration = "Max duration";
        MaxLineLength = "Max line length";
        MinDuration = "Min duration";
        MaxNumberOfLines = "Max number of lines";
        OneToTenSpellOut = "One to ten spell out";
        ShotChanges = "Shot changes";
        StartNumberSpellOut = "Start number spell out";
        TextforHiUseBrackets = "Text for HI, use brackets";
        FrameRate = "Frame rate";
        TwoFrameGrap = "Two frame gap";
        WhiteSpace = "White space";
    }
}