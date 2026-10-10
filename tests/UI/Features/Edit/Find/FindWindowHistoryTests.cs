using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

    private static System.Collections.Generic.List<MenuItem> HistoryEntries(MenuFlyout flyout) =>
        flyout.Items.OfType<MenuItem>().Where(m => m.CommandParameter is string).ToList();

    [AvaloniaFact]
    public void HistoryFlyout_DeleteIcon_RemovesEntry_AndClearRemovesAll()
    {
        var (window, historyButton) = BuildShownWindow();

        try
        {
            var flyout = Assert.IsType<MenuFlyout>(historyButton.Flyout);
            var vm = (FindViewModel)historyButton.DataContext!;

            var fooEntry = HistoryEntries(flyout).First(m => (string)m.CommandParameter! == "foo");
            var deleteButton = ((Grid)fooEntry.Header!).Children.OfType<Button>().Single();
            deleteButton.Command!.Execute(deleteButton.CommandParameter);

            Assert.Equal(new[] { "bar" }, vm.SearchHistory);
            Assert.Equal(new[] { "bar" }, HistoryEntries(flyout).Select(m => (string)m.CommandParameter!));

            var clearItem = flyout.Items.OfType<MenuItem>().Single(m => m.CommandParameter == null);
            clearItem.Command!.Execute(null);

            Assert.Empty(vm.SearchHistory);
            Assert.Empty(flyout.Items);
            Assert.False(historyButton.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FindService_RemoveFromSearchHistory_UpdatesSettings()
    {
        var saved = Nikse.SubtitleEdit.Logic.Config.Se.Settings.Tools.FindHistory;
        try
        {
            Nikse.SubtitleEdit.Logic.Config.Se.Settings.Tools.FindHistory = new System.Collections.Generic.List<string> { "foo", "bar" };
            var findService = new Nikse.SubtitleEdit.Logic.FindService();

            findService.RemoveFromSearchHistory("foo");

            Assert.Equal(new[] { "bar" }, Nikse.SubtitleEdit.Logic.Config.Se.Settings.Tools.FindHistory);
        }
        finally
        {
            Nikse.SubtitleEdit.Logic.Config.Se.Settings.Tools.FindHistory = saved;
        }
    }

    [AvaloniaFact]
    public void HistoryFlyout_ClickDeleteIcon_KeepsSearchTextAndMenuOpen()
    {
        var (window, historyButton) = BuildShownWindow();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var vm = (FindViewModel)historyButton.DataContext!;
            vm.SearchText = "typed";
            var flyout = Assert.IsType<MenuFlyout>(historyButton.Flyout);
            flyout.ShowAt(historyButton);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            var fooEntry = HistoryEntries(flyout).First(m => (string)m.CommandParameter! == "foo");
            var deleteButton = ((Grid)fooEntry.Header!).Children.OfType<Button>().Single();
            var root = TopLevel.GetTopLevel(deleteButton)!;
            var center = deleteButton.TranslatePoint(new Point(deleteButton.Bounds.Width / 2, deleteButton.Bounds.Height / 2), (Visual)root)!.Value;
            root.MouseDown(center, MouseButton.Left);
            root.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { "bar" }, vm.SearchHistory);
            Assert.Equal("typed", vm.SearchText);
            Assert.True(flyout.IsOpen, "menu should stay open after deleting an entry");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SearchBoxDropDown_ClickDeleteIcon_RemovesEntryAndStaysOpen()
    {
        var (window, historyButton) = BuildShownWindow();

        try
        {
            var vm = (FindViewModel)historyButton.DataContext!;
            var searchBox = window.GetVisualDescendants().OfType<AutoCompleteBox>().First();
            searchBox.Focus();
            searchBox.IsDropDownOpen = true;
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            var deleteButton = window.GetVisualDescendants().OfType<ListBox>()
                .SelectMany(l => l.GetVisualDescendants().OfType<Button>())
                .First(b => (string?)b.CommandParameter == "foo");
            var center = deleteButton.TranslatePoint(new Point(deleteButton.Bounds.Width / 2, deleteButton.Bounds.Height / 2), window)!.Value;
            window.MouseDown(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { "bar" }, vm.SearchHistory);
            Assert.Equal(string.Empty, vm.SearchText);
            Assert.True(searchBox.IsDropDownOpen, "drop-down should stay open after deleting an entry");
        }
        finally
        {
            window.Close();
        }
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
            Assert.Equal(2, HistoryEntries(flyout).Count);

            // Simulate InitializeFindData refreshing the history after window construction.
            var vm = (FindViewModel)historyButton.DataContext!;
            vm.SearchHistory.Add("baz");

            Assert.Equal(3, HistoryEntries(flyout).Count);
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
            Assert.Equal(2, HistoryEntries(flyout).Count);
        }
        finally
        {
            window.Close();
        }
    }
}
