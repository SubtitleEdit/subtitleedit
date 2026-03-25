using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class JsonTest
{
    [Fact]
    public void TestUnicodeFirst()
    {
        var result = Json.DecodeJsonText("\u05d1 ");
        Assert.Equal("ב ", result);
    }

    [Fact]
    public void TestUnicodeLast()
    {
        var result = Json.DecodeJsonText(" \u05d1");
        Assert.Equal(" ב", result);
    }

    [Fact]
    public void TestConsecutiveUnicodeEscapes()
    {
        // Raw JSON unicode escapes (as they appear in API responses like Baidu)
        // \u5bf9 = 对, \u4e0d = 不, \u8d77 = 起 → "对不起" (I'm sorry in Chinese)
        var input = "\\u5bf9\\u4e0d\\u8d77";
        var result = Json.DecodeJsonText(input);
        Assert.Equal("\u5bf9\u4e0d\u8d77", result);
    }

    [Fact]
    public void TestMixedUnicodeAndLiteralChars()
    {
        // Mixed: literal 对, escaped 不, literal 起 (as Baidu API may return)
        var input = "\u5bf9\\u4e0d\u8d77";
        var result = Json.DecodeJsonText(input);
        Assert.Equal("\u5bf9\u4e0d\u8d77", result);
    }
}
