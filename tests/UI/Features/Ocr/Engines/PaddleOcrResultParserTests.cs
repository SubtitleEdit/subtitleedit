using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr.Engines;

public class PaddleOcrResultParserTests
{
    private readonly PaddleOcrResultParser _parser = new();

    private static string MakeLine(string pythonText, double confidence = 0.99) =>
        $"[[[100.0, 200.0], [300.0, 200.0], [300.0, 250.0], [100.0, 250.0]], ('{pythonText}', {confidence.ToString(System.Globalization.CultureInfo.InvariantCulture)})]";

    [Fact]
    public void Parse_NormalText_ReturnsText()
    {
        var result = _parser.Parse(MakeLine("Hello world"));
        Assert.Equal("Hello world", result.Text);
    }

    [Fact]
    public void Parse_TextWithEscapedApostrophe_UnescapesApostrophe()
    {
        // Python repr wraps in single quotes and escapes inner apostrophes when text starts with "
        var input = "[[[100.0, 200.0], [300.0, 200.0], [300.0, 250.0], [100.0, 250.0]], ('\" It\\'s raining...', 0.99)]";
        var result = _parser.Parse(input);
        Assert.Equal("\" It's raining...", result.Text);
    }

    [Fact]
    public void Parse_TextWithDoubleQuoteWrappedInSingleQuotes_ReturnsText()
    {
        // Python: 'He said "hello"'
        var input = "[[[100.0, 200.0], [300.0, 200.0], [300.0, 250.0], [100.0, 250.0]], ('He said \"hello\"', 0.99)]";
        var result = _parser.Parse(input);
        Assert.Equal("He said \"hello\"", result.Text);
    }

    [Fact]
    public void Parse_ConfidenceValue_ParsedCorrectly()
    {
        var result = _parser.Parse(MakeLine("Test", 0.9753));
        Assert.Equal(0.9753, result.Confidence, precision: 4);
    }

    [Fact]
    public void Parse_BoundingBox_ParsedCorrectly()
    {
        var result = _parser.Parse(MakeLine("Test"));
        Assert.Equal(100.0, result.BoundingBox.TopLeft.X);
        Assert.Equal(200.0, result.BoundingBox.TopLeft.Y);
        Assert.Equal(300.0, result.BoundingBox.TopRight.X);
        Assert.Equal(250.0, result.BoundingBox.BottomLeft.Y);
    }

    // Real output shape from the PaddleOCR 3.x Python CLI (--save_path -> "<n>_res.json").
    private const string PaddleOcr3Json = """
        {
            "input_path": "0001.png",
            "rec_texts": ["Hello world"],
            "rec_scores": [0.9967420101165771],
            "rec_polys": [[[12, 11], [203, 14], [203, 56], [12, 54]]],
            "rec_boxes": [[12, 11, 203, 56]]
        }
        """;

    [Fact]
    public void ParseJson3x_SingleLine_ReturnsTextScoreAndBox()
    {
        var results = PaddleOcr.ParsePaddleOcrJsonContent(PaddleOcr3Json);

        Assert.Single(results);
        Assert.Equal("Hello world", results[0].Text);
        Assert.Equal(0.9967, results[0].Confidence, precision: 4);
        Assert.Equal(12, results[0].BoundingBox.TopLeft.X);
        Assert.Equal(11, results[0].BoundingBox.TopLeft.Y);
        Assert.Equal(203, results[0].BoundingBox.TopRight.X);
    }

    [Fact]
    public void ParseJson3x_TwoLines_ParsesBoth()
    {
        const string json = """
            {
                "rec_texts": ["First line", "Second line"],
                "rec_scores": [0.99, 0.98],
                "rec_polys": [
                    [[10, 10], [200, 10], [200, 40], [10, 40]],
                    [[10, 50], [200, 50], [200, 80], [10, 80]]
                ]
            }
            """;

        var results = PaddleOcr.ParsePaddleOcrJsonContent(json);

        Assert.Equal(2, results.Count);
        Assert.Equal("First line", results[0].Text);
        Assert.Equal("Second line", results[1].Text);
        Assert.Equal(50, results[1].BoundingBox.TopLeft.Y);
    }

    [Fact]
    public void ParseJson3x_NoText_ReturnsEmpty()
    {
        var results = PaddleOcr.ParsePaddleOcrJsonContent("""{ "input_path": "0001.png" }""");
        Assert.Empty(results);
    }
}
