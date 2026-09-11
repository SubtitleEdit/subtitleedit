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
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// Reordering has to be repeatable: Ctrl+Up on a rule moved it once and then dropped keyboard
/// focus, so the tree handed focus - and with it the selection - to the category above on the
/// next key press and the second Ctrl+Up moved the category instead (#14136). These tests drive
/// the real window, because the bug lives in the container lookup, not in the reorder itself.
/// </summary>
public class MultipleReplaceMoveFocusTests : IDisposable
{
    // A window left open outlives the test: it keeps the application-wide activation and focused
    // element, so a later test's click or key press is delivered to it instead. Closing here rather
    // than at the end of each test also covers the tests that stop early on a failed assertion.
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

        for (var c = 1; c <= 2; c++)
        {
            var category = new RuleTreeNode(true) { CategoryName = $"c{c}", IsActive = true, IsExpanded = true };
            for (var r = 1; r <= 4; r++)
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
        subtitle.Paragraphs.Add(new Paragraph("hello", 0, 2000));
        vm.Initialize(subtitle);

        var window = new MultipleReplaceWindow(vm);
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    private static string Rules(RuleTreeNode category) =>
        string.Join(",", category.SubNodes!.Select(n => n.Find));

    private static string Categories(MultipleReplaceViewModel vm) =>
        string.Join(",", vm.Nodes.Select(n => n.CategoryName));

    private static TreeViewItem ContainerOf(Window window, RuleTreeNode node)
    {
        var container = window.GetVisualDescendants().OfType<TreeViewItem>()
            .FirstOrDefault(i => ReferenceEquals(i.DataContext, node));
        Assert.NotNull(container);
        return container!;
    }

    // Clicking the row is the only way to get real keyboard focus into the tree, and the bug is
    // about what happens to that focus afterwards.
    private static void ClickNode(Window window, MultipleReplaceViewModel vm, RuleTreeNode node)
    {
        var container = ContainerOf(window, node);
        var bounds = container.Bounds;
        var point = container.TranslatePoint(new Point(bounds.Width / 2, 8), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Same(node, vm.SelectedNode);
    }

    private static void CtrlKey(Window window, PhysicalKey key)
    {
        window.KeyPressQwerty(key, RawInputModifiers.Control);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void CtrlUp_WalksTheSameRuleUpTheCategory()
    {
        var (vm, window) = Open();
        var category = vm.Nodes[0];
        var rule = category.SubNodes![3];
        ClickNode(window, vm, rule);

        CtrlKey(window, PhysicalKey.ArrowUp);
        Assert.Equal("c1r1,c1r2,c1r4,c1r3", Rules(category));
        Assert.Same(rule, vm.SelectedNode);

        CtrlKey(window, PhysicalKey.ArrowUp);
        Assert.Equal("c1r1,c1r4,c1r2,c1r3", Rules(category));
        Assert.Same(rule, vm.SelectedNode);

        CtrlKey(window, PhysicalKey.ArrowUp);
        Assert.Equal("c1r4,c1r1,c1r2,c1r3", Rules(category));
        Assert.Same(rule, vm.SelectedNode);

        // The category never becomes the selection, so it never gets reordered either.
        Assert.Equal("c1,c2", Categories(vm));
    }

    [AvaloniaFact]
    public void CtrlDown_WalksTheSameRuleDownTheCategory()
    {
        var (vm, window) = Open();
        var category = vm.Nodes[1];
        var rule = category.SubNodes![0];
        ClickNode(window, vm, rule);

        CtrlKey(window, PhysicalKey.ArrowDown);
        CtrlKey(window, PhysicalKey.ArrowDown);

        Assert.Equal("c2r2,c2r3,c2r1,c2r4", Rules(category));
        Assert.Same(rule, vm.SelectedNode);
        Assert.Equal("c1,c2", Categories(vm));
    }

    // The moved row has to keep keyboard focus - that is what makes the next key press reach it.
    [AvaloniaFact]
    public void MovedRule_KeepsKeyboardFocus()
    {
        var (vm, window) = Open();
        var rule = vm.Nodes[0].SubNodes![2];
        ClickNode(window, vm, rule);

        CtrlKey(window, PhysicalKey.ArrowUp);

        var focused = window.FocusManager?.GetFocusedElement();
        Assert.Same(rule, (focused as TreeViewItem)?.DataContext);
    }

    [AvaloniaFact]
    public void CtrlUp_WalksTheSameCategory()
    {
        var (vm, window) = Open();
        var category = vm.Nodes[1];
        ClickNode(window, vm, category);

        CtrlKey(window, PhysicalKey.ArrowUp);

        Assert.Equal("c2,c1", Categories(vm));
        Assert.Same(category, vm.SelectedNode);
    }
}
