using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace LibSETests.Common;

public class UtilitiesTest
{
    // DisplayFileSizeToBytes used to cast each result to int, so any value >= 2 GB
    // (and large mb inputs) overflowed to a negative/garbage number. The method
    // returns long, so the result must survive past int.MaxValue.

    [Fact]
    public void DisplayFileSizeToBytes_Gigabytes_DoesNotOverflow()
    {
        Assert.Equal(3221225472L, Utilities.DisplayFileSizeToBytes("3 gb"));
    }

    [Fact]
    public void DisplayFileSizeToBytes_LargeMegabytes_DoesNotOverflow()
    {
        // 2048 mb = 2147483648 bytes, exactly one more than int.MaxValue.
        Assert.Equal(2147483648L, Utilities.DisplayFileSizeToBytes("2048 mb"));
    }

    [Fact]
    public void DisplayFileSizeToBytes_Kilobytes_RoundTrips()
    {
        Assert.Equal(2048L, Utilities.DisplayFileSizeToBytes("2 kb"));
    }

    // A second line fully wrapped in music symbols (♪ ... ♪) must not be auto-broken.
    // A copy/paste bug used to test line 0's ending instead of line 1's, so this case
    // slipped through and the line got merged/re-broken.
    [Fact]
    public void AutoBreakLine_KeepsSecondLineWrappedInMusicSymbols()
    {
        var input = "♪ La la la" + Environment.NewLine + "♪ da da ♪";

        var result = Utilities.AutoBreakLinePrivate(input, 43, 100, string.Empty, false);

        Assert.Equal(input, result);
    }

    // GetColorFromFontString measured colorStart relative to the extracted <font> tag
    // but indexed the full string, so a tag that wasn't at the start of the line read
    // the wrong region and returned the default color.
    [Fact]
    public void GetColorFromFontString_WithLeadingText_ReturnsTagColor()
    {
        var result = Utilities.GetColorFromFontString("Hi <font color=\"#FF0000\">x</font>", SKColors.Blue);

        Assert.Equal(new SKColor(255, 0, 0), result);
    }
}
