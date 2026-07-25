namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageGrammarCheck
{
    public string Title { get; set; }
    public string Check { get; set; }
    public string Stop { get; set; }
    public string Server { get; set; }
    public string ServerHint { get; set; }
    public string TestConnection { get; set; }
    public string Connecting { get; set; }
    public string ConnectedXLanguages { get; set; }
    public string Picky { get; set; }
    public string PickyHint { get; set; }
    public string SettingsTitle { get; set; }
    public string SettingsInfo { get; set; }
    public string Username { get; set; }
    public string DisabledRules { get; set; }
    public string DisabledRulesHint { get; set; }
    public string MaxLinesPerBatch { get; set; }
    public string Issue { get; set; }
    public string Replacement { get; set; }
    public string NoAutomaticFix { get; set; }
    public string ApplyXFixes { get; set; }
    public string XIssuesYSelected { get; set; }
    public string CheckingLineXOfY { get; set; }
    public string CheckDone { get; set; }
    public string NoIssuesFound { get; set; }
    public string CategoryAll { get; set; }
    public string CategorySpelling { get; set; }
    public string CategoryGrammar { get; set; }
    public string CategoryPunctuation { get; set; }
    public string CategoryCasing { get; set; }
    public string CategoryStyle { get; set; }
    public string LineX { get; set; }
    public string ServerError { get; set; }

    public LanguageGrammarCheck()
    {
        Title = "Grammar check (LanguageTool)";
        Check = "Check";
        Stop = "Stop";
        Server = "Server";
        ServerHint = "Base address of the LanguageTool server, e.g. https://api.languagetool.org or your own installation";
        TestConnection = "Test connection and reload languages";
        Connecting = "Connecting to {0}...";
        ConnectedXLanguages = "Connected - {0} languages available";
        Picky = "Picky";
        PickyHint = "Also report the stricter style rules LanguageTool leaves off by default";
        SettingsTitle = "LanguageTool settings";
        SettingsInfo = "Username and API key are only needed for a premium account or a server that requires them.";
        Username = "User name";
        DisabledRules = "Disabled rules";
        DisabledRulesHint = "Comma separated rule ids to ignore, e.g. WHITESPACE_RULE,UPPERCASE_SENTENCE_START";
        MaxLinesPerBatch = "Lines per request";
        Issue = "Issue";
        Replacement = "Replacement";
        NoAutomaticFix = "no automatic fix - correct this one by hand";
        ApplyXFixes = "Apply {0} fixes";
        XIssuesYSelected = "{0} issues - {1} selected";
        CheckingLineXOfY = "Checking line {0} of {1}...";
        CheckDone = "Check done - {0} issues in {1} lines";
        NoIssuesFound = "No issues found";
        CategoryAll = "All";
        CategorySpelling = "Spelling";
        CategoryGrammar = "Grammar";
        CategoryPunctuation = "Punctuation";
        CategoryCasing = "Casing";
        CategoryStyle = "Style";
        LineX = "Line {0}";
        ServerError = "The LanguageTool server could not be reached: {0}";
    }
}
