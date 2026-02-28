using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Ocr;

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

    internal OcrPoint GetScaledStart(NOcrChar ocrChar, int width, int height)
    {
        return new OcrPoint((int)Math.Round(Start.X * width / (double)ocrChar.Width, MidpointRounding.AwayFromZero), (int)Math.Round(Start.Y * height / (double)ocrChar.Height, MidpointRounding.AwayFromZero));
    }

    internal OcrPoint GetScaledEnd(NOcrChar ocrChar, int width, int height)
    {
        return new OcrPoint((int)Math.Round(End.X * width / (double)ocrChar.Width, MidpointRounding.AwayFromZero), (int)Math.Round(End.Y * height / (double)ocrChar.Height, MidpointRounding.AwayFromZero));
    }
}