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
/// Clicking a vertex on the canvas must make the vertex's shape the target of the shape commands,
/// not leave them acting on the previously selected shape.
/// </summary>
public class AssaDrawPointSelectionTests
{
    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) Open()
    {
        var format = new AdvancedSubStationAlpha();
        var lines = new List<SubtitleLineViewModel>
        {
            new(new Paragraph("{\\p1}m 100 100 l 600 100 600 600 100 600{\\p0}", 0, 2000) { Extra = "Default", Layer = 0 }, format),
            new(new Paragraph("{\\p1}m 1200 200 l 1600 200 1600 600 1200 600{\\p0}", 0, 2000) { Extra = "Default", Layer = 1 }, format),
        };

        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, lines, 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.Canvas!.FitToView();
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static void Click(AssaDrawWindow window, AssaDrawCanvas canvas, double x, double y)
    {
        var origin = canvas.TranslatePoint(new Point(0, 0), window)!.Value;
        var zoom = canvas.ZoomFactor;
        var point = new Point(
            origin.X + (canvas.Bounds.Width - 1920 * zoom) / 2 + x * zoom,
            origin.Y + (canvas.Bounds.Height - 1080 * zoom) / 2 + y * zoom);
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void ClickVertexOfOtherShape_MakesItTheTargetShape()
    {
        var (window, vm) = Open();
        try
        {
            var a = vm.Shapes[0];
            var b = vm.Shapes[1];
            vm.SelectToolCommand.Execute(null);

            Click(window, vm.Canvas!, 300, 300);
            Assert.Same(a, vm.TargetShape);

            Click(window, vm.Canvas!, 1600, 600);
            Assert.NotNull(vm.ActivePoint);
            Assert.Same(b, vm.ActivePoint!.DrawShape);
            Assert.Same(b, vm.TargetShape);
            Assert.Same(vm.ActivePoint, vm.SelectedTreeItem?.Point);
            Assert.Same(b, vm.Canvas!.SelectedShape);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ClickVertexOfOtherShape_ThenDelete_DeletesClickedShape()
    {
        var (window, vm) = Open();
        try
        {
            var a = vm.Shapes[0];
            var b = vm.Shapes[1];
            vm.SelectToolCommand.Execute(null);

            Click(window, vm.Canvas!, 300, 300);
            Click(window, vm.Canvas!, 1600, 600);
            vm.DeleteShapeCommand.Execute(null);

            Assert.Contains(a, vm.Shapes);
            Assert.DoesNotContain(b, vm.Shapes);
        }
        finally
        {
            window.Close();
        }
    }
}
