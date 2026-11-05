using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Edit;

/// <summary>
/// A rule is edited in its own row of the rules list - the separate "Edit rule" panel under the
/// list is gone. Enter / F2 / double-click open the row editor, Enter keeps the change, Escape
/// puts the texts back, and the preview follows while typing.
/// </summary>
public class MultipleReplaceInlineEditTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private (MultipleReplaceViewModel Vm, MultipleReplaceWindow Window, RuleTreeNode Rule) Open()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25;
        vm.Nodes.Clear();

        var category = new RuleTreeNode(true) { CategoryName = "c1", IsActive = true };
        vm.Nodes.Add(category);
        var rule = new RuleTreeNode(false)
        {
            Find = "colour",
            ReplaceWith = "color",
            Description = "US spelling",
            IsActive = true,
            Parent = category,
            Type = MultipleReplaceType.CaseInsensitive,
        };
        category.SubNodes!.Add(rule);

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("The colour is red.", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("The flavour is sweet.", 2000, 4000));
        vm.Initialize(subtitle);

        var window = new MultipleReplaceWindow(vm);
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.SelectedCategory = category;
        Dispatcher.UIThread.RunJobs();
        return (vm, window, rule);
    }

    private static void WaitFor(Func<bool> condition)
    {
        var end = Environment.TickCount64 + 3000;
        while (Environment.TickCount64 < end && !condition())
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static TextBox? FocusedTextBox(Window window) => window.FocusManager?.GetFocusedElement() as TextBox;

    private static void BeginEdit(Window window, MultipleReplaceViewModel vm, RuleTreeNode rule, string field)
    {
        vm.SelectedNode = rule;
        Dispatcher.UIThread.RunJobs();
        vm.RulesTableDoubleTapped(field);
        WaitFor(() => FocusedTextBox(window) != null);
    }

    [AvaloniaFact]
    public void EditRulePanel_IsGone()
    {
        var (_, window, _) = Open();

        var titles = window.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.DoesNotContain(Nikse.SubtitleEdit.Logic.Config.Se.Language.Edit.MultipleReplace.EditRule, titles);
    }

    [AvaloniaFact]
    public void DoubleClickedCell_GetsTheCaret_AndEnterKeepsTheChange()
    {
        var (vm, window, rule) = Open();
        BeginEdit(window, vm, rule, nameof(RuleTreeNode.ReplaceWith));

        Assert.True(rule.IsEditing);
        var box = FocusedTextBox(window);
        Assert.NotNull(box);
        Assert.Equal("color", box!.Text);

        window.KeyTextInput(" x");
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.False(rule.IsEditing);
        Assert.Equal("color x", rule.ReplaceWith); // the space was typed, not "toggle active"
        Assert.True(rule.IsActive);
        Assert.True(window.IsVisible); // Enter did not reach the window's OK
    }

    [AvaloniaFact]
    public void Escape_PutsTheTextsBack_AndKeepsTheWindowOpen()
    {
        var (vm, window, rule) = Open();
        BeginEdit(window, vm, rule, nameof(RuleTreeNode.Find));

        window.KeyTextInput("zzz");
        Assert.Equal("colourzzz", rule.Find);
        window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.False(rule.IsEditing);
        Assert.Equal("colour", rule.Find);
        Assert.True(window.IsVisible);
    }

    [AvaloniaFact]
    public void Preview_FollowsTheRowEditor()
    {
        var (vm, window, rule) = Open();
        WaitFor(() => vm.Fixes.Count == 1);
        Assert.Single(vm.Fixes);

        BeginEdit(window, vm, rule, nameof(RuleTreeNode.Find));
        var box = FocusedTextBox(window)!;
        box.Text = "our";
        WaitFor(() => vm.Fixes.Count == 2);

        Assert.Equal(2, vm.Fixes.Count);
    }

    [AvaloniaFact]
    public void SelectingAnotherRule_KeepsTheChange()
    {
        var (vm, window, rule) = Open();
        var other = new RuleTreeNode(false) { Find = "a", ReplaceWith = "b", IsActive = true, Parent = rule.Parent };
        rule.Parent!.SubNodes!.Add(other);
        Dispatcher.UIThread.RunJobs();

        BeginEdit(window, vm, rule, nameof(RuleTreeNode.Description));
        window.KeyTextInput("!");
        vm.SelectedNode = other;
        Dispatcher.UIThread.RunJobs();

        Assert.False(rule.IsEditing);
        Assert.Equal("US spelling!", rule.Description);
    }

    // Recycled cells raise the text and check box events, so scrolling rebuilds the preview.
    [AvaloniaFact]
    public void ScrollingTheRules_KeepsAnUntickedPreviewLineUnticked()
    {
        var (vm, window, rule) = Open();
        for (var i = 0; i < 200; i++)
        {
            rule.Parent!.SubNodes!.Add(new RuleTreeNode(false) { Find = "nomatch" + i, ReplaceWith = "x" + i, IsActive = true, Parent = rule.Parent });
        }

        vm.SelectedCategory = null;
        Dispatcher.UIThread.RunJobs();
        vm.SelectedCategory = rule.Parent;
        WaitFor(() => vm.Fixes.Count == 1);
        Settle();

        var fix = vm.Fixes[0];
        fix.Apply = false;
        var scrollViewer = vm.RulesTable.GetVisualDescendants().OfType<ScrollViewer>().First();
        scrollViewer.Offset = new Avalonia.Vector(0, scrollViewer.Extent.Height);
        window.UpdateLayout();
        WaitFor(() => !ReferenceEquals(fix, vm.Fixes[0]));

        Assert.NotSame(fix, vm.Fixes[0]); // the preview really was rebuilt
        Assert.False(vm.Fixes[0].Apply);
    }

    // A changed fix is a new one: it starts ticked.
    [AvaloniaFact]
    public void ChangingWhatARuleDoes_TicksThePreviewLineAgain()
    {
        var (vm, window, rule) = Open();
        WaitFor(() => vm.Fixes.Count == 1);
        Settle();
        vm.Fixes[0].Apply = false;

        BeginEdit(window, vm, rule, nameof(RuleTreeNode.ReplaceWith));
        FocusedTextBox(window)!.Text = "shade";
        WaitFor(() => vm.Fixes[0].After == "The shade is red.");

        Assert.True(vm.Fixes[0].Apply);
    }

    private static void Settle()
    {
        var end = Environment.TickCount64 + 300;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(20);
        }
    }
}
