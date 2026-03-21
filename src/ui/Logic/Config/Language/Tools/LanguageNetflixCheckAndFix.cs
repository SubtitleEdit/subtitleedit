namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageNetflixCheckAndFix
{
    public string Title { get; set; }
    public string GenerateReport { get; set; }
    public string NothingToReport { get; set; }
    public string SaveNetflixQualityReport { get; set; }
    public string NetflixReportSaved { get; set; }
    public string NetFlixQualityReportSavedToX { get; set; }

    public LanguageNetflixCheckAndFix()
    {
        Title = "Netflix check and fix errors";
        GenerateReport = "Generate report";
        NothingToReport = "No issues found.";
        SaveNetflixQualityReport = "Save Netflix quality report";
        NetflixReportSaved = "Netflix quality report saved";
        NetFlixQualityReportSavedToX = "Netflix quality report saved to:\n {0}";
    }
}