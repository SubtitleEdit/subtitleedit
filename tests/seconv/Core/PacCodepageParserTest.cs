using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class PacCodepageParserTest
{
    [Theory]
    [InlineData("Latin", Pac.CodePageLatin)]
    [InlineData("latin", Pac.CodePageLatin)]
    [InlineData("LATIN", Pac.CodePageLatin)]
    [InlineData("Greek", Pac.CodePageGreek)]
    [InlineData("Hebrew", Pac.CodePageHebrew)]
    [InlineData("Korean", Pac.CodePageKorean)]
    [InlineData("Turkish", Pac.CodePageLatinTurkish)]
    [InlineData("LatinTurkish", Pac.CodePageLatinTurkish)]
    [InlineData("Portuguese", Pac.CodePageLatinPortuguese)]
    [InlineData("0", Pac.CodePageLatin)]
    [InlineData("4", Pac.CodePageHebrew)]
    [InlineData("12", Pac.CodePageLatinPortuguese)]
    public void Parse_Valid(string input, int expected)
    {
        Assert.Equal(expected, PacCodepageParser.Parse(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("UnknownLanguage")]
    [InlineData("-1")]
    [InlineData("13")]
    [InlineData("100")]
    public void Parse_Invalid_Throws(string input)
    {
        Assert.Throws<FormatException>(() => PacCodepageParser.Parse(input));
    }
}
