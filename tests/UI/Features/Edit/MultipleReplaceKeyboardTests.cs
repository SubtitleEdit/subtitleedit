using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Edit;

/// <summary>
/// Keyboard use of Multiple replace (#17247): Space / Shift+Space on the category and rule lists
/// (the list box took Space as "select" before the bubbling handler saw it), a Tab order without
/// dead stops, and the panel / preview / apply shortcuts.
/// </summary>
public class MultipleReplaceKeyboardTests : IDisposable
{
    // A window left open keeps the application-wide focus, so a later test's key press would go to it.
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

    private (MultipleReplaceViewModel Vm, Window Window) Open()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();

        for (var c = 1; c <= 3; c++)
        {
            var category = new RuleTreeNode(true) { CategoryName = $"c{c}", IsActive = true };
            for (var r = 1; r <= 3; r++)
            {
                category.SubNodes!.Add(new RuleTreeNode(false)
                {
                    Find = $"c{c}r{r}",
                    ReplaceWith = "x",
                    IsActive = true,
                    Parent = category,
                    Type = MultipleReplaceType.CaseInsensitive,
                });
            }

            vm.Nodes.Add(category);
        }

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("c1r1 one", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("c1r2 two", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("c1r3 three", 4000, 6000));
        vm.Initialize(subtitle);

        var window = new MultipleReplaceWindow(vm);
        _windows.Add(window);
        window.Show();
        WaitForPreview(vm, 3);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    // Opening the window can mark the preview dirty again, and that second pass rebuilds the
    // rows - so wait until the fixes have stopped changing.
    private static void WaitForPreview(MultipleReplaceViewModel vm, int expectedFixes)
    {
        var lastChange = Environment.TickCount64;
        void OnChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => lastChange = Environment.TickCount64;
        vm.Fixes.CollectionChanged += OnChanged;
        try
        {
            var end = Environment.TickCount64 + 3000;
            while (Environment.TickCount64 < end)
            {
                Dispatcher.UIThread.RunJobs();
                if (vm.Fixes.Count == expectedFixes && Environment.TickCount64 - lastChange > 200)
                {
                    return;
                }

                Thread.Sleep(20);
            }
        }
        finally
        {
            vm.Fixes.CollectionChanged -= OnChanged;
            Dispatcher.UIThread.RunJobs();
        }
    }

    // Clicking the row is the only way to get real keyboard focus into a list.
    private static void Click(Window window, Control container)
    {
        var point = container.TranslatePoint(new Point(container.Bounds.Width / 2, 8), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void ClickCategory(Window window, MultipleReplaceViewModel vm, int index)
    {
        Click(window, vm.CategoriesListBox.ContainerFromItem(vm.Nodes[index])!);
        Assert.Same(vm.Nodes[index], vm.SelectedCategory);
    }

    // A rule of the first category - the rules list only shows the selected category's rules.
    private static void ClickRule(Window window, MultipleReplaceViewModel vm, int index)
    {
        vm.SelectedCategory = vm.Nodes[0];
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var rule = vm.Nodes[0].SubNodes![index];
        Click(window, vm.RulesTable.ContainerFromItem(rule)!);
        Assert.Same(rule, vm.SelectedNode);
    }

    private static void Press(Window window, PhysicalKey key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPressQwerty(key, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    private static Control? Focused(Window window) => window.FocusManager?.GetFocusedElement() as Control;

    private static string Active(IEnumerable<RuleTreeNode> nodes) =>
        string.Concat(nodes.Select(n => n.IsActive ? '1' : '0'));

    [AvaloniaFact]
    public void Space_TogglesTheSelectedCategory()
    {
        var (vm, window) = Open();
        ClickCategory(window, vm, 1);

        Press(window, PhysicalKey.Space);
        Assert.Equal("101", Active(vm.Nodes));

        Press(window, PhysicalKey.Space);
        Assert.Equal("111", Active(vm.Nodes));
    }

    [AvaloniaFact]
    public void Space_TogglesTheSelectedRule()
    {
        var (vm, window) = Open();
        ClickRule(window, vm, 2);

        Press(window, PhysicalKey.Space);
        Assert.Equal("110", Active(vm.Nodes[0].SubNodes!));

        Press(window, PhysicalKey.Space);
        Assert.Equal("111", Active(vm.Nodes[0].SubNodes!));
    }

    [AvaloniaFact]
    public void ShiftSpace_MakesTheSelectedCategoryTheOnlyActiveOne()
    {
        var (vm, window) = Open();
        ClickCategory(window, vm, 2);

        Press(window, PhysicalKey.Space, RawInputModifiers.Shift);

        Assert.Equal("001", Active(vm.Nodes));
        // ...and the rules inside are left as they were.
        Assert.All(vm.Nodes, c => Assert.Equal("111", Active(c.SubNodes!)));
    }

    [AvaloniaFact]
    public void ShiftSpace_MakesTheSelectedRuleTheOnlyActiveOneInItsCategory()
    {
        var (vm, window) = Open();
        ClickRule(window, vm, 1);

        Press(window, PhysicalKey.Space, RawInputModifiers.Shift);

        Assert.Equal("010", Active(vm.Nodes[0].SubNodes!));
        Assert.Equal("111", Active(vm.Nodes[1].SubNodes!));
        Assert.Equal("111", Active(vm.Nodes));
    }

    [AvaloniaFact]
    public void Tab_WalksTheDialogWithoutDeadStops_AndRuleButtonsLeftToRight()
    {
        var (vm, window) = Open();
        ClickRule(window, vm, 0); // the rule buttons only show while a rule is selected

        var names = new List<string>();
        for (var i = 0; i < 40; i++)
        {
            Press(window, PhysicalKey.Tab);
            var focused = Focused(window);
            Assert.NotNull(focused);
            Assert.IsNotType<GridSplitter>(focused);
            Assert.Null(focused!.FindAncestorOfType<ListBoxItem>()); // not the "active" box inside a category row
            if (vm.RulesTable.IsKeyboardFocusWithin)
            {
                break; // round the dialog and back at the rules list
            }

            names.Add(AutomationProperties.GetName(focused!) ?? (focused as ContentControl)?.Content as string ?? focused!.GetType().Name);
        }

        // Round the whole dialog: rule buttons, preview, the button bar, then the categories.
        Assert.Contains(Se.Language.Edit.MultipleReplace.NewCategory, names);
        Assert.Equal(Se.Language.General.Remove, names[0]);
        var remove = names.IndexOf(Se.Language.General.Remove);
        var addRules = names.IndexOf(Se.Language.Edit.MultipleReplace.AddRulesDotDotDot);
        Assert.True(remove >= 0 && addRules > remove, string.Join(" | ", names));
    }

    [AvaloniaFact]
    public void CtrlDigits_JumpBetweenCategoriesRulesAndPreview()
    {
        var (vm, window) = Open();
        ClickCategory(window, vm, 0);

        Press(window, PhysicalKey.Digit3, RawInputModifiers.Control);
        Assert.True(vm.FixesTable!.IsKeyboardFocusWithin);
        Assert.NotNull(vm.SelectedFix);

        Press(window, PhysicalKey.Digit2, RawInputModifiers.Control);
        Assert.True(vm.RulesTable.IsKeyboardFocusWithin);
        Assert.Same(vm.Nodes[0].SubNodes![0], vm.SelectedNode);

        Press(window, PhysicalKey.Digit1, RawInputModifiers.Control);
        Assert.True(vm.CategoriesListBox.IsKeyboardFocusWithin);
        Assert.Same(vm.Nodes[0], vm.SelectedCategory);
    }

    [AvaloniaFact]
    public void F3_StepsThroughThePreview_WithoutMovingTheFocus()
    {
        var (vm, window) = Open();
        ClickRule(window, vm, 0);
        var selectedBefore = vm.SelectedFix;

        Press(window, PhysicalKey.F3);
        var first = vm.SelectedFix;
        Assert.NotNull(first);
        var firstIndex = vm.Fixes.IndexOf(first!);
        Assert.Equal(selectedBefore == null ? 0 : vm.Fixes.IndexOf(selectedBefore) + 1, firstIndex);

        Press(window, PhysicalKey.F3);
        Assert.Equal(firstIndex + 1, vm.Fixes.IndexOf(vm.SelectedFix!));

        Press(window, PhysicalKey.F3, RawInputModifiers.Shift);
        Assert.Equal(firstIndex, vm.Fixes.IndexOf(vm.SelectedFix!));

        Assert.True(vm.RulesTable.IsKeyboardFocusWithin);
    }

    [AvaloniaFact]
    public void CtrlE_FocusesTheRulesFilter()
    {
        var (vm, window) = Open();
        ClickCategory(window, vm, 0);

        Press(window, PhysicalKey.E, RawInputModifiers.Control);

        Assert.Same(vm.RulesFilterTextBox, Focused(window));
    }

    [AvaloniaFact]
    public void CtrlEnter_AppliesWithoutClosing()
    {
        var (vm, window) = Open();
        var applied = -1;
        vm.OnApply = (_, count) => applied = count;
        ClickCategory(window, vm, 0);

        Press(window, PhysicalKey.Enter, RawInputModifiers.Control);

        Assert.Equal(3, applied);
        Assert.False(vm.OkPressed);
        Assert.True(window.IsVisible);
    }

    [AvaloniaFact]
    public void Insert_StartsANewRuleInTheTypeInRow()
    {
        var (vm, window) = Open();
        ClickRule(window, vm, 0);

        Press(window, PhysicalKey.Insert);

        Assert.Same(vm.NewFindTextBox, Focused(window));
    }
}
