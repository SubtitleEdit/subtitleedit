using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// Creates main editor windows. The primary window is created here at startup (via
/// Program.SetupMainWindow) and File &gt; New window creates additional independent ones,
/// all inside this same process so they stack under a single Dock/taskbar icon and show
/// up in its window list. Every window hosts its own MainView + MainViewModel (both
/// registered transient), so each has its own subtitle, video and undo history; on macOS
/// each also gets its own NSMenuBar bound to its own view model (see InitNativeMacMenu).
/// </summary>
public static class MainWindowFactory
{
    public const string AppName = "Subtitle Edit";

    // Open main editor windows. The process force-exits when the last one closes: under
    // ShutdownMode.OnLastWindowClose a single stray helper window (e.g. a leaked
    // non-taskbar "please wait" progress window) is enough to keep the app alive as an
    // invisible background process holding file/folder handles (#12172). The window's own
    // OnClosing has already saved settings and cleaned up by the time Closed fires, and
    // Closed never fires on a cancelled (unsaved-changes) close, so exiting is safe.
    private static int _openMainWindows;

    public static Window Create(bool isPrimary)
    {
        var window = new Window
        {
            Title = AppName,
            Name = "MainWindow",
            Icon = UiUtil.GetSeIcon(),
            MinWidth = 800,
            MinHeight = 500,
        };

        // Build runs from the MainView constructor, so the host window is handed over via
        // a static; each view model must own its actual window, not desktop.MainWindow.
        MainView.NextHostWindow = window;
        var mainView = new MainView();

        // Always host MainView inside a LayoutTransformControl, even at scale 1.0.
        // SetCurrentTheme() runs before this window exists, so the saved UI scale is
        // applied here at creation. Pre-wrapping also means later scale changes only
        // update the transform instead of swapping window.Content, which would
        // reparent the video player's native HWND and freeze the UI.
        window.Content = new LayoutTransformControl { Child = mainView };
        UiTheme.ApplyScaleToWindow(window);

        // Restore the saved position only for the primary window; extra windows opening
        // exactly on top of it would hide that a new window appeared at all.
        if (isPrimary && Se.Settings.General.RememberPositionAndSize)
        {
            UiUtil.RestoreWindowPosition(window);
        }

        // Full macOS menu bar (File, Edit, Tools, ...) per window: the NSMenuBar follows
        // the focused window, so every editor window carries a menu bound to its own view
        // model. NativeMenu.SetMenu(Application, ...) only controls the App menu dropdown;
        // NativeMenu.SetMenu(Window, ...) calls avnWindow.SetMainMenu and builds the full
        // NSMenuBar with separate top-level menus.
        if (OperatingSystem.IsMacOS())
        {
            var menuBarRoot = new NativeMenu();
            InitNativeMacMenu.MakeStructure(menuBarRoot, window);
            NativeMenu.SetMenu(window, menuBarRoot);
        }

        _openMainWindows++;
        window.Closed += (_, _) =>
        {
            _openMainWindows--;
            if (_openMainWindows <= 0)
            {
                Environment.Exit(0);
            }
        };

        return window;
    }

    /// <summary>File &gt; New window: opens another independent editor window.</summary>
    public static void OpenNewWindow()
    {
        var window = Create(isPrimary: false);
        window.Show();
    }
}
