using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public partial class ReviewSuggestionItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    /// <summary>
    /// The replacement text. Observable because the user can edit it in place in the grid's
    /// After column before applying - the model's fix is often right in spirit but not to the
    /// letter, and correcting it here beats declining the fix and editing the line by hand.
    /// </summary>
    [ObservableProperty] private string _after = string.Empty;

    public int Number { get; init; }
    public int ParagraphIndex { get; init; }
    public int UnitId { get; init; }
    public ReviewCategory Category { get; init; }
    public string Before { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public bool IsWarning { get; init; }

    public string CategoryDisplay => Category switch
    {
        ReviewCategory.Spelling => Se.Language.Tools.AiReview.CategorySpelling,
        ReviewCategory.Grammar => Se.Language.Tools.AiReview.CategoryGrammar,
        ReviewCategory.Punctuation => Se.Language.Tools.AiReview.CategoryPunctuation,
        ReviewCategory.Casing => Se.Language.Tools.AiReview.CategoryCasing,
        _ => Se.Language.Tools.AiReview.CategoryOther,
    };

    public IBrush CategoryBrush => GetBrushForCategory(Category);

    public IBrush WarningBrush => IsWarning ? GetWarningBrush() : Brushes.Transparent;

    // The pastel brushes are tuned for the dark theme and are hard to read on white, so the
    // light theme gets darker shades of the same hues for text and icons (#15768).
    public static IBrush GetBrushForCategory(ReviewCategory category) => UiTheme.IsDarkThemeEnabled()
        ? category switch
        {
            ReviewCategory.Spelling => CategoryBrushes.Spelling,
            ReviewCategory.Grammar => CategoryBrushes.Grammar,
            ReviewCategory.Punctuation => CategoryBrushes.Punctuation,
            ReviewCategory.Casing => CategoryBrushes.Casing,
            _ => CategoryBrushes.Other,
        }
        : category switch
        {
            ReviewCategory.Spelling => CategoryBrushes.SpellingLight,
            ReviewCategory.Grammar => CategoryBrushes.GrammarLight,
            ReviewCategory.Punctuation => CategoryBrushes.PunctuationLight,
            ReviewCategory.Casing => CategoryBrushes.CasingLight,
            _ => CategoryBrushes.OtherLight,
        };

    public static IBrush GetWarningBrush() => UiTheme.IsDarkThemeEnabled()
        ? CategoryBrushes.Warning
        : CategoryBrushes.WarningLight;

    public string CategoryIconName => Category switch
    {
        ReviewCategory.Spelling => "mdi-spellcheck",
        ReviewCategory.Grammar => "mdi-text",
        ReviewCategory.Punctuation => "mdi-comma",
        ReviewCategory.Casing => "mdi-format-letter-case",
        _ => "mdi-dots-horizontal",
    };

    public IBrush CategoryBackgroundBrush => Category switch
    {
        ReviewCategory.Spelling => CategoryBrushes.SpellingBackground,
        ReviewCategory.Grammar => CategoryBrushes.GrammarBackground,
        ReviewCategory.Punctuation => CategoryBrushes.PunctuationBackground,
        ReviewCategory.Casing => CategoryBrushes.CasingBackground,
        _ => CategoryBrushes.OtherBackground,
    };

    private static class CategoryBrushes
    {
        public static readonly IBrush Spelling = new ImmutableSolidColorBrush(Color.FromRgb(0xe8, 0xb0, 0x4c));
        public static readonly IBrush Grammar = new ImmutableSolidColorBrush(Color.FromRgb(0xb4, 0x8c, 0xe8));
        public static readonly IBrush Punctuation = new ImmutableSolidColorBrush(Color.FromRgb(0x5f, 0xc6, 0xd8));
        public static readonly IBrush Casing = new ImmutableSolidColorBrush(Color.FromRgb(0xe8, 0x8c, 0xb0));
        public static readonly IBrush Other = new ImmutableSolidColorBrush(Color.FromRgb(0x9a, 0xa3, 0xad));
        public static readonly IBrush Warning = new ImmutableSolidColorBrush(Color.FromRgb(0xf0, 0xa6, 0x3c));
        public static readonly IBrush SpellingLight = new ImmutableSolidColorBrush(Color.FromRgb(0x8a, 0x5a, 0x00));
        public static readonly IBrush GrammarLight = new ImmutableSolidColorBrush(Color.FromRgb(0x6a, 0x3d, 0xb8));
        public static readonly IBrush PunctuationLight = new ImmutableSolidColorBrush(Color.FromRgb(0x00, 0x6b, 0x7d));
        public static readonly IBrush CasingLight = new ImmutableSolidColorBrush(Color.FromRgb(0xa8, 0x2e, 0x66));
        public static readonly IBrush OtherLight = new ImmutableSolidColorBrush(Color.FromRgb(0x55, 0x5e, 0x68));
        public static readonly IBrush WarningLight = new ImmutableSolidColorBrush(Color.FromRgb(0x9e, 0x55, 0x00));
        public static readonly IBrush SpellingBackground = new ImmutableSolidColorBrush(Color.FromArgb(0x20, 0xe8, 0xb0, 0x4c));
        public static readonly IBrush GrammarBackground = new ImmutableSolidColorBrush(Color.FromArgb(0x20, 0xb4, 0x8c, 0xe8));
        public static readonly IBrush PunctuationBackground = new ImmutableSolidColorBrush(Color.FromArgb(0x20, 0x5f, 0xc6, 0xd8));
        public static readonly IBrush CasingBackground = new ImmutableSolidColorBrush(Color.FromArgb(0x20, 0xe8, 0x8c, 0xb0));
        public static readonly IBrush OtherBackground = new ImmutableSolidColorBrush(Color.FromArgb(0x20, 0x9a, 0xa3, 0xad));
    }
}

public partial class ReviewFilterChip : ObservableObject
{
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private int _count;

    public ReviewCategory? Category { get; init; } // null = all
    public string Label { get; init; } = string.Empty;

    public string Display => Count > 0 ? $"{Label} ({Count})" : Label;

    partial void OnCountChanged(int value)
    {
        OnPropertyChanged(nameof(Display));
    }
}
