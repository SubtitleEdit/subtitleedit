namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

/// <summary>
/// Bridges the gap between asking the player to start and the player reporting that it has.
/// <c>IsPlaying</c> is fed by mpv's asynchronous <c>pause</c> property-change event (see
/// LibMpvDynamicPlayer's event loop and <c>_observedPause</c>), so for a tick or two after
/// <c>Play()</c> the player still reports paused.
///
/// That matters because the position timer treats "not playing" as "playback stopped" and drops
/// the play-selection stop/loop point. Landing in this window throws away a stop point that was
/// set microseconds earlier, and playback then runs on past the end of the line it was supposed
/// to stop at - "go to video position, play current, and pause" never pausing (#14167). While a
/// play is pending, that reset is held off.
/// </summary>
public sealed class PlayStartGate
{
    /// <summary>
    /// Upper bound on how long a play may stay pending. A play command that is never honoured
    /// (no file loaded, a player that ignored it) must not hold the play selection forever.
    /// Generous next to the ~1 ms mpv normally takes, because the point is only to bound the
    /// damage of a play that never lands.
    /// </summary>
    public const double MaxPendingMilliseconds = 500;

    private long? _requestedTimestamp;

    /// <summary>Playback has been asked to start; <paramref name="timestamp"/> is Stopwatch ticks.</summary>
    public void PlayRequested(long timestamp)
    {
        _requestedTimestamp = timestamp;
    }

    /// <summary>A pause was asked for, so any pending play is moot.</summary>
    public void PauseRequested()
    {
        _requestedTimestamp = null;
    }

    /// <summary>The player now reports playing, so the request has landed.</summary>
    public void PlayingObserved()
    {
        _requestedTimestamp = null;
    }

    /// <summary>
    /// True while a requested play has neither been observed nor timed out. Expires the request
    /// once past <see cref="MaxPendingMilliseconds"/> so a play that never took effect stops
    /// holding anything back.
    /// </summary>
    public bool IsPending(long now, long ticksPerSecond)
    {
        if (_requestedTimestamp == null || ticksPerSecond <= 0)
        {
            return false;
        }

        var elapsedMilliseconds = (now - _requestedTimestamp.Value) * 1000.0 / ticksPerSecond;
        if (elapsedMilliseconds >= 0 && elapsedMilliseconds <= MaxPendingMilliseconds)
        {
            return true;
        }

        _requestedTimestamp = null;
        return false;
    }
}
