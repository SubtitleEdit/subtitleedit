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
/// So a seek that joins a burst is served with "keyframes" instead, which skips that decode.
/// Precision is not given up, only deferred: the keyframe seek is recorded as owing an exact
/// landing, and when the burst settles (that seek lands and nothing newer has replaced it) one
/// exact seek to the final target is issued. A burst takes three seeks to establish itself - see
/// <see cref="JoinsBurst"/> - so a single click, and the click paths that seek twice in one input
/// event (pointer release plus tap), behave exactly as before.
/// </para>
/// <para>
/// A keyframe seek lands on the keyframe BEFORE the target (mpv's rule for absolute seeks), which
/// on a long-GOP file can be many seconds back, so the deferred exact landing is never optional:
/// a burst that ends without it leaves the video, and everything that reads mpv's position, a
/// whole GOP away from where the user pointed.
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
    public static string FlagsFor(bool joinsBurst)
    {
        return joinsBurst ? KeyframeSeekFlags : ExactSeekFlags;
    }

    /// <summary>
    /// Whether a seek issued now is the continuation of a burst, and so may be served fast.
    /// <para>
    /// One seek arriving while another is in flight is not enough: the waveform click path seeks
    /// twice in the same input event (the pointer release seeks, the tap that follows seeks
    /// again, to the frame-snapped position), and serving that second seek at keyframes would
    /// flash a frame from the previous keyframe - seconds back on a long-GOP file - before the
    /// exact landing was paid. So the fast path needs a sequence: the seek in flight must itself
    /// have been issued while a seek was in flight. A drag or a wheel spin gets there on its third
    /// event and pays two exact seeks instead of one at its start; a click never gets there.
    /// </para>
    /// </summary>
    /// <param name="seekInFlight">A seek SE issued has not landed yet (<see cref="SeekIsInFlight"/>).</param>
    /// <param name="previousSeekIssuedInFlight">That seek was itself issued while a seek was in flight.</param>
    public static bool JoinsBurst(bool seekInFlight, bool previousSeekIssuedInFlight)
    {
        return seekInFlight && previousSeekIssuedInFlight;
    }

    /// <summary>
    /// Whether a seek SE issued is still on its way - the burst signal <see cref="JoinsBurst"/>
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
    /// Whether the exact seek owed by a keyframe seek should be issued now: once that seek has
    /// landed and nothing newer has replaced it, always.
    /// <para>
    /// This deliberately does not look at where mpv says it is. While a seek is in progress mpv
    /// reports the seek TARGET as "time-pos", and the observed-property cache is refreshed only
    /// after the event queue drains, so at the playback-restart event the cached position is the
    /// target itself. A "close enough, skip the landing" check compares the target with the
    /// target, passes, and the keyframe landing - a GOP short of it - stands for good (#14441:
    /// wheel steps that kept landing on the same spot, clicks whose cursor and video ended up
    /// elsewhere). An exact seek to a spot that is already a keyframe costs nothing to speak of,
    /// so there is nothing worth saving by guessing.
    /// </para>
    /// </summary>
    /// <param name="followUpSeekId">Id of the keyframe seek owing an exact landing, 0 if none.</param>
    /// <param name="lastSeekCommandId">Newest seek id handed out.</param>
    /// <param name="restartAckedSeekCommandId">Newest seek id mpv has finished a playback restart for.</param>
    public static bool ShouldIssueFollowUp(
        long followUpSeekId,
        long lastSeekCommandId,
        long restartAckedSeekCommandId)
    {
        if (followUpSeekId == 0)
        {
            return false; // nothing owes an exact landing
        }

        if (followUpSeekId != lastSeekCommandId)
        {
            return false; // a newer seek owns the position now, and carries its own follow-up
        }

        // Its own restart has not arrived: the burst is still running, a later restart asks again.
        return restartAckedSeekCommandId >= followUpSeekId;
    }
}
