namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageAbout
{

    public string Title { get; set; }
    public string TranslatedBy { get; set; }
    public string LicenseText { get; set; }
    public string DescriptionTextBeta { get; set; }
    public string IssueTrackingAndSourceCode { get; set; }
    public string GitHub { get; set; }
    public string Donate { get; set; }
    public string PayPal { get; set; }
    public string GitHubSponsor { get; set; }
    public string Or { get; set; }

    public LanguageAbout()
    {
        Title = "About Subtite Edit";
        TranslatedBy = "Translated by: {0}";
        LicenseText = "Subtitle Edit is free software under the MIT license.";
        DescriptionTextBeta = "Subtitle Edit 5 beta is a development version of our upcoming major release.\nWe are actively refining the new tools and appreciate your help in testing.\nPlease share your feedback to help us ensure the best possible final version.\n\nThank you for being part of the Subtitle Edit community! :)";
        IssueTrackingAndSourceCode = "Issue tracking and source code: ";
        GitHub = "Github";
        Donate = "Donate: ";
        PayPal = "PayPal";
        GitHubSponsor = "Github sponsor";
        Or = " or ";
    }
}