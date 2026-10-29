using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// Keyboard and selection behaviour of ASSA draw: Delete on a point, commands on a multi-selection,
/// arrow nudge, Esc stepping back, hidden shapes in a saved .assadraw and the command states.
/// </summary>
public class AssaDrawKeyboardSelectionTests
{
    private const string Square = "{\\p1}m 100 100 l 200 100 200 200 100 200{\\p0}";
    private const string SecondSquare = "{\\p1}m 300 100 l 400 100 400 200 300 200{\\p0}";

    private static AssaDrawViewModel MakeViewModel(params string[] lines)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var rows = new List<SubtitleLineViewModel>();
        for (var i = 0; i < lines.Length; i++)
        {
            rows.Add(new SubtitleLineViewModel(new Paragraph(lines[i], 0, 2000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha()));
        }

        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, rows, 1920, 1080);
        return vm;
    }

    private static void Press(AssaDrawViewModel vm, Key key, KeyModifiers modifiers = KeyModifiers.None, object? source = null)
    {
        vm.OnKeyDown(new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent, Source = source });
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Delete_WithPointSelected_DeletesOnlyThePoint_AndUndoRestoresIt()
    {
        var vm = MakeViewModel(Square);
        vm.SelectPoint(vm.Shapes[0].Points[1]);

        Press(vm, Key.Delete);

        Assert.Single(vm.Shapes);
        Assert.Equal("m 100 100 l 200 200 100 200", vm.Shapes[0].ToAssa());

        vm.UndoCommand.Execute(null);

        Assert.Equal("m 100 100 l 200 100 200 200 100 200", vm.Shapes[0].ToAssa());
    }

    [AvaloniaFact]
    public void Delete_WithControlPointSelected_KeepsTheShape()
    {
        var vm = MakeViewModel("{\\p1}m 100 100 b 150 50 250 50 300 100 l 200 300{\\p0}");
        var controlPoint = vm.Shapes[0].Points.First(p => p.DrawType == DrawCoordinateType.BezierCurveSupport1);
        vm.SelectPoint(controlPoint);

        Press(vm, Key.Delete);

        Assert.Single(vm.Shapes);
        Assert.Equal(5, vm.Shapes[0].Points.Count);
    }

    [AvaloniaFact]
    public void SelectAll_ThenDelete_DeletesEveryShape_InOneUndoStep()
    {
        var vm = MakeViewModel(Square, SecondSquare);

        Press(vm, Key.A, KeyModifiers.Control);
        Press(vm, Key.Delete);

        Assert.Empty(vm.Shapes);

        vm.UndoCommand.Execute(null);

        Assert.Equal(2, vm.Shapes.Count);
        Assert.False(vm.UndoCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void Duplicate_MultiSelection_CopiesEveryShape_AndSelectsTheCopies()
    {
        var vm = MakeViewModel(Square, SecondSquare);
        vm.SelectAllShapesCommand.Execute(null);

        vm.DuplicateShapeCommand.Execute(null);

        Assert.Equal(4, vm.Shapes.Count);
        Assert.Equal(2, vm.SelectedShapes.Count);
        Assert.DoesNotContain(vm.SelectedShapes, s => s.Points[0].X is 100 or 300);
        Assert.Equal(100 + DrawSettings.GridSize, vm.SelectedShapes[0].Points[0].X);

        vm.UndoCommand.Execute(null);
        Assert.Equal(2, vm.Shapes.Count);
    }

    [AvaloniaFact]
    public void FlipHorizontal_MultiSelection_MirrorsAroundCombinedBounds()
    {
        var vm = MakeViewModel(Square, SecondSquare);
        vm.SelectAllShapesCommand.Execute(null);

        vm.FlipShapeHorizontalCommand.Execute(null);

        // Combined bounds 100..400: the left square ends up on the right and vice versa
        Assert.Equal(300, vm.Shapes[0].GetBounds().Left);
        Assert.Equal(100, vm.Shapes[1].GetBounds().Left);
        Assert.Equal(2, vm.SelectedShapes.Count);
    }

    [AvaloniaFact]
    public void Rotate_MultiSelection_TurnsAroundCombinedCenter_InOneUndoStep()
    {
        var vm = MakeViewModel(Square, SecondSquare);
        vm.SelectAllShapesCommand.Execute(null);

        vm.RotateShapeClockwiseCommand.Execute(null);

        // Center (250, 150): the left square (100..200, 100..200) goes above, to x 200..300, y 0..100
        var (left, top, right, bottom) = vm.Shapes[0].GetBounds();
        Assert.Equal((200f, 0f, 300f, 100f), (left, top, right, bottom));

        vm.UndoCommand.Execute(null);
        Assert.Equal(100, vm.Shapes[0].GetBounds().Left);
        Assert.False(vm.UndoCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void FlipVertical_And_MoveToLayer_ActOnMultiSelection()
    {
        var vm = MakeViewModel(Square, "{\\p1}m 100 300 l 200 300 200 400 100 400{\\p0}");
        vm.SelectAllShapesCommand.Execute(null);

        vm.FlipShapeVerticalCommand.Execute(null);
        Assert.Equal(300, vm.Shapes[0].GetBounds().Top);
        Assert.Equal(100, vm.Shapes[1].GetBounds().Top);

        vm.MoveShapeToLayerCommand.Execute(5);
        Assert.All(vm.Shapes, s => Assert.Equal(5, s.Layer));
    }

    [AvaloniaFact]
    public void Arrow_NudgesSelectedShape_OnePixel_ShiftTenPixels()
    {
        var vm = MakeViewModel(Square);
        vm.SelectShape(vm.Shapes[0]);
        var treeItem = vm.ShapeTreeItems[0];

        Press(vm, Key.Right);
        Press(vm, Key.Down, KeyModifiers.Shift);
        Press(vm, Key.Left, KeyModifiers.Control);
        Press(vm, Key.Up, KeyModifiers.Alt);

        Assert.Equal((91f, 109f), (vm.Shapes[0].Points[0].X, vm.Shapes[0].Points[0].Y));

        // Labels updated in place, no tree rebuild per key press
        Assert.Same(treeItem, vm.ShapeTreeItems[0]);
        Assert.Equal(vm.Shapes[0].Points[0].GetText(91, 109), treeItem.Children[0].Children[0].Name);
        Assert.Equal(91, vm.ShapeX);

        // Quick nudges share one undo step
        vm.UndoCommand.Execute(null);
        Assert.Equal((100f, 100f), (vm.Shapes[0].Points[0].X, vm.Shapes[0].Points[0].Y));
    }

    [AvaloniaFact]
    public void Arrow_WithPointSelected_MovesOnlyThePoint()
    {
        var vm = MakeViewModel(Square);
        vm.SelectPoint(vm.Shapes[0].Points[2]);

        Press(vm, Key.Right, KeyModifiers.Shift);

        Assert.Equal("m 100 100 l 200 100 210 200 100 200", vm.Shapes[0].ToAssa());
        Assert.Equal(210, vm.PointX);
        Assert.Same(vm.Shapes[0].Points[2], vm.ActivePoint);
    }

    [AvaloniaFact]
    public void Arrow_MovesEveryShapeOfMultiSelection()
    {
        var vm = MakeViewModel(Square, SecondSquare);
        vm.SelectAllShapesCommand.Execute(null);

        Press(vm, Key.Down);
        Press(vm, Key.Down);

        Assert.Equal(102, vm.Shapes[0].Points[0].Y);
        Assert.Equal(102, vm.Shapes[1].Points[0].Y);
        Assert.Equal(2, vm.SelectedShapes.Count);
    }

    [AvaloniaFact]
    public void Arrow_InTextBox_IsNotANudge()
    {
        var vm = MakeViewModel(Square);
        vm.SelectShape(vm.Shapes[0]);

        Press(vm, Key.Right, source: new TextBox());
        Press(vm, Key.Right, KeyModifiers.Control, source: new TextBox());

        Assert.Equal(100, vm.Shapes[0].Points[0].X);
    }

    [AvaloniaFact]
    public void Arrow_WithNothingSelected_IsNotHandled()
    {
        var vm = MakeViewModel(Square);
        var e = new KeyEventArgs { Key = Key.Right, RoutedEvent = InputElement.KeyDownEvent };

        vm.OnKeyDown(e);

        Assert.False(e.Handled);
        Assert.Equal(100, vm.Shapes[0].Points[0].X);
    }

    [AvaloniaFact]
    public void Escape_ClearsPointThenSelection_BeforeClosing()
    {
        var vm = MakeViewModel(Square);
        vm.SelectPoint(vm.Shapes[0].Points[1]);

        Press(vm, Key.Escape);

        Assert.Null(vm.ActivePoint);
        Assert.Same(vm.Shapes[0], vm.SelectedTreeItem?.Shape);

        Press(vm, Key.Escape);

        Assert.Null(vm.SelectedTreeItem);
        Assert.Null(vm.TargetShape);
        Assert.Empty(vm.SelectedShapes);
    }

    [AvaloniaFact]
    public void Escape_WithNothingSelected_ClosesTheWindow()
    {
        var vm = MakeViewModel(Square);
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var closed = false;
        window.Closed += (_, _) => closed = true;
        try
        {
            vm.SelectShape(vm.Shapes[0]);

            Press(vm, Key.Escape);
            Assert.False(closed);

            Press(vm, Key.Escape);
            Assert.True(closed);
        }
        finally
        {
            if (!closed)
            {
                window.Close();
            }
        }
    }

    [AvaloniaFact]
    public void SaveAndLoad_KeepsHiddenShapes_Hidden()
    {
        var vm = MakeViewModel(Square);
        vm.SelectShape(vm.Shapes[0]);
        vm.DuplicateShapeCommand.Execute(null);
        vm.SelectShape(vm.Shapes[0]);
        vm.ToggleShapeVisibilityCommand.Execute(null);
        Assert.True(vm.Shapes[0].Hidden);

        var text = vm.GetAssaDrawFileText();
        Assert.Contains("Comment:", text);

        var loaded = MakeViewModel();
        loaded.LoadFromText(text);

        Assert.Equal(2, loaded.Shapes.Count);
        Assert.True(loaded.Shapes[0].Hidden);
        Assert.False(loaded.Shapes[1].Hidden);
        Assert.Equal(vm.Shapes[0].ToAssa(), loaded.Shapes[0].ToAssa());
        Assert.Equal(vm.Shapes[1].ToAssa(), loaded.Shapes[1].ToAssa());
    }

    [AvaloniaFact]
    public void CopyToClipboardCode_LeavesHiddenShapesOut()
    {
        var vm = MakeViewModel(Square, SecondSquare);
        vm.SelectShape(vm.Shapes[0]);
        vm.ToggleShapeVisibilityCommand.Execute(null);

        // Save keeps the hidden shape, the code for the clipboard does not
        Assert.Contains("m 100 100", vm.GetAssaDrawFileText());
        var code = vm.GenerateAssaCode();
        Assert.DoesNotContain("m 100 100", code);
        Assert.Contains("m 300 100", code);
    }

    [AvaloniaFact]
    public void CommandStates_FollowSelectionAndShapes()
    {
        var vm = MakeViewModel();
        Assert.False(vm.DeleteShapeCommand.CanExecute(null));
        Assert.False(vm.DuplicateShapeCommand.CanExecute(null));
        Assert.False(vm.ClearAllCommand.CanExecute(null));
        Assert.False(vm.CloseShapeCommand.CanExecute(null));

        vm.LoadFromText(new AdvancedSubStationAlpha().ToText(new Subtitle
        {
            Header = AdvancedSubStationAlpha.DefaultHeader,
            Paragraphs = { new Paragraph(Square, 0, 1000) },
        }, string.Empty));

        Assert.True(vm.ClearAllCommand.CanExecute(null));
        Assert.False(vm.DeleteShapeCommand.CanExecute(null));

        vm.SelectShape(vm.Shapes[0]);
        Assert.True(vm.DeleteShapeCommand.CanExecute(null));
        Assert.True(vm.DuplicateShapeCommand.CanExecute(null));
        Assert.False(vm.CloseShapeCommand.CanExecute(null));

        // Keyboard shortcut still runs the command when it is allowed
        Press(vm, Key.D, KeyModifiers.Control);
        Assert.Equal(2, vm.Shapes.Count);

        Press(vm, Key.Escape);
        Assert.False(vm.DeleteShapeCommand.CanExecute(null));

        Press(vm, Key.A, KeyModifiers.Control);
        Assert.True(vm.DeleteShapeCommand.CanExecute(null));

        vm.ClearAllCommand.Execute(null);
        Assert.False(vm.ClearAllCommand.CanExecute(null));
        Assert.False(vm.DeleteShapeCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void CloseShapeCommand_EnabledOnlyWhileDrawingWithEnoughPoints()
    {
        var oldGrid = DrawSettings.ShowGrid;
        var vm = MakeViewModel();
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            vm.Canvas!.FitToView();
            DrawSettings.ShowGrid = false;
            vm.ShowGrid = false;
            Dispatcher.UIThread.RunJobs();
            vm.LineToolCommand.Execute(null);

            Click(window, vm.Canvas!, 400, 400);
            Assert.True(vm.IsDrawing);
            Assert.False(vm.CloseShapeCommand.CanExecute(null));
            Assert.True(vm.DeleteShapeCommand.CanExecute(null));

            Click(window, vm.Canvas!, 800, 400);
            Click(window, vm.Canvas!, 800, 800);
            Assert.True(vm.CloseShapeCommand.CanExecute(null));

            vm.CloseShapeCommand.Execute(null);
            Assert.Single(vm.Shapes);
            Assert.False(vm.CloseShapeCommand.CanExecute(null));
        }
        finally
        {
            window.Close();
            DrawSettings.ShowGrid = oldGrid;
        }
    }

    private static void Click(AssaDrawWindow window, AssaDrawCanvas canvas, float x, float y)
    {
        var origin = canvas.TranslatePoint(new Point(0, 0), window)!.Value;
        var zoom = canvas.ZoomFactor;
        var panX = (canvas.Bounds.Width - canvas.CanvasWidth * zoom) / 2;
        var panY = (canvas.Bounds.Height - canvas.CanvasHeight * zoom) / 2;
        var point = new Point(origin.X + panX + x * zoom, origin.Y + panY + y * zoom);
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }
}
