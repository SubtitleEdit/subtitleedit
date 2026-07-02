namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageAiReview
{
    public string Title { get; set; }
    public string Review { get; set; }
    public string Stop { get; set; }
    public string EditPrompt { get; set; }
    public string EditPromptTitle { get; set; }
    public string PromptInfo { get; set; }
    public string ProtocolInfo { get; set; }
    public string ResetToDefault { get; set; }
    public string ApplyXFixes { get; set; }
    public string XSuggestionsYSelected { get; set; }
    public string ReviewingLineXOfY { get; set; }
    public string ReviewDone { get; set; }
    public string NoIssuesFound { get; set; }
    public string CategoryAll { get; set; }
    public string CategorySpelling { get; set; }
    public string CategoryGrammar { get; set; }
    public string CategoryPunctuation { get; set; }
    public string CategoryCasing { get; set; }
    public string CategoryOther { get; set; }
    public string XNeedACloserLook { get; set; }
    public string LineX { get; set; }
    public string LinesXToY { get; set; }
    public string LargeChangeWarning { get; set; }
    public string EngineError { get; set; }

    public LanguageAiReview()
    {
        Title = "AI review";
        Review = "Review";
        Stop = "Stop";
        EditPrompt = "Edit prompt...";
        EditPromptTitle = "Edit review prompt";
        PromptInfo = "Instructions sent to the model - {language} is replaced with the subtitle language.";
        ProtocolInfo = "Subtitle Edit appends a strict JSON contract to the prompt: lines are sent as numbered JSON, and the model must answer with valid JSON containing only the changed lines. Replies that break the contract are retried, then skipped.";
        ResetToDefault = "Reset to default";
        ApplyXFixes = "Apply {0} fixes";
        XSuggestionsYSelected = "{0} suggestions - {1} selected";
        ReviewingLineXOfY = "Reviewing line {0} of {1}...";
        ReviewDone = "Review done - {0} suggestions in {1} lines";
        NoIssuesFound = "No issues found";
        CategoryAll = "All";
        CategorySpelling = "Spelling";
        CategoryGrammar = "Grammar";
        CategoryPunctuation = "Punctuation";
        CategoryCasing = "Casing";
        CategoryOther = "Other";
        XNeedACloserLook = "{0} suggestion(s) need a closer look";
        LineX = "Line {0}";
        LinesXToY = "Lines {0}-{1}";
        LargeChangeWarning = "Large change - looks like a rewrite, review carefully";
        EngineError = "The AI engine could not be reached: {0}";
    }
}
