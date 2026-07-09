namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageAiAssistant
{
    public string Title { get; set; }
    public string CurrentLine { get; set; }
    public string ContextLines { get; set; }
    public string FixErrors { get; set; }
    public string FitReadingSpeed { get; set; }
    public string MakeFormal { get; set; }
    public string MakeInformal { get; set; }
    public string AskPlaceholder { get; set; }
    public string Ask { get; set; }
    public string Result { get; set; }
    public string Apply { get; set; }
    public string Hint { get; set; }
    public string ShowReasoning { get; set; }

    public LanguageAiAssistant()
    {
        Title = "AI assistant";
        CurrentLine = "Current line";
        ContextLines = "Context (lines before and after)";
        FixErrors = "Fix errors";
        FitReadingSpeed = "Fit reading speed";
        MakeFormal = "More formal";
        MakeInformal = "More casual";
        AskPlaceholder = "Ask about this line, or ask for a change...";
        Ask = "Ask";
        Result = "Suggestion";
        Apply = "Apply to line";
        Hint = "AI assistant for this line";
        ShowReasoning = "Show the model's reasoning";
    }
}
