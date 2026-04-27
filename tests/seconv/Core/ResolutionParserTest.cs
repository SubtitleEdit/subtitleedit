using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class ResolutionParserTest
{
    [Theory]
    [InlineData("1920x1080", 1920, 1080)]
    [InlineData("1280X720", 1280, 720)]
    [InlineData("640x480", 640, 480)]
    public void Parse_Valid(string input, int w, int h)
    {
        var (width, height) = ResolutionParser.Parse(input);
        Assert.Equal(w, width);
        Assert.Equal(h, height);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1920")]
    [InlineData("1920x")]
    [InlineData("x1080")]
    [InlineData("1920x1080x")]
    [InlineData("-1920x1080")]
    [InlineData("0x1080")]
    [InlineData("1920.0x1080")]
    public void Parse_Invalid_Throws(string input)
    {
        Assert.Throws<FormatException>(() => ResolutionParser.Parse(input));
    }
}
