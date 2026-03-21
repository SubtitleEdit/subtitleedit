namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageChangeCasing
{
    public string Title { get; set; }
    public string FixNames { get; set; }
    public string ExtraNames { get; set; }
    public string EnterExtraNamesHint { get; set; }
    public string OnlyFixUppercaseLines { get; set; }
    public string FixNamesOnly { get;  set; }
    public string AllUppercase { get; set; }
    public string AllLowercase { get; set; }

    public LanguageChangeCasing()
    {
        Title = "Change casing";
        FixNames = "Fix names";
        ExtraNames = "Extra names";
        EnterExtraNamesHint = "Enter extra names to fix, separated by comma";
        OnlyFixUppercaseLines = "Only fix uppercase lines";
        FixNamesOnly = "Fix names only";
        AllUppercase = "All uppercase";
        AllLowercase = "All lowercase";
    }
}