using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
public class MainMenuKeyboardActivationTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown
    // ("Test Case Cleanup Failure" - different thread owns the window).
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithEmptyGrid()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
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

    /// <summary>
    /// The key cycles below only reach the window handlers once the grid actually holds focus;
    /// FocusRow + Settle can still leave the focus a frame behind on a loaded runner, and a key
    /// pressed before that lands nowhere (menu never opens - CI flake).
    /// </summary>
    private static async Task WaitForGridFocus(Window window, MainViewModel vm)
    {
        await WaitUntil(
            () => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()),
            "the grid should hold keyboard focus before the key cycle");
    }

    private static void PressAndRelease(Window window, PhysicalKey key, RawInputModifiers modifiers)
    {
        window.KeyPressQwerty(key, modifiers);
        Dispatcher.UIThread.RunJobs();
        window.KeyReleaseQwerty(key, modifiers);
        Settle(window);
    }

    /// <summary>
    /// Waits for a state a key cycle is expected to produce. Even with the RunJobs/Settle calls in
    /// PressAndRelease, the access-key handling can still be a few dispatcher frames behind under
    /// CPU load (CI flake: a different menu test failed on every run); polling with a delay pumps
    /// the headless dispatcher until the expected state appears, or fails on timeout.
    /// </summary>
    private static async Task WaitUntil(Func<bool> condition, string failureMessage, int timeoutMs = 500)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.ElapsedMilliseconds > timeoutMs)
            {
                Assert.Fail(failureMessage);
            }

            // RunJobs pumps any queued dispatcher work the condition may depend on; Task.Delay
            // additionally yields the thread so the headless session can process input.
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }
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
    public async Task F10_OpensTheMenuBarVisibly()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);

        // Menu.Open (the same path as Avalonia's bare-Alt activation) selects the first
        // top-level item, so "File" is highlighted - focus alone gave no visual feedback.
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");
        await WaitUntil(() => IsFocusOnMenuItem(window), "F10 should focus the first menu item");

        window.Close();
    }

    [AvaloniaFact]
    public async Task F10_TogglesTheMenuBarOffAgain_WithEmptyGrid()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);

        await WaitUntil(() => !vm.Menu.IsOpen, "the second F10 should close the menu bar");
        await WaitUntil(() => !IsFocusOnMenuItem(window), "the menu item should lose focus");
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()), "focus should return to the grid");

        window.Close();
    }

    [AvaloniaFact]
    public async Task Escape_DeactivatesTheMenuBar_WithEmptyGrid()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");

        PressAndRelease(window, PhysicalKey.Escape, RawInputModifiers.None);

        await WaitUntil(() => !vm.Menu.IsOpen, "Escape should close the menu bar");
        await WaitUntil(() => !IsFocusOnMenuItem(window), "Escape should release the menu item focus");
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()), "Escape should restore focus to the grid");

        window.Close();
    }

    [AvaloniaFact]
    public async Task BareAlt_ClosesTheMenu_WhileFocusIsInsideAnOpenDropDown()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        // Bare Alt activates the bar (Avalonia's AccessKeyHandler opens it on the release).
        PressAndRelease(window, PhysicalKey.AltLeft, RawInputModifiers.Alt);
        await WaitUntil(() => vm.Menu.IsOpen, "bare Alt should open the menu bar");

        // Down opens the File drop-down and focuses its first item. (The headless platform hosts
        // drop-downs in the window's overlay layer, so this cannot reproduce the desktop bug
        // where they live in a separate popup top-level - the test below covers that part.)
        PressAndRelease(window, PhysicalKey.ArrowDown, RawInputModifiers.None);
        await WaitUntil(() => IsFocusOnMenuItem(window), "Down should focus the first drop-down item");

        PressAndRelease(window, PhysicalKey.AltLeft, RawInputModifiers.Alt);

        await WaitUntil(() => !vm.Menu.IsOpen, "bare Alt with a drop-down item focused should close the bar");
        await WaitUntil(() => !IsFocusOnMenuItem(window), "the drop-down item should lose focus");
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()), "focus should return to the grid");

        window.Close();
    }

    [AvaloniaFact]
    public async Task BareAlt_ClosesTheMenu_WhenTheFocusedMenuItemLivesInAnotherTopLevel()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");

        // On desktop an open drop-down hosts its items in a PopupRoot - a separate top-level,
        // where Avalonia's AccessKeyHandler ignores every key, so its bare-Alt toggle never saw
        // the second Alt (#12087). The headless platform has no popup top-levels, so model the
        // same focus topology with a second window hosting the focused menu item.
        var popupStandIn = new Window { Content = new Menu { Items = { new MenuItem { Header = "Item" } } } };
        _windows.Add(popupStandIn);
        popupStandIn.Show();
        Dispatcher.UIThread.RunJobs();
        var popupItem = (MenuItem)((Menu)popupStandIn.Content!).Items[0]!;
        popupItem.Focusable = true;
        Assert.True(popupItem.Focus());
        Settle(window);
        await WaitUntil(() => !ReferenceEquals(window, TopLevel.GetTopLevel(window.FocusManager?.GetFocusedElement() as Visual)), "focus should move to the second top-level");

        // Guard against a false pass: if showing the second window had already closed the menu
        // (e.g. via a deactivation handler), the Alt cycle below would have nothing to do.
        Assert.True(vm.Menu.IsOpen, "the menu must still be open before the Alt cycle");

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

        await WaitUntil(() => !vm.Menu.IsOpen, "bare Alt should close the menu when focus is in another top-level");
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()), "focus should return to the grid");

        popupStandIn.Close();
        window.Close();
    }

    [AvaloniaFact]
    public async Task F10_ShowsAccessKeyUnderlines_LikeAlt()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");

        // Alt activation shows the access-key underlines via this inherited window property;
        // F10 is its synonym and must do the same - the underlines still *worked* after F10,
        // they were just invisible (#13111 beta-4/5 feedback).
        await WaitUntil(() => window.GetValue(AccessText.ShowAccessKeyProperty), "F10 should show the access-key underlines");

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);

        await WaitUntil(() => !vm.Menu.IsOpen, "the second F10 should close the menu bar");
        await WaitUntil(() => !window.GetValue(AccessText.ShowAccessKeyProperty), "closing should clear the access-key underlines");

        window.Close();
    }

    [AvaloniaFact]
    public async Task Escape_ClosesDropDownKeepingItemHighlighted_ThenDeactivates()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        // Open the bar and move to the second top-level item, then drop its menu down - the
        // regression showed on any non-first menu: the first Escape yanked the highlight back
        // to "File" (#13111 beta-4 feedback).
        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");
        PressAndRelease(window, PhysicalKey.ArrowRight, RawInputModifiers.None);
        PressAndRelease(window, PhysicalKey.ArrowDown, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.Items.OfType<MenuItem>().Any(mi => mi.IsSubMenuOpen), "Down should open the second item's drop-down");

        var openItem = vm.Menu.Items.OfType<MenuItem>().First(mi => mi.IsSubMenuOpen);
        Assert.NotSame(vm.Menu.Items[0], openItem);

        // First Escape: only the drop-down closes; the bar stays active with the same item
        // highlighted (Windows standard).
        PressAndRelease(window, PhysicalKey.Escape, RawInputModifiers.None);

        await WaitUntil(() => !openItem.IsSubMenuOpen, "Escape should close the drop-down");
        Assert.True(vm.Menu.IsOpen, "the bar must stay active after the first Escape");
        Assert.Same(openItem, vm.Menu.SelectedItem);

        // Second Escape: the bar deactivates completely - no leftover highlight, focus back in
        // the editing area.
        PressAndRelease(window, PhysicalKey.Escape, RawInputModifiers.None);

        await WaitUntil(() => !vm.Menu.IsOpen, "the second Escape should close the menu bar");
        await WaitUntil(() => vm.Menu.SelectedIndex == -1, "deactivation should clear the top-level highlight");
        await WaitUntil(() => !IsFocusOnMenuItem(window), "the menu item should lose focus");
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()), "focus should return to the grid");

        window.Close();
    }

    [AvaloniaFact]
    public async Task BareAlt_DeactivatesAnF10OpenedMenuBar()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        PressAndRelease(window, PhysicalKey.F10, RawInputModifiers.None);
        await WaitUntil(() => vm.Menu.IsOpen, "F10 should open the menu bar");

        // Avalonia's AccessKeyHandler closes the bar on the Alt press but only restores focus
        // for bars it opened itself - after an F10 activation the close would strand focus on
        // the menu item, where the bar keeps swallowing every key.
        window.KeyPressQwerty(PhysicalKey.AltLeft, RawInputModifiers.Alt);
        Dispatcher.UIThread.RunJobs();
        window.KeyReleaseQwerty(PhysicalKey.AltLeft, RawInputModifiers.None);
        Settle(window);

        await WaitUntil(() => !vm.Menu.IsOpen, "bare Alt should close an F10-opened bar");
        await WaitUntil(() => !IsFocusOnMenuItem(window), "the menu item should lose focus");
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()), "focus should return to the grid");

        window.Close();
    }

    /// <summary>
    /// Alt+letter (e.g. Alt+F for "_File") must open the Menu control itself, not just the
    /// item's drop-down popup. Avalonia's DefaultMenuInteractionHandler.AccessKeyPressed skips
    /// IMainMenu.Open() - unlike the pointer and bare-Alt paths - so Menu.Opened never fired
    /// and everything keyed to it silently skipped; most visibly the undocked tool windows
    /// kept their topmost and covered the drop-down (#13361). MainMenuInteractionHandler
    /// closes that gap; this test pins it across Avalonia upgrades (the handler subclasses an
    /// [Unstable] Avalonia class).
    /// </summary>
    [AvaloniaFact]
    public async Task AltLetter_OpensTheMenuBarItself_AndSuspendsUndockedTopmost()
    {
        var (window, vm) = ShowMainWindowWithEmptyGrid();
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitForGridFocus(window, vm);

        // Observe the undocked-topmost suspension the way MainViewModel's setter would be
        // driven (the suspension is keyed to Menu.Opened/Closed, WindowService ref-counted).
        var topmostCalls = new List<bool>();
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.RegisterUndockedTopmostSetter(topmostCalls.Add);

        try
        {
            // Alt held, F pressed: the access key opens the File drop-down. The assertions run
            // before the Alt release on purpose - on a desktop the drop-down popup is a separate
            // top-level that swallows the release, so the release must not be what opens the bar.
            window.KeyPressQwerty(PhysicalKey.AltLeft, RawInputModifiers.Alt);
            Dispatcher.UIThread.RunJobs();
            window.KeyPressQwerty(PhysicalKey.F, RawInputModifiers.Alt);
            Dispatcher.UIThread.RunJobs();
            window.KeyReleaseQwerty(PhysicalKey.F, RawInputModifiers.Alt);
            Settle(window);

            await WaitUntil(() => vm.Menu.Items.OfType<MenuItem>().Any(mi => mi.IsSubMenuOpen), "Alt+F should open the File drop-down");
            Assert.True(vm.Menu.IsOpen, "Alt+F must open the menu bar itself, not just the drop-down (#13361)");
            Assert.Equal([false], topmostCalls);

            window.KeyReleaseQwerty(PhysicalKey.AltLeft, RawInputModifiers.None);
            Settle(window);

            // First Escape closes the drop-down (bar stays armed), second deactivates the bar -
            // the suspension must be released exactly once, when the bar actually closes.
            PressAndRelease(window, PhysicalKey.Escape, RawInputModifiers.None);
            Assert.Equal([false], topmostCalls);
            PressAndRelease(window, PhysicalKey.Escape, RawInputModifiers.None);

            await WaitUntil(() => !vm.Menu.IsOpen, "the second Escape should close the menu bar");
            await WaitUntil(() => topmostCalls.Count == 2 && topmostCalls[1], "closing the bar should restore the undocked windows' topmost");
        }
        finally
        {
            WindowService.RegisterUndockedTopmostSetter(null);
            WindowService.ResetUndockedTopmostSuspensionsForTests();
        }

        window.Close();
    }
}
