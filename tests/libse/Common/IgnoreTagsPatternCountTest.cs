using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

namespace Tests.Common;

/// <summary>
/// A client platform's own markup inside SRT/DFXP - &lt;=title&gt;, &lt;@&gt;, &lt;?note&gt; - is
/// not something SE strips on its own, so it counted as text (#16163). A user pattern names
/// those tags and the counters drop the matches, on top of the normal HTML/ASSA strip.
/// </summary>
public class IgnoreTagsPatternCountTest
{
    private const string ClientTags = "<[=?@][^<>]*>";

    [Theory]
    [InlineData("<=title>Main Title", "Main Title")]
    [InlineData("<@>Hello, <?note>there.", "Hello, there.")]
    [InlineData("{\\an8}<=tag>\r\n<i>Hello.</i>", "\r\nHello.")]      // ASSA and HTML tags go too
    [InlineData("2 < 3 is still text", "2 < 3 is still text")]        // a bare '<' never matches
    [InlineData("<b>bold</b> and <box>boxed</box>", "bold and boxed")] // the normal strip is unchanged
    [InlineData("", "")]
    public void RemoveTags_StripsPatternMatches(string input, string expected)
    {
        RunWithPattern(ClientTags, () => Assert.Equal(expected, CalcFactory.RemoveTags(input)));
    }

    [Fact]
    public void CountCharacters_FollowsThePattern()
    {
        const string text = "{\\an8}<=tag>\r\n<i>Hello.</i>";
        RunWithPattern(null, () => Assert.Equal(12, new CalcAll().CountCharacters(text, true)));
        RunWithPattern(ClientTags, () =>
        {
            Assert.Equal(6, new CalcAll().CountCharacters(text, true));
            Assert.Equal(6, new CalcNoSpace().CountCharacters(text, true));
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<[unclosed")] // invalid regex: ignored rather than thrown
    public void NoPattern_IsANoOp(string? pattern)
    {
        RunWithPattern(pattern, () =>
        {
            Assert.False(CalcFactory.HasIgnoreTagsPattern);
            Assert.Equal("<=title>Main Title", CalcFactory.RemoveTags("<=title>Main Title"));
        });
    }

    private static void RunWithPattern(string? pattern, Action assert)
    {
        try
        {
            CalcFactory.SetIgnoreTagsPattern(pattern!);
            assert();
        }
        finally
        {
            CalcFactory.SetIgnoreTagsPattern(null!);
        }
    }
}
