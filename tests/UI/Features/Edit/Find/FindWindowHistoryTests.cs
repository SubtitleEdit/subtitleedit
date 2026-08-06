using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Edit.Find;

namespace UITests.Features.Edit.Find;

/// <summary>
/// The find-history flyout must have its menu items built before the flyout opens:
/// items added from the Opening event come too late for the popup's initial measure,
/// which made the menu display as an empty sliver on Windows (PR #12409 follow-up).
/// </summary>
public class FindWindowHistoryTests
{
    private static (FindWindow window, Button historyButton) BuildShownWindow()
    {
        var vm = new FindViewModel();
        vm.SearchHistory.Add("foo");
        vm.SearchHistory.Add("bar");

        var window = new FindWindow(vm);
        window.Show();
        // Settle the window's Opened callbacks (UiUtil posts a working-area clamp at
        // Background priority) while the window is alive, so closing it afterwards
        // cannot hit the disposed platform implementation during session teardown.
        Dispatcher.UIThread.RunJobs();

        var historyButton = window.GetLogicalDescendants().OfType<Button>().First(b => b.Flyout != null);
        return (window, historyButton);
    }

    [AvaloniaFact]
    public void HistoryButton_IsVisible_WhenHistoryExists()
    {
        var (window, historyButton) = BuildShownWindow();

        try
        {
            Assert.True(historyButton.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HistoryButton_IsHidden_WhenHistoryIsEmpty()
    {
        var vm = new FindViewModel();
        var window = new FindWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var historyButton = window.GetLogicalDescendants().OfType<Button>().First(b => b.Flyout != null);

            Assert.False(historyButton.IsVisible);

            // First search lands in history -> button appears.
            vm.SearchHistory.Add("foo");
            Assert.True(historyButton.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HistoryFlyout_HasItemsBeforeOpening_AndFollowsHistoryChanges()
    {
        var (window, historyButton) = BuildShownWindow();

        try
        {
            var flyout = Assert.IsType<MenuFlyout>(historyButton.Flyout);
            Assert.Equal(2, flyout.Items.Count);

            // Simulate InitializeFindData refreshing the history after window construction.
            var vm = (FindViewModel)historyButton.DataContext!;
            vm.SearchHistory.Add("baz");

            Assert.Equal(3, flyout.Items.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HistoryButton_MouseClick_OpensFlyoutWithItems()
    {
        var (window, historyButton) = BuildShownWindow();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        try
        {
            var center = historyButton.TranslatePoint(
                new Point(historyButton.Bounds.Width / 2, historyButton.Bounds.Height / 2), window)!.Value;
            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left);

            var flyout = Assert.IsType<MenuFlyout>(historyButton.Flyout);
            Assert.True(flyout.IsOpen, "history flyout should open on click");
            Assert.Equal(2, flyout.Items.Count);
        }
        finally
        {
            window.Close();
        }
    }
}
