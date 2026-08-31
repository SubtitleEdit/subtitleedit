using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Logic;

// The undocked tool windows sit above the main window in the process' z-order (they are topmost
// while SE is active, and dropping topmost on Windows re-inserts them at the top of the
// non-topmost band). So when the application the user switched to closes, the OS hands the
// foreground to the audio visualizer instead of the main window the user left (#14168).
// MainViewModel.IsUndockedActivationAimedAtToolWindow classifies who caused a tool-window
// activation, and MainViewModel.ShouldHandForegroundBackToMainWindow decides whether an
// OS-caused one is corrected.
public class UndockedForegroundStealTests
{
    private static bool Decide(
        bool undockedActive = true,
        bool mainActive = false,
        bool mainMinimized = false,
        bool modalOpen = false)
    {
        return MainViewModel.ShouldHandForegroundBackToMainWindow(
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
    public void DoesNothing_WhenTheForegroundAlreadyMovedOn()
    {
        // The beat between activation and decision is long enough for the user to have clicked
        // into the main window, or for the foreground to have bounced to the other tool window -
        // whose own Activated handler owns the decision then.
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

    [Fact]
    public void ActivationIsAimed_WhenAPointerButtonIsDown()
    {
        // A click that activates a window (client area or title bar, including a drag by the
        // title bar) still has the button physically down while the activation is delivered.
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: true, pointerOverToolWindow: false));
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: true, pointerOverToolWindow: true));
    }

    [Fact]
    public void ActivationIsNotAimed_WhenTheCursorMerelyRestsOverTheToolWindow()
    {
        // Closing another application's window exposes the tool window under a cursor that never
        // moved (Windows synthesizes the mouse-move that flips IsPointerOver), so hover must not
        // veto the correction when the button state says no click happened - the beta 26 failure.
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: true));
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: false));
    }

    [Fact]
    public void HoverDecides_OnlyWhereTheButtonsCannotBeSampled()
    {
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: null, pointerOverToolWindow: true));
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: null, pointerOverToolWindow: false));
    }
}
