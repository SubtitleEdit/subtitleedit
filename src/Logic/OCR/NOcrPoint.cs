using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.OCR
{
    public class NOcrPoint
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }
        public string Id { get; set; }

        public NOcrPoint()
        {
            Id = Guid.NewGuid().ToString();
            Start = new Point();
            End = new Point();
        }

        public NOcrPoint(Point start, Point end, int pixelWidth, int pixelHeight) : this()
        {
            Start = PointPixelsToPercent(start, pixelWidth, pixelHeight);
            End = PointPixelsToPercent(end, pixelWidth, pixelHeight);
        }

        public PointF PointPixelsToPercent(Point p, int pixelWidth, int pixelHeight)
        {
            return new PointF((float)(p.X * 100.0 / pixelWidth), (float)(p.Y * 100.0 / pixelHeight));
        }

        public Point PointPercentToPixels(PointF p, int pixelWidth, int pixelHeight)
        {
            return new Point((int)Math.Round(p.X / 100.0 * pixelWidth), (int)Math.Round(p.Y / 100.0 * pixelHeight));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.#},{1:0.#} -> {2:0.#},{3:0.#} ", Start.X, Start.Y, End.X, End.Y);
        }

        public Point GetStart(int width, int height)
        {
            return PointPercentToPixels(Start, width, height);
        }

        public Point GetEnd(int width, int height)
        {
            return PointPercentToPixels(End, width, height);
        }

        public List<Point> GetPoints(int width, int height)
        {
            return GetPoints(GetStart(width, height), GetEnd(width, height));
        }

        public static List<Point> GetPoints(Point start, Point end)
        {                       
            var list = new List<Point>();
            int x1 = start.X;
            int x2 = end.X;
            int y1 = start.Y;
            int y2 = end.Y;
            if (Math.Abs(start.X - end.X) > Math.Abs(start.Y - end.Y))
            {
                if (x1 > x2)
                {
                    x2 = start.X;
                    x1 = end.X;
                    y2 = start.Y;
                    y1 = end.Y;
                }
                double factor = (double) (y2-y1) /(x2-x1);
                for (int i=x1; i<=x2; i++)
                    list.Add(new Point(i, (int)Math.Round(y1 + factor * (i-x1))));
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
                double factor = (double) (x2 - x1) / (y2 - y1);
                for (int i = y1; i <= y2; i++)
                    list.Add(new Point((int)Math.Round(x1 + factor * (i-y1)), i));
            }
            return list;
        }

    }
}
