using Avalonia;
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
/// Undo from canvas interaction: the last click while drawing, and a point drag as one step.
/// </summary>
public class AssaDrawCanvasUndoTests
{
    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) Open(string text)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var lines = string.IsNullOrEmpty(text)
            ? new List<SubtitleLineViewModel>()
            : [new SubtitleLineViewModel(new Paragraph(text, 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha())];
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, lines, 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.Canvas!.FitToView();
        DrawSettings.ShowGrid = false;
        vm.ShowGrid = false;
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static Point ToWindow(AssaDrawWindow window, AssaDrawCanvas canvas, float x, float y)
    {
        var origin = canvas.TranslatePoint(new Point(0, 0), window)!.Value;
        var zoom = canvas.ZoomFactor;
        var panX = (canvas.Bounds.Width - canvas.CanvasWidth * zoom) / 2;
        var panY = (canvas.Bounds.Height - canvas.CanvasHeight * zoom) / 2;
        return new Point(origin.X + panX + x * zoom, origin.Y + panY + y * zoom);
    }

    private static void Click(AssaDrawWindow window, Point point)
    {
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void UndoWhileDrawing_RemovesLastClick()
    {
        var oldGrid = DrawSettings.ShowGrid;
        var (window, vm) = Open(string.Empty);
        try
        {
            vm.LineToolCommand.Execute(null);
            Click(window, ToWindow(window, vm.Canvas!, 400, 400));
            Click(window, ToWindow(window, vm.Canvas!, 800, 400));
            Click(window, ToWindow(window, vm.Canvas!, 800, 800));
            Assert.Equal(3, vm.ActiveShape!.Points.Count);

            vm.UndoCommand.Execute(null);

            Assert.Equal(2, vm.ActiveShape!.Points.Count);
            Assert.Empty(vm.Shapes);
        }
        finally
        {
            window.Close();
            DrawSettings.ShowGrid = oldGrid;
        }
    }

    [AvaloniaFact]
    public void PointDrag_IsOneUndoStep()
    {
        var oldGrid = DrawSettings.ShowGrid;
        var (window, vm) = Open("{\\p1}m 400 400 l 800 400 800 800{\\p0}");
        try
        {
            vm.SelectToolCommand.Execute(null);
            var start = ToWindow(window, vm.Canvas!, 800, 400);
            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(ToWindow(window, vm.Canvas!, 850, 420), RawInputModifiers.LeftMouseButton);
            window.MouseMove(ToWindow(window, vm.Canvas!, 900, 450), RawInputModifiers.LeftMouseButton);
            window.MouseUp(ToWindow(window, vm.Canvas!, 900, 450), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(900, vm.Shapes[0].Points[1].X, 0);

            vm.UndoCommand.Execute(null);

            Assert.Equal("m 400 400 l 800 400 800 800", vm.Shapes[0].ToAssa());
            Assert.False(vm.UndoCommand.CanExecute(null));
        }
        finally
        {
            window.Close();
            DrawSettings.ShowGrid = oldGrid;
        }
    }

    private static void Drag(AssaDrawWindow window, AssaDrawCanvas canvas, float x1, float y1, float x2, float y2)
    {
        var start = ToWindow(window, canvas, x1, y1);
        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        window.MouseMove(ToWindow(window, canvas, (x1 + x2) / 2, (y1 + y2) / 2), RawInputModifiers.LeftMouseButton);
        window.MouseMove(ToWindow(window, canvas, x2, y2), RawInputModifiers.LeftMouseButton);
        window.MouseUp(ToWindow(window, canvas, x2, y2), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void DragShape_WithSelectTool_MovesIt()
    {
        var oldGrid = DrawSettings.ShowGrid;
        var (window, vm) = Open("{\\p1}m 400 400 l 800 400 800 800 400 800{\\p0}");
        try
        {
            vm.SelectToolCommand.Execute(null);
            Drag(window, vm.Canvas!, 600, 600, 700, 650);
            Assert.Equal(500, vm.Shapes[0].GetBounds().Left, 0);

            // Second drag, now that it is selected
            Drag(window, vm.Canvas!, 700, 650, 800, 650);
            Assert.Equal(600, vm.Shapes[0].GetBounds().Left, 0);
        }
        finally
        {
            window.Close();
            DrawSettings.ShowGrid = oldGrid;
        }
    }

    [AvaloniaFact]
    public void DragBubbleWithText_AndTextBox_MovesThem()
    {
        var oldGrid = DrawSettings.ShowGrid;
        var (window, vm) = Open(string.Empty);
        try
        {
            vm.PickLibraryShapeCommand.Execute(ShapeLibrary.BuiltIn[0]);
            Drag(window, vm.Canvas!, 400, 300, 1000, 800);
            Assert.Single(vm.Shapes);
            var left = vm.Shapes[0].GetBounds().Left;
            vm.ShapeText = "Hello";
            Dispatcher.UIThread.RunJobs();
            Drag(window, vm.Canvas!, 700, 450, 800, 450);
            Assert.Equal(left + 100, vm.Shapes[0].GetBounds().Left, 0);

            vm.TextToolCommand.Execute(null);
            Drag(window, vm.Canvas!, 100, 100, 500, 200);
            Assert.Equal(2, vm.Shapes.Count);
            Drag(window, vm.Canvas!, 300, 150, 300, 250);
            Assert.Equal(200, vm.Shapes[1].GetBounds().Top, 0);
        }
        finally
        {
            window.Close();
            DrawSettings.ShowGrid = oldGrid;
        }
    }
}
