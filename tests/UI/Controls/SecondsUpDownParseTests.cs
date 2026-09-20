using System;
using System.Reflection;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Controls;

// In frame mode the duration field shows "seconds:frames", and typing the value without the
// colon (300 for 3:00) used to reset it to 0:00 - the masked start/end fields accept that form.
public class SecondsUpDownParseTests
{
    private static readonly MethodInfo ParseTime =
        typeof(SecondsUpDown).GetMethod("ParseTime", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static int ParseMs(string input, bool frameMode)
    {
        var oldFrameMode = Se.Settings.General.UseFrameMode;
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Se.Settings.General.UseFrameMode = frameMode;
            Configuration.Settings.General.CurrentFrameRate = 25;
            return (int)((TimeSpan)ParseTime.Invoke(null, new object?[] { input })!).TotalMilliseconds;
        }
        finally
        {
            Se.Settings.General.UseFrameMode = oldFrameMode;
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [AvaloniaTheory]
    [InlineData("3:00", 3000)]
    [InlineData("3:12", 3480)]
    [InlineData("300", 3000)]      // no colon - last two digits are frames
    [InlineData("312", 3480)]
    [InlineData("1200", 12000)]
    [InlineData("5", 5000)]        // one or two digits are whole seconds
    [InlineData("12", 12000)]
    [InlineData("012", 480)]       // ...but a leading zero means 0 seconds and 12 frames
    [InlineData("199", 1960)]      // 99 frames cannot fit at 25 fps - clamped to 24
    [InlineData("0", 0)]
    public void ParsesFrameMode(string input, int expectedMs)
    {
        Assert.Equal(expectedMs, ParseMs(input, true));
    }

    [AvaloniaTheory]
    [InlineData("3.000", 3000)]
    [InlineData("3,000", 3000)]
    [InlineData("300", 300000)]   // seconds mode - a bare number is seconds, as before
    public void ParsesSecondsMode(string input, int expectedMs)
    {
        Assert.Equal(expectedMs, ParseMs(input, false));
    }
}
