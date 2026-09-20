using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// The undocked tool windows (audio visualizer / video player) are topmost while SE is
// active, which covered the main window's popups and stole OS activation from modal
// dialogs (#13325). WindowService.SuspendUndockedTopmost drops their topmost for the
// lifetime of whatever would be covered; the suspension is ref-counted so nested users
// (menu -> dialog -> message box) restore only when the last one closes.
public class SuspendUndockedTopmostTests : IDisposable
{
    private readonly List<bool> _calls = [];

    public SuspendUndockedTopmostTests()
    {
        // Earlier tests may have opened a menu or dialog and torn the window down without the
        // matching Closed event, leaving the static suspend count non-zero.
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.RegisterUndockedTopmostSetter(_calls.Add);
    }

    public void Dispose()
    {
        WindowService.RegisterUndockedTopmostSetter(null);
        WindowService.ResetUndockedTopmostSuspensionsForTests();
    }

    [Fact]
    public void SingleSuspension_SuppressesAndRestores()
    {
        var suspension = WindowService.SuspendUndockedTopmost();
        Assert.Equal([false], _calls);

        suspension.Dispose();
        Assert.Equal([false, true], _calls);
    }

    [Fact]
    public void NestedSuspensions_RestoreOnlyWhenLastOneIsDisposed()
    {
        // Menu opens, a dialog is launched from a menu item, then the menu closes while
        // the dialog is still up - the topmost state must stay suppressed until the
        // dialog closes too.
        var menu = WindowService.SuspendUndockedTopmost();
        var dialog = WindowService.SuspendUndockedTopmost();
        Assert.Equal([false], _calls);

        menu.Dispose();
        Assert.Equal([false], _calls);

        dialog.Dispose();
        Assert.Equal([false, true], _calls);
    }

    [Fact]
    public void DisposingTwice_DoesNotUnbalanceTheCount()
    {
        var outer = WindowService.SuspendUndockedTopmost();
        var inner = WindowService.SuspendUndockedTopmost();

        inner.Dispose();
        inner.Dispose();
        Assert.Equal([false], _calls);

        outer.Dispose();
        Assert.Equal([false, true], _calls);
    }

    [Fact]
    public void LiveQuery_FollowsOutstandingSuspensions()
    {
        Assert.False(WindowService.IsUndockedTopmostSuspended);

        var suspension = WindowService.SuspendUndockedTopmost();
        Assert.True(WindowService.IsUndockedTopmostSuspended);

        suspension.Dispose();
        Assert.False(WindowService.IsUndockedTopmostSuspended);
    }

    [Fact]
    public void SuspensionWhoseOwnerClosedWithoutReleasing_IsHealedOnTheNextQuery()
    {
        // A menu that raised Opened but never Closed (#14622): the token stays undisposed, but
        // its owner reports closed, so the next consultation must drop it and restore topmost.
        var menuIsOpen = true;
        _ = WindowService.SuspendUndockedTopmost(() => menuIsOpen, "test menu");
        Assert.True(WindowService.IsUndockedTopmostSuspended);
        Assert.Equal([false], _calls);

        menuIsOpen = false;
        Assert.False(WindowService.IsUndockedTopmostSuspended);
        Assert.Equal([false, true], _calls);
    }

    [Fact]
    public void LeakedSuspension_IsHealedWhenAnotherSuspensionIsReleased()
    {
        // The reporter's Ctrl+N after the leak: the "save changes?" prompt takes and releases
        // its own suspension, and that release must be enough to bring the tool windows back.
        var flyoutIsOpen = true;
        _ = WindowService.SuspendUndockedTopmost(() => flyoutIsOpen, "test flyout");
        flyoutIsOpen = false;

        var prompt = WindowService.SuspendUndockedTopmost();
        Assert.Equal([false], _calls);

        prompt.Dispose();
        Assert.Equal([false, true], _calls);
    }

    [Fact]
    public void SuspensionTakenBeforeItsOwnerOpens_IsNotTreatedAsLeaked()
    {
        // Flyouts take the suspension at Opening, before IsOpen flips - "not open yet" must not
        // read as "closed without releasing".
        var flyoutIsOpen = false;
        var suspension = WindowService.SuspendUndockedTopmost(() => flyoutIsOpen, "test flyout");

        Assert.True(WindowService.IsUndockedTopmostSuspended);
        Assert.Equal([false], _calls);

        flyoutIsOpen = true;
        Assert.True(WindowService.IsUndockedTopmostSuspended);

        suspension.Dispose();
        Assert.Equal([false, true], _calls);
    }

    [Fact]
    public void SuspensionWithoutLivenessCheck_IsNeverHealedAway()
    {
        // Modal dialogs hold their token in a using block; nothing but its disposal may end it.
        var dialog = WindowService.SuspendUndockedTopmost();
        Assert.True(WindowService.IsUndockedTopmostSuspended);
        Assert.True(WindowService.IsUndockedTopmostSuspended);
        Assert.Equal([false], _calls);

        dialog.Dispose();
        Assert.Equal([false, true], _calls);
    }
}
