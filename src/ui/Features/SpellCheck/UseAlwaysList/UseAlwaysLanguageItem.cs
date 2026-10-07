namespace Nikse.SubtitleEdit.Features.SpellCheck.UseAlwaysList;

public class UseAlwaysLanguageItem
{
    public string Name { get; }

    /// <summary>Five-letter language name used in the file name, e.g. "en_US" for en_US_UseAlways.xml.</summary>
    public string Code { get; }

    /// <summary>Hunspell dictionary for this language, if installed - used to spot pairs that can never apply.</summary>
    public string? DictionaryFileName { get; }

    public UseAlwaysLanguageItem(string name, string code, string? dictionaryFileName)
    {
        Name = name;
        Code = code;
        DictionaryFileName = dictionaryFileName;
    }

    public override string ToString() => Name;
}
