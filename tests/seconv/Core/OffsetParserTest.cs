using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class OffsetParserTest
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("500", 500)]
    [InlineData("1500", 1500)]
    [InlineData("-1500", -1500)]
    [InlineData("+1500", 1500)]
    [InlineData("--500", 500)]   // double negative cancels
    [InlineData("+-500", -500)]
    public void Parse_PlainMilliseconds(string input, long expectedMs)
    {
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMs), OffsetParser.Parse(input));
    }

    [Theory]
    [InlineData("00:00:01:500", 1500)]    // hh:mm:ss:ms = 1s + 500ms
    [InlineData("00:00:01.500", 1500)]    // period as separator (NOT decimal)
    [InlineData("00:00:01,500", 1500)]    // comma as separator
    [InlineData("01:30:00", 90_000)]      // mm:ss:ms = 1m + 30s + 0ms = 90s
    [InlineData("0:0:1:0", 1000)]         // hh:mm:ss:ms with zero ms
    [InlineData("30:500", 30_500)]        // ss:ms = 30s + 500ms
    public void Parse_MultiPart(string input, long expectedMs)
    {
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMs), OffsetParser.Parse(input));
    }

    [Fact]
    public void Parse_FullHmsMs_ComputesTotal()
    {
        // hh:mm:ss:ms = 1h 2m 3s 400ms
        var expected = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(400);
        Assert.Equal(expected, OffsetParser.Parse("01:02:03:400"));
    }

    [Fact]
    public void Parse_NegativeFullHms_Negates()
    {
        var expected = -(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(30));
        Assert.Equal(expected, OffsetParser.Parse("-01:30:00"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1:2:3:4:5")]
    [InlineData("-")]
    public void Parse_Invalid_Throws(string input)
    {
        Assert.Throws<FormatException>(() => OffsetParser.Parse(input));
    }
}
