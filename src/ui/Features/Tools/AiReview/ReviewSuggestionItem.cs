using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public partial class ReviewSuggestionItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public int Number { get; init; }
    public int ParagraphIndex { get; init; }
    public int UnitId { get; init; }
    public ReviewCategory Category { get; init; }
    public string Before { get; init; } = string.Empty;
    public string After { get; init; } = string.Empty;
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

    public IBrush CategoryBrush => Category switch
    {
        ReviewCategory.Spelling => CategoryBrushes.Spelling,
        ReviewCategory.Grammar => CategoryBrushes.Grammar,
        ReviewCategory.Punctuation => CategoryBrushes.Punctuation,
        ReviewCategory.Casing => CategoryBrushes.Casing,
        _ => CategoryBrushes.Other,
    };

    public IBrush WarningBrush => IsWarning ? CategoryBrushes.Warning : Brushes.Transparent;

    public static IBrush GetBrushForCategory(ReviewCategory category) => category switch
    {
        ReviewCategory.Spelling => CategoryBrushes.Spelling,
        ReviewCategory.Grammar => CategoryBrushes.Grammar,
        ReviewCategory.Punctuation => CategoryBrushes.Punctuation,
        ReviewCategory.Casing => CategoryBrushes.Casing,
        _ => CategoryBrushes.Other,
    };

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
        public static readonly IBrush Spelling = new SolidColorBrush(Color.FromRgb(0xe8, 0xb0, 0x4c));
        public static readonly IBrush Grammar = new SolidColorBrush(Color.FromRgb(0xb4, 0x8c, 0xe8));
        public static readonly IBrush Punctuation = new SolidColorBrush(Color.FromRgb(0x5f, 0xc6, 0xd8));
        public static readonly IBrush Casing = new SolidColorBrush(Color.FromRgb(0xe8, 0x8c, 0xb0));
        public static readonly IBrush Other = new SolidColorBrush(Color.FromRgb(0x9a, 0xa3, 0xad));
        public static readonly IBrush Warning = new SolidColorBrush(Color.FromRgb(0xf0, 0xa6, 0x3c));
        public static readonly IBrush SpellingBackground = new SolidColorBrush(Color.FromArgb(0x20, 0xe8, 0xb0, 0x4c));
        public static readonly IBrush GrammarBackground = new SolidColorBrush(Color.FromArgb(0x20, 0xb4, 0x8c, 0xe8));
        public static readonly IBrush PunctuationBackground = new SolidColorBrush(Color.FromArgb(0x20, 0x5f, 0xc6, 0xd8));
        public static readonly IBrush CasingBackground = new SolidColorBrush(Color.FromArgb(0x20, 0xe8, 0x8c, 0xb0));
        public static readonly IBrush OtherBackground = new SolidColorBrush(Color.FromArgb(0x20, 0x9a, 0xa3, 0xad));
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
