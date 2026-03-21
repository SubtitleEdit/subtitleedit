namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Represents the type of ASSA drawing coordinate/command.
/// </summary>
public enum DrawCoordinateType
{
    /// <summary>No specific type (default)</summary>
    None,

    /// <summary>Move command (m x y) - moves cursor to position</summary>
    Move,

    /// <summary>Line command (l x y) - draws a line to position</summary>
    Line,

    /// <summary>Bezier curve endpoint (b x1 y1 x2 y2 x3 y3)</summary>
    BezierCurve,

    /// <summary>First control point of a Bezier curve</summary>
    BezierCurveSupport1,

    /// <summary>Second control point of a Bezier curve</summary>
    BezierCurveSupport2,
}
