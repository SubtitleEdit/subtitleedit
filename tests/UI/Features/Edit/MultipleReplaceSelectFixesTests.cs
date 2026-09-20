using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Linq;
using System.Threading;

namespace UITests.Features.Edit;

/// <summary>
/// The Multiple replace preview ticks every fix it finds, so keeping a handful of lines out of a
/// long list meant unticking the rest by hand (#13502). Tick all / untick all / invert are the
/// same three commands the sibling lists offer.
/// </summary>
public class MultipleReplaceSelectFixesTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static MultipleReplaceViewModel NewViewModel()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();
        return vm;
    }

    // Three lines that all match one rule, so the preview holds three fixes.
    private static MultipleReplaceViewModel WithThreeFixes()
    {
        var vm = NewViewModel();
        var category = new RuleTreeNode(true) { CategoryName = "c1", IsActive = true };
        vm.Nodes.Add(category);
        category.SubNodes!.Add(new RuleTreeNode(false)
        {
            Find = "colour",
            ReplaceWith = "color",
            IsActive = true,
            Parent = category,
            Type = MultipleReplaceType.CaseInsensitive,
        });

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("The colour is red.", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("The colour is green.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("The colour is blue.", 4000, 6000));
        vm.Initialize(subtitle);

        WaitForPreview(vm, 3);
        Assert.Equal(3, vm.Fixes.Count);
        return vm;
    }

    // The preview is generated on a background timer, so waiting is the only way to observe it.
    private static void WaitForPreview(MultipleReplaceViewModel vm, int expectedFixes)
    {
        var end = Environment.TickCount64 + 3000;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
            if (vm.Fixes.Count == expectedFixes)
            {
                Dispatcher.UIThread.RunJobs();
                return;
            }

            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static string Ticked(MultipleReplaceViewModel vm) =>
        string.Join(",", vm.Fixes.Where(f => f.Apply).Select(f => f.Number));

    private static bool SendKey(MultipleReplaceViewModel vm, Key key, KeyModifiers modifiers)
    {
        var e = new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent };
        return vm.HandleFixesSelectionKey(e);
    }

    [AvaloniaFact]
    public void SelectNoFixes_UnticksEveryFix()
    {
        var vm = WithThreeFixes();

        vm.SelectNoFixesCommand.Execute(null);

        Assert.Equal(string.Empty, Ticked(vm));
    }

    [AvaloniaFact]
    public void SelectAllFixes_TicksEveryFix()
    {
        var vm = WithThreeFixes();
        vm.SelectNoFixesCommand.Execute(null);

        vm.SelectAllFixesCommand.Execute(null);

        Assert.Equal("1,2,3", Ticked(vm));
    }

    [AvaloniaFact]
    public void InvertFixesSelection_FlipsEveryFix()
    {
        var vm = WithThreeFixes();
        vm.Fixes[1].Apply = false;

        vm.InvertFixesSelectionCommand.Execute(null);

        Assert.Equal("2", Ticked(vm));
    }

    // The checkboxes are bound to Apply, so they only follow the commands if the fix raises
    // property changed - it was a plain property until #13502.
    [AvaloniaFact]
    public void Fix_RaisesPropertyChangedForApply()
    {
        var fix = new MultipleReplaceFix();
        var raised = 0;
        fix.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MultipleReplaceFix.Apply))
            {
                raised++;
            }
        };

        fix.Apply = false;

        Assert.Equal(1, raised);
    }

    [AvaloniaTheory]
    [InlineData(Key.A, KeyModifiers.Control, "1,2,3")]
    [InlineData(Key.A, KeyModifiers.Meta, "1,2,3")]
    [InlineData(Key.D, KeyModifiers.Control, "")]
    [InlineData(Key.D, KeyModifiers.Meta, "")]
    [InlineData(Key.I, KeyModifiers.Control | KeyModifiers.Shift, "1,3")]
    [InlineData(Key.I, KeyModifiers.Meta | KeyModifiers.Shift, "1,3")]
    public void AdvertisedGestures_TickUntickAndInvert(Key key, KeyModifiers modifiers, string expected)
    {
        var vm = WithThreeFixes();
        vm.Fixes[0].Apply = false;
        vm.Fixes[2].Apply = false;

        Assert.True(SendKey(vm, key, modifiers));

        Assert.Equal(expected, Ticked(vm));
    }

    // Ctrl+D is "duplicate rule" in this window and Ctrl+A is "select all rows" in the grid, so
    // everything the fixes grid does not claim has to travel on untouched.
    [AvaloniaTheory]
    [InlineData(Key.A, KeyModifiers.None)]
    [InlineData(Key.A, KeyModifiers.Shift)]
    [InlineData(Key.A, KeyModifiers.Control | KeyModifiers.Alt)]
    [InlineData(Key.D, KeyModifiers.Control | KeyModifiers.Shift)]
    [InlineData(Key.I, KeyModifiers.Control)]
    [InlineData(Key.N, KeyModifiers.Control)]
    public void OtherGestures_AreLeftAlone(Key key, KeyModifiers modifiers)
    {
        var vm = WithThreeFixes();
        vm.Fixes[1].Apply = false;

        Assert.False(SendKey(vm, key, modifiers));

        Assert.Equal("1,3", Ticked(vm));
    }

    // "Untick everything, then OK" has to leave the subtitle alone. OK regenerates the preview
    // before reverting the unticked lines, and only survives that because the regenerated list
    // reaches Fixes through the dispatcher - so assert on the result, not on the commands.
    [AvaloniaFact]
    public void Ok_AfterSelectNone_LeavesEveryLineUnchanged()
    {
        var vm = WithThreeFixes();

        vm.SelectNoFixesCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.Equal("The colour is red.", vm.FixedSubtitle.Paragraphs[0].Text);
        Assert.Equal("The colour is green.", vm.FixedSubtitle.Paragraphs[1].Text);
        Assert.Equal("The colour is blue.", vm.FixedSubtitle.Paragraphs[2].Text);
    }

    [AvaloniaFact]
    public void Ok_AfterInvert_KeepsOnlyTheTickedLines()
    {
        var vm = WithThreeFixes();

        vm.SelectNoFixesCommand.Execute(null);
        vm.Fixes[1].Apply = true;
        vm.InvertFixesSelectionCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("The color is red.", vm.FixedSubtitle.Paragraphs[0].Text);
        Assert.Equal("The colour is green.", vm.FixedSubtitle.Paragraphs[1].Text);
        Assert.Equal("The color is blue.", vm.FixedSubtitle.Paragraphs[2].Text);
    }
}
