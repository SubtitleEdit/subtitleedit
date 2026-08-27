using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Logic;

// The undocked tool windows sit above the main window in the process' z-order (they are topmost
// while SE is active, and dropping topmost on Windows re-inserts them at the top of the
// non-topmost band). So when the application the user switched to closes, the OS hands the
// foreground to the audio visualizer instead of the main window the user left (#14168).
// MainViewModel.ShouldHandForegroundBackToMainWindow is the rule that decides when to correct it.
public class UndockedForegroundStealTests
{
    private static bool Decide(
        bool pointerPressed = false,
        bool pointerOver = false,
        bool undockedActive = true,
        bool mainActive = false,
        bool mainMinimized = false,
        bool modalOpen = false)
    {
        return MainViewModel.ShouldHandForegroundBackToMainWindow(
            pointerPressed,
            pointerOver,
            undockedActive,
            mainActive,
            mainMinimized,
            modalOpen);
    }

    [Fact]
    public void HandsForegroundBack_WhenTheOsActivatedTheToolWindowByItself()
    {
        Assert.True(Decide());
    }

    [Fact]
    public void KeepsToolWindowInFront_WhenTheUserClickedIt()
    {
        // A click that activates a window is delivered after the activation on Windows, which is
        // why the decision waits a beat for the press instead of reading it at activation time.
        Assert.False(Decide(pointerPressed: true));
    }

    [Fact]
    public void KeepsToolWindowInFront_WhenThePointerIsOverIt()
    {
        Assert.False(Decide(pointerOver: true));
    }

    [Fact]
    public void DoesNothing_WhenTheForegroundAlreadyMovedOn()
    {
        Assert.False(Decide(undockedActive: false));
        Assert.False(Decide(mainActive: true));
    }

    [Fact]
    public void DoesNothing_WhenTheMainWindowIsMinimized()
    {
        // Activate() on a minimized window misfires - same guard as the startup case (#13569).
        Assert.False(Decide(mainMinimized: true));
    }

    [Fact]
    public void DoesNothing_WhileAModalDialogIsOpen()
    {
        // The modal owns the foreground; pulling it to the input-disabled main window is #13405.
        Assert.False(Decide(modalOpen: true));
    }
}
