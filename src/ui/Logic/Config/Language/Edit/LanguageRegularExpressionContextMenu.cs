namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageRegularExpressionContextMenu
{
    public string WordBoundary { get; set; }
    public string NonWordBoundary { get; set; }
    public string NewLine { get; set; }
    public string NewLineShort { get; set; }
    public string AnyDigit { get; set; }
    public string NonDigit { get; set; }
    public string AnyCharacter { get; set; }
    public string AnyWhitespace { get; set; }
    public string NonSpaceCharacter { get; set; }
    public string ZeroOrMore { get; set; }
    public string OneOrMore { get; set; }
    public string InCharacterGroup { get; set; }
    public string NotInCharacterGroup { get; set; }

    public LanguageRegularExpressionContextMenu()
    {
        WordBoundary = "Word boundary (\\b)";
        NonWordBoundary = "Non word boundary (\\B)";
        NewLine = "New line (\\r\\n)";
        NewLineShort = "New line (\\n)";
        AnyDigit = "Any digit (\\d)";
        NonDigit = "Non digit (\\D)";
        AnyCharacter = "Any character (.)";
        AnyWhitespace = "Any whitespace (\\s)";
        NonSpaceCharacter = "Non space character (\\S)";
        ZeroOrMore = "Zero or more (*)";
        OneOrMore = "One or more (+)";
        InCharacterGroup = "In character group ([test])";
        NotInCharacterGroup = "Not in character group ([^test])";
    }
}
