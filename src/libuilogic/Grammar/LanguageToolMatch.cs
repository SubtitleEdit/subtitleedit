namespace Nikse.SubtitleEdit.UiLogic.Grammar;

/// <summary>
/// One issue reported by LanguageTool. <see cref="Offset"/> and <see cref="Length"/> are relative
/// to the complete text that was sent - markup included - see <see cref="LanguageToolAnnotatedText"/>.
/// </summary>
public class LanguageToolMatch
{
    public int Offset { get; init; }
    public int Length { get; init; }

    /// <summary>The full explanation, e.g. "The pronoun 'He' is usually used with a third-person verb".</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>A few words naming the issue, e.g. "Agreement error". Often empty.</summary>
    public string ShortMessage { get; init; } = string.Empty;

    /// <summary>Rule id, e.g. "HE_VERB_AGR" - the value to put in "disabled rules".</summary>
    public string RuleId { get; init; } = string.Empty;

    public string RuleDescription { get; init; } = string.Empty;

    /// <summary>LanguageTool issue type, e.g. "misspelling", "grammar", "typographical", "style".</summary>
    public string IssueType { get; init; } = string.Empty;

    /// <summary>Rule category id, e.g. "TYPOS", "GRAMMAR", "PUNCTUATION", "CASING".</summary>
    public string CategoryId { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    /// <summary>Suggested replacements for the flagged text, best first. Can be empty.</summary>
    public IReadOnlyList<string> Replacements { get; init; } = new List<string>();
}
