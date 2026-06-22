using System;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

public class AutoSaveDebounceTests
{
    private static readonly DateTime Now = new(2026, 6, 22, 12, 0, 0, DateTimeKind.Utc);
    private const double Idle = 1.5;

    [Fact]
    public void NoChanges_Skips()
    {
        var action = AutoSaveDebounce.Decide(
            mainDirty: false, originalDirty: false, settleHash: 123, previousSettleHash: 123,
            lastChangeUtc: Now.AddSeconds(-10), nowUtc: Now, idleSeconds: Idle);

        Assert.Equal(AutoSaveDebounce.Action.Skip, action);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ContentChangedSinceLastTick_Arms(bool mainDirty, bool originalDirty)
    {
        var action = AutoSaveDebounce.Decide(
            mainDirty, originalDirty, settleHash: 999, previousSettleHash: 123,
            lastChangeUtc: Now.AddSeconds(-10), nowUtc: Now, idleSeconds: Idle);

        Assert.Equal(AutoSaveDebounce.Action.Arm, action);
    }

    [Fact]
    public void SameContentButNotIdleLongEnough_Skips()
    {
        var action = AutoSaveDebounce.Decide(
            mainDirty: true, originalDirty: false, settleHash: 123, previousSettleHash: 123,
            lastChangeUtc: Now.AddSeconds(-1.0), nowUtc: Now, idleSeconds: Idle);

        Assert.Equal(AutoSaveDebounce.Action.Skip, action);
    }

    [Fact]
    public void SameContentAndIdleLongEnough_Saves()
    {
        var action = AutoSaveDebounce.Decide(
            mainDirty: true, originalDirty: false, settleHash: 123, previousSettleHash: 123,
            lastChangeUtc: Now.AddSeconds(-Idle), nowUtc: Now, idleSeconds: Idle);

        Assert.Equal(AutoSaveDebounce.Action.Save, action);
    }

    [Fact]
    public void DirtyButUnchanged_DoesNotReArm_SoIdleWindowCanElapse()
    {
        // Regression guard: a dirty-but-stable buffer must keep waiting (Skip), not re-Arm forever,
        // otherwise the idle window would never elapse and auto-save would never fire.
        var action = AutoSaveDebounce.Decide(
            mainDirty: true, originalDirty: true, settleHash: 42, previousSettleHash: 42,
            lastChangeUtc: Now.AddSeconds(-0.5), nowUtc: Now, idleSeconds: Idle);

        Assert.Equal(AutoSaveDebounce.Action.Skip, action);
    }
}
