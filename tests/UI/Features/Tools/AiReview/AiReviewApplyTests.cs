using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// The single button used to work like OK: it wrote the checked fixes and closed the window, so
/// applying a second batch meant running the whole review again - minutes of model time for a
/// subtitle that needs suggestion-by-suggestion judgement (issue #13807). The window now offers the
/// standard Apply/Ok pair: Apply hands a pass to the caller and stays open, dropping the rows it
/// applied; Ok writes the checked fixes and closes.
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

        vm.ApplyCommand.Execute(null);

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

        vm.ApplyCommand.Execute(null);

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

        vm.ApplyCommand.Execute(null);
        second.IsSelected = true;
        vm.ApplyCommand.Execute(null);

        Assert.Equal(2, applied.Count);
        // The second pass carries the first pass's fix - it is applied to the updated subtitle.
        Assert.Equal("They're going home.", applied[1].Paragraphs[0].Text);
        Assert.Equal("Whose car is that?", applied[1].Paragraphs[2].Text);
        Assert.Empty(vm.Suggestions);
    }

    [AvaloniaFact]
    public void Ok_WithoutCallback_KeepsTheApplyAndCloseContract()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null);
        AddSuggestions(vm, MakeSuggestion(0, "Their going home.", "They're going home."));

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal("They're going home.", vm.FixedSubtitle.Paragraphs[0].Text);
        Assert.Single(vm.Suggestions); // nothing pruned - the window is closing
    }

    /// <summary>
    /// Ok is the other half of the pair: it applies the checked fixes through the same callback and
    /// closes, so the last pass does not need Apply plus a separate close.
    /// </summary>
    [AvaloniaFact]
    public void Ok_WithCallback_AppliesThroughTheCallbackAndCloses()
    {
        var applied = new List<Subtitle>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, null, null, applied.Add);
        AddSuggestions(vm, MakeSuggestion(1, "Its to late.", "It's too late."));

        vm.OkCommand.Execute(null);

        Assert.Single(applied);
        Assert.Equal("It's too late.", applied[0].Paragraphs[1].Text);
        // The callback delivered the pass, so the pull-based contract stays off - a caller reading
        // both would apply the same fixes twice.
        Assert.False(vm.OkPressed);
    }

    /// <summary>
    /// Applying nothing would cost the caller an undo step and a "fixed 0 lines" status for an
    /// unchanged subtitle, so Apply is disabled until something is checked. Ok stays enabled: with
    /// no fixes checked it is simply a close.
    /// </summary>
    [AvaloniaFact]
    public void Apply_IsDisabledWhenNothingIsChecked()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, null, null, _ => { });
        var suggestion = MakeSuggestion(0, "Their going home.", "They're going home.");
        AddSuggestions(vm, suggestion);
        Assert.True(vm.ApplyCommand.CanExecute(null));

        vm.SelectNoneCommand.Execute(null);

        Assert.False(vm.ApplyCommand.CanExecute(null));
        Assert.True(vm.OkCommand.CanExecute(null));
    }

    /// <summary>
    /// Apply only makes sense with somewhere to push a pass to, so callers without a live target
    /// keep the plain Ok/Cancel pair - and the button bar must follow, not just the view model.
    /// </summary>
    [AvaloniaFact]
    public void ApplyButton_IsShownOnlyForCallersWithALiveTarget()
    {
        var inPasses = MakeViewModel();
        inPasses.Initialize(MakeSubtitle(), null, null, null, _ => { });
        var windowInPasses = new AiReviewWindow(inPasses);
        try
        {
            Assert.True(inPasses.IsApplyVisible);
            Assert.True(FindButton(windowInPasses, inPasses.ApplyCommand)?.IsVisible);
            Assert.NotNull(FindButton(windowInPasses, inPasses.OkCommand));
            Assert.NotNull(FindButton(windowInPasses, inPasses.CancelCommand));
        }
        finally
        {
            windowInPasses.Close();
        }

        var applyAndClose = MakeViewModel();
        applyAndClose.Initialize(MakeSubtitle(), null);
        var windowApplyAndClose = new AiReviewWindow(applyAndClose);
        try
        {
            Assert.False(applyAndClose.IsApplyVisible);
            Assert.False(FindButton(windowApplyAndClose, applyAndClose.ApplyCommand)?.IsVisible);
            Assert.NotNull(FindButton(windowApplyAndClose, applyAndClose.OkCommand));
        }
        finally
        {
            windowApplyAndClose.Close();
        }
    }

    private static Button? FindButton(Control root, ICommand command)
    {
        return root.GetLogicalDescendants().OfType<Button>().FirstOrDefault(b => b.Command == command);
    }
}
