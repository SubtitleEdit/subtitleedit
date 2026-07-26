using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// GetMaxLineLength walks line breaks directly instead of splitting into a list, so it has to
/// agree with SplitToLines on every break form it recognises - including a lone \r, a U+2028 line separator, and
/// \r\n counting as one break.
/// </summary>
public class UtilitiesLineLengthTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("Hello", 5)]
    [InlineData("Hello\r\nworld!", 6)]
    [InlineData("Hello\nworld!", 6)]
    [InlineData("Hello\rworld!", 6)]
    [InlineData("Hello\u2028world!", 6)]
    [InlineData("short\r\nlonger line", 11)]
    [InlineData("longer line\r\nshort", 11)]
    [InlineData("a\r\n\r\nbbbb", 4)]
    [InlineData("trailing\r\n", 8)]
    [InlineData("\r\nleading", 7)]
    [InlineData("one\r\ntwo\r\nthree", 5)]
    public void GetMaxLineLength_MatchesLongestLine(string text, int expected)
    {
        Assert.Equal(expected, Utilities.GetMaxLineLength(text));
    }

    [Theory]
    [InlineData("<i>Hello</i>", 5)]
    [InlineData("<i>Hello</i>\r\n<i>bigger line</i>", 11)]
    [InlineData("{\\an8}Hello", 5)]
    public void GetMaxLineLength_IgnoresTags(string text, int expected)
    {
        Assert.Equal(expected, Utilities.GetMaxLineLength(text));
    }

    /// <summary>The rewrite must agree with the list-building version it replaced.</summary>
    [Theory]
    [InlineData("Hello\r\nworld!")]
    [InlineData("a\rb\nc\u2028d")]
    [InlineData("<i>tagged</i>\r\nplain")]
    [InlineData("trailing break\r\n")]
    [InlineData("")]
    [InlineData("no breaks at all")]
    public void GetMaxLineLength_AgreesWithSplitToLines(string text)
    {
        var viaSplit = 0;
        foreach (var line in HtmlUtil.RemoveHtmlTags(text, true).SplitToLines())
        {
            if (line.Length > viaSplit)
            {
                viaSplit = line.Length;
            }
        }

        Assert.Equal(viaSplit, Utilities.GetMaxLineLength(text));
    }
}
