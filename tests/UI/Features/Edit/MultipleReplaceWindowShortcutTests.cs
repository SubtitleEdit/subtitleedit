using Avalonia;
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
using System;
using System.Linq;
using System.Threading;

namespace UITests.Features.Edit;

/// <summary>
/// The fixes grid offers tick all / untick all / invert via context menu and gestures (#13502).
/// With the grid focused those have to beat the TableView's own Ctrl+A ("select all rows") and
/// the window's Ctrl+D ("duplicate rule"), which is why the grid takes them while tunneling.
/// </summary>
public class MultipleReplaceWindowShortcutTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static (MultipleReplaceViewModel Vm, Window Window, RuleTreeNode Category) OpenWithThreeFixes()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();

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

        var window = new MultipleReplaceWindow(vm);
        window.Show();
        WaitForPreview(vm, 3);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(3, vm.Fixes.Count);
        return (vm, window, category);
    }

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

    // The rules panel is a TreeView, so the fixes grid is the only TableView in the window.
    private static TableView FixesGrid(Window window)
    {
        return window.GetVisualDescendants().OfType<TableView>().First();
    }

    private static Point CenterOf(Visual container, Window window)
    {
        var bounds = container.Bounds;
        return container.TranslatePoint(new Point(bounds.Width / 2, bounds.Height / 2), window)!.Value;
    }

    // Clicking a row is the only way to get real keyboard focus into the grid - TableView.Focus()
    // does not move it (the row containers take focus, not the control).
    private static TableView ClickRow(Window window, MultipleReplaceViewModel vm, int index, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var grid = FixesGrid(window);
        var container = (Visual?)grid.ContainerFromItem(vm.Fixes[index]);
        Assert.NotNull(container);

        var point = CenterOf(container!, window);
        window.MouseDown(point, MouseButton.Left, modifiers);
        window.MouseUp(point, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();

        Assert.True(grid.IsKeyboardFocusWithin, "the gestures have to work with the grid focused");
        return grid;
    }

    private static string Ticked(MultipleReplaceViewModel vm) =>
        string.Join(",", vm.Fixes.Where(f => f.Apply).Select(f => f.Number));

    [AvaloniaFact]
    public void GridCtrlD_UnticksEveryFix_InsteadOfDuplicatingTheRule()
    {
        var (vm, window, category) = OpenWithThreeFixes();
        ClickRow(window, vm, 0);

        window.KeyPressQwerty(PhysicalKey.D, RawInputModifiers.Control);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(string.Empty, Ticked(vm));
        // ...and the window's own Ctrl+D never saw it.
        Assert.Single(category.SubNodes!);

        window.Close();
    }

    [AvaloniaFact]
    public void GridCtrlA_TicksEveryFix_InsteadOfSelectingAllRows()
    {
        var (vm, window, _) = OpenWithThreeFixes();
        foreach (var fix in vm.Fixes)
        {
            fix.Apply = false;
        }

        var grid = ClickRow(window, vm, 0);

        window.KeyPressQwerty(PhysicalKey.A, RawInputModifiers.Control);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("1,2,3", Ticked(vm));
        // ...and the TableView did not turn it into "select all rows" on top of that.
        Assert.Equal(1, grid.SelectedItems?.Count ?? 0);

        window.Close();
    }

    [AvaloniaFact]
    public void GridCtrlShiftI_InvertsEveryFix()
    {
        var (vm, window, _) = OpenWithThreeFixes();
        vm.Fixes[1].Apply = false;

        ClickRow(window, vm, 0);

        window.KeyPressQwerty(PhysicalKey.I, RawInputModifiers.Control | RawInputModifiers.Shift);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("2", Ticked(vm));

        window.Close();
    }

    [AvaloniaFact]
    public void FixesGrid_HasSelectAllSelectNoneAndInvertInItsContextMenu()
    {
        var (_, window, _) = OpenWithThreeFixes();

        var headers = FixesGrid(window).ContextMenu?.Items.OfType<MenuItem>().Select(m => m.Header as string).ToList();

        Assert.NotNull(headers);
        Assert.Equal(3, headers!.Count);
        Assert.Contains(Se.Language.General.SelectAll, headers);
        Assert.Contains(Se.Language.General.SelectNone, headers);
        Assert.Contains(Se.Language.General.InvertSelection, headers);

        window.Close();
    }

    // A range picked with Shift+click is flipped in one go - the reason the grid went
    // multi-select in #13502.
    [AvaloniaFact]
    public void Space_TogglesTheHighlightedRange()
    {
        var (vm, window, _) = OpenWithThreeFixes();

        ClickRow(window, vm, 0);
        var grid = ClickRow(window, vm, 1, RawInputModifiers.Shift);
        Assert.Equal(2, grid.SelectedItems?.Count ?? 0);

        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("3", Ticked(vm));

        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("1,2,3", Ticked(vm));

        window.Close();
    }
}
