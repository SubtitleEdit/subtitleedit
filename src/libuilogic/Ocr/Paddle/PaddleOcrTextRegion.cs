namespace Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

/// <summary>One corner of a detected text box, in image pixels.</summary>
public readonly record struct PaddleOcrPoint(double X, double Y);

/// <summary>
/// The quadrilateral PaddleOCR reports around a detected text box. It is a polygon rather
/// than a rectangle because the detector follows skewed text, so the four corners are kept
/// as given instead of being flattened into x/y/w/h.
/// </summary>
public sealed class PaddleOcrBoundingBox
{
    public PaddleOcrPoint TopLeft { get; }
    public PaddleOcrPoint TopRight { get; }
    public PaddleOcrPoint BottomRight { get; }
    public PaddleOcrPoint BottomLeft { get; }

    public PaddleOcrBoundingBox(
        PaddleOcrPoint topLeft, PaddleOcrPoint topRight, PaddleOcrPoint bottomRight, PaddleOcrPoint bottomLeft)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        BottomLeft = bottomLeft;
    }

    /// <summary>An all-zero box, for results that carry no polygon.</summary>
    public static PaddleOcrBoundingBox Empty { get; } = new(default, default, default, default);

    public double Width => Math.Max(
        Math.Abs(TopRight.X - TopLeft.X),
        Math.Abs(BottomRight.X - BottomLeft.X));

    public double Height => Math.Max(
        Math.Abs(BottomLeft.Y - TopLeft.Y),
        Math.Abs(BottomRight.Y - TopRight.Y));

    public PaddleOcrPoint Center => new(
        (TopLeft.X + TopRight.X + BottomRight.X + BottomLeft.X) / 4,
        (TopLeft.Y + TopRight.Y + BottomRight.Y + BottomLeft.Y) / 4);

    /// <summary>Topmost y of the two top corners - the box may be skewed.</summary>
    public double MinY => Math.Min(TopLeft.Y, TopRight.Y);

    /// <summary>Bottommost y of the two bottom corners.</summary>
    public double MaxY => Math.Max(BottomLeft.Y, BottomRight.Y);
}

/// <summary>One text box PaddleOCR recognised: what it read, how sure it was, and where.</summary>
public sealed class PaddleOcrTextRegion
{
    public string Text { get; set; } = string.Empty;

    /// <summary>0..1 as PaddleOCR reports it; 0 means "not reported" rather than "certainly wrong".</summary>
    public double Confidence { get; set; }

    public PaddleOcrBoundingBox BoundingBox { get; set; } = PaddleOcrBoundingBox.Empty;
}
