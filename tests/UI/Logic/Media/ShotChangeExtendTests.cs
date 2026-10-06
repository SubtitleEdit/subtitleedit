using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace UITests.Logic.Media;

/// <summary>
/// The rules behind "extend selected lines to next/previous shot change" (issue #13811). The
/// command buys reading time without letting a line cross a cut, so: take the first cut at or after
/// the end (never a later one), stop the configured gap short of it, and never let a cut move the
/// cue the wrong way - only the minimum gap to a neighbouring subtitle may trim a line (#15719).
/// </summary>
public class ShotChangeExtendTests
{
    private const double NoMaxDuration = 100000;
    private static readonly List<double> ShotChanges = new() { 1.0, 2.0, 5.0 }; // seconds

    [Fact]
    public void ExtendEnd_StopsTheGapBeforeTheNextShotChange()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 500, nextStartMs: null,
            outCuesGapMs: 80, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(920, result); // shot change at 1000 ms, minus the 80 ms out cues gap
    }

    // Rule 1: the cut the line stops at is the first one ahead. Skipping it - which is what a
    // "gap-adjusted position must be after the end" filter does when the end already sits inside
    // the gap zone - extends the line straight across that cut.
    [Fact]
    public void ExtendEnd_EndInsideTheGapZone_DoesNotJumpToTheNextShotChange()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 960, nextStartMs: null,
            outCuesGapMs: 80, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Null(result); // 920 would shorten the line; 1920 would span the cut at 1000
    }

    [Fact]
    public void ExtendEnd_EndExactlyOnAShotChange_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 1000, nextStartMs: null,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Null(result);
    }

    // The minimum gap always wins, like SE4: an overlapping line is trimmed to the gap before the
    // next subtitle instead of being left alone (issue #15719).
    [Fact]
    public void ExtendEnd_NextSubtitleStartsBeforeTheCurrentEnd_TrimsToTheGap()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 900, nextStartMs: 800,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(776, result); // 800 - 24 ms minimum gap
    }

    [Fact]
    public void ExtendEnd_EndInsideTheMinimumGap_TrimsToTheGap()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 790, nextStartMs: 800,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(776, result);
    }

    [Fact]
    public void ExtendEnd_AlreadyExactlyTheGapBeforeTheNextSubtitle_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 776, nextStartMs: 800,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Null(result);
    }

    [Fact]
    public void ExtendEnd_TrimWouldLeaveNoDuration_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 790, endMs: 900, nextStartMs: 800,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Null(result); // 776 is before the start
    }

    [Fact]
    public void ExtendEnd_NoShotChangesLoaded_UsesTheNextSubtitle()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            new List<double>(), startMs: 100, endMs: 500, nextStartMs: 3000,
            outCuesGapMs: 80, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2976, result);
    }

    [Fact]
    public void ExtendEnd_NextSubtitleIsCloserThanTheShotChange_StopsBeforeIt()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 500, nextStartMs: 700,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(676, result); // 700 - 24 ms minimum gap
    }

    [Fact]
    public void ExtendEnd_NoShotChangeAhead_UsesTheNextSubtitle()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 5100, endMs: 5200, nextStartMs: 6000,
            outCuesGapMs: 80, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(5976, result);
    }

    [Fact]
    public void ExtendEnd_ResultLongerThanTheMaximumDuration_IsDropped()
    {
        var result = ShotChangesHelper.GetExtendedEndMs(
            ShotChanges, startMs: 100, endMs: 500, nextStartMs: null,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: 500);

        Assert.Null(result); // 1000 - 100 = 900 ms > 500 ms; a clamped end would sit mid-shot
    }

    [Fact]
    public void ExtendStart_StopsTheGapAfterThePreviousShotChange()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            ShotChanges, startMs: 2500, endMs: 3000, previousEndMs: null,
            inCuesGapMs: 40, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2040, result); // shot change at 2000 ms, plus the 40 ms in cues gap
    }

    [Fact]
    public void ExtendStart_StartInsideTheGapZone_DoesNotJumpToTheEarlierShotChange()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            ShotChanges, startMs: 2020, endMs: 3000, previousEndMs: null,
            inCuesGapMs: 40, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Null(result); // 2040 would shorten the line; 1040 would span the cut at 2000
    }

    [Fact]
    public void ExtendStart_PreviousSubtitleIsCloser_StopsAfterIt()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            ShotChanges, startMs: 2500, endMs: 3000, previousEndMs: 2300,
            inCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2324, result);
    }

    [Fact]
    public void ExtendStart_PreviousSubtitleEndsAfterTheCurrentStart_TrimsToTheGap()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            ShotChanges, startMs: 2500, endMs: 3000, previousEndMs: 2600,
            inCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2624, result); // 2600 + 24 ms minimum gap
    }

    [Fact]
    public void ExtendStart_StartInsideTheMinimumGap_TrimsToTheGap()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            ShotChanges, startMs: 2610, endMs: 3000, previousEndMs: 2600,
            inCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2624, result);
    }

    [Fact]
    public void ExtendStart_TrimWouldLeaveNoDuration_LeavesTheLineAlone()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            ShotChanges, startMs: 2500, endMs: 2610, previousEndMs: 2600,
            inCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Null(result); // 2624 is after the end
    }

    [Fact]
    public void ExtendStart_NoShotChangesLoaded_UsesThePreviousSubtitle()
    {
        var result = ShotChangesHelper.GetExtendedStartMs(
            new List<double>(), startMs: 2500, endMs: 3000, previousEndMs: 1000,
            inCuesGapMs: 40, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(1024, result);
    }
}
