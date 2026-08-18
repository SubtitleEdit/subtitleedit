using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// "Apply" used to work like OK: it wrote the checked fixes and closed the window, so applying a
/// second batch meant running the whole review again - minutes of model time for a subtitle that
/// needs suggestion-by-suggestion judgement (issue #13807). With a live target the window now stays
/// open, hands each pass to the caller and drops the rows it applied.
/// </summary>
public class AiReviewApplyTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static AiReviewViewModel MakeViewModel()
    {
        return new AiReviewViewModel(new WindowService(new NullServiceProvider()));
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Their going home.", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("Its to late.", 1500, 2500));
        subtitle.Paragraphs.Add(new Paragraph("Who's car is that?", 3000, 4000));
        return subtitle;
    }

    private static ReviewSuggestionItem MakeSuggestion(int paragraphIndex, string before, string after)
    {
        return new ReviewSuggestionItem
        {
            Number = paragraphIndex + 1,
            ParagraphIndex = paragraphIndex,
            UnitId = paragraphIndex,
            Category = ReviewCategory.Spelling,
            Before = before,
            After = after,
            IsSelected = true,
        };
    }

    /// <summary>
    /// Fills the view model the way a finished review does: the suggestions the grid shows are the
    /// public collection, which Apply also prunes.
    /// </summary>
    private static void AddSuggestions(AiReviewViewModel vm, params ReviewSuggestionItem[] items)
    {
        foreach (var item in items)
        {
            vm.AddSuggestionItem(item);
        }
    }

    [AvaloniaFact]
    public void Apply_WithCallback_HandsFixesOverAndKeepsTheWindowOpen()
    {
        var applied = new List<Subtitle>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, null, null, applied.Add);
        AddSuggestions(vm,
            MakeSuggestion(0, "Their going home.", "They're going home."),
            MakeSuggestion(1, "Its to late.", "It's too late."));

        vm.OkCommand.Execute(null);

        Assert.Single(applied);
        Assert.Equal("They're going home.", applied[0].Paragraphs[0].Text);
        Assert.Equal("It's too late.", applied[0].Paragraphs[1].Text);
        Assert.Equal("Who's car is that?", applied[0].Paragraphs[2].Text); // untouched line
        Assert.False(vm.OkPressed); // OkPressed closes the dialog for callers without a target
    }

    [AvaloniaFact]
    public void Apply_WithCallback_DropsTheAppliedRowsAndKeepsTheRest()
    {
        var applied = new List<Subtitle>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, null, null, applied.Add);
        var first = MakeSuggestion(0, "Their going home.", "They're going home.");
        var second = MakeSuggestion(2, "Who's car is that?", "Whose car is that?");
        second.IsSelected = false;
        AddSuggestions(vm, first, second);

        vm.OkCommand.Execute(null);

        Assert.Equal(new[] { second }, vm.Suggestions.ToArray());
        Assert.False(second.IsSelected); // the remaining row keeps its own checkbox state
        Assert.Equal(0, vm.SelectedCount);
    }

    [AvaloniaFact]
    public void Apply_SecondPass_BuildsOnTheFirstOne()
    {
        var applied = new List<Subtitle>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, null, null, applied.Add);
        var first = MakeSuggestion(0, "Their going home.", "They're going home.");
        var second = MakeSuggestion(2, "Who's car is that?", "Whose car is that?");
        second.IsSelected = false;
        AddSuggestions(vm, first, second);

        vm.OkCommand.Execute(null);
        second.IsSelected = true;
        vm.OkCommand.Execute(null);

        Assert.Equal(2, applied.Count);
        // The second pass carries the first pass's fix - it is applied to the updated subtitle.
        Assert.Equal("They're going home.", applied[1].Paragraphs[0].Text);
        Assert.Equal("Whose car is that?", applied[1].Paragraphs[2].Text);
        Assert.Empty(vm.Suggestions);
    }

    [AvaloniaFact]
    public void Apply_WithoutCallback_KeepsTheApplyAndCloseContract()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null);
        AddSuggestions(vm, MakeSuggestion(0, "Their going home.", "They're going home."));

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal("They're going home.", vm.FixedSubtitle.Paragraphs[0].Text);
        Assert.Single(vm.Suggestions); // nothing pruned - the window is closing
    }

    [AvaloniaFact]
    public void CloseButtonText_FollowsTheApplyMode()
    {
        var closing = MakeViewModel();
        closing.Initialize(MakeSubtitle(), null);
        Assert.Equal(Se.Language.General.Cancel, closing.CloseButtonText);

        var staying = MakeViewModel();
        staying.Initialize(MakeSubtitle(), null, null, null, _ => { });
        Assert.Equal(Se.Language.General.Done, staying.CloseButtonText);
    }
}
