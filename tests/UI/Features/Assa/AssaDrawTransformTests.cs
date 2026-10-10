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
/// Select tool scale/rotate handles around the selection, plus Rotate 90° from the menu.
/// </summary>
public class AssaDrawTransformTests
{
    private const double Padding = 8;

    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) Open(params string[] texts)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var lines = texts.Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, 0, 2000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, lines, 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.Canvas!.FitToView();
        vm.SelectToolCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static Point ToWindow(AssaDrawWindow window, AssaDrawCanvas canvas, double x, double y, double offsetX = 0, double offsetY = 0)
    {
        var origin = canvas.TranslatePoint(new Point(0, 0), window)!.Value;
        var zoom = canvas.ZoomFactor;
        var panX = (canvas.Bounds.Width - canvas.CanvasWidth * zoom) / 2;
        var panY = (canvas.Bounds.Height - canvas.CanvasHeight * zoom) / 2;
        return new Point(origin.X + panX + x * zoom + offsetX, origin.Y + panY + y * zoom + offsetY);
    }

    private static void Drag(AssaDrawWindow window, Point from, Point to, RawInputModifiers extra = RawInputModifiers.None)
    {
        window.MouseMove(from);
        window.MouseDown(from, MouseButton.Left, extra);
        var middle = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2);
        window.MouseMove(middle, RawInputModifiers.LeftMouseButton | extra);
        window.MouseMove(to, RawInputModifiers.LeftMouseButton | extra);
        window.MouseUp(to, MouseButton.Left, extra);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void RightHandle_ScalesWidthOnly_AndIsOneUndoStep()
    {
        var (window, vm) = Open("{\\p1}m 400 400 l 800 400 800 600 400 600{\\p0}");
        try
        {
            vm.SelectShape(vm.Shapes[0]);
            Dispatcher.UIThread.RunJobs();
            var zoom = vm.Canvas!.ZoomFactor;

            Drag(window,
                ToWindow(window, vm.Canvas, 800, 500, Padding),
                ToWindow(window, vm.Canvas, 1000, 500, Padding));

            var (left, top, right, bottom) = vm.Shapes[0].GetBounds();
            Assert.Equal(400, left, 0);
            Assert.Equal(1000, right, 0);
            Assert.Equal(400, top, 0);
            Assert.Equal(600, bottom, 0);

            vm.UndoCommand.Execute(null);
            Assert.Equal("m 400 400 l 800 400 800 600 400 600", vm.Shapes[0].ToAssa());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CornerHandleWithShift_KeepsAspectRatio()
    {
        var (window, vm) = Open("{\\p1}m 400 400 l 800 400 800 600 400 600{\\p0}");
        try
        {
            vm.SelectShape(vm.Shapes[0]);
            Dispatcher.UIThread.RunJobs();

            Drag(window,
                ToWindow(window, vm.Canvas!, 800, 600, Padding, Padding),
                ToWindow(window, vm.Canvas!, 1200, 650, Padding, Padding),
                RawInputModifiers.Shift);

            var (left, top, right, bottom) = vm.Shapes[0].GetBounds();
            Assert.Equal(400, left, 0);
            Assert.Equal(400, top, 0);
            Assert.Equal(800, right - left, 0);
            Assert.Equal(400, bottom - top, 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void RotateHandle_RotatesAroundCenter_ShiftSnapsTo15Degrees()
    {
        var (window, vm) = Open("{\\p1}m 400 400 l 800 400 800 600 400 600{\\p0}");
        try
        {
            vm.SelectShape(vm.Shapes[0]);
            Dispatcher.UIThread.RunJobs();

            // Handle sits above the top-center; drag it to the right of the center = +90°
            var handle = ToWindow(window, vm.Canvas!, 600, 400, 0, -Padding - 26);
            Drag(window, handle, ToWindow(window, vm.Canvas!, 1000, 510), RawInputModifiers.Shift);

            var (left, top, right, bottom) = vm.Shapes[0].GetBounds();
            Assert.Equal(500, left, 0);
            Assert.Equal(700, right, 0);
            Assert.Equal(300, top, 0);
            Assert.Equal(700, bottom, 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DraggingShapeOfMultiSelection_MovesAllSelected()
    {
        var (window, vm) = Open(
            "{\\p1}m 400 400 l 600 400 600 600 400 600{\\p0}",
            "{\\p1}m 1000 400 l 1200 400 1200 600 1000 600{\\p0}");
        try
        {
            vm.SelectAllShapesCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Drag(window, ToWindow(window, vm.Canvas!, 500, 500), ToWindow(window, vm.Canvas!, 600, 600));

            Assert.Equal(500, vm.Shapes[0].GetBounds().Left, 0);
            Assert.Equal(1100, vm.Shapes[1].GetBounds().Left, 0);
            Assert.Equal(2, vm.SelectedShapes.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RotateClockwise_FromMenu_TurnsAroundCenter()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var line = new SubtitleLineViewModel(new Paragraph("{\\p1}m 400 400 l 800 400 800 600 400 600{\\p0}", 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [line], 1920, 1080);
        vm.SelectShape(vm.Shapes[0]);

        vm.RotateShapeClockwiseCommand.Execute(null);

        Assert.Equal("m 700 300 l 700 700 500 700 500 300", vm.Shapes[0].ToAssa());
        vm.UndoCommand.Execute(null);
        Assert.Equal("m 400 400 l 800 400 800 600 400 600", vm.Shapes[0].ToAssa());
    }
}
