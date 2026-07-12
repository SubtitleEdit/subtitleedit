using System.Globalization;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class NOcrLine
{
    public OcrPoint Start { get; set; }
    public OcrPoint End { get; set; }

    public string DisplayName => ToString();

    public NOcrLine()
    {
        Start = new OcrPoint();
        End = new OcrPoint();
    }

    public bool IsEmpty => Start.X == End.X && Start.Y == End.Y;

    public NOcrLine(OcrPoint start, OcrPoint end)
    {
        Start = new OcrPoint(start.X, start.Y);
        End = new OcrPoint(end.X, end.Y);
    }

    public static OcrPointF PointPixelsToPercent(OcrPoint p, int pixelWidth, int pixelHeight)
    {
        return new OcrPointF((float)(p.X * 100.0 / pixelWidth), (float)(p.Y * 100.0 / pixelHeight));
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0},{1} -> {2},{3} ", Start.X, Start.Y, End.X, End.Y);
    }

    public OcrPointF GetStartPercent(int width, int height)
    {
        return PointPixelsToPercent(Start, width, height);
    }

    public OcrPointF GetEnd(int width, int height)
    {
        return PointPixelsToPercent(End, width, height);
    }

    public IEnumerable<OcrPoint> GetPoints()
    {
        return GetPoints(Start, End);
    }

    public IEnumerable<OcrPoint> ScaledGetPoints(NOcrChar nOcrChar, int width, int height)
    {
        return GetPoints(GetScaledStart(nOcrChar, width, height), GetScaledEnd(nOcrChar, width, height));
    }

    /// <summary>
    /// Allocation-free version of <see cref="GetPoints()"/> for the OCR matcher's inner loop.
    /// Yields the same points as <see cref="GetPoints(OcrPoint, OcrPoint)"/> but via a struct
    /// enumerator, so a foreach compiles to direct calls with no heap allocation - the
    /// IEnumerable version allocates an iterator object per line per candidate character,
    /// which dominated batch nOCR runs.
    /// </summary>
    public LinePointWalker WalkPoints()
    {
        return new LinePointWalker(Start, End);
    }

    /// <summary>Allocation-free version of <see cref="ScaledGetPoints"/>. See <see cref="WalkPoints()"/>.</summary>
    public LinePointWalker ScaledWalkPoints(NOcrChar nOcrChar, int width, int height)
    {
        return new LinePointWalker(GetScaledStart(nOcrChar, width, height), GetScaledEnd(nOcrChar, width, height));
    }

    /// <summary>
    /// Struct enumerable/enumerator over the pixel points of a line. Must stay arithmetically
    /// identical to <see cref="GetPoints(OcrPoint, OcrPoint)"/> (verified by unit test) so old
    /// and new match paths accept exactly the same pixels.
    /// </summary>
    public struct LinePointWalker
    {
        private readonly int _x1;
        private readonly int _y1;
        private readonly int _end;
        private readonly double _factor;
        private readonly bool _horizontal;
        private int _i;

        public LinePointWalker(OcrPoint start, OcrPoint end)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            _horizontal = Math.Abs(dx) > Math.Abs(dy);
            if (_horizontal)
            {
                int y2;
                (_x1, _y1, _end, y2) = dx > 0 ? (start.X, start.Y, end.X, end.Y) : (end.X, end.Y, start.X, start.Y);
                _factor = (double)(y2 - _y1) / (_end - _x1);
                _i = _x1 - 1;
            }
            else
            {
                int x2;
                (_x1, _y1, x2, _end) = dy > 0 ? (start.X, start.Y, end.X, end.Y) : (end.X, end.Y, start.X, start.Y);
                _factor = (double)(x2 - _x1) / (_end - _y1);
                _i = _y1 - 1;
            }
        }

        public OcrPoint Current { get; private set; }

        public LinePointWalker GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            _i++;
            if (_i > _end)
            {
                return false;
            }

            Current = _horizontal
                ? new OcrPoint(_i, (int)Math.Round(_y1 + _factor * (_i - _x1), MidpointRounding.AwayFromZero))
                : new OcrPoint((int)Math.Round(_x1 + _factor * (_i - _y1), MidpointRounding.AwayFromZero), _i);
            return true;
        }
    }

    public static IEnumerable<OcrPoint> GetPoints(OcrPoint start, OcrPoint end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var absDx = Math.Abs(dx);
        var absDy = Math.Abs(dy);

        if (absDx > absDy)
        {
            var (x1, y1, x2, y2) = dx > 0 ? (start.X, start.Y, end.X, end.Y) : (end.X, end.Y, start.X, start.Y);
            var factor = (double)(y2 - y1) / (x2 - x1);
            for (var i = x1; i <= x2; i++)
            {
                yield return new OcrPoint(i, (int)Math.Round(y1 + factor * (i - x1), MidpointRounding.AwayFromZero));
            }
        }
        else
        {
            var (x1, y1, x2, y2) = dy > 0 ? (start.X, start.Y, end.X, end.Y) : (end.X, end.Y, start.X, start.Y);
            var factor = (double)(x2 - x1) / (y2 - y1);
            for (var i = y1; i <= y2; i++)
            {
                yield return new OcrPoint((int)Math.Round(x1 + factor * (i - y1), MidpointRounding.AwayFromZero), i);
            }
        }
    }

    public OcrPoint GetScaledStart(NOcrChar ocrChar, int width, int height)
    {
        return new OcrPoint((int)Math.Round(Start.X * width / (double)ocrChar.Width, MidpointRounding.AwayFromZero), (int)Math.Round(Start.Y * height / (double)ocrChar.Height, MidpointRounding.AwayFromZero));
    }

    public OcrPoint GetScaledEnd(NOcrChar ocrChar, int width, int height)
    {
        return new OcrPoint((int)Math.Round(End.X * width / (double)ocrChar.Width, MidpointRounding.AwayFromZero), (int)Math.Round(End.Y * height / (double)ocrChar.Height, MidpointRounding.AwayFromZero));
    }
}