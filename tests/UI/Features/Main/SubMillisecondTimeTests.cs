using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace UITests.Features.Main;

/// <summary>
/// Sub-millisecond time codes (issue #14056).
///
/// Since .NET Core 3.0 TimeSpan.FromSeconds/FromMilliseconds truncate to ticks instead of rounding
/// to whole milliseconds the way .NET Framework did, so TimeSpan.FromSeconds(0.82) is 819.9999 ms.
/// A value like that reads back as "0,820" in the duration up/down (which rounds) but as "0,819" in
/// the grid (which truncates, like every format writer does), and the end time it produces really
/// is a millisecond early. Subtitle times are whole milliseconds, so they are snapped on the way in.
/// </summary>
public class SubMillisecondTimeTests
{
    private static string GridText(TimeSpan ts) => new TimeCode(ts).ToShortString();

    private static string FieldText(TimeSpan ts) => ts.TotalSeconds.ToString("0.000");

    private static SubtitleLineViewModel Line(double startMs, double endMs) =>
        new() { StartTime = TimeSpan.FromMilliseconds(startMs), EndTime = TimeSpan.FromMilliseconds(endMs) };

    // The reported case: Show 00:11:22,700 and a typed duration of 0,820 must hide at ,520.
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
        Assert.Equal("00:11:23,520", new TimeCode(line.EndTime).ToDisplayString());
        Assert.Equal("0,820", GridText(line.Duration));
        Assert.Equal("0,820", textBox.Text);
    }

    // The other reported case: a waveform drag lands the cues on fractional milliseconds, so the
    // grid (truncating) and the duration up/down (rounding) disagreed by one millisecond.
    [Theory]
    [InlineData(589_780.6, 590_910.3)] // pixel-derived, as a waveform drag produces
    [InlineData(682_700.0, 683_519.9999)] // tick-truncated, as TimeSpan.FromSeconds produces
    [InlineData(1_000.4, 2_000.5)]
    [InlineData(1_000.5, 2_000.6)]
    public void TheGridAndTheDurationFieldNeverDisagree(double startMs, double endMs)
    {
        using var _ = new SettingsScope("General.UseFrameMode");
        Se.Settings.General.UseFrameMode = false;

        var line = Line(startMs, endMs);

        Assert.Equal(0, line.StartTime.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(0, line.EndTime.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(0, line.Duration.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(GridText(line.Duration), FieldText(line.Duration));
        Assert.Equal(line.EndTime - line.StartTime, line.Duration);
    }

    // Nudging Show must not leave the duration reading one value in the grid and another in the
    // field - the reporter's follow-up test, which moved both cues but kept the bad span.
    [AvaloniaFact]
    public void MovingALineKeepsAWholeMillisecondDuration()
    {
        var line = Line(682_700, 683_520);

        // A pixel-derived waveform drag, i.e. an arbitrary fraction of a millisecond.
        line.SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(682_700.6));

        Assert.Equal(682_701, line.StartTime.TotalMilliseconds);
        Assert.Equal(683_521, line.EndTime.TotalMilliseconds);
        Assert.Equal(820, line.Duration.TotalMilliseconds);
        Assert.Equal(GridText(line.Duration), FieldText(line.Duration));
    }

    [Theory]
    [InlineData(0.82, 820)]
    [InlineData(1.13, 1130)]
    [InlineData(1.129, 1129)]
    [InlineData(0.0004, 0)]
    [InlineData(0.0005, 1)]
    [InlineData(-0.0005, -1)]
    [InlineData(-1.13, -1130)]
    public void SecondsAreSnappedToWholeMilliseconds(double seconds, double expectedMs)
    {
        var snapped = TimeSpanExtensions.FromSecondsWholeMilliseconds(seconds);

        Assert.Equal(expectedMs, snapped.TotalMilliseconds);
        Assert.Equal(0, snapped.Ticks % TimeSpan.TicksPerMillisecond);
    }

    // The extremes have no whole millisecond to round to - they must stay put, not wrap around.
    [Fact]
    public void SnappingTheExtremesDoesNotWrapAround()
    {
        Assert.Equal(TimeSpan.MaxValue, TimeSpan.MaxValue.SnapToWholeMilliseconds());
        Assert.Equal(TimeSpan.MinValue, TimeSpan.MinValue.SnapToWholeMilliseconds());
        Assert.Equal(TimeSpan.MaxValue, TimeSpanExtensions.FromSecondsWholeMilliseconds(double.MaxValue));
        Assert.Equal(TimeSpan.MinValue, TimeSpanExtensions.FromSecondsWholeMilliseconds(double.MinValue));
        Assert.Equal(TimeSpan.Zero, TimeSpanExtensions.FromSecondsWholeMilliseconds(double.NaN));
    }

    [Fact]
    public void SnappingIsAlreadyWholeMillisecondsIdempotent()
    {
        var whole = TimeSpan.FromMilliseconds(1130);

        Assert.Equal(whole, whole.SnapToWholeMilliseconds());
        Assert.Equal(TimeSpan.Zero, TimeSpan.Zero.SnapToWholeMilliseconds());
        Assert.Equal(whole, TimeSpan.FromSeconds(1.13).SnapToWholeMilliseconds());
    }
}
