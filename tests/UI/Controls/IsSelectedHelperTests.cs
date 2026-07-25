using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace UITests.Controls;

/// <summary>
/// The waveform asks this helper once per horizontal pixel whether the sample under that pixel
/// belongs to a selected line. Reset only loads the ranges that intersect the window about to be
/// drawn, which is what keeps a "select all" from turning every waveform rebuild into
/// O(pixels x selection) - so these tests check that the filtering never changes an answer for a
/// position inside the window.
/// </summary>
public class IsSelectedHelperTests
{
    private const int SampleRate = 100;

    private static SubtitleLineViewModel Line(double startSeconds, double endSeconds) => new()
    {
        Text = "text",
        StartTime = TimeSpan.FromSeconds(startSeconds),
        EndTime = TimeSpan.FromSeconds(endSeconds),
    };

    [Fact]
    public void IsSelected_InsideAndOutsideARange()
    {
        var helper = new IsSelectedHelper();
        var selection = new List<SubtitleLineViewModel> { Line(10, 12) };

        helper.Reset(selection, SampleRate, 5 * SampleRate, 20 * SampleRate);

        Assert.False(helper.IsSelected((int)(9.5 * SampleRate)));
        Assert.True(helper.IsSelected(10 * SampleRate));
        Assert.True(helper.IsSelected(11 * SampleRate));
        Assert.True(helper.IsSelected(12 * SampleRate));
        Assert.False(helper.IsSelected((int)(12.5 * SampleRate)));
    }

    [Fact]
    public void IsSelected_WalksMultipleRangesInOnePass()
    {
        var helper = new IsSelectedHelper();
        var selection = new List<SubtitleLineViewModel> { Line(10, 11), Line(13, 14), Line(16, 17) };

        helper.Reset(selection, SampleRate, 5 * SampleRate, 20 * SampleRate);

        Assert.True(helper.IsSelected((int)(10.5 * SampleRate)));
        Assert.False(helper.IsSelected(12 * SampleRate));
        Assert.True(helper.IsSelected((int)(13.5 * SampleRate)));
        Assert.False(helper.IsSelected(15 * SampleRate));
        Assert.True(helper.IsSelected((int)(16.5 * SampleRate)));
        Assert.False(helper.IsSelected(18 * SampleRate));
    }

    [Fact]
    public void IsSelected_IgnoresSelectionOutsideTheWindow()
    {
        var helper = new IsSelectedHelper();
        var selection = new List<SubtitleLineViewModel>
        {
            Line(0, 1),      // before the window
            Line(30, 31),    // inside
            Line(500, 501),  // after the window
        };

        helper.Reset(selection, SampleRate, 20 * SampleRate, 40 * SampleRate);

        Assert.True(helper.IsSelected((int)(30.5 * SampleRate)));
        Assert.False(helper.IsSelected(25 * SampleRate));
        Assert.False(helper.IsSelected(35 * SampleRate));
    }

    [Fact]
    public void IsSelected_KeepsRangesThatOnlyOverlapTheWindowEdges()
    {
        var helper = new IsSelectedHelper();
        var selection = new List<SubtitleLineViewModel>
        {
            Line(10, 25),  // starts before the window, ends inside
            Line(35, 60),  // starts inside, ends after the window
        };

        helper.Reset(selection, SampleRate, 20 * SampleRate, 40 * SampleRate);

        Assert.True(helper.IsSelected(21 * SampleRate));
        Assert.False(helper.IsSelected(30 * SampleRate));
        Assert.True(helper.IsSelected(39 * SampleRate));
    }

    [Fact]
    public void IsSelected_IsFalseWithoutSelection()
    {
        var helper = new IsSelectedHelper();

        helper.Reset(new List<SubtitleLineViewModel>(), SampleRate, 0, 40 * SampleRate);

        Assert.False(helper.IsSelected(0));
        Assert.False(helper.IsSelected(10 * SampleRate));
    }

    [Fact]
    public void Reset_ShrinkingSelection_DropsThePreviousRanges()
    {
        var helper = new IsSelectedHelper();
        var selection = new List<SubtitleLineViewModel> { Line(10, 11), Line(13, 14) };

        helper.Reset(selection, SampleRate, 5 * SampleRate, 20 * SampleRate);
        Assert.True(helper.IsSelected((int)(13.5 * SampleRate)));

        // The backing array is reused across resets, so a shorter selection must not leave the
        // old ranges visible past the new count.
        helper.Reset(new List<SubtitleLineViewModel> { Line(10, 11) }, SampleRate, 5 * SampleRate, 20 * SampleRate);

        Assert.True(helper.IsSelected((int)(10.5 * SampleRate)));
        Assert.False(helper.IsSelected((int)(13.5 * SampleRate)));
    }
}
