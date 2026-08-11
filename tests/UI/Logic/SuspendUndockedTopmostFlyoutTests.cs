using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The flyout overload of WindowService.SuspendUndockedTopmostWhileOpen must take the
/// suspension at Opening, before the flyout's popup window exists. Opened fires after the
/// popup is already on screen, and on Windows demoting a topmost window re-inserts it at
/// the top of the non-topmost band - above the popup it was supposed to uncover, so the
/// undocked audio visualizer kept covering the subtitle grid's context menu (#13493).
/// </summary>
public class SuspendUndockedTopmostFlyoutTests : IDisposable
{
    private readonly List<bool> _calls = [];
    private readonly List<Window> _windows = [];

    public SuspendUndockedTopmostFlyoutTests()
    {
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.RegisterUndockedTopmostSetter(_calls.Add);
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        WindowService.RegisterUndockedTopmostSetter(null);
        WindowService.ResetUndockedTopmostSuspensionsForTests();
    }

    private Button ShowWindowWithTarget()
    {
        var target = new Button { Content = "target" };
        var window = new Window { Width = 400, Height = 300, Content = target };
        _windows.Add(window);
        window.Show();
        return target;
    }

    [AvaloniaFact]
    public void SuspensionEngagesAtOpening_BeforeThePopupExists_AndRestoresOnClose()
    {
        var target = ShowWindowWithTarget();
        var flyout = new MenuFlyout { Items = { new MenuItem { Header = "item" } } };

        WindowService.SuspendUndockedTopmostWhileOpen(flyout);

        // Subscribed after the helper, so the helper's Opening handler has already run when
        // this one observes the calls - and the popup itself has not been created yet.
        List<bool>? callsAtOpening = null;
        flyout.Opening += (_, _) => callsAtOpening = [.. _calls];

        flyout.ShowAt(target);

        Assert.True(flyout.IsOpen);
        Assert.Equal([false], callsAtOpening);

        flyout.Hide();
        Assert.Equal([false, true], _calls);
    }

    [AvaloniaFact]
    public void ReopeningTakesAFreshSuspension()
    {
        var target = ShowWindowWithTarget();
        var flyout = new MenuFlyout { Items = { new MenuItem { Header = "item" } } };

        WindowService.SuspendUndockedTopmostWhileOpen(flyout);

        flyout.ShowAt(target);
        flyout.Hide();
        flyout.ShowAt(target);

        Assert.Equal([false, true, false], _calls);

        flyout.Hide();
        Assert.Equal([false, true, false, true], _calls);
    }

    [AvaloniaFact]
    public void CancelledOpenDoesNotLeakTheSuspension()
    {
        var target = ShowWindowWithTarget();
        var flyout = new CancellingMenuFlyout { Items = { new MenuItem { Header = "item" } } };

        WindowService.SuspendUndockedTopmostWhileOpen(flyout);

        flyout.ShowAt(target);
        Assert.False(flyout.IsOpen);

        // Closed never fires for a cancelled open; the deferred backstop must release the
        // suspension so the tool windows do not stay non-topmost forever.
        Dispatcher.UIThread.RunJobs();
        Assert.Equal([false, true], _calls);
    }

    private sealed class CancellingMenuFlyout : MenuFlyout
    {
        protected override void OnOpening(CancelEventArgs args)
        {
            // Raise Opening for subscribers first (the helper takes its suspension there),
            // then cancel - the worst case for a leak.
            base.OnOpening(args);
            args.Cancel = true;
        }
    }
}
