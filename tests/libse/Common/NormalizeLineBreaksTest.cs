using Nikse.SubtitleEdit.Core.Common;
using System;
using Xunit;

namespace LibSETests.Common;

public class NormalizeLineBreaksTest
{
    // #13591: text from the outside (a paste from a LF file) must end up with the same line
    // break SE builds text with, or tools that rebuild the text report changes that render
    // identically.
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    [InlineData("\u2028")]
    public void AnyLineBreak_BecomesEnvironmentNewLine(string lineBreak)
    {
        var text = "I told him to get lost," + lineBreak + "and when he vanished with the light,";

        var result = text.NormalizeLineBreaks();

        Assert.Equal("I told him to get lost," + Environment.NewLine + "and when he vanished with the light,", result);
    }

    [Fact]
    public void CrCrLf_StaysTwoLineBreaks()
    {
        // "\r\r\n" is deliberately two breaks, same as SplitToLines (#8854).
        var result = "one\r\r\ntwo".NormalizeLineBreaks();

        Assert.Equal("one" + Environment.NewLine + Environment.NewLine + "two", result);
    }

    [Fact]
    public void KeepsEmptyLinesAndTrailingBreak()
    {
        var result = "one\n\ntwo\n".NormalizeLineBreaks();

        Assert.Equal($"one{Environment.NewLine}{Environment.NewLine}two{Environment.NewLine}", result);
    }

    [Fact]
    public void SameLinesAsSplitToLines()
    {
        const string text = "one\rtwo\r\nthree\n\u2028four\r\r\nfive";

        Assert.Equal(text.SplitToLines(), text.NormalizeLineBreaks().SplitToLines());
    }

    [Theory]
    [InlineData("")]
    [InlineData("no line break here")]
    public void TextWithoutLineBreak_IsReturnedAsIs(string text)
    {
        Assert.Same(text, text.NormalizeLineBreaks());
    }
}
