using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class AltMenuActivationGuardTests
{
    [Fact]
    public void AltReleaseAfterAltPointerPressCancelsActivation()
    {
        var guard = new AltMenuActivationGuard();

        guard.NotifyPointerPressed(isAltDown: true, isMenuPress: false);

        Assert.True(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
    }

    [Fact]
    public void BareAltReleaseKeepsActivation()
    {
        var guard = new AltMenuActivationGuard();

        Assert.False(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
    }

    [Fact]
    public void PointerPressWithoutAltKeepsActivation()
    {
        var guard = new AltMenuActivationGuard();

        guard.NotifyPointerPressed(isAltDown: false, isMenuPress: false);

        Assert.False(guard.TryConsumeAltRelease(Key.RightAlt, out _));
    }

    [Fact]
    public void PressOnTheMenuItselfKeepsActivation()
    {
        var guard = new AltMenuActivationGuard();

        guard.NotifyPointerPressed(isAltDown: true, isMenuPress: true);

        Assert.False(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
    }

    [Fact]
    public void OnlyTheAltReleaseConsumesTheGuard()
    {
        var guard = new AltMenuActivationGuard();

        guard.NotifyPointerPressed(isAltDown: true, isMenuPress: false);

        Assert.False(guard.TryConsumeAltRelease(Key.A, out _));
        Assert.True(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
    }

    [Fact]
    public void ConsumingIsIdempotent()
    {
        var guard = new AltMenuActivationGuard();

        guard.NotifyPointerPressed(isAltDown: true, isMenuPress: false);

        // The window handler runs on both the tunnel and the bubble pass, so the second call must
        // not undo a menu activation the user just made on purpose.
        Assert.True(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
        Assert.False(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
    }

    [Fact]
    public void ResetDropsArmedState()
    {
        var guard = new AltMenuActivationGuard();

        guard.NotifyPointerPressed(isAltDown: true, isMenuPress: false);
        guard.Reset();

        Assert.False(guard.TryConsumeAltRelease(Key.LeftAlt, out _));
    }

    [Fact]
    public void FocusIsOnlyRecordedWhileArmed()
    {
        var guard = new AltMenuActivationGuard();

        // Not armed: a posted focus read from an earlier press must not stick around.
        guard.NotifyFocusAfterPointerPress(null);
        guard.NotifyPointerPressed(isAltDown: true, isMenuPress: false);

        Assert.True(guard.TryConsumeAltRelease(Key.LeftAlt, out var focusToRestore));
        Assert.Null(focusToRestore);
    }
}
