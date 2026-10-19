using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// Groups (editor-only), rubber-band selection and Ctrl+click selection in ASSA draw.
/// </summary>
public class AssaDrawGroupTests
{
    private const string SquareA = "{\\p1}m 100 100 l 200 100 200 200 100 200{\\p0}";
    private const string SquareB = "{\\p1}m 300 100 l 400 100 400 200 300 200{\\p0}";
    private const string SquareC = "{\\p1}m 600 600 l 700 600 700 700 600 700{\\p0}";

    private static AssaDrawViewModel MakeViewModel(params string[] lines)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var rows = lines.Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, 0, 2000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, rows, 1920, 1080);
        return vm;
    }

    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) Open(params string[] lines)
    {
        var vm = MakeViewModel(lines);
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.Canvas!.FitToView();
        vm.SelectToolCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    /// <summary>
    /// A, B grouped (layers 0 and 1), C on its own.
    /// </summary>
    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) OpenWithGroup()
    {
        var (window, vm) = Open(SquareA, SquareB, SquareC);
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static Point ToWindow(AssaDrawWindow window, AssaDrawCanvas canvas, double x, double y)
    {
        var origin = canvas.TranslatePoint(new Point(0, 0), window)!.Value;
        var zoom = canvas.ZoomFactor;
        var panX = (canvas.Bounds.Width - canvas.CanvasWidth * zoom) / 2;
        var panY = (canvas.Bounds.Height - canvas.CanvasHeight * zoom) / 2;
        return new Point(origin.X + panX + x * zoom, origin.Y + panY + y * zoom);
    }

    private static void Click(AssaDrawWindow window, AssaDrawViewModel vm, double x, double y, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var point = ToWindow(window, vm.Canvas!, x, y);
        window.MouseMove(point, modifiers);
        window.MouseDown(point, MouseButton.Left, modifiers);
        window.MouseUp(point, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Drag(AssaDrawWindow window, AssaDrawViewModel vm, (double X, double Y) from, (double X, double Y) to, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var start = ToWindow(window, vm.Canvas!, from.X, from.Y);
        var end = ToWindow(window, vm.Canvas!, to.X, to.Y);
        window.MouseMove(start, modifiers);
        window.MouseDown(start, MouseButton.Left, modifiers);
        window.MouseMove(new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2), RawInputModifiers.LeftMouseButton | modifiers);
        window.MouseMove(end, RawInputModifiers.LeftMouseButton | modifiers);
        window.MouseUp(end, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Press(AssaDrawViewModel vm, Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        vm.OnKeyDown(new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent });
        Dispatcher.UIThread.RunJobs();
    }

    private static IEnumerable<ShapeTreeItem> AllTreeItems(IEnumerable<ShapeTreeItem> items) =>
        items.SelectMany(i => new[] { i }.Concat(AllTreeItems(i.Children)));

    [AvaloniaFact]
    public void InsertClipArt_IsOneGroup_SelectedAsAGroupNodeOfItsOwn()
    {
        var vm = MakeViewModel(SquareA);
        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawClipArtSun);

        vm.InsertLibraryShape(new CanvasInsertEventArgs(100, 100, 400, 400, isClick: false));

        var inserted = vm.Shapes.Skip(1).ToList();
        Assert.True(inserted.Count > 1);
        Assert.NotEqual(0, inserted[0].GroupId);
        Assert.All(inserted, s => Assert.Equal(inserted[0].GroupId, s.GroupId));
        Assert.Equal(0, vm.Shapes[0].GroupId);
        Assert.Equal(inserted, vm.SelectedShapes);

        // Spread over several layers: one group node next to the layers, its shapes named with their layer
        var groupItem = Assert.IsType<ShapeTreeItem>(vm.SelectedTreeItem);
        Assert.True(groupItem.IsGroup);
        Assert.Contains(groupItem, vm.ShapeTreeItems);
        Assert.Equal(string.Format(Se.Language.Assa.DrawGroupX, inserted[0].GroupId), groupItem.Name);
        Assert.Equal(inserted.Count, groupItem.Children.Count);
        Assert.StartsWith(string.Format(Se.Language.Assa.DrawLayerX, inserted[0].Layer), groupItem.Children[0].Name);
        Assert.Equal(string.Format(Se.Language.Assa.DrawXShapes, inserted.Count), vm.SelectionText[(vm.SelectionText.IndexOf('·') + 2)..]);
    }

    [AvaloniaFact]
    public void InsertRing_OnOneLayer_IsAGroupNodeInsideItsLayer()
    {
        var vm = MakeViewModel();
        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeRing);

        vm.InsertLibraryShape(new CanvasInsertEventArgs(100, 100, 400, 400, isClick: false));

        Assert.Equal(2, vm.Shapes.Count);
        var layerItem = Assert.Single(vm.ShapeTreeItems);
        Assert.True(layerItem.IsLayer);
        var groupItem = Assert.Single(layerItem.Children);
        Assert.True(groupItem.IsGroup);
        Assert.Same(groupItem, vm.SelectedTreeItem);
        Assert.Equal(2, groupItem.Children.Count);
        Assert.Equal(string.Format(Se.Language.Assa.DrawShapeXDraw, 1), groupItem.Children[0].Name);
    }

    [AvaloniaFact]
    public void SvgImport_IsOneGroup()
    {
        var vm = MakeViewModel();

        Assert.True(vm.ImportSvgText("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 1920 1080\"><rect width=\"100\" height=\"100\"/><rect x=\"300\" width=\"100\" height=\"100\" fill=\"red\"/></svg>"));

        Assert.Equal(2, vm.Shapes.Count);
        Assert.NotEqual(0, vm.Shapes[0].GroupId);
        Assert.Equal(vm.Shapes[0].GroupId, vm.Shapes[1].GroupId);
        Assert.Equal(2, vm.SelectedShapes.Count);
        Assert.True(vm.SelectedTreeItem?.IsGroup);
    }

    [AvaloniaFact]
    public void Click_OnAGroupPart_SelectsTheWholeGroup_AltClickSelectsThePart()
    {
        var (window, vm) = OpenWithGroup();
        try
        {
            Click(window, vm, 650, 650);
            Assert.Same(vm.Shapes[2], vm.SelectedTreeItem?.Shape);
            Assert.Empty(vm.SelectedShapes);

            Click(window, vm, 150, 150);
            Assert.Equal([vm.Shapes[0], vm.Shapes[1]], vm.SelectedShapes);
            Assert.True(vm.SelectedTreeItem?.IsGroup);
            Assert.Equal(vm.SelectedShapes, vm.Canvas!.SelectedShapes);

            // A drag on a part moves the whole group, as one undo step
            Drag(window, vm, (180, 180), (200, 210));
            Assert.Equal(120, vm.Shapes[0].Points[0].X, 0);
            Assert.Equal(320, vm.Shapes[1].Points[0].X, 0);
            Assert.Equal(130, vm.Shapes[1].Points[0].Y, 0);
            Assert.True(vm.SelectedTreeItem?.IsGroup);

            // Alt+click: the part alone, so its points can be edited
            Click(window, vm, 350, 150, RawInputModifiers.Alt);
            Assert.Same(vm.Shapes[1], vm.SelectedTreeItem?.Shape);
            Assert.Empty(vm.SelectedShapes);

            Click(window, vm, 320, 130);
            Assert.Same(vm.Shapes[1].Points[0], vm.ActivePoint);

            // Esc: point -> part -> nothing
            Press(vm, Key.Escape);
            Assert.Null(vm.ActivePoint);
            Assert.Same(vm.Shapes[1], vm.SelectedTreeItem?.Shape);
            Press(vm, Key.Escape);
            Assert.Null(vm.SelectedTreeItem);

            // Undo of the drag
            vm.UndoCommand.Execute(null);
            Assert.Equal(100, vm.Shapes[0].Points[0].X, 0);
            Assert.Equal(300, vm.Shapes[1].Points[0].X, 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Click_NearAGroupPartsPoint_SelectsTheGroup_NotThePoint()
    {
        var (window, vm) = OpenWithGroup();
        try
        {
            Click(window, vm, 650, 650);
            Click(window, vm, 101, 101);

            Assert.Null(vm.ActivePoint);
            Assert.True(vm.SelectedTreeItem?.IsGroup);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CtrlG_Groups_CtrlShiftG_Ungroups_BothUndoable()
    {
        var vm = MakeViewModel(SquareA, SquareB, SquareC);
        Assert.False(vm.GroupShapesCommand.CanExecute(null));

        vm.SetSelection([vm.Shapes[0], vm.Shapes[2]]);
        Assert.True(vm.GroupShapesCommand.CanExecute(null));
        Assert.False(vm.UngroupShapesCommand.CanExecute(null));

        Press(vm, Key.G, KeyModifiers.Control);

        Assert.NotEqual(0, vm.Shapes[0].GroupId);
        Assert.Equal(vm.Shapes[0].GroupId, vm.Shapes[2].GroupId);
        Assert.Equal(0, vm.Shapes[1].GroupId);
        Assert.True(vm.SelectedTreeItem?.IsGroup);
        Assert.False(vm.GroupShapesCommand.CanExecute(null));
        Assert.True(vm.UngroupShapesCommand.CanExecute(null));

        Press(vm, Key.G, KeyModifiers.Control | KeyModifiers.Shift);

        Assert.All(vm.Shapes, s => Assert.Equal(0, s.GroupId));
        Assert.Equal([vm.Shapes[0], vm.Shapes[2]], vm.SelectedShapes);
        Assert.False(vm.SelectedTreeItem?.IsGroup == true);

        vm.UndoCommand.Execute(null);
        Assert.NotEqual(0, vm.Shapes[0].GroupId);
        Assert.Equal(vm.Shapes[0].GroupId, vm.Shapes[2].GroupId);
        Assert.True(vm.SelectedTreeItem?.IsGroup);
        Assert.Equal(2, vm.SelectedShapes.Count);

        vm.UndoCommand.Execute(null);
        Assert.All(vm.Shapes, s => Assert.Equal(0, s.GroupId));
    }

    [AvaloniaFact]
    public void CtrlQuote_TogglesTheGrid()
    {
        var vm = MakeViewModel(SquareA);
        var oldGrid = DrawSettings.ShowGrid;
        try
        {
            var before = vm.ShowGrid;
            Press(vm, Key.OemQuotes, KeyModifiers.Control);
            Assert.NotEqual(before, vm.ShowGrid);
        }
        finally
        {
            DrawSettings.ShowGrid = oldGrid;
        }
    }

    [AvaloniaFact]
    public void Ungroup_OnePart_LeavesNoOneShapeGroup()
    {
        var vm = MakeViewModel(SquareA, SquareB);
        vm.SetSelection(vm.Shapes);
        vm.GroupShapesCommand.Execute(null);

        vm.SelectShape(vm.Shapes[0]);
        vm.UngroupShapesCommand.Execute(null);

        Assert.All(vm.Shapes, s => Assert.Equal(0, s.GroupId));
        Assert.DoesNotContain(AllTreeItems(vm.ShapeTreeItems), i => i.IsGroup);
    }

    [AvaloniaFact]
    public void Duplicate_OfAGroup_GetsANewGroup_ACopiedPartGetsNone()
    {
        var vm = MakeViewModel(SquareA, SquareB, SquareC);
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);
        var groupId = vm.Shapes[0].GroupId;

        Press(vm, Key.D, KeyModifiers.Control);

        Assert.Equal(5, vm.Shapes.Count);
        var copies = vm.Shapes.Where(s => s.GroupId != groupId && s.GroupId != 0).ToList();
        Assert.Equal(2, copies.Count);
        Assert.Equal(copies[0].GroupId, copies[1].GroupId);
        Assert.Equal(2, vm.Shapes.Count(s => s.GroupId == groupId));
        Assert.Equal(copies, vm.SelectedShapes);
        Assert.Equal(copies[0].GroupId, vm.SelectedTreeItem?.GroupId);

        // One part copied out of its group is no group member
        vm.SelectShape(vm.Shapes[0]);
        vm.DuplicateShapeCommand.Execute(null);
        Assert.Equal(0, vm.SelectedTreeItem?.Shape?.GroupId);
        Assert.Equal(2, vm.Shapes.Count(s => s.GroupId == groupId));
    }

    [AvaloniaFact]
    public void Delete_OfAGroupNode_DeletesTheWholeGroup()
    {
        var vm = MakeViewModel(SquareA, SquareB, SquareC);
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);

        Press(vm, Key.Delete);

        var remaining = Assert.Single(vm.Shapes);
        Assert.Equal(600, remaining.Points[0].X);
    }

    [AvaloniaFact]
    public void SaveAndLoad_KeepsGroups_OutputHasNoGroupData()
    {
        var vm = MakeViewModel(SquareA, SquareB, SquareC);
        vm.Shapes[1].Hidden = true;
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);

        // An erase shape and a text box in a second group
        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeRing);
        vm.InsertLibraryShape(new CanvasInsertEventArgs(800, 100, 200, 200, isClick: false));
        var ring = vm.Shapes.Skip(3).ToList();
        ring[1].IsEraser = true;
        var textBox = vm.InsertTextBox(new CanvasInsertEventArgs(1000, 500, 300, 100, isClick: false));
        textBox.GroupId = ring[0].GroupId;
        var expected = vm.Shapes.Select(s => (s.Layer, s.IsEraser, s.IsTextBox, s.Hidden, s.GroupId)).OrderBy(t => t.Layer).ThenBy(t => t.IsTextBox).ThenBy(t => t.IsEraser).ToList();

        var text = vm.GetAssaDrawFileText();
        Assert.Contains("SeGroups:", text);
        Assert.DoesNotContain("SeGroups", vm.GenerateAssaCode());
        vm.OkCommand.Execute(null);
        Assert.All(vm.ResultSubtitle.Paragraphs, p => Assert.True(string.IsNullOrEmpty(p.Effect)));

        var loaded = MakeViewModel();
        loaded.LoadFromText(text);

        var actual = loaded.Shapes.Select(s => (s.Layer, s.IsEraser, s.IsTextBox, s.Hidden, s.GroupId)).OrderBy(t => t.Layer).ThenBy(t => t.IsTextBox).ThenBy(t => t.IsEraser).ToList();
        Assert.Equal(expected, actual);
        Assert.Equal(2, AllTreeItems(loaded.ShapeTreeItems).Count(i => i.IsGroup));

        // Saving again writes the same file
        Assert.Equal(text, loaded.GetAssaDrawFileText());
    }

    [AvaloniaFact]
    public void LoadingAFileWithoutGroups_GivesNoGroups()
    {
        var vm = MakeViewModel(SquareA, SquareB);
        var loaded = MakeViewModel();

        loaded.LoadFromText(vm.GetAssaDrawFileText());

        Assert.DoesNotContain("SeGroups", vm.GetAssaDrawFileText());
        Assert.All(loaded.Shapes, s => Assert.Equal(0, s.GroupId));
    }

    [AvaloniaFact]
    public void Marquee_SelectsTouchedShapes_WithTheirGroups_AltJustTheParts_CtrlAdds()
    {
        var (window, vm) = OpenWithGroup();
        try
        {
            // Touches A only: B comes along as A's group
            Drag(window, vm, (50, 50), (150, 150));
            Assert.Equal([vm.Shapes[0], vm.Shapes[1]], vm.SelectedShapes);
            Assert.True(vm.SelectedTreeItem?.IsGroup);

            // Alt: just A
            Drag(window, vm, (50, 50), (150, 150), RawInputModifiers.Alt);
            Assert.Same(vm.Shapes[0], vm.SelectedTreeItem?.Shape);
            Assert.Empty(vm.SelectedShapes);

            // Ctrl adds C to it
            Drag(window, vm, (550, 550), (650, 650), RawInputModifiers.Control);
            Assert.Equal([vm.Shapes[0], vm.Shapes[2]], vm.SelectedShapes);
            Assert.Equal(string.Format(Se.Language.Assa.DrawXShapes, 2), vm.SelectionText);
            Assert.True(vm.DeleteShapeCommand.CanExecute(null));
            Assert.True(vm.GroupShapesCommand.CanExecute(null));

            // A plain click on empty canvas clears it
            Click(window, vm, 1000, 900);
            Assert.Empty(vm.SelectedShapes);
            Assert.Null(vm.SelectedTreeItem);
            Assert.False(vm.DeleteShapeCommand.CanExecute(null));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ShiftDrag_OnEmptyCanvas_StillPans()
    {
        var (window, vm) = Open(SquareA);
        try
        {
            var before = ToWindow(window, vm.Canvas!, 0, 0);
            var start = new Point(before.X + 900, before.Y + 600);
            window.MouseMove(start, RawInputModifiers.Shift);
            window.MouseDown(start, MouseButton.Left, RawInputModifiers.Shift);
            window.MouseMove(new Point(start.X + 40, start.Y + 30), RawInputModifiers.LeftMouseButton | RawInputModifiers.Shift);
            window.MouseUp(new Point(start.X + 40, start.Y + 30), MouseButton.Left, RawInputModifiers.Shift);
            Dispatcher.UIThread.RunJobs();

            var (x, _) = vm.Canvas!.ToCanvasPoint(start);
            Assert.NotEqual(900 / vm.Canvas.ZoomFactor, x, 0);
            Assert.Empty(vm.SelectedShapes);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CtrlClick_TogglesAShapeOrItsGroup()
    {
        var (window, vm) = OpenWithGroup();
        try
        {
            Click(window, vm, 650, 650);
            Click(window, vm, 150, 150, RawInputModifiers.Control);
            Assert.Equal([vm.Shapes[0], vm.Shapes[1], vm.Shapes[2]], vm.SelectedShapes);

            // The whole group goes out again, leaving C selected on its own
            Click(window, vm, 350, 150, RawInputModifiers.Control);
            Assert.Empty(vm.SelectedShapes);
            Assert.Same(vm.Shapes[2], vm.SelectedTreeItem?.Shape);

            // Cmd on macOS
            Click(window, vm, 150, 150, RawInputModifiers.Meta);
            Assert.Equal(3, vm.SelectedShapes.Count);

            Click(window, vm, 650, 650, RawInputModifiers.Control);
            Assert.True(vm.SelectedTreeItem?.IsGroup);
            Assert.Equal([vm.Shapes[0], vm.Shapes[1]], vm.SelectedShapes);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupTreeNode_SelectsTheWholeGroup_AndKeepsIt()
    {
        var vm = MakeViewModel(SquareA, SquareB, SquareC);
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);
        vm.SelectShape(vm.Shapes[2]);
        Assert.Empty(vm.SelectedShapes);

        var groupItem = AllTreeItems(vm.ShapeTreeItems).Single(i => i.IsGroup);
        vm.SelectedTreeItem = groupItem;

        Assert.Equal([vm.Shapes[0], vm.Shapes[1]], vm.SelectedShapes);
        Assert.True(vm.DuplicateShapeCommand.CanExecute(null));

        // Commands keep it: rotate rebuilds the tree, the group stays selected
        vm.RotateShapeClockwiseCommand.Execute(null);
        Assert.True(vm.SelectedTreeItem?.IsGroup);
        Assert.Equal(2, vm.SelectedShapes.Count);

        // Arrow nudge moves the group
        Press(vm, Key.Right);
        Assert.Equal(2, vm.SelectedShapes.Count);

        // Visibility toggle hides the group's shapes
        vm.ToggleItemVisibilityCommand.Execute(vm.SelectedTreeItem);
        Assert.True(vm.Shapes[0].Hidden && vm.Shapes[1].Hidden);
        Assert.False(vm.Shapes[2].Hidden);
    }

    [AvaloniaFact]
    public void Tree_CtrlClick_SelectsSeveralShapesAndGroups()
    {
        var (window, vm) = OpenWithGroup();
        try
        {
            var treeView = window.GetLogicalDescendants().OfType<TreeView>().Single();
            Assert.Equal(SelectionMode.Multiple, treeView.SelectionMode);

            var shapeC = AllTreeItems(vm.ShapeTreeItems).Single(i => i.Shape == vm.Shapes[2]);
            var groupItem = AllTreeItems(vm.ShapeTreeItems).Single(i => i.IsGroup);

            ClickTreeItem(window, treeView, shapeC, RawInputModifiers.None);
            Assert.Same(shapeC, vm.SelectedTreeItem);
            Assert.Empty(vm.SelectedShapes);

            ClickTreeItem(window, treeView, groupItem, RawInputModifiers.Control);
            Assert.Equal([vm.Shapes[0], vm.Shapes[1], vm.Shapes[2]], vm.SelectedShapes);
            Assert.Equal(string.Format(Se.Language.Assa.DrawXShapes, 3), vm.SelectionText);

            // A plain click picks one again
            ClickTreeItem(window, treeView, shapeC, RawInputModifiers.None);
            Assert.Same(shapeC, vm.SelectedTreeItem);
            Assert.Empty(vm.SelectedShapes);
        }
        finally
        {
            window.Close();
        }
    }

    private static void ClickTreeItem(AssaDrawWindow window, TreeView treeView, ShapeTreeItem item, RawInputModifiers modifiers)
    {
        Dispatcher.UIThread.RunJobs();
        var container = treeView.GetVisualDescendantsOfType<TreeViewItem>().First(c => c.DataContext == item);
        var header = container.GetVisualDescendantsOfType<Avalonia.Controls.Presenters.ContentPresenter>().First(p => p.Name == "PART_HeaderPresenter");
        var point = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point, modifiers);
        window.MouseDown(point, MouseButton.Left, modifiers);
        window.MouseUp(point, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void HitTest_PicksTheHigherLayer_EvenWhenItComesFirstInTheList()
    {
        var vm = MakeViewModel(SquareA, SquareA);
        vm.Shapes[0].Layer = 5;
        var canvas = new AssaDrawCanvas { Shapes = vm.Shapes };

        Assert.Same(vm.Shapes[0], canvas.HitTestShape(150, 150));

        // Same layer: the later shape is drawn on top
        vm.Shapes[0].Layer = 0;
        vm.Shapes[1].Layer = 0;
        Assert.Same(vm.Shapes[1], canvas.HitTestShape(150, 150));
    }
}

internal static class VisualTestExtensions
{
    public static IEnumerable<T> GetVisualDescendantsOfType<T>(this Visual visual) where T : Visual =>
        Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(visual).OfType<T>();
}
