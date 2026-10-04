using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace UITests.Controls;

// "Frame numbers" time code mode (#15603): times are shown and typed as absolute frame numbers,
// so a sync offset is "video frame - start frame" with no hh:mm:ss:ff arithmetic.
public class FrameNumbersModeTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly SettingsScope _scope;
    private readonly long _videoOffset;
    private readonly double _frameRate;

    public FrameNumbersModeTests()
    {
        _scope = new SettingsScope("General.UseFrameMode", "General.UseFrameNumbersPersisted");
        _videoOffset = Se.Settings.General.CurrentVideoOffsetInMs;
        _frameRate = Configuration.Settings.General.CurrentFrameRate;

        Se.Settings.General.UseFrameMode = true;
        Se.Settings.General.UseFrameNumbersPersisted = true;
        Se.Settings.General.CurrentVideoOffsetInMs = 0;
        Configuration.Settings.General.CurrentFrameRate = 25;
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        Configuration.Settings.General.CurrentFrameRate = _frameRate;
        Se.Settings.General.CurrentVideoOffsetInMs = _videoOffset;
        _scope.Dispose();
    }

    private (Window window, TextBox textBox) Show(Control control)
    {
        var window = new Window { Content = control };
        _windows.Add(window);
        window.Show();
        var textBox = control.GetVisualDescendants().OfType<TextBox>().Single();
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();
        return (window, textBox);
    }

    [Fact]
    public void FrameNumbersNeedFrameMode()
    {
        Se.Settings.General.UseFrameMode = false;

        Assert.False(Se.Settings.General.UseFrameNumbers);
    }

    [Theory]
    [InlineData("15230", 609200)]
    [InlineData(" 0 ", 0)]
    [InlineData("-25", -1000)]
    public void ParsesFrameNumbers(string text, int expectedMs)
    {
        Assert.True(FrameNumbers.TryParse(text, out var time));
        Assert.Equal(expectedMs, time.TotalMilliseconds);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("12.5")]
    [InlineData("00:00:01:00")]
    [InlineData("999999999")] // past 99:59:59 - the millisecond conversion would overflow
    public void RejectsNonFrameNumbers(string text)
    {
        Assert.False(FrameNumbers.TryParse(text, out _));
    }

    [Fact]
    public void GridShowsStartAndDurationAsFrames()
    {
        var start = TimeCodeFullConvert(TimeSpan.FromMilliseconds(609200));
        var duration = TimeSpanToDisplayShortConverter.Instance.Convert(TimeSpan.FromMilliseconds(2480), typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("15230", start);
        Assert.Equal("62", duration);
        Assert.Equal(TimeSpan.FromMilliseconds(609200),
            TimeSpanToDisplayFullConverter.Instance.ConvertBack("15230", typeof(TimeSpan), null, CultureInfo.InvariantCulture));
    }

    private static object TimeCodeFullConvert(TimeSpan time) =>
        TimeSpanToDisplayFullConverter.Instance.Convert(time, typeof(string), null, CultureInfo.InvariantCulture);

    // 23.976 frames are not whole milliseconds and do not line up with the HH:MM:SS:FF grid, so
    // snapping must keep the frame number the user typed.
    [Fact]
    public void SnapKeepsTheTypedFrameNumberAt23976()
    {
        Configuration.Settings.General.CurrentFrameRate = 23.976;

        for (var frame = 14380; frame < 14420; frame++)
        {
            var time = TimeSpan.FromMilliseconds(FrameNumbers.ToMilliseconds(frame));
            Assert.Equal(frame, FrameNumbers.FromMilliseconds(FrameModeTimeSnapper.Snap(time).TotalMilliseconds));
        }
    }

    [AvaloniaFact]
    public void TimeCodeUpDownTypesAndStepsFrames()
    {
        var control = new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(609200) };
        var (window, textBox) = Show(control);
        Assert.Equal("15230", textBox.Text);

        textBox.SelectAll();
        window.KeyTextInput("16470");
        Assert.Equal("16470", textBox.Text);
        Assert.Equal(658800, control.Value.TotalMilliseconds);

        window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
        Assert.Equal("16471", textBox.Text);

        window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, null);
        Assert.Equal("1647", textBox.Text);
        Assert.Equal(65880, control.Value.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void SecondsUpDownShowsDurationAsFrameCount()
    {
        var control = new SecondsUpDown { Value = TimeSpan.FromMilliseconds(2480) };
        var (window, textBox) = Show(control);
        Assert.Equal("62", textBox.Text);

        window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
        Assert.Equal("63", textBox.Text);
        Assert.Equal(2520, control.Value.TotalMilliseconds);
    }
}
