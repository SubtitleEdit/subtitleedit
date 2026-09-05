using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

public static class Locator
{
    public static IServiceProvider Services { get; set; } = default!;
}

public class MainView : ViewBase
{
    private MainViewModel? _vm;

    /// <summary>
    /// The window that will host the next MainView. Set by MainWindowFactory right before
    /// constructing the view (Build runs from the constructor, so it can't be passed in).
    /// With File &gt; New window there can be several editor windows, and each view model
    /// must own its actual host window, not desktop.MainWindow (always the first window),
    /// for dialog owners and the Closing/Deactivated/Loaded hooks to target correctly.
    /// </summary>
    internal static Window? NextHostWindow;

    protected override object Build()
    {
        _vm = Locator.Services.GetRequiredService<MainViewModel>();
        if (_vm == null)
        {
            throw new InvalidOperationException("MainViewModel is not registered in the service provider.");
        }

        _vm.MainView = this;
        DataContext = _vm;

        var hostWindow = NextHostWindow;
        NextHostWindow = null;
        if (hostWindow == null &&
            Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop)
        {
            hostWindow = desktop.MainWindow;
        }

        if (hostWindow != null)
        {
            _vm.Window = hostWindow;
            // A user-assigned Alt+Space shortcut beats the Windows system menu (#14536).
            UiUtil.SetWindowSystemMenuOverride(hostWindow, _vm.HasAltSpaceShortcut);
            _vm.Window.Closing += _vm.OnClosing;
            _vm.Window.Deactivated += _vm.OnWindowDeactivated;
            _vm.Window.Activated += _vm.OnWindowActivated;
            _vm.Window.Loaded += (_, _) =>
            {
                _vm.OnLoaded();
            };

            // Clipboard-manager compatibility (Ditto, CopyQ, ClipClip, ...) - see the
            // hook for what it intercepts and why (#13822).
            if (OperatingSystem.IsWindows())
            {
                Win32Properties.AddWndProcHookCallback(_vm.Window, MainWindowWndProcHook);
            }
        }

        // Load language (normally already loaded in Program.Main before the window is built; this
        // is a no-op safety net for windows created via other entry points).
        Se.LoadLanguage();

        var root = new DockPanel();

        // Menu bar
        InitMenu.Make(_vm);
        if (OperatingSystem.IsMacOS())
        {
            _vm.Menu.IsVisible = false;
        }
        DockPanel.SetDock(_vm.Menu, Dock.Top);
        root.Children.Add(_vm.Menu);

        _vm.ToolbarTopSeparator = UiUtil.MakeHorizontalSeparator(0.5, 0.5, new Thickness(0, 0, 0, 0));
        _vm.ToolbarTopSeparator.IsVisible = Se.Settings.Appearance.ShowHorizontalLineAboveToolbar;
        DockPanel.SetDock(_vm.ToolbarTopSeparator, Dock.Top);
        root.Children.Add(_vm.ToolbarTopSeparator);

        // Toolbar
        _vm.Toolbar = InitToolbar.Make(_vm);
        DockPanel.SetDock(_vm.Toolbar, Dock.Top);
        root.Children.Add(_vm.Toolbar);

        // Footer
        var footer = InitFooter.Make(_vm);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        // Main content (fills all remaining space)
        _vm.ContentGrid = ViewContent.Make(_vm);

        // Wait for the view to be attached to visual tree before initializing layout
        this.AttachedToVisualTree += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                InitLayout.MakeLayout(this, _vm, Se.Settings.General.LayoutNumber);

                // The window level right to left pass runs when the window opens,
                // which is before this deferred layout build, so the freshly built
                // grid and text boxes would stay left to right until the mode is
                // toggled. Re-apply after building.
                if (Se.Settings.Appearance.RightToLeft && TopLevel.GetTopLevel(this) is Window rtlWindow)
                {
                    MainHelpers.RightToLeftHelper.SetRightToLeftForDataGridAndText(rtlWindow);
                }

                _vm.ContentGrid.InvalidateMeasure();
                _vm.ContentGrid.InvalidateArrange();
                Dispatcher.UIThread.Post(() => TableViewExtras.FocusRow(_vm.SubtitleGrid));
            }, DispatcherPriority.Loaded);
        };

        root.Children.Add(_vm.ContentGrid);

        AddHandler(KeyDownEvent, _vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        AddHandler(KeyUpEvent, _vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        // Tunnelling, and handledEventsToo since controls like the waveform mark their presses handled:
        // needed to see that Alt was held for a mouse gesture and cancel the menu-bar activation that
        // would otherwise fire on the Alt release (discussion #11744).
        AddHandler(PointerPressedEvent, _vm.OnPointerPressedHandler, RoutingStrategies.Tunnel, handledEventsToo: true);

        return root;
    }

    // Whether the window has seen (and passed through) an Alt key-down whose release is still
    // pending. Used by MainWindowWndProcHook to tell a real Alt release from an injected one.
    private bool _altKeyDownDelivered;

    /// <summary>
    /// Window-message hook that makes pasting from Windows clipboard managers (Ditto, CopyQ,
    /// ClipClip, ...) work (#13822). Runs on the UI thread, before Avalonia's own wndproc;
    /// setting handled skips Avalonia's processing of the message entirely.
    ///
    /// 1) Stray Alt key-ups: before synthesizing Ctrl+V, Ditto force-releases every key
    ///    (CSendKeys::AllKeysUp sends left/right Alt key-ups even when Alt was never down).
    ///    Avalonia's AccessKeyHandler arms its "showing access keys" state on ANY Alt key-down -
    ///    including Ctrl+Alt chords - and never disarms the field on pointer-press or menu-close,
    ///    so once the user has tapped Alt or used a Ctrl+Alt hotkey, ANY later Alt key-up opens
    ///    the main menu. The menu steals keyboard focus and the clipboard manager's synthesized
    ///    Ctrl+V lands in the menu instead of the text box: pasting "randomly" fails and the File
    ///    menu lights up instead (also reported as "Ctrl+1 activates the File menu"). Only let an
    ///    Alt key-up through when it releases an Alt key-down this window actually delivered.
    ///    As a bonus this stops Alt+Tab-ing back into SE from activating the menu bar via the
    ///    orphaned Alt release.
    /// 2) Left Alt key-down while Ctrl is already held: a Ctrl+Alt chord is never a menu
    ///    gesture, but it arms the AccessKeyHandler state above, so swallow it before Avalonia
    ///    sees it. The chord's later keys still carry Ctrl+Alt in KeyEventArgs.KeyModifiers
    ///    (Avalonia reads the live key state per event), so Ctrl+Alt shortcuts keep firing.
    ///    Left Alt only: AltGr is physically right Alt, and its key-down must keep reaching
    ///    the app for ShortcutManager's AltGr detection (which suppresses Ctrl+Alt shortcuts
    ///    while typing AltGr characters).
    /// 3) WM_CHAR 0x16 (the Ctrl+V control char): tools that post keystrokes to the window
    ///    instead of injecting real input can't put Ctrl into the key state, so their paste
    ///    arrives only as this classic control char - native Win32 edit controls paste on it,
    ///    Avalonia discards it. Route it to the focused control, but only while Ctrl is really
    ///    up: during physical Ctrl+V typing the same char arrives right after the TextBox
    ///    already pasted on the key-down.
    /// 4) WM_PASTE: some tools deliver the paste as this message. It targets the focused HWND,
    ///    which in Avalonia is always the window itself, so route it to the focused control.
    /// </summary>
    private IntPtr MainWindowWndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const uint wmKillFocus = 0x0008;
        const uint wmKeyDown = 0x0100;
        const uint wmKeyUp = 0x0101;
        const uint wmChar = 0x0102;
        const uint wmSysKeyDown = 0x0104;
        const uint wmSysKeyUp = 0x0105;
        const uint wmPaste = 0x0302;
        const int vkControl = 0x11;
        const int vkMenu = 0x12;
        const int ctrlVChar = 0x16;

        if (msg is wmKeyDown or wmSysKeyDown && (int)wParam == vkMenu)
        {
            var isLeftAlt = (((long)lParam >> 24) & 1) == 0; // bit 24: extended key = right Alt
            if (isLeftAlt && GetKeyState(vkControl) < 0)
            {
                handled = true;
            }
            else
            {
                _altKeyDownDelivered = true;
            }
        }
        else if (msg is wmKeyUp or wmSysKeyUp && (int)wParam == vkMenu)
        {
            if (_altKeyDownDelivered)
            {
                _altKeyDownDelivered = false;
            }
            else
            {
                handled = true;
            }
        }
        else if (msg == wmKillFocus)
        {
            // The matching Alt release will go to whichever window has focus then; a later
            // unmatched release here (e.g. after Alt+Tab back in) must not open the menu.
            _altKeyDownDelivered = false;
        }
        else if ((msg == wmChar && (int)wParam == ctrlVChar && GetKeyState(vkControl) >= 0) ||
                 msg == wmPaste)
        {
            if (_vm != null && _vm.PasteViaWindowMessage())
            {
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetKeyState(int keyCode);

    internal async Task OpenFile(string fileName)
    {
        if (_vm == null || !System.IO.File.Exists(fileName))
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            await _vm.SubtitleOpen(fileName);
        });
    }
}