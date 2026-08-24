using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// "Beautify time codes" snaps cues to frame boundaries. Without a frame list it has to assume a
/// perfect n/fps grid, which is wrong for variable frame rate and remuxed material (issue #10235).
/// These cover reading the real frame times out of ffmpeg and the guards that keep a bad list from
/// being trusted - the beautifier binary searches the list and indexes it by frame number, and
/// validates neither, so a malformed list silently produces wrong frame numbers.
/// </summary>
public class TimeCodesTests
{
    [Theory]
    // Real "-vf showinfo" lines, as they arrive on ffmpeg's stderr.
    [InlineData("[Parsed_showinfo_0 @ 0xb8ad4c0c0] n:   0 pts:      0 pts_time:0       pos:      626 fmt:yuv420p sar:1/1 s:320x240 i:P iskey:1 type:I", 0.0)]
    [InlineData("[Parsed_showinfo_0 @ 0xb8ad4c0c0] n:   1 pts:     40 pts_time:0.04    pos:     5049 fmt:yuv420p sar:1/1 s:320x240 i:P iskey:0 type:B", 0.04)]
    [InlineData("[Parsed_showinfo_0 @ 0x7f8] n: 1234 pts: 1234567 pts_time:1234.567 duration:1 duration_time:0.04", 1234.567)]
    public void TryParsePtsTime_ReadsPresentationTime(string line, double expected)
    {
        Assert.True(TimeCodesGenerator.TryParsePtsTime(line, out var seconds));
        Assert.Equal(expected, seconds, 3);
    }

    [Theory]
    [InlineData("[Parsed_showinfo_0 @ 0x7f8] n: 5 pts: N/A pts_time:N/A pos: 100")] // frame without a time stamp
    [InlineData("frame=  123 fps= 25 q=-0.0 size=N/A time=00:00:04.92 bitrate=N/A speed=9.8x")] // progress line
    [InlineData("out_time_us=40000")]
    [InlineData("")]
    public void TryParsePtsTime_IgnoresLinesWithoutAUsableTime(string line)
    {
        Assert.False(TimeCodesGenerator.TryParsePtsTime(line, out _));
    }

    [Fact]
    public void Normalize_SortsPacketOrderedInputAscending()
    {
        // Packet/decode-ordered sources list B-frames out of presentation order. The beautifier
        // binary searches the list, so an unsorted list would return nonsense frame numbers.
        var normalized = TimeCodesHelper.Normalize(new[] { 0.0, 0.12, 0.04, 0.08, 0.24, 0.16, 0.2 });

        Assert.Equal(new[] { 0.0, 0.04, 0.08, 0.12, 0.16, 0.2, 0.24 }, normalized);
    }

    [Fact]
    public void Normalize_DropsValuesThatCannotBeFrameTimes()
    {
        var normalized = TimeCodesHelper.Normalize(new[] { 0.04, double.NaN, 0.04, -1.0, double.PositiveInfinity, 0.08 });

        Assert.Equal(new[] { 0.04, 0.08 }, normalized);
    }

    [Fact]
    public void IsUsableFor_RejectsAnIncompleteList()
    {
        var fullList = Enumerable.Range(0, 250).Select(n => n / 25.0).ToList();
        Assert.True(TimeCodesHelper.IsUsableFor(fullList, 10));

        // Cancelled or truncated half-way: worse than nothing, because cues past the end would
        // snap to whatever frame happens to sit at that index instead of falling back to n/fps.
        Assert.False(TimeCodesHelper.IsUsableFor(fullList.Take(125).ToList(), 10));
        Assert.False(TimeCodesHelper.IsUsableFor(new List<double>(), 10));
    }

    [Fact]
    public void IsUsableFor_RejectsAnyListWhenTheDurationIsUnknown()
    {
        // With no duration to check against, a cancelled 3-frame extraction is
        // indistinguishable from a complete one - it must not be trusted (callers that know
        // the run completed pass the last frame time as the duration instead).
        var fullList = Enumerable.Range(0, 250).Select(n => n / 25.0).ToList();
        Assert.False(TimeCodesHelper.IsUsableFor(fullList.Take(3).ToList(), 0));
        Assert.False(TimeCodesHelper.IsUsableFor(fullList, 0));
        Assert.False(TimeCodesHelper.IsUsableFor(fullList, -1));

        // A completed run can vouch for itself: its own last frame time is the duration.
        Assert.True(TimeCodesHelper.IsUsableFor(fullList, fullList[fullList.Count - 1]));
    }

    /// <summary>
    /// The point of the whole feature: on material whose real frames are not on an n/fps grid,
    /// beautifying with the video's own time codes must land cues on real frames, where nominal
    /// frame-rate arithmetic cannot.
    /// </summary>
    [Fact]
    public void Beautify_WithExactTimeCodes_SnapsToRealFramesNotTheNominalGrid()
    {
        // 12.5 fps nominal, but the frames actually sit at an irregular 0/67/167/233/333... ms
        // (the pattern a VFR source produces).
        var timeCodes = new List<double> { 0, 0.067, 0.167, 0.233, 0.333, 0.4, 0.5, 0.567, 0.667, 0.733, 0.833, 0.9, 1.0 };

        var row = new SubtitleLineViewModel(new Paragraph("Hello", 230, 900), new SubRip());

        var withExact = new List<SubtitleLineViewModel> { new SubtitleLineViewModel(row) };
        BeautifyTimeCodesViewModel.CommitBeautifiedTimes(withExact, 12.5, new List<double>(), timeCodes);

        var nominalOnly = new List<SubtitleLineViewModel> { new SubtitleLineViewModel(row) };
        BeautifyTimeCodesViewModel.CommitBeautifiedTimes(nominalOnly, 12.5, new List<double>());

        // Exact time codes put both cues on times the video actually has frames at.
        Assert.Equal(233, withExact[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(900, withExact[0].EndTime.TotalMilliseconds, 0);
        foreach (var ms in new[] { withExact[0].StartTime.TotalMilliseconds, withExact[0].EndTime.TotalMilliseconds })
        {
            Assert.Contains(timeCodes, t => System.Math.Abs(t * 1000 - ms) < 1);
        }

        // The nominal 12.5 fps grid is multiples of 80 ms - it has no frame at either instant.
        Assert.Equal(240, nominalOnly[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(880, nominalOnly[0].EndTime.TotalMilliseconds, 0);
    }
}
