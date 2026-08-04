using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Keyboard activation of the main menu bar with an empty subtitle grid (#13111). Before a
/// subtitle is loaded the grid has no rows, and TableView itself is not focusable by default -
/// so the window had no focusable content at all. Keyboard focus either stayed on the window
/// root, where Avalonia's AccessKeyHandler ignores every key (bare Alt looked dead, no access
/// keys underlined), or it reached the menu bar and could never leave again because menu
/// deactivation had no focus target to restore to: Escape, F10 and Alt+Tab all appeared to
/// close the bar while arrow keys proved it was still armed. F10 activation was also invisible
/// (focus only, no "File" highlight); it now goes through Menu.Open like Avalonia's own
/// bare-Alt handling.
/// </summary>
public class MainMenuKeyboardActivationTests
{
    private static (Window Window, MainViewModel Vm) ShowMainWindowWithEmptyGrid()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;

        // On macOS the in-window menu is hidden in favor of the native menu bar; these tests
        // exercise the in-window menu (the Windows/Linux path), so show it regardless of the
        // host platform.
        vm.Menu.IsVisible = true;

        Settle(window);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>
    /// True when keyboard focus is on a menu item - the state in which the window key handler
    /// hands every key to the menu instead of running shortcuts.
    /// </summary>
    private static bool IsFocusOnMenuItem(Window window) =>
        window.FocusManager?.GetFocusedElement() is MenuItem;

    private static void PressAndRelease(Window window, PhysicalKey key, RawInputModifiers modifiers)
    {
        window.KeyPressQwerty(key, modifiers);
        Dispatcher.UIThread.RunJobs();
        window.KeyReleaseQwerty(key, modifiers);
        Settle(window);
    }

    [AvaloniaFact]
    public void EmptyGrid_IsFocusable_AndHoldsStartupFocus()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();

        // The startup focus is what keeps Avalonia's AccessKeyHandler alive: it ignores all
        // keyboard events (including bare Alt) while nothing inside the window has focus.
        Assert.True(vm.SubtitleGrid.Focusable);

        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        Assert.Same(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement());

        window.Close();
    }

    [AvaloniaFact]
    public void F10_OpensTheMenuBarVisibly()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);

        // Menu.Open (the same path as Avalonia's bare-Alt activation) selects the first
        // top-level item, so "File" is highlighted - focus alone gave no visual feedback.
        Assert.True(vm.Menu.IsOpen);
        Assert.True(IsFocusOnMenuItem(window));

        window.Close();
    }

    [AvaloniaFact]
    public void F10_TogglesTheMenuBarOffAgain_WithEmptyGrid()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        Assert.True(vm.Menu.IsOpen);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);

        Assert.False(vm.Menu.IsOpen);
        Assert.False(IsFocusOnMenuItem(window));
        Assert.Same(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement());

        window.Close();
    }

    [AvaloniaFact]
    public void Escape_DeactivatesTheMenuBar_WithEmptyGrid()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        Assert.True(vm.Menu.IsOpen);

        PressAndRelease(window, PhysicalKey.Escape, RawInputModifiers.None);

        Assert.False(vm.Menu.IsOpen);
        Assert.False(IsFocusOnMenuItem(window));
        Assert.Same(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement());

        window.Close();
    }

    [AvaloniaFact]
    public void BareAlt_ClosesTheMenu_WhileFocusIsInsideAnOpenDropDown()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);

        // Bare Alt activates the bar (Avalonia's AccessKeyHandler opens it on the release).
        PressAndRelease(window, PhysicalKey.AltLeft, RawInputModifiers.Alt);
        Assert.True(vm.Menu.IsOpen);

        // Down opens the File drop-down and focuses its first item. (The headless platform hosts
        // drop-downs in the window's overlay layer, so this cannot reproduce the desktop bug
        // where they live in a separate popup top-level - the test below covers that part.)
        PressAndRelease(window, PhysicalKey.ArrowDown, RawInputModifiers.None);
        Assert.True(IsFocusOnMenuItem(window));

        PressAndRelease(window, PhysicalKey.AltLeft, RawInputModifiers.Alt);

        Assert.False(vm.Menu.IsOpen);
        Assert.False(IsFocusOnMenuItem(window));
        Assert.Same(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement());

        window.Close();
    }

    [AvaloniaFact]
    public void BareAlt_ClosesTheMenu_WhenTheFocusedMenuItemLivesInAnotherTopLevel()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        Assert.True(vm.Menu.IsOpen);

        // On desktop an open drop-down hosts its items in a PopupRoot - a separate top-level,
        // where Avalonia's AccessKeyHandler ignores every key, so its bare-Alt toggle never saw
        // the second Alt (#12087). The headless platform has no popup top-levels, so model the
        // same focus topology with a second window hosting the focused menu item.
        var popupStandIn = new Window { Content = new Menu { Items = { new MenuItem { Header = "Item" } } } };
        popupStandIn.Show();
        Dispatcher.UIThread.RunJobs();
        var popupItem = (MenuItem)((Menu)popupStandIn.Content!).Items[0]!;
        popupItem.Focusable = true;
        Assert.True(popupItem.Focus());
        Settle(window);
        Assert.NotSame(window, TopLevel.GetTopLevel(window.FocusManager?.GetFocusedElement() as Visual));

        // Guard against a false pass: if showing the second window had already closed the menu
        // (e.g. via a deactivation handler), the Alt cycle below would have nothing to do.
        Assert.True(vm.Menu.IsOpen);

        // The key events reach the main window's handlers through the popup's event route; the
        // headless KeyPress helpers only target one window, so raise them the way the route would.
        vm.OnKeyDownHandler(null, new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Source = popupItem,
            Key = Key.LeftAlt,
            PhysicalKey = PhysicalKey.AltLeft,
            KeyModifiers = KeyModifiers.Alt,
        });
        vm.OnKeyUpHandler(null, new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyUpEvent,
            Source = popupItem,
            Key = Key.LeftAlt,
            PhysicalKey = PhysicalKey.AltLeft,
            KeyModifiers = KeyModifiers.None,
        });
        Settle(window);

        Assert.False(vm.Menu.IsOpen);
        Assert.Same(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement());

        popupStandIn.Close();
        window.Close();
    }

    [AvaloniaFact]
    public void BareAlt_DeactivatesAnF10OpenedMenuBar()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        Assert.True(vm.Menu.IsOpen);

        // Avalonia's AccessKeyHandler closes the bar on the Alt press but only restores focus
        // for bars it opened itself - after an F10 activation the close would strand focus on
        // the menu item, where the bar keeps swallowing every key.
        window.KeyPressQwerty(PhysicalKey.AltLeft, RawInputModifiers.Alt);
        Dispatcher.UIThread.RunJobs();
        window.KeyReleaseQwerty(PhysicalKey.AltLeft, RawInputModifiers.None);
        Settle(window);

        Assert.False(vm.Menu.IsOpen);
        Assert.False(IsFocusOnMenuItem(window));
        Assert.Same(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement());

        window.Close();
    }
}
