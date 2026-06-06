namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageAbout
{

    public string Title { get; set; }
    public string TranslatedBy { get; set; }
    public string LicenseText { get; set; }
    public string DescriptionText { get; set; }
    public string IssueTrackingAndSourceCode { get; set; }
    public string GitHub { get; set; }
    public string Donate { get; set; }
    public string PayPal { get; set; }
    public string GitHubSponsor { get; set; }
    public string Or { get; set; }

    public LanguageAbout()
    {
        Title = "About Subtitle Edit";
        TranslatedBy = "Translated by: {0}";
        LicenseText = "Subtitle Edit is free software under the MIT license.";
        DescriptionText = "Subtitle Edit is a free and open source editor for subtitle files, available on Windows, macOS, and Linux.\nIt supports 300+ subtitle formats, OCR, speech recognition, AI translation, and much more.\n\nThank you for being part of the Subtitle Edit community! :)";
        IssueTrackingAndSourceCode = "Issue tracking and source code: ";
        GitHub = "GitHub";
        Donate = "Donate: ";
        PayPal = "PayPal";
        GitHubSponsor = "GitHub sponsor";
        Or = " or ";
    }
}