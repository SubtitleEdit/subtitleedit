namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public class LanguageItem
{
    public string Name { get; set; }
    public string TwoLetterISOLanguageName { get; set; }
    public string CultureSpecificCode { get; set; }

    public LanguageItem(string name, string twoLetterISOLanguageName, string cultureSpecificCode)
    {
        Name = name;
        TwoLetterISOLanguageName = twoLetterISOLanguageName;
        CultureSpecificCode = cultureSpecificCode;
    }

    public override string ToString() => Name;
}
