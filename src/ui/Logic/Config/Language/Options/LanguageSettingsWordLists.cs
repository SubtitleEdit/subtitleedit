namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageSettingsWordLists
{
    public string Title { get; set; }
    public string AddName { get; set; }
    public string NameAndIgnoreList { get; set; }
    public string UserWords { get; set; }
    public string RemovedWords { get; set; }
    public string AddRemovedWord { get; set; }
    public string OcrFixList { get; set; }
    public string UnableToAddItem { get; set; }
    public string UnableToRemoveItem { get; set; }
    public string RegexRulesDotDotDot { get; set; }
    public string RegexRulesTitle { get; set; }
    public string RegexRulesDescription { get; set; }
    public string RegexRules { get; set; }
    public string ShowAllRules { get; set; }
    public string ShowEnabledRules { get; set; }
    public string ShowDisabledRules { get; set; }
    public string ShowMyRules { get; set; }
    public string EnableShown { get; set; }
    public string DisableShown { get; set; }
    public string XOfYEnabled { get; set; }
    public string BuiltInRule { get; set; }
    public string MyRule { get; set; }
    public string Rule { get; set; }
    public string Pattern { get; set; }
    public string ValidPattern { get; set; }
    public string InvalidPatternX { get; set; }
    public string AddAsNewRule { get; set; }
    public string UpdateRule { get; set; }
    public string DeleteRule { get; set; }
    public string NewRule { get; set; }
    public string BuiltInRulesCanOnlyBeDisabled { get; set; }
    public string RuleAlreadyExists { get; set; }
    public string Insert { get; set; }
    public string TestText { get; set; }
    public string TestTextPlaceholder { get; set; }
    public string Matches { get; set; }
    public string Result { get; set; }
    public string ApplyAllEnabledRules { get; set; }
    public string XMatches { get; set; }
    public string OneMatch { get; set; }
    public string NoMatches { get; set; }
    public string XRulesChangedText { get; set; }
    public string EnableRule { get; set; }
    public string DisableRule { get; set; }
    public string CopyPattern { get; set; }
    public string StartOfLine { get; set; }
    public string EndOfLine { get; set; }
    public string WordCharacter { get; set; }
    public string Optional { get; set; }
    public string Either { get; set; }
    public string CaptureGroup { get; set; }
    public string StraightQuote { get; set; }
    public string CaptureGroupReference { get; set; }
    public string WholeMatch { get; set; }

    public LanguageSettingsWordLists()
    {
        Title = "Word lists";
        AddName = "Add name";
        NameAndIgnoreList = "Name/ignore list";
        UserWords = "User word list";
        RemovedWords = "Removed from dictionary";
        AddRemovedWord = "Add removed word";
        OcrFixList = "OCR fix list";
        UnableToAddItem = "Unable to add item, it probably already exists!";
        UnableToRemoveItem = "Unable to remove item!";
        RegexRulesDotDotDot = "Regex rules...";
        RegexRulesTitle = "OCR regex rules - {0}";
        RegexRulesDescription = "Regular expressions used by \"Fix common OCR errors\" in Fix common errors. Turn a rule off to stop it, or add your own.";
        RegexRules = "Rules";
        ShowAllRules = "All rules";
        ShowEnabledRules = "Enabled";
        ShowDisabledRules = "Disabled";
        ShowMyRules = "My rules";
        EnableShown = "Enable shown";
        DisableShown = "Disable shown";
        XOfYEnabled = "{0:#,##0} of {1:#,##0} enabled";
        BuiltInRule = "Built-in";
        MyRule = "My rule";
        Rule = "Rule";
        Pattern = "Find (regular expression)";
        ValidPattern = "Valid pattern";
        InvalidPatternX = "Invalid pattern: {0}";
        AddAsNewRule = "Add as new rule";
        UpdateRule = "Update rule";
        DeleteRule = "Delete rule";
        NewRule = "New rule";
        BuiltInRulesCanOnlyBeDisabled = "Built-in rules can only be turned off - change the pattern and add it as your own rule.";
        RuleAlreadyExists = "A rule with this pattern already exists.";
        Insert = "Insert";
        TestText = "Test";
        TestTextPlaceholder = "Type or paste text to try the rule on...";
        Matches = "Matches";
        Result = "Result";
        ApplyAllEnabledRules = "Run all enabled rules";
        XMatches = "{0:#,##0} matches";
        OneMatch = "1 match";
        NoMatches = "No matches";
        XRulesChangedText = "{0:#,##0} rules changed the text";
        EnableRule = "Enable rule";
        DisableRule = "Disable rule";
        CopyPattern = "Copy pattern";
        StartOfLine = "Start of line (^)";
        EndOfLine = "End of line ($)";
        WordCharacter = "Word character (\\w)";
        Optional = "Optional (?)";
        Either = "Either/or (|)";
        CaptureGroup = "Capture group ((...))";
        StraightQuote = "Straight double quote (\\x22)";
        CaptureGroupReference = "Text of capture group 1 ($1)";
        WholeMatch = "Whole match ($&)";
    }
}
