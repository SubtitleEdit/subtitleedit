using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using System;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// The context strip under the suggestion grid shows the lines before and after the selected
/// suggestion so a fix can be judged against its neighbors (issue #14619).
/// </summary>
public class AiReviewContextTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static AiReviewViewModel MakeViewModel()
    {
        var vm = new AiReviewViewModel(new WindowService(new NullServiceProvider()));
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line," + Environment.NewLine + "wrapped.", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("second line.", 1500, 2500));
        subtitle.Paragraphs.Add(new Paragraph("Third line.", 3000, 4000));
        vm.Initialize(subtitle, null);
        return vm;
    }

    private static ReviewSuggestionItem MakeSuggestion(int paragraphIndex, string after)
    {
        return new ReviewSuggestionItem
        {
            Number = paragraphIndex + 1,
            ParagraphIndex = paragraphIndex,
            UnitId = paragraphIndex,
            Category = ReviewCategory.Casing,
            Before = "before",
            After = after,
        };
    }

    [AvaloniaFact]
    public void SelectingSuggestion_ShowsPreviousAndNextLines()
    {
        var vm = MakeViewModel();
        var item = MakeSuggestion(1, "Second line.");
        vm.AddSuggestionItem(item);

        vm.SelectedSuggestion = item;

        Assert.True(vm.HasContext);
        Assert.Equal("Line 1", vm.ContextPreviousLabel);
        Assert.Equal("First line, wrapped.", vm.ContextPreviousText);
        Assert.Equal("Line 3", vm.ContextNextLabel);
        Assert.Equal("Third line.", vm.ContextNextText);
    }

    [AvaloniaFact]
    public void FirstLine_HasNoPreviousContext()
    {
        var vm = MakeViewModel();
        var item = MakeSuggestion(0, "First line, wrapped.");
        vm.AddSuggestionItem(item);

        vm.SelectedSuggestion = item;

        Assert.True(vm.HasContext);
        Assert.Equal(string.Empty, vm.ContextPreviousLabel);
        Assert.Equal(string.Empty, vm.ContextPreviousText);
        Assert.Equal("second line.", vm.ContextNextText);
    }

    [AvaloniaFact]
    public void NeighborWithCheckedFix_ShowsItsAfterText()
    {
        var vm = MakeViewModel();
        var first = MakeSuggestion(0, "First line fixed.");
        var second = MakeSuggestion(1, "Second line.");
        vm.AddSuggestionItem(first);
        vm.AddSuggestionItem(second);
        first.IsSelected = true;

        vm.SelectedSuggestion = second;
        Assert.Equal("First line fixed.", vm.ContextPreviousText);

        first.IsSelected = false;
        Assert.Equal("First line, wrapped.", vm.ContextPreviousText);
    }

    [AvaloniaFact]
    public void ClearingSelection_HidesContext()
    {
        var vm = MakeViewModel();
        var item = MakeSuggestion(1, "Second line.");
        vm.AddSuggestionItem(item);
        vm.SelectedSuggestion = item;

        vm.SelectedSuggestion = null;

        Assert.False(vm.HasContext);
        Assert.Equal(string.Empty, vm.ContextNextText);
    }
}
