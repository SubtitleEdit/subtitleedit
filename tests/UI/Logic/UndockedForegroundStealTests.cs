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
            pointerButtonDown: true, pointerOverToolWindow: false, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: true, pointerOverToolWindow: true, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
    }

    [Fact]
    public void ActivationIsNotAimed_WhenTheCursorMerelyRestsOverTheToolWindow()
    {
        // Closing another application's window exposes the tool window under a cursor that never
        // moved (Windows synthesizes the mouse-move that flips IsPointerOver), so hover must not
        // veto the correction when the button state says no click happened - the beta 26 failure.
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: true, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: false, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
    }

    [Fact]
    public void HoverDecides_OnlyWhereTheButtonsCannotBeSampled()
    {
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: null, pointerOverToolWindow: true, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: null, pointerOverToolWindow: false, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
    }

    [Fact]
    public void ActivationIsAimed_WhenTheTaskSwitcherJustCommittedASwitch()
    {
        // Alt+Tab aims by keyboard: no button is down and the cursor can be anywhere, so the
        // pointer rules alone read it as an OS handover and bounced the tool window straight
        // back to the main window (#14354). The task switcher's SWITCHSTART/SWITCHEND WinEvents
        // override the pointer verdict.
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: false, taskSwitchJustCommitted: true,
            previousForegroundWindowStillUsable: null));
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: null, pointerOverToolWindow: false, taskSwitchJustCommitted: true,
            previousForegroundWindowStillUsable: null));
    }

    [Fact]
    public void ActivationIsAimed_WhenThePreviousForegroundWindowIsStillThere()
    {
        // Windows 11's XAML task switcher was not seen raising SWITCHSTART/SWITCHEND, so Alt+Tab
        // still read as an OS handover in beta 31 (#14354). What settles it without the switcher
        // announcing itself: the OS only has to pick a new foreground window when the old one went
        // away, so a previous window that is still alive and on screen means the user aimed here -
        // by Alt+Tab, the taskbar button, Alt+Esc or Win+Tab alike.
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: false, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: true));
    }

    [Fact]
    public void ActivationIsNotAimed_WhenThePreviousForegroundWindowWentAway()
    {
        // The #14168 case itself: the application the user switched to closed or was minimized, so
        // Windows handed the foreground to the frontmost window of this process - a tool window.
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: false, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: false));

        // ... and hover still must not veto it, the beta 26 failure.
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: true, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: false));
    }

    [Fact]
    public void AClickWinsOverAVanishedPreviousForegroundWindow()
    {
        // A button physically down while the activation is delivered is a click on this window,
        // and outranks the history: closing another application by clicking its X can leave the
        // button down over the tool window the close exposed.
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: true, pointerOverToolWindow: false, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: false));
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: false, taskSwitchJustCommitted: true,
            previousForegroundWindowStillUsable: false));
    }

    [Fact]
    public void FallsBackToThePointerRules_WhenTheForegroundHistoryIsUnknown()
    {
        // Null is "not known" - off Windows, with no hook installed, before the history has caught
        // up with the real foreground, or when the previous window is alive but not visible (a
        // helper or task-switcher island rather than anything a user did) - and must leave the
        // older rules in charge.
        Assert.False(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: false, pointerOverToolWindow: true, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
        Assert.True(MainViewModel.IsUndockedActivationAimedAtToolWindow(
            pointerButtonDown: null, pointerOverToolWindow: true, taskSwitchJustCommitted: false,
            previousForegroundWindowStillUsable: null));
    }
}
