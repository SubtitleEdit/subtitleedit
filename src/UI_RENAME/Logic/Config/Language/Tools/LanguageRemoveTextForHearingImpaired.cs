namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRemoveTextForHearingImpaired
{
    public string Title { get; set; }
    public string Interjections { get; set; }
    public string SkipIfStartWith { get; set; }
    public string RemoveTextBetween { get; set; }
    public string Brackets { get; set; }
    public string CurlyBrackets { get; set; }
    public string Parentheses { get; set; }
    public string And { get; set; }
    public string OnlySeparateLines { get; set; }
    public string RemoveTextBeforeColon { get; set; }
    public string OnlyIfTextIsUppercase { get; set; }
    public string OnlyOnSeparateLine { get; set; }
    public string IfLineIsUppercase { get; set; }
    public string IfLineContains { get; set; }
    public string IfLineOnlyContainsMusicSymbols { get; set; }
    public string RemoveInterjections { get; set; }

    public LanguageRemoveTextForHearingImpaired()
    {
        Title = "Remove text for hearing impaired";
        Interjections = "Interjections";
        SkipIfStartWith = "Skip if start with";
        RemoveTextBetween = "Remove text between";
        Brackets = "Brackets";
        CurlyBrackets = "Curly brackets";
        Parentheses = "Parentheses";
        And = "and";
        OnlySeparateLines = "Only separate lines";
        RemoveTextBeforeColon = "Remove text before colon";
        OnlyIfTextIsUppercase = "Only if text is uppercase";
        OnlyOnSeparateLine = "Only on separate line";
        IfLineIsUppercase = "If line is uppercase";
        IfLineContains = "If line contains";
        IfLineOnlyContainsMusicSymbols = "If line only contains music symbols";
        RemoveInterjections = "Remove interjections";
    }
}
