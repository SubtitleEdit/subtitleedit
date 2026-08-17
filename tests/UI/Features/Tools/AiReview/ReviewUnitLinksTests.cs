using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// Suggestions that share a sentence unit are checked and applied as one, so checking a single row
/// checks its siblings too - it looked like the grid selected two rows for one click (issue #13775).
/// These tests pin the marker that explains it: every suggestion in a multi-line unit carries the
/// line span, a suggestion that stands alone carries nothing.
/// </summary>
public class ReviewUnitLinksTests
{
    private static ReviewSuggestionItem MakeSuggestion(int number, int unitId)
    {
        return new ReviewSuggestionItem
        {
            Number = number,
            ParagraphIndex = number - 1,
            UnitId = unitId,
            Category = ReviewCategory.Spelling,
            Before = "before",
            After = "after",
        };
    }

    [Fact]
    public void Update_SentenceAcrossTwoLines_MarksBothWithTheLineSpan()
    {
        var seven = MakeSuggestion(7, 3);
        var eight = MakeSuggestion(8, 3);

        ReviewUnitLinks.Update(new List<ReviewSuggestionItem> { seven, eight });

        Assert.True(seven.IsLinked);
        Assert.True(eight.IsLinked);
        Assert.Equal(seven.LinkedLinesText, eight.LinkedLinesText);
        Assert.Contains("7", seven.LinkedLinesText);
        Assert.Contains("8", seven.LinkedLinesText);
    }

    [Fact]
    public void Update_SuggestionAloneInItsUnit_IsNotLinked()
    {
        var alone = MakeSuggestion(7, 3);
        var other = MakeSuggestion(9, 4);

        ReviewUnitLinks.Update(new List<ReviewSuggestionItem> { alone, other });

        Assert.False(alone.IsLinked);
        Assert.Equal(string.Empty, alone.LinkedLinesText);
        Assert.False(other.IsLinked);
    }

    [Fact]
    public void Update_SiblingArrivesLater_MarksTheEarlierSuggestionToo()
    {
        // Replies stream in chunk by chunk: line 7's suggestion exists on its own for a while, and
        // only a later reply makes its unit a linked one. The earlier row has to pick the marker up.
        var seven = MakeSuggestion(7, 3);
        var suggestions = new List<ReviewSuggestionItem> { seven };
        ReviewUnitLinks.Update(suggestions);
        Assert.False(seven.IsLinked);

        suggestions.Add(MakeSuggestion(8, 3));
        ReviewUnitLinks.Update(suggestions);

        Assert.True(seven.IsLinked);
    }

    [Fact]
    public void Update_LinkedLinesTextReachesTheAccessibleName()
    {
        var seven = MakeSuggestion(7, 3);
        var eight = MakeSuggestion(8, 3);

        ReviewUnitLinks.Update(new List<ReviewSuggestionItem> { seven, eight });

        Assert.Contains(seven.CategoryDisplay, seven.ApplyAccessibleName);
        Assert.Contains(seven.LinkedLinesText, seven.ApplyAccessibleName);
    }

    [AvaloniaFact]
    public void Window_LinkIcon_ShowsOnLinkedRowsOnly()
    {
        var linked = MakeSuggestion(7, 3);
        var alone = MakeSuggestion(9, 4);
        ReviewUnitLinks.Update(new List<ReviewSuggestionItem> { linked, MakeSuggestion(8, 3), alone });

        var vm = new AiReviewViewModel(new WindowService(new NullServiceProvider()));
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Line one", 0, 1000));
        vm.Initialize(subtitle, null);
        vm.Suggestions.Add(linked);
        vm.Suggestions.Add(alone);

        var window = new AiReviewWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var icons = window.GetVisualDescendants()
                .OfType<Optris.Icons.Avalonia.Icon>()
                .Where(i => i.Value == "mdi-link-variant")
                .ToList();

            Assert.Equal(2, icons.Count); // one per realized row
            Assert.Contains(icons, i => i.IsVisible && ReferenceEquals(i.DataContext, linked));
            Assert.Contains(icons, i => !i.IsVisible && ReferenceEquals(i.DataContext, alone));
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
