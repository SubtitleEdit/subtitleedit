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
}
