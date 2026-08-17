using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    // OS activation is only half the invariant: Avalonia delivers key presses to the app-global
    // focused element regardless of which window the OS sent them to, so a deferred Focus() call
    // into the owner (the menu bar's post-close restore, a posted grid focus) re-routes the
    // keyboard under the dialog while its title bar still looks active (#13405 beta-9 feedback).
    // These tests cover the focus half.

    private static (Window Owner, TextBox OwnerTextBox, Window Dialog, TextBox DialogTextBox, Task DialogTask) OpenModalWithFocusableContent()
    {
        var ownerTextBox = new TextBox();
        var owner = new Window { Content = ownerTextBox };
        owner.Show();

        var dialogTextBox = new TextBox();
        var dialog = new Window { Content = dialogTextBox };
        var dialogTask = WindowService.ShowModalAsync(owner, dialog);
        Dispatcher.UIThread.RunJobs();
        Assert.True(dialog.IsVisible);

        dialogTextBox.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(dialogTextBox.IsFocused);

        return (owner, ownerTextBox, dialog, dialogTextBox, dialogTask);
    }

    [AvaloniaFact]
    public void FocusStolenIntoTheOwnerWhileModalIsOpen_IsHandedBackToTheDialog()
    {
        var (_, ownerTextBox, _, dialogTextBox, _) = OpenModalWithFocusableContent();

        // The steal: a deferred focus restore lands on the owner after the dialog opened.
        ownerTextBox.Focus();
        Assert.True(ownerTextBox.IsFocused);

        Dispatcher.UIThread.RunJobs();

        Assert.False(ownerTextBox.IsFocused);
        Assert.True(dialogTextBox.IsFocused);
    }

    [AvaloniaFact]
    public void FocusReclaim_ReturnsToTheControlTheDialogLastFocused()
    {
        var firstBox = new TextBox();
        var secondBox = new TextBox();
        var ownerTextBox = new TextBox();
        var owner = new Window { Content = ownerTextBox };
        owner.Show();

        var dialog = new Window { Content = new StackPanel { Children = { firstBox, secondBox } } };
        _ = WindowService.ShowModalAsync(owner, dialog);
        Dispatcher.UIThread.RunJobs();

        secondBox.Focus();
        Dispatcher.UIThread.RunJobs();

        ownerTextBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.True(secondBox.IsFocused); // not firstBox - the reclaim remembers the caret position
    }

    [AvaloniaFact]
    public void KeyDownReachingTheDisabledOwner_IsSwallowed()
    {
        var (_, ownerTextBox, _, _, _) = OpenModalWithFocusableContent();

        var keyDown = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Tab,
            Source = ownerTextBox,
        };
        ownerTextBox.RaiseEvent(keyDown);

        Assert.True(keyDown.Handled);
    }

    [AvaloniaFact]
    public void KeyDownInTheDialog_IsNotSwallowed()
    {
        var (_, _, _, dialogTextBox, _) = OpenModalWithFocusableContent();

        var keyDown = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.A,
            Source = dialogTextBox,
        };
        dialogTextBox.RaiseEvent(keyDown);

        Assert.False(keyDown.Handled);
    }

    [AvaloniaFact]
    public void FocusInAnIndependentEnabledWindow_IsLeftAlone()
    {
        var (_, _, _, dialogTextBox, _) = OpenModalWithFocusableContent();

        // An undocked tool window: enabled, independent, not part of the modal owner chain.
        var independentTextBox = new TextBox();
        var independent = new Window { Content = independentTextBox };
        independent.Show();

        independentTextBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.True(independentTextBox.IsFocused);
        Assert.False(dialogTextBox.IsFocused);
    }

    [AvaloniaFact]
    public void FocusStolenIntoTheOwner_WithNestedModals_GoesToTheTopDialog()
    {
        var (owner, ownerTextBox, dialog, _, _) = OpenModalWithFocusableContent();

        var topTextBox = new TextBox();
        var topDialog = new Window { Content = topTextBox };
        _ = WindowService.ShowModalAsync(dialog, topDialog);
        Dispatcher.UIThread.RunJobs();
        topTextBox.Focus();
        Dispatcher.UIThread.RunJobs();

        ownerTextBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.False(ownerTextBox.IsFocused);
        Assert.True(topTextBox.IsFocused);
        _ = owner;
    }

    [AvaloniaFact]
    public void FocusInTheOwnerAfterTheDialogCloses_IsLeftAlone()
    {
        var (_, ownerTextBox, dialog, _, dialogTask) = OpenModalWithFocusableContent();

        dialog.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.True(dialogTask.IsCompletedSuccessfully);

        ownerTextBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.True(ownerTextBox.IsFocused);
    }
}
