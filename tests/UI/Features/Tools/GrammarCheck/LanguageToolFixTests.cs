using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Features.Tools.GrammarCheck;
using Nikse.SubtitleEdit.UiLogic.Grammar;
using System.Collections.Generic;

namespace UITests.Features.Tools.GrammarCheck;

public class LanguageToolFixTests
{
    [Fact]
    public void Apply_SingleFix_ReplacesTheSpan()
    {
        var text = "He go to school.";

        var result = LanguageToolFix.Apply(text, new[] { new LanguageToolFixItem(3, 2, "goes") });

        Assert.Equal("He goes to school.", result);
    }

    [Fact]
    public void Apply_SeveralFixesInOneLine_AllLandOnTheRightSpans()
    {
        var text = "I has a apple";

        var result = LanguageToolFix.Apply(text, new List<LanguageToolFixItem>
        {
            new(2, 3, "have"),
            new(6, 1, "an"),
        });

        Assert.Equal("I have an apple", result);
    }

    [Fact]
    public void Apply_OverlappingFixes_KeepsTheRightMostAndSkipsTheOverlap()
    {
        var text = "a apple";

        var result = LanguageToolFix.Apply(text, new List<LanguageToolFixItem>
        {
            new(0, 7, "an apple"),
            new(2, 5, "orange"),
        });

        // the fix starting at 2 is applied first (right to left), the one covering 0-7 overlaps it
        Assert.Equal("a orange", result);
    }

    [Fact]
    public void Apply_FixOutsideTheLine_IsIgnored()
    {
        var text = "Short";

        var result = LanguageToolFix.Apply(text, new[] { new LanguageToolFixItem(10, 3, "x") });

        Assert.Equal("Short", result);
    }

    [Fact]
    public void Apply_TagsBeforeTheFixDoNotShiftIt()
    {
        var text = "<i>He go</i> home";

        var result = LanguageToolFix.Apply(text, new[] { new LanguageToolFixItem(6, 2, "goes") });

        Assert.Equal("<i>He goes</i> home", result);
    }

    [Theory]
    [InlineData("CASING", "uppercase", ReviewCategory.Casing)]
    [InlineData("TYPOS", "misspelling", ReviewCategory.Spelling)]
    [InlineData("PUNCTUATION", "typographical", ReviewCategory.Punctuation)]
    [InlineData("TYPOGRAPHY", "whitespace", ReviewCategory.Punctuation)]
    [InlineData("GRAMMAR", "grammar", ReviewCategory.Grammar)]
    [InlineData("MISC", "misspelling", ReviewCategory.Spelling)]
    [InlineData("MISC", "grammar", ReviewCategory.Grammar)]
    [InlineData("STYLE", "style", ReviewCategory.Other)]
    [InlineData("", "", ReviewCategory.Other)]
    public void MapCategory_UsesTheRuleCategoryFirstAndTheIssueTypeAsFallback(string categoryId, string issueType, ReviewCategory expected)
    {
        Assert.Equal(expected, GrammarCheckSuggestionItem.MapCategory(categoryId, issueType));
    }
}
