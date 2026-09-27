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

    /// <summary>
    /// The last conversion is reused (every RTF based format converts the same document when a
    /// file is opened), so switching documents must never hand back the previous result.
    /// </summary>
    [Fact]
    public void RepeatedConversionsFollowTheInput()
    {
        var first = RichTextToPlainText.ConvertToRtf("First document");
        var second = RichTextToPlainText.ConvertToRtf("Second document");

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal("First document", RichTextToPlainText.ConvertToText(first).Trim());
            Assert.Equal("First document", RichTextToPlainText.ConvertToText(new string(first.ToCharArray())).Trim());
            Assert.Equal("Second document", RichTextToPlainText.ConvertToText(second).Trim());
        }
    }

}
