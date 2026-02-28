using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class RichTextToPlainTextTest
{
    [Fact]
    public void TestConvertToRtfSlash()
    {
        var result = RichTextToPlainText.ConvertToRtf(@"Brian\Benny!");
        Assert.Contains(@"Brian\\Benny!", result);
        result = RichTextToPlainText.ConvertToText(result);
        Assert.True(result.Trim() == @"Brian\Benny!");
    }

    [Fact]
    public void TestConvertToRtfCurlyBracketStart()
    {
        var result = RichTextToPlainText.ConvertToRtf(@"Brian{Benny!");
        Assert.Contains(@"Brian\{Benny!", result);
        result = RichTextToPlainText.ConvertToText(result);
        Assert.True(result.Trim() == @"Brian{Benny!");
    }

    [Fact]
    public void TestConvertToRtfCurlyBracketEnd()
    {
        var result = RichTextToPlainText.ConvertToRtf(@"Brian}Benny!");
        Assert.Contains(@"Brian\}Benny!", result);
        result = RichTextToPlainText.ConvertToText(result);
        Assert.True(result.Trim() == @"Brian}Benny!");
    }

}
