using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
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
/// Shape library + shape tool: built-in shapes, placing them, saving your own.
/// </summary>
public class AssaDrawShapeLibraryTests
{
    private static AssaDrawViewModel MakeViewModel(params string[] texts)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var lines = texts.Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, 0, 2000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, lines, 1920, 1080);
        return vm;
    }

    [Fact]
    public void EveryBuiltInShape_HasClosedContours_AndAThumbnail()
    {
        Assert.True(ShapeLibrary.BuiltIn.Count >= 25);
        foreach (var item in ShapeLibrary.BuiltIn)
        {
            Assert.NotEmpty(item.Template);
            Assert.All(item.Template, s => Assert.True(s.Points.Count >= 3, item.Name));
            var (left, top, right, bottom) = item.Bounds;
            Assert.True(right - left > 100 && bottom - top > 100, item.Name);
            Assert.False(item.ToGeometry().Bounds.Width <= 0, item.Name);
        }

        Assert.Equal(2, ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeRing).Template.Count);
        Assert.True(ShapeLibrary.BuiltIn.Count(i => i.Category == Se.Language.Assa.DrawCategorySpeechBubbles) >= 6);
    }

    [Fact]
    public void Insert_FillsTheRectangle_OnANewLayer_InTheCurrentColor_AndCanBeUndone()
    {
        var vm = MakeViewModel("{\\p1}m 0 0 l 10 0 10 10{\\p0}");
        vm.LayerColor = Colors.Orange;
        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeSpeechBubble);

        vm.InsertLibraryShape(new CanvasInsertEventArgs(100, 200, 400, 300, isClick: false));

        var inserted = Assert.Single(vm.Shapes.Skip(1));
        var (left, top, right, bottom) = inserted.GetBounds();
        Assert.Equal(100, left, 1);
        Assert.Equal(200, top, 1);
        Assert.Equal(500, right, 1);
        Assert.Equal(500, bottom, 1);
        Assert.Equal(1, inserted.Layer);
        Assert.Equal(Colors.Orange, inserted.ForeColor);
        Assert.Equal(DrawingTool.Select, vm.CurrentTool);
        Assert.Same(inserted, vm.SelectedTreeItem?.Shape);

        vm.UndoCommand.Execute(null);
        Assert.Single(vm.Shapes);
    }

    [Fact]
    public void Click_PlacesDefaultSize_CenteredOnTheClick_KeepingProportions()
    {
        var vm = MakeViewModel();
        var item = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeArrow);
        vm.CurrentLibraryShape = item;

        vm.InsertLibraryShape(new CanvasInsertEventArgs(960, 540, 0, 0, isClick: true));

        var (left, top, right, bottom) = vm.Shapes[0].GetBounds();
        Assert.Equal(1920 / 5f, right - left, 1);
        Assert.Equal(960, (left + right) / 2, 1);
        Assert.Equal(540, (top + bottom) / 2, 1);
        Assert.Equal(item.AspectRatio, (bottom - top) / (right - left), 2);
    }

    [Fact]
    public void SavedShape_KeepsItsColorsPerLayer_AndCanBeRemoved()
    {
        var saved = Se.Settings.Assa.DrawShapeLibrary.ToList();
        try
        {
            var vm = MakeViewModel(
                "{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}",
                "{\\p1}m 25 25 l 75 25 75 75 25 75{\\p0}");
            vm.Shapes[0].ForeColor = Colors.Red;
            vm.Shapes[1].ForeColor = Colors.Blue;

            var item = vm.AddToLibrary("Target", vm.Shapes.ToList());
            Assert.Same(item, vm.CurrentLibraryShape);
            Assert.Contains(ShapeLibrary.GetUserShapes(), i => i.Name == "Target");

            vm.InsertLibraryShape(new CanvasInsertEventArgs(1000, 500, 200, 200, isClick: false));

            var inserted = vm.Shapes.Skip(2).ToList();
            Assert.Equal(2, inserted.Count);
            Assert.Equal(Colors.Red, inserted[0].ForeColor);
            Assert.Equal(Colors.Blue, inserted[1].ForeColor);
            Assert.Equal(2, inserted[0].Layer);
            Assert.Equal(3, inserted[1].Layer);
            Assert.Equal(1050, inserted[1].GetBounds().Left, 1);

            vm.RemoveFromLibraryCommand.Execute(item);
            Assert.DoesNotContain(ShapeLibrary.GetUserShapes(), i => i.Name == "Target");
            Assert.NotSame(item, vm.CurrentLibraryShape);
        }
        finally
        {
            Se.Settings.Assa.DrawShapeLibrary = saved;
        }
    }

    [AvaloniaFact]
    public void ShapeTool_DragOnCanvas_PlacesShape()
    {
        var vm = MakeViewModel();
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var canvas = vm.Canvas!;
            canvas.FitToView();
            vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeStar);
            vm.ShapeToolCommand.Execute(null);

            Point ToWindow(double x, double y)
            {
                var origin = canvas.TranslatePoint(new Point(0, 0), window)!.Value;
                var zoom = canvas.ZoomFactor;
                return new Point(origin.X + (canvas.Bounds.Width - 1920 * zoom) / 2 + x * zoom, origin.Y + (canvas.Bounds.Height - 1080 * zoom) / 2 + y * zoom);
            }

            window.MouseMove(ToWindow(400, 300));
            window.MouseDown(ToWindow(400, 300), MouseButton.Left);
            window.MouseMove(ToWindow(600, 450), RawInputModifiers.LeftMouseButton);
            window.MouseMove(ToWindow(800, 600), RawInputModifiers.LeftMouseButton);
            window.MouseUp(ToWindow(800, 600), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            var (left, top, right, bottom) = Assert.Single(vm.Shapes).GetBounds();
            Assert.Equal(400, left, 0);
            Assert.Equal(300, top, 0);
            Assert.Equal(800, right, 0);
            Assert.Equal(600, bottom, 0);
        }
        finally
        {
            window.Close();
        }
    }
}
