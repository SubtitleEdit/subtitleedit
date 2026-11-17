using System.Reflection;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace UITests.Controls;

// Covers the paste parsing added for #12056: a bare number is milliseconds and replaces the
// whole time code; separator forms parse as (partial) time codes.
public class TimeCodeUpDownPasteTests
{
    private static readonly MethodInfo TryParse =
        typeof(TimeCodeUpDown).GetMethod("TryParsePastedValue", BindingFlags.NonPublic | BindingFlags.Instance)!;

    // The input limit is static (the main window sets it from the current format), so set the one
    // the test expects: the default format limit (99:59:59,999) unless given.
    private static (bool ok, TimeSpan value) Parse(string? input, SubtitleFormat? format = null)
    {
        var old = TimeCodeUpDown.MaxInputTotalMilliseconds;
        try
        {
            TimeCodeUpDown.MaxInputTotalMilliseconds = format?.MaxTimeCodeTotalMilliseconds ?? new MicroDvd().MaxTimeCodeTotalMilliseconds;
            var control = new TimeCodeUpDown();
            var args = new object?[] { input, null };
            var ok = (bool)TryParse.Invoke(control, args)!;
            return (ok, ok ? (TimeSpan)args[1]! : TimeSpan.Zero);
        }
        finally
        {
            TimeCodeUpDown.MaxInputTotalMilliseconds = old;
        }
    }

    [AvaloniaTheory]
    [InlineData("231", 231)]
    [InlineData("5500", 5500)]
    [InlineData(" 231 ", 231)]
    [InlineData("231\r\n", 231)]
    [InlineData("00:00:05,500", 5500)]
    [InlineData("00:00:05.500", 5500)]
    [InlineData("00:00:05:500", 5500)]
    [InlineData("01:02,300", 62300)]      // mm:ss,fff
    [InlineData("01:00:00,000", 3600000)]
    [InlineData("359999999", 359999999)]  // max in-range milliseconds by default (99:59:59,999)
    [InlineData("-5", -5)]                // negative time codes are legal - see #13695
    [InlineData("-00:00:05,500", -5500)]
    public void ParsesAcceptedInput(string input, int expectedMs)
    {
        var (ok, value) = Parse(input);
        Assert.True(ok, $"expected '{input}' to parse");
        Assert.Equal(expectedMs, (int)value.TotalMilliseconds);
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("-")]            // a sign on its own is not a time code
    [InlineData("5:5")]          // too few parts to be a time code
    [InlineData("360000000")]    // one ms past the default max range (99:59:59,999)
    [InlineData("123:04:05,678")] // past 99 hours: only formats with long time codes allow it
    [InlineData("99999999999999999")] // huge bare number: would overflow TimeSpan.FromMilliseconds
    [InlineData("999999999:0:0,0")]   // int-parseable but overflows the TimeSpan ctor
    [InlineData(null)]
    public void RejectsInvalidInput(string? input)
    {
        var (ok, _) = Parse(input);
        Assert.False(ok, $"expected '{input}' to be rejected");
    }

    // #16937: SubRip, WebVTT, TTML and ASSA/SSA allow time codes up to 9999:59:59,999; the main
    // window sets the limit from the current format.
    [AvaloniaTheory]
    [InlineData("35999999999", true, 35999999999)] // 9999:59:59,999
    [InlineData("123:04:05,678", true, 443045678)]
    [InlineData("1234:00:00,000", true, 4442400000)]
    [InlineData("36000000000", false, 0)]          // one ms past 9999:59:59,999
    [InlineData("10000:00:00,000", false, 0)]
    public void LongTimeCodesFollowTheFormatLimit(string input, bool expectedOk, long expectedMs)
    {
        var (ok, value) = Parse(input, new SubRip());
        Assert.Equal(expectedOk, ok);
        Assert.Equal(expectedMs, (long)value.TotalMilliseconds);
    }
}
