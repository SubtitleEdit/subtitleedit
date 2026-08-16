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
    public void FixesCommaDecimalSeparators_SoItParses()
    {
        // Mirrors qwen3-asr-cli v0.1.6/v0.1.7 output on French/German Windows: the UTF-8 console
        // setup leaked the user's LC_NUMERIC into the "%.3f" timestamp formatting.
        var bad = "{\n  \"words\": [\n    {\"word\": \"な\", \"start\": 0,152, \"end\": 1,840},\n    {\"word\": \"んか\", \"start\": 2,000, \"end\": 29,999}\n  ]\n}";

        var repaired = JsonRepair.FixCommaDecimalSeparators(bad);

        using var doc = JsonDocument.Parse(repaired); // would throw before the repair
        var words = doc.RootElement.GetProperty("words");
        Assert.Equal(0.152, words[0].GetProperty("start").GetDouble());
        Assert.Equal(1.840, words[0].GetProperty("end").GetDouble());
        Assert.Equal(29.999, words[1].GetProperty("end").GetDouble());
    }

    [Fact]
    public void FixCommaDecimalSeparators_LeavesValidJsonUntouched()
    {
        // Structural commas (followed by whitespace/quote) and commas inside strings must survive.
        var ok = "{\"words\": [{\"word\": \"1,5 litres\", \"start\": 0.100, \"end\": 1.500}, {\"word\": \"b\", \"start\": 2.000, \"end\": 3.000}]}";

        var repaired = JsonRepair.FixCommaDecimalSeparators(ok);

        Assert.Equal(ok, repaired);
        using var doc = JsonDocument.Parse(repaired);
        Assert.Equal("1,5 litres", doc.RootElement.GetProperty("words")[0].GetProperty("word").GetString());
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
