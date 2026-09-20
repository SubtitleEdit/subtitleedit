using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// Reordering on the Multiple replace rule tree. Subtitle Edit 4 had move up / down / to top /
/// to bottom on both the rules and the groups context menu, all four bound to Ctrl+Up / Down /
/// Home / End; SE 5 shipped with only up and down and no shortcuts at all (#13523).
/// The order is data - it decides which rule gets to a line first - so this is real reordering.
/// </summary>
public class MultipleReplaceMoveTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    // Three categories of three rules each, named "c1"/"c1r1" and so on, so a wrong move shows up
    // as a readable sequence rather than an index.
    private static MultipleReplaceViewModel BuildViewModel()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();

        for (var c = 1; c <= 3; c++)
        {
            var category = new RuleTreeNode(true) { CategoryName = $"c{c}" };
            for (var r = 1; r <= 3; r++)
            {
                category.SubNodes!.Add(new RuleTreeNode(false) { Find = $"c{c}r{r}", Parent = category });
            }

            vm.Nodes.Add(category);
        }

        return vm;
    }

    private static string Categories(MultipleReplaceViewModel vm) =>
        string.Join(",", vm.Nodes.Select(n => n.CategoryName));

    private static string Rules(RuleTreeNode category) =>
        string.Join(",", category.SubNodes!.Select(n => n.Find));

    private static bool SendKey(MultipleReplaceViewModel vm, Key key, KeyModifiers modifiers)
    {
        var e = new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent };
        vm.RulesTreeView_PreviewKeyDown(null, e);
        return e.Handled;
    }

    [AvaloniaTheory]
    [InlineData(0, "MoveDown", "c2,c1,c3")]
    [InlineData(2, "MoveUp", "c1,c3,c2")]
    [InlineData(2, "MoveToTop", "c3,c1,c2")]
    [InlineData(0, "MoveToBottom", "c2,c3,c1")]
    public void CategoryMove_ReordersCategories(int index, string command, string expected)
    {
        var vm = BuildViewModel();
        var category = vm.Nodes[index];

        switch (command)
        {
            case "MoveUp": vm.CategoryMoveUpCommand.Execute(category); break;
            case "MoveDown": vm.CategoryMoveDownCommand.Execute(category); break;
            case "MoveToTop": vm.CategoryMoveToTopCommand.Execute(category); break;
            case "MoveToBottom": vm.CategoryMoveToBottomCommand.Execute(category); break;
        }

        Assert.Equal(expected, Categories(vm));
    }

    [AvaloniaTheory]
    [InlineData(0, "MoveDown", "c2r2,c2r1,c2r3")]
    [InlineData(2, "MoveUp", "c2r1,c2r3,c2r2")]
    [InlineData(2, "MoveToTop", "c2r3,c2r1,c2r2")]
    [InlineData(0, "MoveToBottom", "c2r2,c2r3,c2r1")]
    public void RuleMove_ReordersRulesInsideTheirCategory(int index, string command, string expected)
    {
        var vm = BuildViewModel();
        var category = vm.Nodes[1];
        var rule = category.SubNodes![index];

        switch (command)
        {
            case "MoveUp": vm.NodeMoveUpCommand.Execute(rule); break;
            case "MoveDown": vm.NodeMoveDownCommand.Execute(rule); break;
            case "MoveToTop": vm.NodeMoveToTopCommand.Execute(rule); break;
            case "MoveToBottom": vm.NodeMoveToBottomCommand.Execute(rule); break;
        }

        Assert.Equal(expected, Rules(category));
        Assert.Equal("c1r1,c1r2,c1r3", Rules(vm.Nodes[0]));
        Assert.Equal("c3r1,c3r2,c3r3", Rules(vm.Nodes[2]));
    }

    // A rule at the top of its category has nowhere to go - it must not fall into the category
    // above it, which is what a naive "move within the flattened tree" would do.
    [AvaloniaFact]
    public void RuleMoveUp_AtTopOfCategory_StaysPut()
    {
        var vm = BuildViewModel();
        var category = vm.Nodes[1];

        vm.NodeMoveUpCommand.Execute(category.SubNodes![0]);
        vm.NodeMoveToTopCommand.Execute(category.SubNodes[0]);

        Assert.Equal("c2r1,c2r2,c2r3", Rules(category));
        Assert.Equal("c1r1,c1r2,c1r3", Rules(vm.Nodes[0]));
        Assert.Equal(3, category.SubNodes.Count);
    }

    [AvaloniaFact]
    public void CategoryMoveDown_AtBottom_StaysPut()
    {
        var vm = BuildViewModel();

        vm.CategoryMoveDownCommand.Execute(vm.Nodes[2]);
        vm.CategoryMoveToBottomCommand.Execute(vm.Nodes[2]);

        Assert.Equal("c1,c2,c3", Categories(vm));
    }

    // A category node handed to a rule command (and the other way round) must be ignored rather
    // than reordered against the wrong collection.
    [AvaloniaFact]
    public void MoveCommands_IgnoreNodesOfTheWrongKind()
    {
        var vm = BuildViewModel();

        vm.NodeMoveToBottomCommand.Execute(vm.Nodes[0]);
        vm.CategoryMoveToBottomCommand.Execute(vm.Nodes[0].SubNodes![0]);

        Assert.Equal("c1,c2,c3", Categories(vm));
        Assert.Equal("c1r1,c1r2,c1r3", Rules(vm.Nodes[0]));
    }

    [AvaloniaTheory]
    [InlineData(Key.Down, "c2r2,c2r1,c2r3")]
    [InlineData(Key.Home, "c2r1,c2r2,c2r3")]
    [InlineData(Key.End, "c2r2,c2r3,c2r1")]
    public void CtrlKey_MovesSelectedRule(Key key, string expected)
    {
        var vm = BuildViewModel();
        var category = vm.Nodes[1];
        vm.SelectedNode = category.SubNodes![0];

        Assert.True(SendKey(vm, key, KeyModifiers.Control));
        Assert.Equal(expected, Rules(category));
    }

    // Ctrl+Up/Down is Mission Control on macOS, so the menus show Cmd there - both have to work.
    [AvaloniaFact]
    public void CmdKey_MovesSelectedRule()
    {
        var vm = BuildViewModel();
        var category = vm.Nodes[1];
        vm.SelectedNode = category.SubNodes![2];

        Assert.True(SendKey(vm, Key.Home, KeyModifiers.Meta));
        Assert.Equal("c2r3,c2r1,c2r2", Rules(category));
    }

    [AvaloniaFact]
    public void CtrlKey_MovesSelectedCategory()
    {
        var vm = BuildViewModel();
        vm.SelectedNode = vm.Nodes[0];

        Assert.True(SendKey(vm, Key.End, KeyModifiers.Control));
        Assert.Equal("c2,c3,c1", Categories(vm));
    }

    // Plain arrows are the tree's own navigation, and Ctrl+Shift+Arrow is not a move either -
    // the handler tunnels, so anything it wrongly claims is lost to the tree view.
    [AvaloniaTheory]
    [InlineData(Key.Down, KeyModifiers.None)]
    [InlineData(Key.Home, KeyModifiers.None)]
    [InlineData(Key.End, KeyModifiers.Control | KeyModifiers.Shift)]
    [InlineData(Key.PageDown, KeyModifiers.Control)]
    public void OtherKeys_AreLeftToTheTreeView(Key key, KeyModifiers modifiers)
    {
        var vm = BuildViewModel();
        var category = vm.Nodes[1];
        vm.SelectedNode = category.SubNodes![0];

        Assert.False(SendKey(vm, key, modifiers));
        Assert.Equal("c2r1,c2r2,c2r3", Rules(category));
        Assert.Equal("c1,c2,c3", Categories(vm));
    }

    [AvaloniaFact]
    public void CtrlKey_WithNothingSelected_IsIgnored()
    {
        var vm = BuildViewModel();
        vm.SelectedNode = null;

        Assert.False(SendKey(vm, Key.End, KeyModifiers.Control));
        Assert.Equal("c1,c2,c3", Categories(vm));
    }

    // The handler is only reached because the window registers it as a tunnel handler - the list
    // box inside the tree view handles Ctrl+Arrow itself, so a bubbling registration silently
    // does nothing. Raise the event on the real tree to pin the registration, not just the method.
    [AvaloniaFact]
    public void CtrlHome_IsHandledThroughTheTreeViewsTunnelRoute()
    {
        var vm = BuildViewModel();
        var window = new MultipleReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var category = vm.Nodes[1];
            vm.SelectedNode = category.SubNodes![2];

            var e = new KeyEventArgs
            {
                Key = Key.Home,
                KeyModifiers = KeyModifiers.Control,
                RoutedEvent = InputElement.KeyDownEvent,
            };
            vm.RulesTreeView.RaiseEvent(e);

            Assert.True(e.Handled);
            Assert.Equal("c2r3,c2r1,c2r2", Rules(category));
        }
        finally
        {
            window.Close();
        }
    }

    // The commands existing is not enough - what regressed in SE 5 was the menu wiring, so pin
    // that both context menus offer all four moves.
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void ContextMenu_OffersAllFourMoves(bool category)
    {
        var vm = BuildViewModel();
        var window = new MultipleReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var node = category ? vm.Nodes[1] : vm.Nodes[1].SubNodes![0];
            if (category)
            {
                vm.NodeCategoryOpenContextMenuCommand.Execute(node);
            }
            else
            {
                vm.NodeOpenContextMenuCommand.Execute(node);
            }

            var headers = vm.RulesTreeView.ContextMenu!.Items
                .OfType<MenuItem>()
                .Select(m => m.Header as string)
                .ToList();

            Assert.Contains(Se.Language.General.MoveUp, headers);
            Assert.Contains(Se.Language.General.MoveDown, headers);
            Assert.Contains(Se.Language.General.MoveToTop, headers);
            Assert.Contains(Se.Language.General.MoveToBottom, headers);
        }
        finally
        {
            window.Close();
        }
    }
}
