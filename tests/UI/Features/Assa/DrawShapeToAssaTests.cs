using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;

namespace UITests.Features.Assa;

/// <summary>
/// A shape can hold both line and bezier segments (the importer appends "l" and "b" runs to one
/// shape). ToAssa serialized any shape with a bezier point as a single "b" run, which turned the
/// straight points into bezier control points and shifted every later triplet.
/// </summary>
public class DrawShapeToAssaTests
{
    private static DrawShape MakeShape(params (DrawCoordinateType type, float x, float y)[] points)
    {
        var shape = new DrawShape();
        foreach (var (type, x, y) in points)
        {
            shape.AddPoint(type, x, y, Colors.White);
        }

        return shape;
    }

    [Fact]
    public void ToAssa_LineOnly_EmitsLineCommands()
    {
        var shape = MakeShape((DrawCoordinateType.Line, 0, 0), (DrawCoordinateType.Line, 100, 0), (DrawCoordinateType.Line, 100, 100));

        Assert.Equal("m 0 0 l 100 0 100 100", shape.ToAssa());
    }

    [Fact]
    public void ToAssa_BezierOnly_EmitsOneBezierRun()
    {
        var shape = MakeShape(
            (DrawCoordinateType.BezierCurve, 0, 0),
            (DrawCoordinateType.BezierCurveSupport1, 10, 20),
            (DrawCoordinateType.BezierCurveSupport2, 30, 40),
            (DrawCoordinateType.BezierCurve, 50, 60));

        Assert.Equal("m 0 0 b 10 20 30 40 50 60", shape.ToAssa());
    }

    [Fact]
    public void ToAssa_MixedShape_KeepsLineSegmentsOutOfTheBezierRun()
    {
        // m 0 0 l 100 0 b 100 50 50 100 0 100 l 0 0
        var shape = MakeShape(
            (DrawCoordinateType.Line, 0, 0),
            (DrawCoordinateType.Line, 100, 0),
            (DrawCoordinateType.BezierCurveSupport1, 100, 50),
            (DrawCoordinateType.BezierCurveSupport2, 50, 100),
            (DrawCoordinateType.BezierCurve, 0, 100),
            (DrawCoordinateType.Line, 0, 0));

        Assert.Equal("m 0 0 l 100 0 b 100 50 50 100 0 100 l 0 0", shape.ToAssa());
    }
}
