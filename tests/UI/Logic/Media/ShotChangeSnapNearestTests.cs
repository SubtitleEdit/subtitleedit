using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace UITests.Logic.Media;

/// <summary>
/// The rules behind "snap selected lines to nearest shot change" - the third snap path, brought in
/// line with the waveform drag (#13984) and the start/end snap shortcuts (#13948): every way of
/// snapping a cue to a cut lands it the profile's gap away from that cut, and the only veto is a
/// result with no positive duration.
/// </summary>
public class ShotChangeSnapNearestTests
{
    private const double InGap = 80;   // ms, 2 frames at 25 fps
    private const double OutGap = 160; // ms, 4 frames at 25 fps
    private const double MaxStart = 1000;
    private const double MaxEnd = 1500;
    private const double MaxSameShotEnd = 500;

    private static (double, double)? Snap(List<double> cuts, double startMs, double endMs,
        double inGap = InGap, double outGap = OutGap) =>
        ShotChangesHelper.GetSnappedToNearestMs(cuts, startMs, endMs, inGap, outGap, MaxStart, MaxEnd, MaxSameShotEnd);

    [Fact]
    public void BothCuesNearDifferentCuts_SnapBoth_TheGapAwayFromEachCut()
    {
        var r = Snap(new List<double> { 1.0, 5.0 }, startMs: 1200, endMs: 4700);

        Assert.Equal((1000 + InGap, 5000 - OutGap), r);
    }

    // The gap used to be ignored here while the sibling shortcuts applied it - so the same cue
    // snapped to the same cut landed in two places depending on which shortcut you pressed.
    [Fact]
    public void LandingOffsetIsTheProfileGap_NotTheCutItself()
    {
        var r = Snap(new List<double> { 1.0, 5.0 }, startMs: 1200, endMs: 4700);

        Assert.NotEqual(1000.0, r!.Value.Item1);
        Assert.NotEqual(5000.0, r!.Value.Item2);
    }

    [Fact]
    public void OnlyStartNearACut_SnapsStart_LeavesEndAlone()
    {
        var r = Snap(new List<double> { 1.0 }, startMs: 1200, endMs: 4700);

        Assert.Equal((1000 + InGap, 4700.0), r);
    }

    [Fact]
    public void OnlyEndNearACut_SnapsEnd_LeavesStartAlone()
    {
        var r = Snap(new List<double> { 5.0 }, startMs: 1200, endMs: 4700);

        Assert.Equal((1200.0, 5000 - OutGap), r);
    }

    [Fact]
    public void NoCutInRange_LeavesTheLineAlone()
    {
        Assert.Null(Snap(new List<double> { 20.0 }, startMs: 1200, endMs: 4700));
    }

    [Fact]
    public void NoShotChangesAtAll_LeavesTheLineAlone()
    {
        Assert.Null(Snap(new List<double>(), startMs: 1200, endMs: 4700));
    }

    // The old code used 0 as "no cut found", so a real shot change at t=0 was indistinguishable
    // from none - a start near the file's first cut never snapped to it.
    [Fact]
    public void ShotChangeAtZero_IsARealCut_NotTheNoCutSentinel()
    {
        var r = Snap(new List<double> { 0.0, 20.0 }, startMs: 300, endMs: 4000);

        Assert.Equal((0 + InGap, 4000.0), r);
    }

    // Straddling a single cut: snapping both cues onto it collapses the line. The start keeps the
    // cut and the end looks for a further one within the tighter same-shot distance.
    [Fact]
    public void BothCuesNearestTheSameCut_StartTakesIt_EndRetriesForAFurtherCut()
    {
        // Start at 2.2 s, end at 2.6 s, cut at 2.4 s (nearest to both); another cut at 3.0 s is
        // 400 ms from the end - inside the 500 ms same-shot distance.
        var r = Snap(new List<double> { 2.4, 3.0 }, startMs: 2200, endMs: 2600);

        Assert.Equal((2400 + InGap, 3000 - OutGap), r);
    }

    [Fact]
    public void BothCuesNearestTheSameCut_NoFurtherCutInRange_EndStaysPut()
    {
        var r = Snap(new List<double> { 2.4, 9.0 }, startMs: 2200, endMs: 2600);

        Assert.Equal((2400 + InGap, 2600.0), r);
    }

    // The retry must look *forward*: a cut behind the start is nearer to the end than nothing, but
    // snapping the end to it would put the end before the start.
    [Fact]
    public void SameCutRetry_IgnoresACutBehindTheStart()
    {
        var r = Snap(new List<double> { 2.0, 2.4 }, startMs: 2300, endMs: 2500);

        // Nearest to both is 2.4; the retry's nearest within 500 ms of 2500 is still 2.4 itself
        // (not > cut), so the end stays.
        Assert.Equal((2400 + InGap, 2500.0), r);
    }

    [Fact]
    public void ResultWouldNotLeaveAPositiveDuration_LeavesTheLineAlone()
    {
        // Start cut at 1.0 (+80 = 1080), end cut at 1.1 (-160 = 940): end before start.
        Assert.Null(Snap(new List<double> { 1.0, 1.1 }, startMs: 1050, endMs: 1120));
    }

    // Deliberately no minimum/maximum display-duration veto: the user asked for this line to move.
    [Fact]
    public void VeryShortResult_StillSnaps()
    {
        var r = Snap(new List<double> { 1.0, 1.3 }, startMs: 1050, endMs: 1250);

        Assert.Equal((1000 + InGap, 1300 - OutGap), r); // a 60 ms line, but it is what was asked for
    }

    [Fact]
    public void AlreadyExactlyWhereItShouldBe_ReportsNoChange()
    {
        Assert.Null(Snap(new List<double> { 1.0, 5.0 }, startMs: 1000 + InGap, endMs: 5000 - OutGap));
    }

    [Fact]
    public void ZeroGaps_LandExactlyOnTheCuts()
    {
        var r = Snap(new List<double> { 1.0, 5.0 }, startMs: 1200, endMs: 4700, inGap: 0, outGap: 0);

        Assert.Equal((1000.0, 5000.0), r);
    }
}
