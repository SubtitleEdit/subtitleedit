using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Grammar;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.GrammarCheck;

/// <summary>
/// One LanguageTool match against one subtitle line. Unlike an AI review suggestion this is a span
/// inside the line, not a rewrite of it, so several of these can point at the same line - they are
/// applied together in <see cref="GrammarCheckViewModel"/>.
///
/// The category vocabulary (and its colours) is shared with AI review on purpose: both windows list
/// the same kinds of issue, and two colour schemes for one concept would only confuse.
/// </summary>
public partial class GrammarCheckSuggestionItem : ObservableObject
{
    private static readonly Dictionary<ReviewCategory, IBrush> BackgroundBrushes = new();

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _replacement = string.Empty;

    public int Number { get; init; }
    public int ParagraphIndex { get; init; }

    /// <summary>Start of the flagged text inside the line.</summary>
    public int Offset { get; init; }

    public int Length { get; init; }
    public ReviewCategory Category { get; init; }

    /// <summary>The line as it is now.</summary>
    public string Before { get; init; } = string.Empty;

    /// <summary>The flagged text itself, e.g. "go" in "He go to school".</summary>
    public string Fragment { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
    public string ShortMessage { get; init; } = string.Empty;
    public string RuleId { get; init; } = string.Empty;
    public IReadOnlyList<string> Replacements { get; init; } = new List<string>();

    /// <summary>False for rules that only point at a problem without offering a fix.</summary>
    public bool CanApply => Replacements.Count > 0;

    /// <summary>The line with this one replacement applied - what the "After" column shows.</summary>
    public string After => CanApply
        ? LanguageToolFix.Apply(Before, new[] { new LanguageToolFixItem(Offset, Length, Replacement) })
        : Before;

    /// <summary>Short label for the grid; LanguageTool leaves the short message out for many rules.</summary>
    public string IssueDisplay => ShortMessage.Length > 0 ? ShortMessage : Message;

    public string CategoryDisplay => Category switch
    {
        ReviewCategory.Spelling => Se.Language.Tools.GrammarCheck.CategorySpelling,
        ReviewCategory.Grammar => Se.Language.Tools.GrammarCheck.CategoryGrammar,
        ReviewCategory.Punctuation => Se.Language.Tools.GrammarCheck.CategoryPunctuation,
        ReviewCategory.Casing => Se.Language.Tools.GrammarCheck.CategoryCasing,
        _ => Se.Language.Tools.GrammarCheck.CategoryStyle,
    };

    public IBrush CategoryBrush => ReviewSuggestionItem.GetBrushForCategory(Category);

    public IBrush CategoryBackgroundBrush => GetBackgroundBrush(Category);

    public string CategoryIconName => Category switch
    {
        ReviewCategory.Spelling => "mdi-spellcheck",
        ReviewCategory.Grammar => "mdi-text",
        ReviewCategory.Punctuation => "mdi-comma",
        ReviewCategory.Casing => "mdi-format-letter-case",
        _ => "mdi-dots-horizontal",
    };

    partial void OnReplacementChanged(string value)
    {
        OnPropertyChanged(nameof(After));
    }

    /// <summary>
    /// Maps a LanguageTool rule onto the shared review categories. The rule category is the better
    /// signal (LanguageTool groups its own rules with it); the issue type only fills the gaps.
    /// </summary>
    public static ReviewCategory MapCategory(string categoryId, string issueType)
    {
        switch ((categoryId ?? string.Empty).ToUpperInvariant())
        {
            case "CASING":
                return ReviewCategory.Casing;
            case "TYPOS":
            case "TYPO":
                return ReviewCategory.Spelling;
            case "PUNCTUATION":
            case "TYPOGRAPHY":
                return ReviewCategory.Punctuation;
            case "GRAMMAR":
            case "COMPOUNDING":
            case "CONFUSED_WORDS":
            case "COLLOCATIONS":
            case "SEMANTICS":
                return ReviewCategory.Grammar;
        }

        switch ((issueType ?? string.Empty).ToLowerInvariant())
        {
            case "misspelling":
                return ReviewCategory.Spelling;
            case "grammar":
            case "duplication":
            case "inconsistency":
            case "agreement":
                return ReviewCategory.Grammar;
            case "typographical":
            case "whitespace":
            case "formatting":
            case "characters":
                return ReviewCategory.Punctuation;
            default:
                return ReviewCategory.Other;
        }
    }

    private static IBrush GetBackgroundBrush(ReviewCategory category)
    {
        if (BackgroundBrushes.TryGetValue(category, out var brush))
        {
            return brush;
        }

        var color = (ReviewSuggestionItem.GetBrushForCategory(category) as ISolidColorBrush)?.Color ?? Colors.Gray;
        brush = new SolidColorBrush(Color.FromArgb(0x20, color.R, color.G, color.B));
        BackgroundBrushes[category] = brush;
        return brush;
    }
}
