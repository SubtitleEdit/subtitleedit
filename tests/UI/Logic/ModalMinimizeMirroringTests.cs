using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// A modal's owner is input-disabled, so its own caption buttons are dead - on Windows the user
// could minimize e.g. the batch convert dialog from its taskbar button, and the main window then
// stayed on screen, unminimizable, blocking the desktop (#13788). WindowService.ShowModalAsync
// mirrors minimize/restore between the dialog and its owner: minimizing either sends the whole
// pair to the taskbar, restoring either brings both back. The undocked tool windows are
// independent (never in the owner chain) and are not part of the mirror.
public class ModalMinimizeMirroringTests : IDisposable
{
    public ModalMinimizeMirroringTests()
    {
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.ResetOpenModalsForTests();
    }

    public void Dispose()
    {
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.ResetOpenModalsForTests();
    }

    private static (Window Owner, Window Dialog, Task DialogTask) OpenModal(WindowState ownerState = WindowState.Normal)
    {
        var owner = new Window { WindowState = ownerState };
        owner.Show();

        var dialog = new Window();
        var dialogTask = WindowService.ShowModalAsync(owner, dialog);
        Dispatcher.UIThread.RunJobs();

        Assert.True(dialog.IsVisible);
        return (owner, dialog, dialogTask);
    }

    // Raises the platform activation callback the OS raises when it moves foreground - the
    // accessors are internal to Avalonia, so go through reflection (same technique as
    // ModalForegroundEnforcementTests).
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

    [AvaloniaFact]
    public void MinimizingTheDialog_MinimizesTheOwner()
    {
        var (owner, dialog, _) = OpenModal();

        dialog.WindowState = WindowState.Minimized;

        Assert.Equal(WindowState.Minimized, owner.WindowState);
    }

    [AvaloniaFact]
    public void RestoringTheDialog_RestoresTheOwner()
    {
        var (owner, dialog, _) = OpenModal();

        dialog.WindowState = WindowState.Minimized;
        dialog.WindowState = WindowState.Normal;

        Assert.Equal(WindowState.Normal, owner.WindowState);
    }

    [AvaloniaFact]
    public void RestoringTheDialog_RestoresAMaximizedOwnerToMaximized()
    {
        var (owner, dialog, _) = OpenModal(WindowState.Maximized);

        dialog.WindowState = WindowState.Minimized;
        Assert.Equal(WindowState.Minimized, owner.WindowState);

        dialog.WindowState = WindowState.Normal;

        Assert.Equal(WindowState.Maximized, owner.WindowState);
    }

    [AvaloniaFact]
    public void MinimizingTheOwner_MinimizesTheDialog()
    {
        var (owner, dialog, _) = OpenModal();

        owner.WindowState = WindowState.Minimized;

        Assert.Equal(WindowState.Minimized, dialog.WindowState);
    }

    [AvaloniaFact]
    public void RestoringTheOwner_RestoresTheDialog()
    {
        var (owner, dialog, _) = OpenModal();

        owner.WindowState = WindowState.Minimized;
        owner.WindowState = WindowState.Normal;

        Assert.Equal(WindowState.Normal, dialog.WindowState);
    }

    [AvaloniaFact]
    public void DialogClosedWhileMinimized_RestoresTheOwner()
    {
        var (owner, dialog, dialogTask) = OpenModal();

        // The user minimized the dialog (owner mirror-minimized), then the dialog closes -
        // e.g. a finished batch job. The dialog's taskbar button is gone, so the owner must
        // come back rather than stay minimized.
        dialog.WindowState = WindowState.Minimized;
        dialog.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(dialogTask.IsCompletedSuccessfully);
        Assert.Equal(WindowState.Normal, owner.WindowState);
    }

    [AvaloniaFact]
    public void DialogClosedAfterTheUserMinimizedTheOwner_LeavesTheOwnerMinimized()
    {
        var (owner, dialog, dialogTask) = OpenModal();

        // Here the minimize was the user's explicit action on the owner itself - the close
        // must not override that choice.
        owner.WindowState = WindowState.Minimized;
        dialog.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(dialogTask.IsCompletedSuccessfully);
        Assert.Equal(WindowState.Minimized, owner.WindowState);
    }

    [AvaloniaFact]
    public void NestedModals_MinimizeAndRestoreCascadeThroughTheChain()
    {
        var (owner, dialog, _) = OpenModal();

        var topDialog = new Window();
        _ = WindowService.ShowModalAsync(dialog, topDialog);
        Dispatcher.UIThread.RunJobs();

        topDialog.WindowState = WindowState.Minimized;
        Assert.Equal(WindowState.Minimized, dialog.WindowState);
        Assert.Equal(WindowState.Minimized, owner.WindowState);

        topDialog.WindowState = WindowState.Normal;
        Assert.Equal(WindowState.Normal, dialog.WindowState);
        Assert.Equal(WindowState.Normal, owner.WindowState);
    }

    [AvaloniaFact]
    public void OwnerActivatedWhileTheDialogIsMinimized_DoesNotPopTheDialogBackUp()
    {
        var (owner, dialog, _) = OpenModal();

        dialog.WindowState = WindowState.Minimized;

        // The foreground churn right after the minimize: the OS briefly hands activation to
        // the owner. The modal foreground enforcement must not answer with dialog.Activate(),
        // which would restore the window the user just minimized.
        RaisePlatformEvent(dialog, "Deactivated");
        RaisePlatformEvent(owner, "Activated");
        Dispatcher.UIThread.RunJobs();

        Assert.False(dialog.IsActive);
        Assert.Equal(WindowState.Minimized, dialog.WindowState);
    }

    [AvaloniaFact]
    public void MirrorStops_WhenTheDialogCloses()
    {
        var (owner, dialog, dialogTask) = OpenModal();

        dialog.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.True(dialogTask.IsCompletedSuccessfully);

        // Minimizing the owner after the close must not touch the dead dialog.
        owner.WindowState = WindowState.Minimized;

        Assert.Equal(WindowState.Normal, dialog.WindowState);
    }
}
