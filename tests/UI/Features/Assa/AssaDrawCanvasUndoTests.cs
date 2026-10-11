using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
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
}
