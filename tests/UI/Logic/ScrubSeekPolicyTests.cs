using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic;

/// <summary>
/// Two-tier scrub seeking: seeks issued mid-burst are served at keyframes (fast) and the exact
/// landing is deferred to when the burst settles, so a drag does not pay for an exact seek to
/// every position it passes through. The precision must always come back - a burst that ends
/// without its follow-up leaves the video on a keyframe instead of the frame the user picked.
/// </summary>
public class ScrubSeekPolicyTests
{
    [Fact]
    public void IsolatedSeek_IsExact()
    {
        // The single click on the waveform, and every non-drag seek: unchanged from before.
        Assert.Equal(ScrubSeekPolicy.ExactSeekFlags, ScrubSeekPolicy.FlagsFor(seekInFlight: false));
    }

    [Fact]
    public void SeekDuringAnUnfinishedSeek_IsServedAtKeyframes()
    {
        Assert.Equal(ScrubSeekPolicy.KeyframeSeekFlags, ScrubSeekPolicy.FlagsFor(seekInFlight: true));
    }

    [Fact]
    public void SeekCountsAsInFlight_UntilItsRestartArrives()
    {
        Assert.True(ScrubSeekPolicy.SeekIsInFlight(
            eventLoopActive: true, lastSeekCommandId: 7, restartAckedSeekCommandId: 6,
            secondsSinceLastSeekIssued: 0.05));

        Assert.False(ScrubSeekPolicy.SeekIsInFlight(
            eventLoopActive: true, lastSeekCommandId: 7, restartAckedSeekCommandId: 7,
            secondsSinceLastSeekIssued: 0.05));
    }

    [Fact]
    public void NothingIsInFlight_WithoutTheEventLoop()
    {
        // No restarts are read there, so a deferred landing could never be paid - seeks must stay
        // exact even though this one looks unfinished.
        Assert.False(ScrubSeekPolicy.SeekIsInFlight(
            eventLoopActive: false, lastSeekCommandId: 7, restartAckedSeekCommandId: 6,
            secondsSinceLastSeekIssued: 0.05));
    }

    [Fact]
    public void AStuckSeek_StopsCountingAsInFlight()
    {
        // A restart that never arrives must not latch scrubbing onto keyframes forever: past the
        // cap the next seek is exact again, and being exact it also clears any stranded debt.
        Assert.False(ScrubSeekPolicy.SeekIsInFlight(
            eventLoopActive: true, lastSeekCommandId: 7, restartAckedSeekCommandId: 6,
            secondsSinceLastSeekIssued: ScrubSeekPolicy.MaxSeekInFlightSeconds + 1));
    }

    [Fact]
    public void NoFollowUp_WhenNothingOwesAnExactLanding()
    {
        Assert.False(ScrubSeekPolicy.ShouldIssueFollowUp(
            followUpSeekId: 0, lastSeekCommandId: 7, restartAckedSeekCommandId: 7,
            target: 12.0, reportedPosition: 8.0));
    }

    [Fact]
    public void NoFollowUp_WhileTheKeyframeSeekIsStillInFlight()
    {
        // Its own restart has not arrived, so the burst may well continue - that restart asks again.
        Assert.False(ScrubSeekPolicy.ShouldIssueFollowUp(
            followUpSeekId: 7, lastSeekCommandId: 7, restartAckedSeekCommandId: 6,
            target: 12.0, reportedPosition: 8.0));
    }

    [Fact]
    public void NoFollowUp_WhenANewerSeekHasTakenOver()
    {
        // Mid-drag: seek 8 already owns the position and carries its own follow-up. Paying seek
        // 7's landing here would seek back to a position the user has moved away from.
        Assert.False(ScrubSeekPolicy.ShouldIssueFollowUp(
            followUpSeekId: 7, lastSeekCommandId: 8, restartAckedSeekCommandId: 7,
            target: 12.0, reportedPosition: 8.0));
    }

    [Fact]
    public void FollowUp_WhenTheBurstHasSettledShortOfTheTarget()
    {
        // Mouse released: the last keyframe seek landed at 8.0 but the user asked for 12.0.
        Assert.True(ScrubSeekPolicy.ShouldIssueFollowUp(
            followUpSeekId: 7, lastSeekCommandId: 7, restartAckedSeekCommandId: 7,
            target: 12.0, reportedPosition: 8.0));
    }

    [Fact]
    public void NoFollowUp_WhenTheKeyframeLandingAlreadyHitTheTarget()
    {
        // The target was a keyframe, so the fast seek was already exact - re-seeking there would
        // cost a decode for no movement.
        Assert.False(ScrubSeekPolicy.ShouldIssueFollowUp(
            followUpSeekId: 7, lastSeekCommandId: 7, restartAckedSeekCommandId: 7,
            target: 12.0, reportedPosition: 12.0));
    }

    [Fact]
    public void FollowUp_WhenTheLandingCannotBeConfirmed()
    {
        // mpv has no position to report yet. Skipping here would strand the video on a keyframe,
        // so an unnecessary exact seek is the cheaper mistake.
        Assert.True(ScrubSeekPolicy.ShouldIssueFollowUp(
            followUpSeekId: 7, lastSeekCommandId: 7, restartAckedSeekCommandId: 7,
            target: 12.0, reportedPosition: null));
    }

    /// <summary>
    /// A burst is a sequence, not one decision: the first seek is exact, everything issued while
    /// a seek is in flight is fast, and the settled end pays exactly one exact landing.
    /// </summary>
    [Fact]
    public void WholeDrag_IsOneExactSeekAtEachEnd()
    {
        var flags = new List<string>();
        long lastId = 0;
        long restartAcked = 0;
        long followUpId = 0;
        var target = 0.0;

        // Mouse down, then eight drag steps arriving faster than mpv can land them.
        foreach (var position in new[] { 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 })
        {
            var inFlight = ScrubSeekPolicy.SeekIsInFlight(
                eventLoopActive: true,
                lastSeekCommandId: lastId,
                restartAckedSeekCommandId: restartAcked,
                secondsSinceLastSeekIssued: 0.02); // one drag step apart
            flags.Add(ScrubSeekPolicy.FlagsFor(inFlight));
            lastId++;
            target = position;
            followUpId = inFlight ? lastId : 0;
        }

        Assert.Equal(ScrubSeekPolicy.ExactSeekFlags, flags[0]);
        Assert.All(flags.Skip(1), f => Assert.Equal(ScrubSeekPolicy.KeyframeSeekFlags, f));

        // Mouse up. The last keyframe seek lands on its keyframe, 3.6 s short of the target.
        restartAcked = lastId;
        Assert.True(ScrubSeekPolicy.ShouldIssueFollowUp(followUpId, lastId, restartAcked, target, 8.4));

        // That follow-up goes out as an exact seek and clears the debt, so its own restart - and
        // any later one - does not start the cycle over.
        var followUpFlags = ScrubSeekPolicy.FlagsFor(seekInFlight: false);
        followUpId = 0;
        lastId++;
        restartAcked = lastId;

        Assert.Equal(ScrubSeekPolicy.ExactSeekFlags, followUpFlags);
        Assert.False(ScrubSeekPolicy.ShouldIssueFollowUp(followUpId, lastId, restartAcked, target, target));
    }
}
