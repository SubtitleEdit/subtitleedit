using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace UITests.Logic.Media;

/// <summary>
/// The rules behind the subtitle grid's "Shot in"/"Shot out" columns: the signed distance from a
/// cue to its nearest shot change, and whether Beautify time codes would move the cue.
/// </summary>
public class ShotChangeOffsetTests
{
    private const double FrameRate = 25; // 40 ms per frame

    [Fact]
    public void CueAfterCut_PositiveOffset()
    {
        var found = ShotChangesHelper.TryGetShotChangeOffset(new List<double> { 10.0 }, 10080, FrameRate, 25, out var ms, out var frames);

        Assert.True(found);
        Assert.Equal(80, ms, 3);
        Assert.Equal(2, frames);
    }

    [Fact]
    public void CueBeforeCut_NegativeOffset()
    {
        var found = ShotChangesHelper.TryGetShotChangeOffset(new List<double> { 10.0 }, 9920, FrameRate, 25, out var ms, out var frames);

        Assert.True(found);
        Assert.Equal(-80, ms, 3);
        Assert.Equal(-2, frames);
    }

    [Fact]
    public void NearestOfTwoCutsWins()
    {
        var found = ShotChangesHelper.TryGetShotChangeOffset(new List<double> { 10.0, 10.5 }, 10440, FrameRate, 25, out _, out var frames);

        Assert.True(found);
        Assert.Equal(-2, frames);
    }

    [Fact]
    public void CutFurtherThanMaxDistance_NotFound()
    {
        Assert.False(ShotChangesHelper.TryGetShotChangeOffset(new List<double> { 10.0 }, 12000, FrameRate, 25, out _, out _));
    }

    [Fact]
    public void NoShotChanges_NotFound()
    {
        Assert.False(ShotChangesHelper.TryGetShotChangeOffset(new List<double>(), 12000, FrameRate, 25, out _, out _));
    }

    // Zones as in the default profile: green 12, red 7 either side of the cut.
    private static bool InCue(int offsetFrames, int gap = 0) =>
        ShotChangesHelper.IsCueInShotChangeZone(offsetFrames, gap, 12, 7, 7, 12);

    [Theory]
    [InlineData(0, false)]    // on the cut = the in cues gap of 0
    [InlineData(3, true)]     // red zone after the cut
    [InlineData(-3, true)]    // red zone before the cut
    [InlineData(-7, true)]    // red zone edge is inclusive
    [InlineData(-11, true)]   // green zone
    [InlineData(-12, false)]  // green zone edge is exclusive
    [InlineData(12, false)]
    [InlineData(20, false)]
    public void InCue_Zones(int offsetFrames, bool expected)
    {
        Assert.Equal(expected, InCue(offsetFrames));
    }

    [Fact]
    public void OutCue_OnTheGapBeforeTheCut_IsFine()
    {
        Assert.False(ShotChangesHelper.IsCueInShotChangeZone(-2, -2, 12, 7, 7, 12));
        Assert.True(ShotChangesHelper.IsCueInShotChangeZone(0, -2, 12, 7, 7, 12));
        Assert.True(ShotChangesHelper.IsCueInShotChangeZone(-5, -2, 12, 7, 7, 12));
    }

    [Fact]
    public void ZeroGreenZones_OnlyRedZonesCount()
    {
        Assert.True(ShotChangesHelper.IsCueInShotChangeZone(-3, 0, 0, 3, 5, 0));
        Assert.False(ShotChangesHelper.IsCueInShotChangeZone(-4, 0, 0, 3, 5, 0));
        Assert.True(ShotChangesHelper.IsCueInShotChangeZone(5, 0, 0, 3, 5, 0));
        Assert.False(ShotChangesHelper.IsCueInShotChangeZone(6, 0, 0, 3, 5, 0));
    }
}
