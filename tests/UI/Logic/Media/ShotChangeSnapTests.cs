using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace UITests.Logic.Media;

/// <summary>
/// The rules behind "snap selected lines' start/end to next/previous shot change" (issue #13948).
/// Snapping parks a cue on the cut it is running past, the configured gap short of it - the same
/// gap the beautifier and the extend commands use - and only refuses when the result would not
/// leave a positive duration.
/// </summary>
public class ShotChangeSnapTests
{
    private const double FrameDurationMs25 = 40; // 25 fps
    private static readonly List<double> ShotChanges = new() { 1.0, 2.0, 5.0 }; // seconds

    [Fact]
    public void SnapEnd_StopsTheGapBeforeThePreviousShotChange()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 100, endMs: 2500,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Equal(1920, result); // shot change at 2000 ms, minus the 80 ms out cues gap
    }

    // The reported bug: the end landed exactly on the cut instead of the gap before it.
    [Fact]
    public void SnapEnd_DoesNotLandOnTheShotChangeItself()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 100, endMs: 2500,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.NotEqual(2000, result);
    }

    // Rule 1: an end already parked on a cut snaps to that cut's gap. Requiring the cut to be
    // strictly before the end skips a whole shot backwards, which then usually fails a duration
    // check and reads as "the shortcut did nothing".
    [Fact]
    public void SnapEnd_EndExactlyOnAShotChange_SnapsToThatShotChange()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 100, endMs: 2000,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Equal(1920, result); // the cut at 2000, not the one at 1000
    }

    // The "on" tolerance is just under a frame, so a cue a hair short of the cut still counts.
    [Fact]
    public void SnapEnd_EndJustBeforeAShotChange_SnapsToThatShotChange()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 100, endMs: 1980,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Equal(1920, result);
    }

    // Rule 3: a short result is still the result the user asked for - minimum display duration
    // must not silently veto it.
    [Fact]
    public void SnapEnd_ResultShorterThanMinimumDisplayDuration_StillSnaps()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 1900, endMs: 4000,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Equal(1920, result); // a 20 ms line, but the user asked for this cue to move
    }

    [Fact]
    public void SnapEnd_ResultWouldNotLeaveAPositiveDuration_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 1950, endMs: 4000,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Null(result); // 1920 is before the start
    }

    [Fact]
    public void SnapEnd_NoShotChangeBeforeTheEnd_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            ShotChanges, startMs: 100, endMs: 500,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Null(result);
    }

    [Fact]
    public void SnapEnd_NoShotChangesAtAll_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetSnappedEndMs(
            new List<double>(), startMs: 100, endMs: 2500,
            outCuesGapMs: 80, frameDurationMs: FrameDurationMs25);

        Assert.Null(result);
    }

    [Fact]
    public void SnapStart_StartsTheGapAfterTheNextShotChange()
    {
        var result = ShotChangesHelper.GetSnappedStartMs(
            ShotChanges, startMs: 1500, endMs: 4000,
            inCuesGapMs: 120, frameDurationMs: FrameDurationMs25);

        Assert.Equal(2120, result); // shot change at 2000 ms, plus the 120 ms in cues gap
    }

    [Fact]
    public void SnapStart_StartExactlyOnAShotChange_SnapsToThatShotChange()
    {
        var result = ShotChangesHelper.GetSnappedStartMs(
            ShotChanges, startMs: 2000, endMs: 4000,
            inCuesGapMs: 120, frameDurationMs: FrameDurationMs25);

        Assert.Equal(2120, result); // the cut at 2000, not the one at 5000
    }

    // A shot change at t=0 is a real cut, not a "nothing found" sentinel.
    [Fact]
    public void SnapStart_ShotChangeAtZero_IsNotMistakenForNoShotChange()
    {
        var result = ShotChangesHelper.GetSnappedStartMs(
            new List<double> { 0.0, 5.0 }, startMs: 0, endMs: 2000,
            inCuesGapMs: 120, frameDurationMs: FrameDurationMs25);

        Assert.Equal(120, result);
    }

    [Fact]
    public void SnapStart_ResultWouldNotLeaveAPositiveDuration_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetSnappedStartMs(
            ShotChanges, startMs: 1500, endMs: 2050,
            inCuesGapMs: 120, frameDurationMs: FrameDurationMs25);

        Assert.Null(result); // 2120 is after the end
    }

    [Fact]
    public void SnapStart_NoShotChangeAfterTheStart_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetSnappedStartMs(
            ShotChanges, startMs: 6000, endMs: 8000,
            inCuesGapMs: 120, frameDurationMs: FrameDurationMs25);

        Assert.Null(result);
    }
}
