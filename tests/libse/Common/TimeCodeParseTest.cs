using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class TimeCodeParseTest
{
    [Theory]
    [InlineData("0:0:1:5", 1500)]     // 4-part h:m:s:ms, "5" pads to "500"
    [InlineData("00:00:01,000", 1000)]
    [InlineData("00:00:01,5", 1500)]  // "5" -> "500"
    [InlineData("00:00:01,50", 1500)] // "50" -> "500"
    [InlineData("00:00:01,123", 1123)]
    public void ParseToMillisecondsPadsFraction(string input, double expected)
    {
        Assert.Equal(expected, TimeCode.ParseToMilliseconds(input));
    }

    [Fact]
    public void WhitespaceOnlyFractionMatchesLegacyPadRight()
    {
        // Regression: the legacy PadRight(3,'0') + int.Parse turned a whitespace-only fraction
        // " " into " 00", which int.Parse reads as 0, so the seconds still count. A hand-rolled
        // "trailing whitespace fails" shortcut got this wrong and returned 0 for the whole code.
        Assert.Equal(1000, TimeCode.ParseToMilliseconds("0:0:1: "));
    }

    [Fact]
    public void DigitThenSpaceFractionFails()
    {
        // "5 " pads to "5 0" (embedded whitespace) which int.Parse rejects, so the whole parse
        // fails and returns 0 - unchanged from the legacy behavior.
        Assert.Equal(0, TimeCode.ParseToMilliseconds("0:0:1:5 "));
    }
}
