using System.Reflection;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.Translate;

public class GoogleTranslateV1Tests
{
    // ConvertJsonObjectToStringLines is private; invoke it directly so the response
    // parser can be tested without a live network call.
    private static List<string> ConvertJsonObjectToStringLines(string json)
    {
        var method = typeof(GoogleTranslateV1).GetMethod(
            "ConvertJsonObjectToStringLines",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return (List<string>)method.Invoke(null, new object[] { json })!;
    }

    [Fact]
    public void SegmentWithTrailingEscapedNewline_DoesNotProduceBlankLine()
    {
        // Google returns a two-line dialogue cue as two segments; the first segment keeps
        // the source line break as an escaped trailing "\n". Before the fix for #13614 the
        // trailing newline survived and AppendLine doubled it into a blank middle line.
        var json = "[[[\"- Ne.\\n\",\"- No.\\n\",null,null,3],[\"- Přijdeme pozdě.\",\"- We're gonna be late.\",null,null,3]],null,\"en\"]";

        var lines = ConvertJsonObjectToStringLines(json);

        Assert.Equal(new[] { "- Ne.", "- Přijdeme pozdě." }, lines);
        Assert.DoesNotContain(lines, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public void SegmentWithTrailingEscapedCrLf_DoesNotProduceBlankLine()
    {
        var json = "[[[\"- Ne.\\r\\n\",\"- No.\\r\\n\",null,null,3],[\"- Přijdeme pozdě.\",\"- We're gonna be late.\",null,null,3]],null,\"en\"]";

        var lines = ConvertJsonObjectToStringLines(json);

        Assert.Equal(new[] { "- Ne.", "- Přijdeme pozdě." }, lines);
    }

    [Fact]
    public void SingleSegment_IsReturnedUnchanged()
    {
        var json = "[[[\"Ahoj.\",\"Hello.\",null,null,3]],null,\"en\"]";

        var lines = ConvertJsonObjectToStringLines(json);

        Assert.Equal(new[] { "Ahoj." }, lines);
    }

    // The clients5.google.com dict-chrome-ex fallback (issue #14015) answers a much simpler
    // shape than gtx: a flat array of translated strings, or [text, detected-language] pairs
    // when the source language is "auto".

    [Fact]
    public void DictChromeEx_SingleLine()
    {
        Assert.Equal("Hej verden", GoogleTranslateV1.ConvertDictChromeExResultToText("[\"Hej verden\"]"));
    }

    [Fact]
    public void DictChromeEx_EscapedNewlineBecomesLineBreak()
    {
        var text = GoogleTranslateV1.ConvertDictChromeExResultToText("[\"Hej verden\\nHvordan har du det?\"]");

        Assert.Equal("Hej verden" + Environment.NewLine + "Hvordan har du det?", text);
    }

    [Fact]
    public void DictChromeEx_AutoDetectPairShape_TakesTranslationOnly()
    {
        Assert.Equal("Bonjour", GoogleTranslateV1.ConvertDictChromeExResultToText("[[\"Bonjour\",\"en\"]]"));
    }

    [Fact]
    public void DictChromeEx_UnicodeEscape_IsDecoded()
    {
        Assert.Equal("café", GoogleTranslateV1.ConvertDictChromeExResultToText("[\"caf\\u00e9\"]"));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("")]
    [InlineData("<html><title>Sorry...</title></html>")]
    public void DictChromeEx_NoTranslation_ReturnsNull(string json)
    {
        Assert.Null(GoogleTranslateV1.ConvertDictChromeExResultToText(json));
    }

    [Fact]
    public void DictChromeEx_TranslationEndingInQuote_KeepsTheQuote()
    {
        // "He said \"hi\"" - trimming every trailing quote char used to leave a dangling
        // backslash, making Regex.Unescape throw and shipping the raw escapes.
        Assert.Equal("He said \"hi\"", GoogleTranslateV1.ConvertDictChromeExResultToText("[\"He said \\\"hi\\\"\"]"));
    }

    [Fact]
    public void DictChromeEx_TranslationThatIsOnlyAQuotedWord_KeepsBothQuotes()
    {
        Assert.Equal("\"Bonjour\"", GoogleTranslateV1.ConvertDictChromeExResultToText("[\"\\\"Bonjour\\\"\"]"));
    }
}
