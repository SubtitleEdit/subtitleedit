using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class NOcrPoint
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public NOcrPoint()
        {
            Start = new Point();
            End = new Point();
        }

        public NOcrPoint(Point start, Point end)
        {
            Start = new Point(start.X, start.Y);
            End = new Point(end.X, end.Y);
        }

        public static PointF PointPixelsToPercent(Point p, int pixelWidth, int pixelHeight)
        {
            return new PointF((float)(p.X * 100.0 / pixelWidth), (float)(p.Y * 100.0 / pixelHeight));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1} -> {2},{3} ", Start.X, Start.Y, End.X, End.Y);
        }

        public PointF GetStartPercent(int width, int height)
        {
            return PointPixelsToPercent(Start, width, height);
        }

        public PointF GetEnd(int width, int height)
        {
            return PointPixelsToPercent(End, width, height);
        }

        public List<Point> GetPoints()
        {
            return GetPoints(Start, End);
        }

        public List<Point> ScaledGetPoints(NOcrChar nOcrChar, int width, int height)
        {
            return GetPoints(GetScaledStart(nOcrChar, width, height), GetScaledEnd(nOcrChar, width, height));
        }

        public static List<Point> GetPoints(Point start, Point end)
        {
            List<Point> list;
            var x1 = start.X;
            var x2 = end.X;
            var y1 = start.Y;
            var y2 = end.Y;
            if (Math.Abs(start.X - end.X) > Math.Abs(start.Y - end.Y))
            {
                if (x1 > x2)
                {
                    x2 = start.X;
                    x1 = end.X;
                    y2 = start.Y;
                    y1 = end.Y;
                }
                var factor = (double)(y2 - y1) / (x2 - x1);
                list = new List<Point>(x2 - x1 + 1);
                for (var i = x1; i <= x2; i++)
                {
                    list.Add(new Point(i, (int)Math.Round(y1 + factor * (i - x1), MidpointRounding.AwayFromZero)));
                }
            }
            else
            {
                if (y1 > y2)
                {
                    x2 = start.X;
                    x1 = end.X;
                    y2 = start.Y;
                    y1 = end.Y;
                }
                var factor = (double)(x2 - x1) / (y2 - y1);
                list = new List<Point>(y2 - y1 + 1);
                for (var i = y1; i <= y2; i++)
                {
                    list.Add(new Point((int)Math.Round(x1 + factor * (i - y1), MidpointRounding.AwayFromZero), i));
                }
            }

            return list;
        }

        internal Point GetScaledStart(NOcrChar ocrChar, int width, int height)
        {
            return new Point((int)Math.Round(Start.X * 100.0 / ocrChar.Width * width / 100.0, MidpointRounding.AwayFromZero), (int)Math.Round(Start.Y * 100.0 / ocrChar.Height * height / 100.0, MidpointRounding.AwayFromZero));
        }

        internal Point GetScaledEnd(NOcrChar ocrChar, int width, int height)
        {
            return new Point((int)Math.Round(End.X * 100.0 / ocrChar.Width * width / 100.0, MidpointRounding.AwayFromZero), (int)Math.Round(End.Y * 100.0 / ocrChar.Height * height / 100.0, MidpointRounding.AwayFromZero));
        }
    }
}
