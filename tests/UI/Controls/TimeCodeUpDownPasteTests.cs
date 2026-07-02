using System;
using System.Reflection;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

// Covers the paste parsing added for #12056: a bare number is milliseconds and replaces the
// whole time code; separator forms parse as (partial) time codes.
public class TimeCodeUpDownPasteTests
{
    private static readonly MethodInfo TryParse =
        typeof(TimeCodeUpDown).GetMethod("TryParsePastedValue", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static (bool ok, TimeSpan value) Parse(string? input)
    {
        var control = new TimeCodeUpDown();
        var args = new object?[] { input, null };
        var ok = (bool)TryParse.Invoke(control, args)!;
        return (ok, ok ? (TimeSpan)args[1]! : TimeSpan.Zero);
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
    [InlineData("-5")]           // negative not a valid time code
    [InlineData("5:5")]          // too few parts to be a time code
    [InlineData(null)]
    public void RejectsInvalidInput(string? input)
    {
        var (ok, _) = Parse(input);
        Assert.False(ok, $"expected '{input}' to be rejected");
    }
}
