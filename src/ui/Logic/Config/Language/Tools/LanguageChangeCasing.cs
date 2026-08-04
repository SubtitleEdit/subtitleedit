namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageChangeCasing
{
    public string FixNames { get; set; }
    public string NamesX { get; set; }
    public string HitsX { get; set; }
    public string ExtraNames { get; set; }
    public string EnterExtraNamesHint { get; set; }
    public string OnlyFixUppercaseLines { get; set; }
    public string FixNamesOnly { get;  set; }
    public string AllUppercase { get; set; }
    public string AllLowercase { get; set; }

    public LanguageChangeCasing()
    {
        FixNames = "Fix names";
        NamesX = "Names: {0:#,##0}";
        HitsX = "Hits: {0:#,##0}";
        ExtraNames = "Extra names";
        EnterExtraNamesHint = "Enter extra names to fix, separated by comma";
        OnlyFixUppercaseLines = "Only fix uppercase lines";
        FixNamesOnly = "Fix names only";
        AllUppercase = "All uppercase";
        AllLowercase = "All lowercase";
    }
}