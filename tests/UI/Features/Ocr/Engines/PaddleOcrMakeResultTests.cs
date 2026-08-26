using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr.Engines;

public class PaddleOcrMakeResultTests
{
    private static PaddleOcrResultParser.TextDetectionResult Box(string text, double confidence, double x, double y, double width = 100, double height = 30)
    {
        return new PaddleOcrResultParser.TextDetectionResult
        {
            Text = text,
            Confidence = confidence,
            BoundingBox = new PaddleOcrResultParser.BoundingBox(
                new PaddleOcrResultParser.Point(x, y),
                new PaddleOcrResultParser.Point(x + width, y),
                new PaddleOcrResultParser.Point(x + width, y + height),
                new PaddleOcrResultParser.Point(x, y + height)),
        };
    }

    [Fact]
    public void MakeResult_NoThreshold_KeepsEverything()
    {
        var ocr = new PaddleOcr();
        var text = ocr.MakeResult(new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("Hello", 0.99, 10, 10),
            Box("junk", 0.30, 400, 10),
        }, out var confidence);

        Assert.Equal("Hello junk", text);
        Assert.Equal((0.99 + 0.30) / 2, confidence, 3);
    }

    [Fact]
    public void MakeResult_Threshold_DropsLowConfidenceRegions()
    {
        var ocr = new PaddleOcr { MinConfidencePercent = 75 };
        var text = ocr.MakeResult(new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("Hello", 0.99, 10, 10),
            Box("junk", 0.30, 400, 10),
        }, out var confidence);

        Assert.Equal("Hello", text);
        Assert.Equal(0.99, confidence, 3);
    }

    [Fact]
    public void MakeResult_Threshold_KeepsUnknownConfidence()
    {
        // Confidence 0 means "not reported" (older output formats) - never drop those.
        var ocr = new PaddleOcr { MinConfidencePercent = 75 };
        var text = ocr.MakeResult(new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("Hello", 0, 10, 10),
        }, out var confidence);

        Assert.Equal("Hello", text);
        Assert.Equal(1.0, confidence, 3);
    }

    [Fact]
    public void MakeResult_AllBelowThreshold_EmptyTextZeroConfidence()
    {
        var ocr = new PaddleOcr { MinConfidencePercent = 75 };
        var text = ocr.MakeResult(new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("junk", 0.30, 10, 10),
        }, out var confidence);

        Assert.Equal(string.Empty, text);
        Assert.Equal(0, confidence, 3);
    }

    [Fact]
    public void MakeLines_VerticalOverlap_GroupsWordsOnSameLine()
    {
        // Two words with slightly different Y but overlapping vertically, plus a second line.
        var input = new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("world", 0.9, 130, 14),
            Box("Second", 0.9, 10, 50),
            Box("Hello", 0.9, 10, 10),
        };

        var lines = PaddleOcr.MakeLines(input, rightToLeft: false);

        Assert.Equal(2, lines.Count);
        Assert.Equal(new[] { "Hello", "world" }, lines[0].Select(p => p.Text));
        Assert.Equal(new[] { "Second" }, lines[1].Select(p => p.Text));
    }

    [Fact]
    public void MakeLines_TallAndShortBoxes_StillOneLine()
    {
        // A short box (no descenders) next to a taller one - midpoint overlap holds; the
        // old "average height" rule split these when heights varied.
        var input = new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("nano", 0.9, 130, 18, height: 16),
            Box("Big", 0.9, 10, 10, height: 34),
        };

        var lines = PaddleOcr.MakeLines(input, rightToLeft: false);

        Assert.Single(lines);
        Assert.Equal(new[] { "Big", "nano" }, lines[0].Select(p => p.Text));
    }

    [Fact]
    public void MakeLines_RightToLeft_OrdersWordsRightFirst()
    {
        var input = new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("left", 0.9, 10, 10),
            Box("right", 0.9, 300, 10),
        };

        var lines = PaddleOcr.MakeLines(input, rightToLeft: true);

        Assert.Single(lines);
        Assert.Equal(new[] { "right", "left" }, lines[0].Select(p => p.Text));
    }

    [Fact]
    public void MakeLines_LinesSortedTopToBottom()
    {
        var input = new List<PaddleOcrResultParser.TextDetectionResult>
        {
            Box("Bottom", 0.9, 10, 90),
            Box("Top", 0.9, 10, 10),
            Box("Middle", 0.9, 10, 50),
        };

        var lines = PaddleOcr.MakeLines(input, rightToLeft: false);

        Assert.Equal(3, lines.Count);
        Assert.Equal("Top", lines[0][0].Text);
        Assert.Equal("Middle", lines[1][0].Text);
        Assert.Equal("Bottom", lines[2][0].Text);
    }
}
