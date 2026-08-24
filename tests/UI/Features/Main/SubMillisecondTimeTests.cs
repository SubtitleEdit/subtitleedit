using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Features.Main;

/// <summary>
/// Sub-millisecond time codes (issue #14056).
///
/// Since .NET Core 3.0 TimeSpan.FromSeconds/FromMilliseconds truncate to ticks instead of rounding
/// to whole milliseconds the way .NET Framework did, so TimeSpan.FromSeconds(0.82) is 819.9999 ms.
/// A value like that reads back as "0,820" in the duration up/down (which rounds) but as "0,819" in
/// the grid (which truncates, like every format writer does), and the end time it produces really
/// is a millisecond early. Subtitle formats store whole milliseconds, so every producer that turns
/// a fractional double (typed input, pixel positions, video positions, scale factors) into a
/// subtitle time rounds through TimeSpanExtensions.FromSeconds/FromMillisecondsWholeMilliseconds.
/// </summary>
public class SubMillisecondTimeTests
{
    private static SubtitleLineViewModel Line(double startMs, double endMs) =>
        new() { StartTime = TimeSpan.FromMilliseconds(startMs), EndTime = TimeSpan.FromMilliseconds(endMs) };

    // The reported case: Show 00:11:22,700 and a typed duration of 0,820 must hide at ,520 -
    // not ,519 - and the grid cell and the field must agree afterwards.
    [AvaloniaFact]
    public void TypingADurationEndsTheLineOnAWholeMillisecond()
    {
        using var _ = new SettingsScope("General.UseFrameMode");
        Se.Settings.General.UseFrameMode = false;

        var line = Line(682_700, 683_200);

        var upDown = new SecondsUpDown { DataContext = line };
        upDown[!SecondsUpDown.ValueProperty] =
            new Binding(nameof(SubtitleLineViewModel.Duration)) { Mode = BindingMode.TwoWay };
        var window = new Window { Content = upDown };
        window.Show();
        var textBox = upDown.GetVisualDescendants().OfType<TextBox>().Single();

        textBox.Focus();
        textBox.Text = "0,820";
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);

        Assert.Equal(820, line.Duration.TotalMilliseconds);
        Assert.Equal(683_520, line.EndTime.TotalMilliseconds);
        // Normalize the decimal separator - display strings follow the runner's culture.
        Assert.Equal("00:11:23.520", new TimeCode(line.EndTime).ToDisplayString().Replace(',', '.'));
        Assert.Equal("0.820", new TimeCode(line.Duration).ToShortString().Replace(',', '.'));
        Assert.Equal("0.820", textBox.Text!.Replace(',', '.'));
    }

    [Theory]
    [InlineData(0.82, 820)]
    [InlineData(1.13, 1130)]
    [InlineData(1.129, 1129)]
    [InlineData(0.0004, 0)]
    [InlineData(0.0005, 1)]
    [InlineData(-0.0005, -1)]
    [InlineData(-1.13, -1130)]
    public void SecondsAreRoundedToWholeMilliseconds(double seconds, double expectedMs)
    {
        Assert.Equal(expectedMs, TimeSpanExtensions.FromSecondsWholeMilliseconds(seconds).TotalMilliseconds);
    }

    // Out-of-range input is clamped to the time-code domain (±99:59:59,999) - still a whole
    // millisecond a TimeCode can hold - and NaN throws like TimeSpan.FromSeconds always has:
    // a NaN time is a bug at the call site and must not silently become 00:00:00,000.
    [Fact]
    public void OutOfRangeIsClampedToTheTimeCodeDomainAndNaNThrows()
    {
        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds,
            TimeSpanExtensions.FromSecondsWholeMilliseconds(1e30).TotalMilliseconds);
        Assert.Equal(-TimeCode.MaxTimeTotalMilliseconds,
            TimeSpanExtensions.FromSecondsWholeMilliseconds(-1e30).TotalMilliseconds);
        Assert.Throws<ArgumentException>(() => TimeSpanExtensions.FromSecondsWholeMilliseconds(double.NaN));
    }

    // Typing a huge duration must not corrupt the line: the parsed value is clamped to the
    // time-code domain instead of TimeSpan.MaxValue, so StartTime + Duration cannot overflow.
    [AvaloniaFact]
    public void TypingAHugeDurationClampsInsteadOfOverflowing()
    {
        using var _ = new SettingsScope("General.UseFrameMode");
        Se.Settings.General.UseFrameMode = false;

        var line = Line(682_700, 683_200);

        var upDown = new SecondsUpDown { DataContext = line };
        upDown[!SecondsUpDown.ValueProperty] =
            new Binding(nameof(SubtitleLineViewModel.Duration)) { Mode = BindingMode.TwoWay };
        var window = new Window { Content = upDown };
        window.Show();
        var textBox = upDown.GetVisualDescendants().OfType<TextBox>().Single();

        textBox.Focus();
        textBox.Text = "1e30";
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);

        Assert.Equal(TimeCode.MaxTimeTotalMilliseconds, line.Duration.TotalMilliseconds);
        Assert.Equal(0, line.EndTime.Ticks % TimeSpan.TicksPerMillisecond);
    }

    // Scaling rounds via start + scaled duration, not start and end independently, so lines of
    // equal length keep equal durations - independent rounding turned uniform 2000 ms lines into
    // a mix of 1666 and 1667 ms, flipping duration/CPS warnings on some rows and not others.
    [Fact]
    public void ChangeFrameRateKeepsEqualDurationsEqual()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < 5; i++)
        {
            subtitles.Add(Line(1000 + i * 3100, 1000 + i * 3100 + 2000));
        }

        ChangeFrameRateViewModel.ChangeFrameRate(subtitles, 25.0, 30.0);

        foreach (var line in subtitles)
        {
            Assert.Equal(0, line.StartTime.Ticks % TimeSpan.TicksPerMillisecond);
            Assert.Equal(1667, line.Duration.TotalMilliseconds); // round(2000 * 25 / 30)
        }
    }

    [Fact]
    public void AdjustKeepsEqualDurationsEqual()
    {
        var early = Line(1000, 3000);
        var late = Line(601_000, 603_000);

        early.Adjust(25.0 / 30.0, 0);
        late.Adjust(25.0 / 30.0, 0);

        Assert.Equal(early.Duration, late.Duration);
        Assert.Equal(1667, early.Duration.TotalMilliseconds);
        Assert.Equal(0, late.StartTime.Ticks % TimeSpan.TicksPerMillisecond);
    }
}
