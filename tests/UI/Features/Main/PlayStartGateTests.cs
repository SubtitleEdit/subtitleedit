using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Xunit;

namespace UITests.Features.Main;

/// <summary>
/// mpv reports its pause state through an asynchronous property-change event, so IsPlaying still
/// reads "paused" for a tick or two after a play command. The position timer treats "not playing"
/// as "playback stopped" and drops the play-selection stop point - and landing in that window
/// dropped a stop point set microseconds earlier, so "go to video position, play current, and
/// pause" ran on past the end of the line (#14167). This gate marks that window.
/// </summary>
public class PlayStartGateTests
{
    private const long TicksPerSecond = 1_000_000;

    private static long At(double milliseconds) => (long)(milliseconds * TicksPerSecond / 1000.0);

    [Fact]
    public void NothingIsPendingBeforeAnyPlayIsRequested()
    {
        var gate = new PlayStartGate();

        Assert.False(gate.IsPending(At(0), TicksPerSecond));
    }

    /// <summary>The window this whole class exists for: play asked, player still says paused.</summary>
    [Fact]
    public void APlayJustRequestedIsPending()
    {
        var gate = new PlayStartGate();
        gate.PlayRequested(At(100));

        Assert.True(gate.IsPending(At(100), TicksPerSecond));
        Assert.True(gate.IsPending(At(150), TicksPerSecond));
    }

    [Fact]
    public void OnceThePlayerReportsPlayingNothingIsHeldBack()
    {
        var gate = new PlayStartGate();
        gate.PlayRequested(At(100));
        gate.PlayingObserved();

        Assert.False(gate.IsPending(At(101), TicksPerSecond));
    }

    /// <summary>
    /// A real pause after playback started must take effect on the very next tick, not wait out
    /// the gate - otherwise pausing would stop clearing the play selection.
    /// </summary>
    [Fact]
    public void APauseRequestSupersedesAPendingPlay()
    {
        var gate = new PlayStartGate();
        gate.PlayRequested(At(100));
        gate.PauseRequested();

        Assert.False(gate.IsPending(At(101), TicksPerSecond));
    }

    /// <summary>
    /// A play that is never honoured (no file loaded, a player that ignored the command) must not
    /// hold the play selection alive forever.
    /// </summary>
    [Fact]
    public void APlayThatIsNeverObservedExpires()
    {
        var gate = new PlayStartGate();
        gate.PlayRequested(At(0));

        Assert.True(gate.IsPending(At(PlayStartGate.MaxPendingMilliseconds), TicksPerSecond));
        Assert.False(gate.IsPending(At(PlayStartGate.MaxPendingMilliseconds + 1), TicksPerSecond));
        // ...and stays expired.
        Assert.False(gate.IsPending(At(PlayStartGate.MaxPendingMilliseconds + 2), TicksPerSecond));
    }

    /// <summary>Re-arming after an expiry starts a fresh window rather than staying expired.</summary>
    [Fact]
    public void ARequestAfterAnExpiryIsPendingAgain()
    {
        var gate = new PlayStartGate();
        gate.PlayRequested(At(0));
        Assert.False(gate.IsPending(At(5000), TicksPerSecond));

        gate.PlayRequested(At(5000));

        Assert.True(gate.IsPending(At(5010), TicksPerSecond));
    }
}
