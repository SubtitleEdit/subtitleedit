using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Decides how each seek SE sends to mpv is served: precisely, or fast and then precisely.
/// <para>
/// An absolute seek is exact by default (mpv's own rule, and SE also sets "hr-seek=yes"), which
/// means decoding every frame from the preceding keyframe up to the target - tens to hundreds of
/// milliseconds on a long-GOP 4K file. That is the right trade for a single click on the waveform.
/// It is the wrong trade for a drag or a wheel burst, where seeks arrive one per input event and
/// all but the last one are about to be superseded: paying for an exact landing on a position the
/// user is already moving away from is what makes scrubbing feel heavy.
/// </para>
/// <para>
/// So a seek issued while an earlier one is still in flight - the signature of a burst - is served
/// with "keyframes" instead, which skips that decode. Precision is not given up, only deferred:
/// the keyframe seek is recorded as owing an exact landing, and when the burst settles (that seek
/// lands and nothing newer has replaced it) one exact seek to the final target is issued. Isolated
/// seeks never take that path - the first seek of a burst is exact, as before - so nothing that
/// clicks once behaves differently.
/// </para>
/// </summary>
public static class ScrubSeekPolicy
{
    /// <summary>
    /// mpv seek flags for a precise landing. "absolute" alone already implies "exact"; saying it
    /// keeps the intent readable and independent of the hr-seek option.
    /// </summary>
    public const string ExactSeekFlags = "absolute+exact";

    /// <summary>
    /// mpv seek flags for a fast landing at the preceding keyframe, used mid-burst.
    /// </summary>
    public const string KeyframeSeekFlags = "absolute+keyframes";

    /// <summary>
    /// How close the keyframe landing has to be to the target to count as already exact. A seek
    /// whose target happens to be a keyframe lands on it, and re-seeking there would cost a
    /// decode for no movement.
    /// </summary>
    public const double FollowUpToleranceSeconds = 0.001;

    /// <summary>
    /// How long a seek may be counted as still in flight. Every seek ends in a playback restart,
    /// but a seek mpv drops - on a core that has stopped, or a file that went away - never gets
    /// one, and "in flight" would then latch forever: every later seek served at keyframes, each
    /// deferring an exact landing that no restart is coming to pay. Past this age the seek is
    /// treated as finished, which puts seeking back on the exact path by itself.
    /// </summary>
    public const double MaxSeekInFlightSeconds = 5.0;

    /// <summary>
    /// The flags for a seek issued now: fast if this one is joining a burst, precise otherwise.
    /// </summary>
    public static string FlagsFor(bool seekInFlight)
    {
        return seekInFlight ? KeyframeSeekFlags : ExactSeekFlags;
    }

    /// <summary>
    /// Whether a seek SE issued is still on its way - the burst signal <see cref="FlagsFor"/>
    /// keys on.
    /// </summary>
    /// <param name="eventLoopActive">
    /// False when no mpv events are being read. Nothing could pay a deferred landing then, so no
    /// seek counts as in flight and every seek stays exact.
    /// </param>
    /// <param name="lastSeekCommandId">Newest seek id handed out.</param>
    /// <param name="restartAckedSeekCommandId">Newest seek id mpv has finished a playback restart for.</param>
    /// <param name="secondsSinceLastSeekIssued">Age of the newest seek.</param>
    public static bool SeekIsInFlight(
        bool eventLoopActive,
        long lastSeekCommandId,
        long restartAckedSeekCommandId,
        double secondsSinceLastSeekIssued)
    {
        if (!eventLoopActive || lastSeekCommandId == 0)
        {
            return false;
        }

        if (restartAckedSeekCommandId >= lastSeekCommandId)
        {
            return false; // it has landed
        }

        return secondsSinceLastSeekIssued < MaxSeekInFlightSeconds;
    }

    /// <summary>
    /// Whether the exact seek owed by a keyframe seek should be issued now.
    /// </summary>
    /// <param name="followUpSeekId">Id of the keyframe seek owing an exact landing, 0 if none.</param>
    /// <param name="lastSeekCommandId">Newest seek id handed out.</param>
    /// <param name="restartAckedSeekCommandId">Newest seek id mpv has finished a playback restart for.</param>
    /// <param name="target">Position that keyframe seek was aiming at.</param>
    /// <param name="reportedPosition">Where mpv says it is now, null if it has no position yet.</param>
    public static bool ShouldIssueFollowUp(
        long followUpSeekId,
        long lastSeekCommandId,
        long restartAckedSeekCommandId,
        double target,
        double? reportedPosition)
    {
        if (followUpSeekId == 0)
        {
            return false; // nothing owes an exact landing
        }

        if (followUpSeekId != lastSeekCommandId)
        {
            return false; // a newer seek owns the position now, and carries its own follow-up
        }

        if (restartAckedSeekCommandId < followUpSeekId)
        {
            return false; // the burst is still running - a later restart gets to ask again
        }

        if (reportedPosition == null)
        {
            return true; // cannot confirm the landing, so pay for it
        }

        return Math.Abs(reportedPosition.Value - target) > FollowUpToleranceSeconds;
    }
}
