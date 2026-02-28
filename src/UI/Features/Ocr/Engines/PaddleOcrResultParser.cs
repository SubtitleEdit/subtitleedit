using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PaddleOcrResultParser
{
    public class BoundingBox
    {
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomRight { get; set; }
        public Point BottomLeft { get; set; }

        public BoundingBox(Point topLeft, Point topRight, Point bottomRight, Point bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public double Width => Math.Max(
            Math.Abs(TopRight.X - TopLeft.X),
            Math.Abs(BottomRight.X - BottomLeft.X)
        );

        public double Height => Math.Max(
            Math.Abs(BottomLeft.Y - TopLeft.Y),
            Math.Abs(BottomRight.Y - TopRight.Y)
        );

        public Point Center => new Point(
            (TopLeft.X + TopRight.X + BottomRight.X + BottomLeft.X) / 4,
            (TopLeft.Y + TopRight.Y + BottomRight.Y + BottomLeft.Y) / 4
        );
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class TextDetectionResult
    {
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; } = new BoundingBox(
            topLeft: new Point(0, 0),
            topRight: new Point(0, 0),
            bottomRight: new Point(0, 0),
            bottomLeft: new Point(0, 0)
        );
    }


    public TextDetectionResult Parse(string input)
    {
        // Extract text using regex
        var textMatch = Regex.Match(input, @"\([""'](.*)[""'],");
        var text = textMatch.Groups[1].Value;

        // Extract confidence using regex
        input = input.Replace(" ", string.Empty);
        var confidenceMatch = Regex.Match(input, @"(\d+\.\d+)\)\]$");
        var confidence = double.Parse(confidenceMatch.Groups[1].Value, CultureInfo.InvariantCulture);

        // Extract coordinates using regex
        var coordMatches = Regex.Matches(input, @"\[(\d+\.\d+),(\d+\.\d+)\]");

        var points = new Point[4];
        for (var i = 0; i < 4; i++)
        {
            var match = coordMatches[i];
            points[i] = new Point(
                double.Parse(match.Groups[1].Value),
                double.Parse(match.Groups[2].Value)
            );
        }

        var boundingBox = new BoundingBox(
            topLeft: points[0],
            topRight: points[1],
            bottomRight: points[2],
            bottomLeft: points[3]
        );

        return new TextDetectionResult
        {
            Text = text,
            Confidence = confidence,
            BoundingBox = boundingBox
        };
    }
}

