using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// Batch convert runs SE4-style: the main window (plus the undocked tool windows) is hidden
// while the tool window is open and comes back when it closes (#14502). It can't be an
// owned modal - Avalonia's Hide() cascades to owned windows and ShowDialog refuses a hidden
// owner - so WindowService.ShowWithWindowsHiddenAsync shows an unowned top-level window and
// owns the hide/re-show bookkeeping.
public class ShowWithWindowsHiddenTests
{
    [AvaloniaFact]
    public void HidesTheOwnerWhileOpen_AndBringsItBackActivatedOnClose()
    {
        var owner = new Window();
        owner.Show();

        var tool = new Window();
        var task = WindowService.ShowWithWindowsHiddenAsync(tool, [owner], activateAfterwards: owner);
        Dispatcher.UIThread.RunJobs();

        Assert.True(tool.IsVisible);
        Assert.False(owner.IsVisible);
        Assert.Null(tool.Owner);
        Assert.False(task.IsCompleted);

        tool.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(owner.IsVisible);
        Assert.True(owner.IsActive);
    }

    [AvaloniaFact]
    public void VisibleCompanionsAreHiddenAndRestored_HiddenOnesStayHidden()
    {
        var owner = new Window();
        owner.Show();
        var shownCompanion = new Window();
        shownCompanion.Show();
        var hiddenCompanion = new Window(); // never shown, e.g. video not undocked

        var tool = new Window();
        var task = WindowService.ShowWithWindowsHiddenAsync(
            tool, [owner, shownCompanion, hiddenCompanion, null], activateAfterwards: owner);
        Dispatcher.UIThread.RunJobs();

        Assert.False(owner.IsVisible);
        Assert.False(shownCompanion.IsVisible);
        Assert.False(hiddenCompanion.IsVisible);

        tool.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(owner.IsVisible);
        Assert.True(shownCompanion.IsVisible);
        Assert.False(hiddenCompanion.IsVisible);
    }

    [AvaloniaFact]
    public void MinimizedOwnerIsRestoredToNormal()
    {
        var owner = new Window();
        owner.Show();
        owner.WindowState = WindowState.Minimized;

        var tool = new Window();
        var task = WindowService.ShowWithWindowsHiddenAsync(tool, [owner], activateAfterwards: owner);
        Dispatcher.UIThread.RunJobs();

        tool.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(owner.IsVisible);
        Assert.Equal(WindowState.Normal, owner.WindowState);
    }
}
