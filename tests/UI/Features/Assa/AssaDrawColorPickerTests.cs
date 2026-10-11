using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;

namespace UITests.Features.Assa;

/// <summary>
/// Eyedropper: picks a shape's color or a background pixel; the color is used for new shapes and
/// recolors a selected shape's layer.
/// </summary>
public class AssaDrawColorPickerTests
{
    private static string MakeHalfRedHalfBluePng()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"assadraw-picker-{Guid.NewGuid():N}.png");
        using var bitmap = new SKBitmap(1920, 1080);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(200, 30, 40));
            using var paint = new SKPaint { Color = new SKColor(20, 60, 220) };
            canvas.DrawRect(960, 0, 960, 1080, paint);
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(fileName, data.ToArray());
        return fileName;
    }

    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) Open(params string[] texts)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var lines = texts.Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, 0, 2000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha())).ToList();
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
        var point = new Point(origin.X + (canvas.Bounds.Width - 1920 * zoom) / 2 + x * zoom, origin.Y + (canvas.Bounds.Height - 1080 * zoom) / 2 + y * zoom);
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void PicksBackgroundPixels()
    {
        var png = MakeHalfRedHalfBluePng();
        var (window, vm) = Open();
        try
        {
            Assert.True(vm.LoadImageBackground(png));
            vm.ColorPickerToolCommand.Execute(null);

            Click(window, vm.Canvas!, 400, 500);
            Assert.Equal(Color.FromRgb(200, 30, 40), vm.LayerColor);

            Click(window, vm.Canvas!, 1500, 500);
            Assert.Equal(Color.FromRgb(20, 60, 220), vm.LayerColor);
        }
        finally
        {
            window.Close();
            File.Delete(png);
        }
    }

    [AvaloniaFact]
    public void ShapeColorWinsOverBackground_AndRecolorsTheSelectedShape_Undoably()
    {
        var png = MakeHalfRedHalfBluePng();
        var (window, vm) = Open(
            "{\\p1}m 100 100 l 600 100 600 600 100 600{\\p0}",
            "{\\p1}m 1200 200 l 1600 200 1600 600 1200 600{\\p0}");
        try
        {
            vm.Shapes[0].ForeColor = Colors.Orange;
            vm.Shapes[1].ForeColor = Colors.White;
            Assert.True(vm.LoadImageBackground(png));
            vm.SelectShape(vm.Shapes[1]);
            vm.ColorPickerToolCommand.Execute(null);

            // Inside the orange square: its color, not the red background under it
            Click(window, vm.Canvas!, 300, 300);

            Assert.Equal(Colors.Orange, vm.LayerColor);
            Assert.Equal(Colors.Orange, vm.Shapes[1].ForeColor);

            vm.UndoCommand.Execute(null);
            Assert.Equal(Colors.White, vm.Shapes[1].ForeColor);
        }
        finally
        {
            window.Close();
            File.Delete(png);
        }
    }

    [AvaloniaFact]
    public void NewShapes_UseTheCurrentColor_OnTheLayerWithThatColor()
    {
        var (window, vm) = Open("{\\p1}m 100 100 l 300 100 300 300{\\p0}");
        var oldGrid = DrawSettings.ShowGrid;
        try
        {
            vm.Shapes[0].ForeColor = Colors.Yellow;
            DrawSettings.ShowGrid = false;
            vm.ShowGrid = false;
            vm.PickColor(Colors.Cyan);
            vm.RectangleToolCommand.Execute(null);

            Click(window, vm.Canvas!, 800, 400);
            Click(window, vm.Canvas!, 1000, 600);

            var rectangle = vm.Shapes[1];
            Assert.Equal(Colors.Cyan, rectangle.ForeColor);
            Assert.Equal(1, rectangle.Layer);

            // Back to the yellow color: the next shape joins the yellow layer
            vm.PickColor(Colors.Yellow);
            Click(window, vm.Canvas!, 1200, 400);
            Click(window, vm.Canvas!, 1400, 600);
            Assert.Equal(0, vm.Shapes[2].Layer);
        }
        finally
        {
            window.Close();
            DrawSettings.ShowGrid = oldGrid;
        }
    }
}
