using System.Text.Json;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

public class JsonRepairTests
{
    [Fact]
    public void EscapesRawNewlineInsideString_SoItParses()
    {
        // Mirrors the qwen3-asr-cli output from issue #11717: a literal newline inside a string value.
        var bad = "{\n  \"text\": \"line one\nline two\",\n  \"words\": []\n}";

        var repaired = JsonRepair.EscapeControlCharsInStrings(bad);

        using var doc = JsonDocument.Parse(repaired); // would throw before the repair
        Assert.Equal("line one\nline two", doc.RootElement.GetProperty("text").GetString());
    }

    [Theory]
    [InlineData("\t", "\\t")]
    [InlineData("\r", "\\r")]
    [InlineData("\b", "\\b")]
    [InlineData("\f", "\\f")]
    [InlineData("", "\\u0001")]
    public void EscapesEachRawControlCharInsideString(string raw, string expectedEscape)
    {
        var bad = "{\"a\":\"x" + raw + "y\"}";

        var repaired = JsonRepair.EscapeControlCharsInStrings(bad);

        Assert.Contains("\"x" + expectedEscape + "y\"", repaired);
        using var doc = JsonDocument.Parse(repaired);
        Assert.Equal("x" + raw + "y", doc.RootElement.GetProperty("a").GetString());
    }

    [Fact]
    public void LeavesAlreadyEscapedSequencesUntouched()
    {
        // The backslash-n here is two characters (already escaped) and must not become \\n.
        var ok = "{\"a\":\"already\\nescaped\"}";

        var repaired = JsonRepair.EscapeControlCharsInStrings(ok);

        Assert.Equal(ok, repaired);
        using var doc = JsonDocument.Parse(repaired);
        Assert.Equal("already\nescaped", doc.RootElement.GetProperty("a").GetString());
    }

    [Fact]
    public void LeavesStructuralWhitespaceUntouched()
    {
        // Newlines/tabs BETWEEN tokens are valid JSON whitespace and must not be escaped.
        var pretty = "{\n\t\"a\": 1,\n\t\"b\": 2\n}";

        var repaired = JsonRepair.EscapeControlCharsInStrings(pretty);

        Assert.Equal(pretty, repaired);
    }

    [Fact]
    public void DoesNotTreatEscapedQuoteAsStringEnd()
    {
        // The \" is an escaped quote; the real string continues, so the raw newline after it
        // must still be escaped.
        var bad = "{\"a\":\"say \\\"hi\\\"\nthere\"}";

        var repaired = JsonRepair.EscapeControlCharsInStrings(bad);

        using var doc = JsonDocument.Parse(repaired);
        Assert.Equal("say \"hi\"\nthere", doc.RootElement.GetProperty("a").GetString());
    }
}
