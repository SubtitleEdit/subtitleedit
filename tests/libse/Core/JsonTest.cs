using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class JsonTest
{
    [Fact]
    public void TestUnicodeFirst()
    {
        var result = Json.DecodeJsonText("ב ");
        Assert.Equal("ב ", result);
    }

    [Fact]
    public void TestUnicodeLast()
    {
        var result = Json.DecodeJsonText(" ב");
        Assert.Equal(" ב", result);
    }

    [Fact]
    public void TestConsecutiveUnicodeEscapes()
    {
        // Raw JSON unicode escapes (as they appear in API responses like Baidu)
        // 对 = 对, 不 = 不, 起 = 起 → "对不起" (I'm sorry in Chinese)
        var input = "\\u5bf9\\u4e0d\\u8d77";
        var result = Json.DecodeJsonText(input);
        Assert.Equal("对不起", result);
    }

    [Fact]
    public void TestMixedUnicodeAndLiteralChars()
    {
        // Mixed: literal 对, escaped 不, literal 起 (as Baidu API may return)
        var input = "对\\u4e0d起";
        var result = Json.DecodeJsonText(input);
        Assert.Equal("对不起", result);
    }

    [Fact]
    public void TestEncodeTabIsEscaped()
    {
        // A literal tab is a JSON control character and must be escaped, otherwise the
        // request body is invalid JSON (see issue #11965).
        var result = Json.EncodeJsonText("a\tb");
        Assert.Equal("a\\tb", result);
    }

    [Fact]
    public void TestEncodeStrayNewLineIsNotRawControlChar()
    {
        // A bare \n (non-platform line ending) must become the placeholder, not a raw
        // control character that breaks the JSON body.
        var result = Json.EncodeJsonText("a\nb");
        Assert.Equal("a<br />b", result);
        Assert.DoesNotContain('\n', result);
    }

    [Fact]
    public void TestEncodeOtherControlCharIsUnicodeEscaped()
    {
        var result = Json.EncodeJsonText("ab");
        Assert.Equal("a\\u0001b", result);
    }

    [Fact]
    public void TestEncodeDecodeTabRoundTrip()
    {
        var result = Json.DecodeJsonText(Json.EncodeJsonText("a\tb"));
        Assert.Equal("a\tb", result);
    }
}
