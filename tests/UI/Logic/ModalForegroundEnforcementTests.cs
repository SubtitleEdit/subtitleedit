using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// A modal dialog's owner is input-disabled, yet keyboard focus kept ending up on it - the
// dialog drawn on top with gray buttons while every key landed in the window underneath
// (#13325/#13398/#13405). WindowService.ShowModalAsync enforces the invariant for the
// dialog's whole lifetime: whenever the owner ends up active while the dialog is open,
// activation is handed back to the dialog. These tests drive the platform activation
// callbacks directly to simulate the OS foreground churn that steals activation.
public class ModalForegroundEnforcementTests : IDisposable
{
    public ModalForegroundEnforcementTests()
    {
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.ResetOpenModalsForTests();
    }

    public void Dispose()
    {
        WindowService.RegisterUndockedTopmostSetter(null);
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.ResetOpenModalsForTests();
    }

    // Raises the same platform callbacks the OS raises when it moves foreground between two
    // windows - Deactivated of the loser can fire before Activated of the winner, which is
    // the order the enforcement has to cope with. The callback properties' accessors are
    // internal to Avalonia, so go through reflection.
    private static void RaisePlatformEvent(Window window, string callbackName)
    {
        var impl = window.PlatformImpl;
        var property = impl?.GetType()
            .GetProperties(System.Reflection.BindingFlags.Instance |
                           System.Reflection.BindingFlags.Public |
                           System.Reflection.BindingFlags.NonPublic)
            .FirstOrDefault(p => p.Name == callbackName || p.Name.EndsWith("." + callbackName));
        Assert.NotNull(property);

        (property.GetValue(impl) as Action)?.Invoke();
    }

    private static void SimulateOsForegroundMove(Window from, Window to)
    {
        RaisePlatformEvent(from, "Deactivated");
        RaisePlatformEvent(to, "Activated");
    }

    private static (Window Owner, Window Dialog, Task DialogTask) OpenModal()
    {
        var owner = new Window();
        owner.Show();

        var dialog = new Window();
        var dialogTask = WindowService.ShowModalAsync(owner, dialog);
        Dispatcher.UIThread.RunJobs();

        Assert.True(dialog.IsVisible);
        return (owner, dialog, dialogTask);
    }

    [AvaloniaFact]
    public void IsModalDialogOpen_TracksTheDialogLifetime()
    {
        Assert.False(WindowService.IsModalDialogOpen);

        var (_, dialog, dialogTask) = OpenModal();
        Assert.True(WindowService.IsModalDialogOpen);

        dialog.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(dialogTask.IsCompletedSuccessfully);
        Assert.False(WindowService.IsModalDialogOpen);
    }

    [AvaloniaFact]
    public void ShowModalAsyncWithResult_ReturnsTheDialogResult()
    {
        var owner = new Window();
        owner.Show();

        var dialog = new Window();
        var dialogTask = WindowService.ShowModalAsync<string>(owner, dialog);
        Dispatcher.UIThread.RunJobs();

        dialog.Close("the result");
        Dispatcher.UIThread.RunJobs();

        Assert.True(dialogTask.IsCompletedSuccessfully);
        Assert.Equal("the result", dialogTask.Result);
        Assert.False(WindowService.IsModalDialogOpen);
    }

    [AvaloniaFact]
    public void OwnerActivatedWhileModalIsOpen_HandsActivationBackToTheDialog()
    {
        var (owner, dialog, _) = OpenModal();
        Assert.True(dialog.IsActive);

        // The churn: the OS puts foreground on the input-disabled owner (#13405).
        SimulateOsForegroundMove(from: dialog, to: owner);
        Assert.False(dialog.IsActive);

        Dispatcher.UIThread.RunJobs();

        Assert.True(dialog.IsActive);
    }

    [AvaloniaFact]
    public void DialogDeactivatedToAnotherApplication_IsLeftAlone()
    {
        var (owner, dialog, _) = OpenModal();
        RaisePlatformEvent(owner, "Deactivated");

        // Foreground went to another application: no SE window is active, so the
        // enforcement must not fight the user over it.
        RaisePlatformEvent(dialog, "Deactivated");
        Dispatcher.UIThread.RunJobs();

        Assert.False(dialog.IsActive);
        Assert.False(owner.IsActive);
    }

    [AvaloniaFact]
    public void EnforcementStops_WhenTheDialogCloses()
    {
        var (owner, dialog, dialogTask) = OpenModal();

        dialog.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.True(dialogTask.IsCompletedSuccessfully);

        // Re-activating the owner after the close must not touch the dead dialog.
        RaisePlatformEvent(owner, "Activated");
        Dispatcher.UIThread.RunJobs();

        Assert.False(dialog.IsActive);
    }

    [AvaloniaFact]
    public void ShowModalAsync_SuspendsUndockedTopmostForTheDialogLifetime()
    {
        var calls = new List<bool>();
        WindowService.RegisterUndockedTopmostSetter(calls.Add);

        var (_, dialog, _) = OpenModal();
        Assert.Equal([false], calls);

        dialog.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal([false, true], calls);
    }
}
