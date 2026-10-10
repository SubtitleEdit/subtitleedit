using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// Shape commands behind the ASSA draw context menus: duplicate, flip, move to layer, delete point,
/// plus the hit testing the canvas uses to find the shape under the pointer.
/// </summary>
public class AssaDrawShapeEditingTests
{
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

    [Fact]
    public void HitTest_InsideAndOnOutline_ButNotOutside()
    {
        var vm = MakeViewModel("{\\p1}m 100 100 l 200 100 200 200 100 200{\\p0}");
        var shape = vm.Shapes[0];

        Assert.True(shape.HitTest(150, 150, 2));
        Assert.True(shape.HitTest(201, 150, 2));
        Assert.False(shape.HitTest(250, 150, 2));
    }

    [Fact]
    public void HitTest_FollowsBezierCurve()
    {
        var shape = CircleBezier.MakeCircle(500, 500, 100, 0, Colors.White);

        Assert.True(shape.HitTest(500, 500, 1));
        Assert.True(shape.HitTest(570, 500, 1));
        // Corner of the bounding box is outside the circle
        Assert.False(shape.HitTest(590, 590, 1));
    }

    [Fact]
    public void DuplicateShape_AddsOffsetCopy_AndSelectsIt()
    {
        var vm = MakeViewModel("{\\p1}m 100 100 l 200 100 200 200{\\p0}");
        vm.SelectShape(vm.Shapes[0]);

        vm.DuplicateShapeCommand.Execute(null);

        Assert.Equal(2, vm.Shapes.Count);
        Assert.Equal(100 + DrawSettings.GridSize, vm.Shapes[1].Points[0].X);
        Assert.Equal(100, vm.Shapes[0].Points[0].X);
        Assert.Same(vm.Shapes[1], vm.SelectedTreeItem?.Shape);
    }

    [Fact]
    public void FlipHorizontal_MirrorsInsideBounds()
    {
        var vm = MakeViewModel("{\\p1}m 100 100 l 300 100 100 200{\\p0}");
        vm.SelectShape(vm.Shapes[0]);

        vm.FlipShapeHorizontalCommand.Execute(null);

        Assert.Equal("m 300 100 l 100 100 300 200", vm.Shapes[0].ToAssa());
    }

    [Fact]
    public void MoveShapeToLayer_TakesLayerColor_AndKeepsSelection()
    {
        var vm = MakeViewModel(
            "{\\p1}m 0 0 l 10 0 10 10{\\p0}",
            "{\\p1}m 50 50 l 60 50 60 60{\\p0}");
        vm.Shapes[1].ForeColor = Colors.Red;
        var shape = vm.Shapes[0];
        vm.SelectShape(shape);

        vm.MoveShapeToLayerCommand.Execute(1);

        Assert.Equal(1, shape.Layer);
        Assert.Equal(Colors.Red, shape.ForeColor);
        Assert.Same(shape, vm.SelectedTreeItem?.Shape);
        Assert.Single(vm.ShapeTreeItems);
    }

    [Fact]
    public void DeletePoint_BezierEnd_RemovesItsControlPoints()
    {
        var vm = MakeViewModel("{\\p1}m 0 0 l 100 0 b 120 20 120 80 100 100{\\p0}");
        var shape = vm.Shapes[0];
        Assert.Equal(5, shape.Points.Count);

        vm.SelectPoint(shape.Points[4]);
        vm.DeletePointCommand.Execute(null);

        Assert.Equal("m 0 0 l 100 0", shape.ToAssa());
        Assert.Same(shape, vm.SelectedTreeItem?.Shape);
    }

    [Fact]
    public void DeletePoint_ControlPoint_IsIgnored()
    {
        var vm = MakeViewModel("{\\p1}m 0 0 b 20 20 80 20 100 0{\\p0}");
        var shape = vm.Shapes[0];

        vm.SelectPoint(shape.Points[1]);
        vm.DeletePointCommand.Execute(null);

        Assert.Equal(4, shape.Points.Count);
    }

    [Fact]
    public void DeletePoint_FirstPointOfBezier_NextEndPointBecomesStart()
    {
        var shape = new DrawShape();
        shape.AddPoint(DrawCoordinateType.BezierCurve, 0, 0, Colors.Red);
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport1, 10, 10, Colors.Green);
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport2, 20, 10, Colors.Green);
        shape.AddPoint(DrawCoordinateType.BezierCurve, 30, 0, Colors.Red);
        shape.AddPoint(DrawCoordinateType.Line, 30, 30, Colors.Red);

        Assert.True(shape.RemovePoint(shape.Points[0]));

        Assert.Equal("m 30 0 l 30 30", shape.ToAssa());
    }

    [Fact]
    public void ToggleShapeVisibility_SelectionSurvivesTreeRebuild()
    {
        var vm = MakeViewModel("{\\p1}m 0 0 l 10 0 10 10{\\p0}");
        var shape = vm.Shapes[0];
        vm.SelectShape(shape);

        vm.ToggleShapeVisibilityCommand.Execute(null);

        Assert.True(shape.Hidden);
        Assert.True(vm.IsShapeSelected);
        Assert.Same(shape, vm.SelectedTreeItem?.Shape);
        Assert.True(vm.SelectedTreeItem!.IsHidden);
    }
}
