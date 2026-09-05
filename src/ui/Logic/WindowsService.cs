using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// Service responsible for creating and managing windows using dependency injection.
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Shows a window of type T.
        /// </summary>
        /// <typeparam name="T">The type of window to show.</typeparam>
        /// <param name="configure">Optional action to configure the window before showing.</param>
        /// <returns>The created window instance.</returns>
        T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window;

        /// <summary>
        /// Shows a window of type T with a specified ViewModel type.
        /// </summary>
        /// <typeparam name="T">The type of window to show.</typeparam>
        /// <typeparam name="TViewModel">The type of ViewModel to associate with the window.</typeparam>
        /// <param name="configure">Optional action to configure the window and ViewModel before showing.</param>
        /// <returns>The created ViewModel instance.</returns>
        TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
            where T : Window
            where TViewModel : class;

        /// <summary>
        /// Shows a window of type T with a specified ViewModel type as an independent top-level
        /// window (no owner). Use this for windows that should appear in the OS Alt+Tab list
        /// independently and not be grouped with the main window — e.g. the undocked video player
        /// and audio visualizer. Owned windows on Windows form an Alt+Tab "group" with their
        /// owner, which traps focus and prevents the user from Alt+Tabbing back to the main
        /// window once one of the owned windows is active.
        /// </summary>
        TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Window
            where TViewModel : class;

        /// <summary>
        /// Shows a window of type T as a dialog.
        /// </summary>
        /// <typeparam name="T">The type of window to show as dialog.</typeparam>
        /// <param name="owner">The owner window.</param>
        /// <param name="configure">Optional action to configure the window before showing.</param>
        /// <returns>A task that completes when the dialog is closed.</returns>
        Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window;

        /// <summary>
        /// Shows a window of type T with a specified ViewModel type as a dialog.
        /// </summary>
        /// <typeparam name="T">The type of window to show as dialog.</typeparam>
        /// <typeparam name="TViewModel">The type of ViewModel to associate with the window.</typeparam>
        /// <param name="owner">The owner window.</param>
        /// <param name="configure">Optional action to configure the window and ViewModel before showing.</param>
        /// <returns>A task that resolves to the ViewModel instance when the dialog is closed.</returns>
        Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null, Action<TWindow>? configureWindow = null)
            where TWindow : Window
            where TViewModel : class;

        /// <summary>
        /// Shows a window of type TWindow as a stand-alone top-level window while
        /// <paramref name="owner"/> - and any of <paramref name="companions"/> currently on
        /// screen - are hidden, and brings them back once it closes. This is how SE4 ran
        /// Batch convert: the editor disappears and only the tool window remains (#14502).
        ///
        /// It can't be a modal dialog: Avalonia's <see cref="Window.Hide"/> cascades to every
        /// owned window (the dialog would vanish with its owner) and
        /// <see cref="Window.ShowDialog(Window)"/> refuses a hidden owner. Minimizing the owner
        /// is no better - on Windows an owner's owned windows go with it.
        /// </summary>
        /// <param name="owner">The window to hide for the tool window's lifetime.</param>
        /// <param name="companions">Further windows to hide along with the owner when they are
        /// visible (the undocked video/waveform tool windows); hidden ones stay hidden.</param>
        Task<TViewModel> ShowWithOwnerHiddenAsync<TWindow, TViewModel>(
            Window owner,
            IReadOnlyList<Window?> companions,
            Action<TViewModel>? configureViewModel = null)
            where TWindow : Window
            where TViewModel : class;
    }

    /// <summary>
    /// Implementation of the window service that uses dependency injection to create windows.
    /// </summary>
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window
        {
            var window = CreateWindow<T>();

            configure?.Invoke(window);

            // Must run before Show(): ApplyScaleToWindow sets the Windows dark-mode title bar, and
            // DWM does not repaint the caption once the window is on screen - it only picks up the
            // change on the next activation. Applying afterwards leaves a light title bar until the
            // window loses and regains focus. (#12665)
            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            window.Show();
            window.Focus();

            return window;
        }

        /// <inheritdoc />
        public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configureViewModel = null)
            where T : Window
            where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            // Create the window using reflection, passing in the viewModel
            var w = Activator.CreateInstance(typeof(T), viewModel);
            if (w == null)
            {
                throw new InvalidOperationException($"Failed to create window of type {typeof(T).Name} with constructor param {typeof(TViewModel).Name}");
            }

            var window = (T)w;
            configureViewModel?.Invoke(window, viewModel);

            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Must run before Show() - see the note in ShowWindow<T>. (#12665)
            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            window.Show(owner);
            window.Focus();

            return viewModel;
        }

        /// <inheritdoc />
        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configureViewModel = null)
            where T : Window
            where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            var w = Activator.CreateInstance(typeof(T), viewModel);
            if (w == null)
            {
                throw new InvalidOperationException($"Failed to create window of type {typeof(T).Name} with constructor param {typeof(TViewModel).Name}");
            }

            var window = (T)w;
            configureViewModel?.Invoke(window, viewModel);

            // Must run before Show() - see the note in ShowWindow<T>. (#12665)
            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            // Show without activating: the callers are the undocked video/waveform tool windows,
            // and taking the foreground from the window the user is in - the main window during
            // startup (#13569), or a dialog whose Apply rebuilt the layout (#13398) - is never
            // wanted. They stay on top via KeepTopmostWhileOwnerActive, not via focus.
            window.ShowActivated = false;
            window.Show();

            return viewModel;
        }

        /// <inheritdoc />
        public async Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
        {
            var window = CreateWindow<T>();

            configure?.Invoke(window);

            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            await ShowModalAsync(owner, window);

            return window;
        }

        public async Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null, Action<TWindow>? configureWindow = null)
            where TWindow : Window
            where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            configureViewModel?.Invoke(viewModel);

            // Create the window using reflection, passing in the viewModel
            var w = Activator.CreateInstance(typeof(TWindow), viewModel);
            if (w == null)
            {
                throw new InvalidOperationException($"Failed to create window of type {typeof(TWindow).Name} with constructor param {typeof(TViewModel).Name}");
            }

            var window = (TWindow)w;

            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            configureWindow?.Invoke(window);

            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            await ShowModalAsync(owner, window);

            return viewModel;
        }

        /// <inheritdoc />
        public async Task<TViewModel> ShowWithOwnerHiddenAsync<TWindow, TViewModel>(
            Window owner,
            IReadOnlyList<Window?> companions,
            Action<TViewModel>? configureViewModel = null)
            where TWindow : Window
            where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            configureViewModel?.Invoke(viewModel);

            var w = Activator.CreateInstance(typeof(TWindow), viewModel);
            if (w == null)
            {
                throw new InvalidOperationException($"Failed to create window of type {typeof(TWindow).Name} with constructor param {typeof(TViewModel).Name}");
            }

            var window = (TWindow)w;

            // No owner to center on; a remembered position (RestoreWindowPosition in the
            // window's Loaded handler) still wins over this.
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Must run before Show() - see the note in ShowWindow<T>. (#12665)
            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            var windowsToHide = new List<Window?>(companions.Count + 1) { owner };
            windowsToHide.AddRange(companions);
            await ShowWithWindowsHiddenAsync(window, windowsToHide, activateAfterwards: owner);

            return viewModel;
        }

        /// <summary>
        /// Shows <paramref name="window"/> as a top-level window with every visible window in
        /// <paramref name="windowsToHide"/> hidden until it closes, then re-shows them in the
        /// given order and activates <paramref name="activateAfterwards"/>. Windows that were
        /// not visible to begin with are left alone. Restoration runs even if showing the
        /// window throws, so the hidden windows can never be lost.
        /// </summary>
        public static async Task ShowWithWindowsHiddenAsync(Window window, IReadOnlyList<Window?> windowsToHide, Window? activateAfterwards)
        {
            var hidden = new List<Window>();
            foreach (var candidate in windowsToHide)
            {
                if (candidate is { IsVisible: true } && !hidden.Contains(candidate))
                {
                    hidden.Add(candidate);
                }
            }

            var closed = new TaskCompletionSource();
            window.Closed += (_, _) => closed.TrySetResult();

            foreach (var w in hidden)
            {
                w.Hide();
            }

            try
            {
                window.Show();
                await closed.Task;
            }
            finally
            {
                foreach (var w in hidden)
                {
                    w.Show();
                }

                if (activateAfterwards != null)
                {
                    if (activateAfterwards.WindowState == WindowState.Minimized)
                    {
                        activateAfterwards.WindowState = WindowState.Normal;
                    }

                    activateAfterwards.Activate();
                }
            }
        }

        /// <summary>
        /// Shows an already-constructed window as a modal dialog with the shared foreground
        /// handling every modal in SE needs: kept above the undocked tool windows (#11971),
        /// undocked topmost suspended for its lifetime (#13325), and foreground enforced while it
        /// is open (#13405). Use this instead of calling <see cref="Window.ShowDialog(Window)"/>
        /// directly - a bare ShowDialog is how dialogs end up drawn on top but never activated.
        /// </summary>
        public static Task ShowModalAsync(Window owner, Window dialog)
        {
            return RunModalAsync(owner, dialog, () => dialog.ShowDialog(owner));
        }

        /// <summary>
        /// Same as <see cref="ShowModalAsync(Window, Window)"/> for dialogs closed with a result.
        /// </summary>
        public static async Task<TResult> ShowModalAsync<TResult>(Window owner, Window dialog)
        {
            TResult result = default!;
            await RunModalAsync(owner, dialog, async () => result = await dialog.ShowDialog<TResult>(owner));
            return result;
        }

        private static async Task RunModalAsync(Window owner, Window dialog, Func<Task> showDialog)
        {
            // Keep the dialog above undocked tool windows (audio visualizer / video player), which
            // float on top of the main window via the same helper. Without this the dialog opens
            // behind them in undocked mode. (#11971)
            KeepTopmostWhileOwnerActive(dialog, owner);

            // Drop the undocked windows' topmost for the dialog's lifetime and keep OS activation
            // on the dialog, not just top-of-z-order drawing (#13325/#13405).
            using var undockedSuspension = SuspendUndockedTopmost();
            var foregroundEnforcement = EnforceModalForegroundWhileOpen(dialog, owner);
            var minimizeMirror = MirrorMinimizeWithOwner(dialog, owner);

            _openModalCount++;
            try
            {
                await YieldForPendingFlyoutDismissAsync();
                await showDialog();
            }
            finally
            {
                _openModalCount--;
                minimizeMirror.Dispose();
                foregroundEnforcement.Dispose();
            }
        }

        // When a dialog is launched from a context menu item, the command runs synchronously while the
        // MenuFlyout is still mid-dismiss. Opening the modal dialog at that moment (notably on macOS)
        // leaves it without keyboard focus, so it cannot receive key input such as Escape. Yielding a
        // dispatcher cycle at Background priority lets the flyout finish its light-dismiss and focus
        // restoration first, so the dialog then opens and takes focus cleanly.
        private static Task YieldForPendingFlyoutDismissAsync()
        {
            return Dispatcher.UIThread.InvokeAsync(static () => { }, DispatcherPriority.Background).GetTask();
        }

        // The undocked tool windows (audio visualizer / video player) float above the main
        // window via KeepTopmostWhileOwnerActive - which also puts them above every popup and
        // dialog the main window opens. Rather than making each popup/dialog fight back with
        // its own Topmost (#11971/#12268/#12899/#13187 - and the OS foreground churn between
        // two competing topmost windows left dialogs drawn on top but never activated,
        // #13325), the undocked windows drop their topmost for the lifetime of whatever
        // would be covered. MainViewModel registers the setter; the count makes nested
        // suspensions safe (menu -> dialog -> message box).
        private static Action<bool>? _setUndockedWindowsTopmost;
        private static int _undockedTopmostSuspendCount;

        /// <summary>
        /// Registers the callback that applies (true) or suppresses (false) the undocked tool
        /// windows' topmost state. Registered by MainViewModel; consulted by
        /// <see cref="SuspendUndockedTopmost"/>.
        /// </summary>
        public static void RegisterUndockedTopmostSetter(Action<bool>? setTopmost)
        {
            _setUndockedWindowsTopmost = setTopmost;
        }

        /// <summary>
        /// Test hook: clears leftover suspensions from tests that opened a menu or dialog and
        /// tore the window down without the matching Closed event ever firing.
        /// </summary>
        internal static void ResetUndockedTopmostSuspensionsForTests()
        {
            _undockedTopmostSuspendCount = 0;
        }

        /// <summary>
        /// Drops the undocked tool windows' topmost state until the returned token is disposed.
        /// Re-entrant: the state is restored when the last outstanding token is disposed.
        /// </summary>
        public static IDisposable SuspendUndockedTopmost()
        {
            if (++_undockedTopmostSuspendCount == 1)
            {
                _setUndockedWindowsTopmost?.Invoke(false);
            }

            return new UndockedTopmostSuspension();
        }

        private sealed class UndockedTopmostSuspension : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                if (--_undockedTopmostSuspendCount == 0)
                {
                    _setUndockedWindowsTopmost?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// Keeps the undocked tool windows non-topmost while <paramref name="flyout"/> is open,
        /// so its popup (a plain non-topmost native window) is not covered by them in undocked
        /// mode (#13325). Cascaded submenus are covered too: they belong to the same open flyout.
        ///
        /// The suspension is keyed to Opening, not Opened: Opened fires after the popup's
        /// native window is already on screen, and on Windows demoting a topmost window
        /// (SetWindowPos HWND_NOTOPMOST) re-inserts it at the top of the non-topmost band -
        /// above the popup it was supposed to uncover, so the tool windows kept covering the
        /// grid's context menu (#13493). At Opening the popup does not exist yet; it is
        /// created right after, on top of the just-demoted tool windows. The main menu never
        /// had this problem because Menu.Opened fires before any drop-down popup opens.
        /// </summary>
        public static void SuspendUndockedTopmostWhileOpen(PopupFlyoutBase flyout)
        {
            IDisposable? suspension = null;
            flyout.Opening += (_, _) =>
            {
                suspension ??= SuspendUndockedTopmost();

                // A subclass can cancel the open in OnOpening after the event has been raised;
                // Closed then never fires and the suspension would leak, leaving the tool
                // windows permanently non-topmost. No SE flyout cancels today - this is a
                // cheap backstop.
                Dispatcher.UIThread.Post(() =>
                {
                    if (!flyout.IsOpen)
                    {
                        suspension?.Dispose();
                        suspension = null;
                    }
                }, DispatcherPriority.Background);
            };
            flyout.Closed += (_, _) =>
            {
                suspension?.Dispose();
                suspension = null;
            };
        }

        /// <summary>
        /// Same as <see cref="SuspendUndockedTopmostWhileOpen(PopupFlyoutBase)"/> for the main
        /// menu bar. Opened is early enough here: MenuBase raises it when the bar opens,
        /// before any drop-down popup window is created.
        /// </summary>
        public static void SuspendUndockedTopmostWhileOpen(MenuBase menu)
        {
            IDisposable? suspension = null;
            menu.Opened += (_, _) => suspension ??= SuspendUndockedTopmost();
            menu.Closed += (_, _) =>
            {
                suspension?.Dispose();
                suspension = null;
            };
        }

        // Every open modal shown through this service (dialogs and message boxes) counts here.
        // While a modal is open its owner is input-disabled, so any code path that would give the
        // owner keyboard focus is wrong by definition - the main window consults this before
        // pulling the caret back on re-activation (#13405).
        private static int _openModalCount;

        /// <summary>
        /// True while any modal dialog shown through <see cref="ShowModalAsync(Window, Window)"/>
        /// (which includes every ShowDialogAsync call and <see cref="Features.Shared.MessageBox"/>)
        /// is open.
        /// </summary>
        public static bool IsModalDialogOpen => _openModalCount > 0;

        /// <summary>
        /// Test hook: clears the open-modal count left behind by tests that tore a dialog down
        /// without completing its ShowDialog task.
        /// </summary>
        internal static void ResetOpenModalsForTests()
        {
            _openModalCount = 0;
            _modalFrames.Clear();
        }

        /// <summary>
        /// Keeps OS activation on <paramref name="dialog"/> for as long as it is open.
        ///
        /// A modal's owner is input-disabled, yet Windows happily leaves (or puts) keyboard focus
        /// on it: the open-time topmost churn in undocked mode (#13325), a rebuild of the undocked
        /// windows (#13398), or a modal opening right as the previous one closes (#13405) all end
        /// in the same state - the dialog drawn on top, gray buttons, and every key landing in the
        /// window underneath. A one-shot activation at open time only wins when the steal has
        /// already happened, so instead enforce the invariant for the dialog's lifetime: whenever
        /// the owner ends up active while the dialog is open, hand activation to the dialog. The
        /// owner.IsActive guard scopes this to the unambiguously broken state - it never yanks
        /// foreground from other applications or from the (independent, enabled) undocked tool
        /// windows.
        ///
        /// OS activation is only half of the invariant. Avalonia routes every key press to its
        /// single app-global focused element, no matter which window the OS delivered the key to
        /// (KeyboardDevice.ProcessRawEvent). So a deferred Focus() call into the owner - the menu
        /// bar's post-close focus restore, a posted grid-focus after an edit - lands after the
        /// dialog opened and silently re-routes the keyboard into the disabled owner while the
        /// dialog stays OS-active with an active-looking title bar: Esc is dead and Tab visibly
        /// walks the window underneath, with no hint at all (#13405 beta-9 feedback). Activation
        /// events never fire for this state, so it is also enforced directly: focus landing on the
        /// owner while the dialog is open is handed straight back to the dialog, and the key that
        /// slipped through is swallowed rather than delivered to the input-disabled owner.
        /// </summary>
        private static IDisposable EnforceModalForegroundWhileOpen(Window dialog, Window owner)
        {
            var disposed = false;
            var frame = new ModalFrame(dialog, owner);
            _modalFrames.Add(frame);

            void BounceToDialog()
            {
                // The minimized check: while the dialog (and via MirrorMinimizeWithOwner the
                // whole pair) is minimized, activation churn must not pop it back up (#13788).
                if (!disposed && owner.IsActive && !dialog.IsActive && !dialog.IsClosing() &&
                    dialog.WindowState != WindowState.Minimized)
                {
                    dialog.Activate();
                }
            }

            void OnActivationChanged(object? sender, EventArgs e)
            {
                // Deferred so IsActive has settled on both windows (Deactivated of one window can
                // fire before Activated of the other).
                Dispatcher.UIThread.Post(BounceToDialog, DispatcherPriority.Background);
            }

            void OnOpened(object? sender, EventArgs e)
            {
                // The unconditional one-shot from #13325: when no SE window kept activation (the
                // user is in another application), Activate() still requests attention there.
                Dispatcher.UIThread.Post(() =>
                {
                    if (!disposed && dialog.IsVisible && !dialog.IsActive &&
                        dialog.WindowState != WindowState.Minimized)
                    {
                        dialog.Activate();
                    }
                }, DispatcherPriority.Background);

                // The open-time churn can leave the dialog inactive without any activation event
                // ever firing afterwards (the owner never lost activation, so nothing changes) -
                // re-check on short timers as well. Guarded by owner.IsActive, so they are no-ops
                // in every healthy state.
                DispatcherTimer.RunOnce(BounceToDialog, TimeSpan.FromMilliseconds(150));
                DispatcherTimer.RunOnce(BounceToDialog, TimeSpan.FromMilliseconds(450));
            }

            void OnDialogGotFocus(object? sender, FocusChangedEventArgs e)
            {
                // Remember where the keyboard focus lives inside the dialog, so a reclaim can
                // put it back exactly where it was instead of on the first focusable control.
                if (e.Source is InputElement element && element != dialog &&
                    TopLevel.GetTopLevel(element) == dialog)
                {
                    frame.LastDialogFocus = element;
                }
            }

            void OnOwnerGotFocus(object? sender, FocusChangedEventArgs e)
            {
                if (disposed || !dialog.IsVisible || dialog.IsClosing())
                {
                    return; // before open / during close, focus in the owner is legitimate
                }

                // Deferred so an immediately-following focus change (the dialog taking focus
                // itself) wins without a fight; the reclaim re-checks the state when it runs.
                Dispatcher.UIThread.Post(ReclaimKeyboardFocusFromDisabledOwner, DispatcherPriority.Background);
            }

            void OnOwnerKeyInput(object? sender, RoutedEventArgs e)
            {
                if (disposed || !dialog.IsVisible || dialog.IsClosing())
                {
                    return;
                }

                // A key reaching the input-disabled owner is always wrong - swallow it (better a
                // dead keystroke than one editing the subtitle underneath the dialog) and repair.
                e.Handled = true;
                Dispatcher.UIThread.Post(ReclaimKeyboardFocusFromDisabledOwner, DispatcherPriority.Background);
            }

            owner.Activated += OnActivationChanged;
            dialog.Deactivated += OnActivationChanged;
            dialog.Opened += OnOpened;
            // handledEventsToo: a control marking GotFocus handled must not hide the steal.
            dialog.AddHandler(InputElement.GotFocusEvent, OnDialogGotFocus, RoutingStrategies.Bubble, handledEventsToo: true);
            owner.AddHandler(InputElement.GotFocusEvent, OnOwnerGotFocus, RoutingStrategies.Bubble, handledEventsToo: true);
            // Tunnel: run before the owner's own handlers (shortcut manager, text boxes) see the key.
            // Key-up included: the AccessKeyHandler arms on Alt key-down but opens the menu bar on
            // the key-up, so letting the release through would pop the disabled owner's menu.
            owner.AddHandler(InputElement.KeyDownEvent, OnOwnerKeyInput, RoutingStrategies.Tunnel);
            owner.AddHandler(InputElement.KeyUpEvent, OnOwnerKeyInput, RoutingStrategies.Tunnel);
            owner.AddHandler(InputElement.TextInputEvent, OnOwnerKeyInput, RoutingStrategies.Tunnel);

            return new ActionDisposable(() =>
            {
                disposed = true;
                _modalFrames.Remove(frame);
                owner.Activated -= OnActivationChanged;
                dialog.Deactivated -= OnActivationChanged;
                dialog.Opened -= OnOpened;
                dialog.RemoveHandler(InputElement.GotFocusEvent, OnDialogGotFocus);
                owner.RemoveHandler(InputElement.GotFocusEvent, OnOwnerGotFocus);
                owner.RemoveHandler(InputElement.KeyDownEvent, OnOwnerKeyInput);
                owner.RemoveHandler(InputElement.KeyUpEvent, OnOwnerKeyInput);
                owner.RemoveHandler(InputElement.TextInputEvent, OnOwnerKeyInput);
            });
        }

        /// <summary>
        /// Minimizes and restores <paramref name="owner"/> together with <paramref name="dialog"/>
        /// (and vice versa) for as long as the dialog is open.
        ///
        /// A modal's owner is input-disabled, so its own caption buttons are dead - on Windows the
        /// user can minimize e.g. the batch convert dialog from its taskbar button, and the main
        /// window then stays on screen, unminimizable, blocking the desktop (#13788). While a
        /// modal is open the owner is unusable on its own, so minimizing either window means "get
        /// SubtitleEdit out of my way": the whole pair steps aside, and restoring either brings
        /// both back. Nested modals cascade naturally - the outer frame's dialog is the inner
        /// frame's owner. The undocked tool windows are independent (never in the owner chain)
        /// and deliberately not part of the mirror.
        /// </summary>
        private static IDisposable MirrorMinimizeWithOwner(Window dialog, Window owner)
        {
            // Guards this frame's own writes: setting one window's state fires its PropertyChanged
            // synchronously, which must not bounce back as a user action. Other frames of a nested
            // chain have their own flag and do react - that is what makes the cascade work.
            var syncing = false;
            var disposed = false;

            // Restore targets. A window is never restored to Minimized; if a state was somehow
            // captured as such, fall back to Normal.
            var ownerRestoreState = NonMinimized(owner.WindowState);
            var dialogRestoreState = NonMinimized(dialog.WindowState);

            // True while the owner's minimize came from this mirror (the user minimized the
            // dialog) rather than from the user minimizing the owner itself. Decides whether
            // closing the dialog while minimized should bring the owner back.
            var ownerMinimizedByMirror = false;

            // True while the pair is minimized, so the way back up can be told apart from an
            // ordinary Normal <-> Maximized change and repair the foreground exactly once.
            var pairMinimized = false;

            // Bounds SyncDialogToOwner's wait for the shell to re-show the owned dialog; after
            // ~2 s the write proceeds anyway (worst case is the pre-fix behavior).
            var dialogShowRetries = 0;

            // The counterpart writes below are POSTED, never made from inside the other window's
            // state-change dispatch. Win32 hides a minimized owner's owned windows and re-shows
            // them on restore, and Avalonia's Win32 backend records a programmatic WindowState
            // write on a hidden window WITHOUT performing it (WindowImpl.WindowState skips
            // ShowWindow when !IsWindowVisible but still sets _lastWindowState). A synchronous
            // "restore the dialog" write from inside the owner's restore dispatch can therefore
            // land while the shell has not yet re-shown the dialog - the write is swallowed, and
            // when the dialog is then actually restored its WM_SIZE reports no state change, so
            // Avalonia never invokes WindowStateChanged and never calls StartRendering: the
            // window is alive for input but permanently stops painting and laying out, frozen at
            // its pre-minimize content (#13865, ~70% of restores). Posting runs the write after
            // the OS has finished the whole restore sequence, when the owned windows are visible
            // again. The posted body re-reads live state instead of replaying the transition it
            // was queued for, so a stale post (state changed again before it ran) is a no-op.
            void SyncOwnerToDialog()
            {
                if (disposed)
                {
                    return;
                }

                syncing = true;
                try
                {
                    if (dialog.WindowState == WindowState.Minimized)
                    {
                        if (owner.WindowState != WindowState.Minimized)
                        {
                            ownerRestoreState = owner.WindowState;
                            owner.WindowState = WindowState.Minimized;
                            ownerMinimizedByMirror = true;
                        }
                    }
                    else
                    {
                        if (owner.WindowState == WindowState.Minimized)
                        {
                            owner.WindowState = ownerRestoreState;
                        }

                        ownerMinimizedByMirror = false;
                    }
                }
                finally
                {
                    syncing = false;
                }
            }

            void SyncDialogToOwner()
            {
                if (disposed)
                {
                    return;
                }

                if (owner.WindowState != WindowState.Minimized &&
                    dialog.WindowState == WindowState.Minimized &&
                    !IsOsVisible(dialog) &&
                    dialogShowRetries++ < 40)
                {
                    // The shell has not re-shown the owned dialog yet - writing now would be
                    // swallowed and poison the platform's state tracking (see above). Retry
                    // shortly; the shell shows owned windows as part of the owner's restore,
                    // so one round is normally enough.
                    DispatcherTimer.RunOnce(SyncDialogToOwner, TimeSpan.FromMilliseconds(50));
                    return;
                }

                syncing = true;
                try
                {
                    if (owner.WindowState == WindowState.Minimized)
                    {
                        if (dialog.WindowState != WindowState.Minimized)
                        {
                            dialogRestoreState = dialog.WindowState;
                            dialog.WindowState = WindowState.Minimized;
                        }
                    }
                    else
                    {
                        if (dialog.WindowState == WindowState.Minimized)
                        {
                            dialog.WindowState = dialogRestoreState;
                        }
                    }
                }
                finally
                {
                    syncing = false;
                }
            }

            void OnDialogStateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
            {
                if (e.Property != Window.WindowStateProperty || syncing)
                {
                    return;
                }

                if (dialog.WindowState == WindowState.Minimized)
                {
                    pairMinimized = true;
                }
                else
                {
                    dialogRestoreState = dialog.WindowState;
                    if (pairMinimized)
                    {
                        pairMinimized = false;
                        RepairModalForegroundAfterRestore(dialog);
                    }
                }

                Dispatcher.UIThread.Post(SyncOwnerToDialog);
            }

            void OnOwnerStateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
            {
                if (e.Property != Window.WindowStateProperty || syncing)
                {
                    return;
                }

                if (owner.WindowState == WindowState.Minimized)
                {
                    pairMinimized = true;
                }
                else
                {
                    ownerRestoreState = owner.WindowState;
                    ownerMinimizedByMirror = false;
                    if (pairMinimized)
                    {
                        pairMinimized = false;
                        RepairModalForegroundAfterRestore(dialog);
                    }
                }

                dialogShowRetries = 0;
                Dispatcher.UIThread.Post(SyncDialogToOwner);
            }

            dialog.PropertyChanged += OnDialogStateChanged;
            owner.PropertyChanged += OnOwnerStateChanged;

            return new ActionDisposable(() =>
            {
                disposed = true;
                dialog.PropertyChanged -= OnDialogStateChanged;
                owner.PropertyChanged -= OnOwnerStateChanged;

                // The dialog closing while the pair is minimized takes its taskbar button with it;
                // bring the owner back when this mirror minimized it, so the app does not end up
                // as a minimized main window the user never asked for (and whose -32000 minimized
                // position would otherwise be persisted on exit). When the user minimized the
                // owner itself, their choice is left alone. (The owner has no owner of its own,
                // so it is never OS-hidden and this synchronous write cannot be swallowed.)
                if (ownerMinimizedByMirror && owner.WindowState == WindowState.Minimized)
                {
                    owner.WindowState = ownerRestoreState;
                }
            });
        }

        /// <summary>
        /// Whether the window is visible at the OS level. Avalonia's IsVisible only tracks its
        /// own Show/Hide calls - it stays true when Windows hides an owned window because its
        /// owner was minimized, which is exactly the state the minimize mirror must not write
        /// WindowState into (#13865). Non-Windows platforms have no owned-window auto-hide, so
        /// the Avalonia flag is the truth there.
        /// </summary>
        private static bool IsOsVisible(Window window)
        {
            if (!OperatingSystem.IsWindows())
            {
                return window.IsVisible;
            }

            var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            return handle == IntPtr.Zero ? window.IsVisible : IsWindowVisible(handle);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Hands the foreground and the keyboard back to <paramref name="dialog"/> after the
        /// minimize mirror brought the pair back up.
        ///
        /// Restoring a window is an activating operation: Avalonia's Win32 backend ends every
        /// non-minimizing WindowState write with SetFocus + SetForegroundWindow on that window
        /// (WindowImpl.ShowWindow). So the mirror restoring the owner deliberately puts the OS
        /// foreground and the keyboard on a window the modal has input-disabled - the #13405
        /// state all over again: the dialog is drawn on top, Esc is dead, and the dialogs it
        /// opens come up behind it (#13865). <see cref="EnforceModalForegroundWhileOpen"/> does
        /// not catch it, because it keys on the owner *becoming* active and the owner has been
        /// active since the minimize churn - no event fires for the restore at all. So the
        /// restore repairs the invariant directly, retried on the same short timers the open
        /// path uses, since the OS is still moving windows around when the first pass runs.
        /// </summary>
        private static void RepairModalForegroundAfterRestore(Window dialog)
        {
            void Repair()
            {
                // Only the top-most dialog may hold the foreground: an inner modal opened over
                // this one owns it instead, and a lower frame's mirror must not steal it back.
                if (_modalFrames.Count == 0 || _modalFrames[^1].Dialog != dialog ||
                    !dialog.IsVisible || dialog.IsClosing() ||
                    dialog.WindowState == WindowState.Minimized)
                {
                    return;
                }

                if (!dialog.IsActive)
                {
                    dialog.Activate();
                }

                ReclaimKeyboardFocusFromDisabledOwner();
            }

            Dispatcher.UIThread.Post(Repair, DispatcherPriority.Background);
            DispatcherTimer.RunOnce(Repair, TimeSpan.FromMilliseconds(150));
            DispatcherTimer.RunOnce(Repair, TimeSpan.FromMilliseconds(450));
        }

        private static WindowState NonMinimized(WindowState state)
        {
            return state == WindowState.Minimized ? WindowState.Normal : state;
        }

        // The open modals in open order - the last entry is the top-most dialog, the only window
        // that may hold keyboard focus. The owners in this list (which include the lower dialogs
        // of a nested chain) are exactly the input-disabled windows; independent enabled windows
        // (undocked video player / audio visualizer) are never in it.
        private static readonly List<ModalFrame> _modalFrames = new();

        private sealed class ModalFrame
        {
            public ModalFrame(Window dialog, Window owner)
            {
                Dialog = dialog;
                Owner = owner;
            }

            public Window Dialog { get; }
            public Window Owner { get; }
            public IInputElement? LastDialogFocus { get; set; }
        }

        /// <summary>
        /// Moves the app-global keyboard focus back into the top-most open modal when it has ended
        /// up in one of the input-disabled windows below it (or nowhere useful at all). No-op when
        /// focus is already in the dialog or in an independent, enabled window.
        /// </summary>
        private static void ReclaimKeyboardFocusFromDisabledOwner()
        {
            if (_modalFrames.Count == 0)
            {
                return;
            }

            var frame = _modalFrames[^1];
            var dialog = frame.Dialog;
            if (!dialog.IsVisible || dialog.IsClosing())
            {
                return;
            }

            var focusedTopLevel = dialog.FocusManager?.GetFocusedElement() is Visual focusedVisual
                ? TopLevel.GetTopLevel(focusedVisual)
                : null;
            if (focusedTopLevel == dialog)
            {
                return; // healthy
            }

            if (focusedTopLevel != null && !IsDisabledByOpenModal(focusedTopLevel))
            {
                return; // focus is in an enabled window (e.g. the undocked video player) - legitimate
            }

            // Focus is in a disabled owner, on a detached control (a closed popup's item), or
            // nowhere: the dialog is the only right place for it while it is open. Prefer the
            // control that last held the caret; fall back to the first focusable control when it
            // cannot take focus anymore (hidden page, disabled button).
            if (frame.LastDialogFocus is InputElement last &&
                TopLevel.GetTopLevel(last) == dialog &&
                last.Focus())
            {
                return;
            }

            (FindFirstFocusable(dialog) as InputElement)?.Focus();
        }

        private static IInputElement? FindFirstFocusable(Visual root)
        {
            foreach (var visual in root.GetVisualDescendants())
            {
                if (visual is InputElement { Focusable: true, IsEffectivelyEnabled: true, IsEffectivelyVisible: true } element)
                {
                    return element;
                }
            }

            return null;
        }

        private static bool IsDisabledByOpenModal(TopLevel topLevel)
        {
            foreach (var frame in _modalFrames)
            {
                if (frame.Owner == topLevel)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class ActionDisposable : IDisposable
        {
            private Action? _dispose;

            public ActionDisposable(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }

        /// <summary>
        /// Keeps <paramref name="child"/> on top of other windows only while <paramref name="owner"/>
        /// or the child itself has focus. Use this instead of a blanket Topmost=true for
        /// non-modal tool windows (Find, Replace, etc.) so they don't float above other
        /// applications on macOS, where Avalonia maps Topmost to NSWindowLevel.Floating
        /// process-wide.
        /// <paramref name="suppress"/> (optional) is consulted on every re-assertion: while it
        /// returns true the child stays non-topmost no matter what the activation state says. The
        /// main menu uses this for the undocked tool windows - dropping their Topmost once when
        /// the menu opened was not enough, because opening a cascaded submenu churns window
        /// activation and the handler below re-asserted Topmost right over the submenu popup
        /// (#13187 follow-up).
        /// </summary>
        public static void KeepTopmostWhileOwnerActive(Window child, Window owner, Func<bool>? suppress = null)
        {
            void OnFocusChanged(object? sender, EventArgs e)
            {
                // Defer so the new active window's IsActive has settled before we read it
                // (Deactivated of the old window can fire before Activated of the new one).
                Dispatcher.UIThread.Post(() =>
                {
                    if (child.IsClosing() || owner.IsClosing())
                    {
                        return;
                    }

                    SetTopmost(child, suppress?.Invoke() != true && (owner.IsActive || child.IsActive), owner);
                });
            }

            owner.Activated += OnFocusChanged;
            owner.Deactivated += OnFocusChanged;
            child.Activated += OnFocusChanged;
            child.Deactivated += OnFocusChanged;
            child.Closed += (_, _) =>
            {
                owner.Activated -= OnFocusChanged;
                owner.Deactivated -= OnFocusChanged;
                child.Activated -= OnFocusChanged;
                child.Deactivated -= OnFocusChanged;
            };

            SetTopmost(child, suppress?.Invoke() != true && (owner.IsActive || child.IsActive), owner);
        }

        /// <summary>
        /// Applies <paramref name="topmost"/> to <paramref name="window"/> - and, when dropping it
        /// because another application took the foreground, keeps the window beneath that
        /// application's window.
        ///
        /// Avalonia's Win32 backend implements Topmost=false as SetWindowPos(HWND_NOTOPMOST),
        /// which Windows defines as "above all non-topmost windows": the demoted window lands at
        /// the top of the normal band. The drop is posted from Deactivated, so it usually runs
        /// after Windows has already raised the window the user clicked - and re-inserts ours
        /// right above it. The other application is active but covered, and clicking its
        /// taskbar button now minimizes it; only minimizing SE gets it out of the way (#14564,
        /// the "some always on top setting?" half of #14283 - the TTS dialog is simply the
        /// modal users leave open long enough to notice). The same band re-insertion is why
        /// flyouts suspend the undocked topmost at Opening rather than Opened (#13493).
        ///
        /// So after the drop, when the foreground window belongs to another process and already
        /// sits above <paramref name="owner"/> in the z-order, ours is moved directly beneath it.
        /// The "already above" check keeps the race benign in the other order: if the foreign
        /// window has not been raised yet, its own raise puts it on top, and re-ordering early
        /// would drop the unowned undocked tool windows behind the main window. A topmost
        /// foreign window is left alone - inserting after a topmost window makes the inserted
        /// window topmost too, and it is above us anyway. Without SWP_NOOWNERZORDER an owned
        /// dialog takes its owner along, so the whole SE stack ends up below the other
        /// application in the order it had. No-op off Windows, where Topmost maps to a window
        /// level and ordering between applications is the OS's own.
        /// </summary>
        internal static void SetTopmost(Window window, bool topmost, Window? owner)
        {
            window.Topmost = topmost;
            if (!topmost)
            {
                KeepBelowForeignForegroundWindow(window, owner ?? window);
            }
        }

        private static void KeepBelowForeignForegroundWindow(Window window, Window reference)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            var referenceHandle = reference.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            var foreground = GetForegroundWindow();
            if (handle == IntPtr.Zero || referenceHandle == IntPtr.Zero ||
                foreground == IntPtr.Zero || foreground == handle || foreground == referenceHandle)
            {
                return;
            }

            GetWindowThreadProcessId(foreground, out var foregroundProcessId);
            if (foregroundProcessId == Environment.ProcessId)
            {
                return; // an SE window (a dialog, a tool window) - the normal in-app churn
            }

            if ((GetWindowLongW(foreground, GwlExStyle) & WsExTopmost) != 0)
            {
                return; // stays above us on its own; inserting after it would make us topmost
            }

            if (!IsAboveInZOrder(foreground, referenceHandle))
            {
                return; // not raised (yet) - its own raise puts it on top
            }

            SetWindowPos(handle, foreground, 0, 0, 0, 0, SwpNoSize | SwpNoMove | SwpNoActivate);
        }

        /// <summary>
        /// True when <paramref name="hwnd"/> is above <paramref name="below"/> in the z-order,
        /// walking upwards from <paramref name="below"/>. Bounded, since the desktop's window
        /// list includes every hidden top-level window of every process.
        /// </summary>
        private static bool IsAboveInZOrder(IntPtr hwnd, IntPtr below)
        {
            var current = below;
            for (var i = 0; i < 4096; i++)
            {
                current = GetWindow(current, GwHwndPrev);
                if (current == IntPtr.Zero)
                {
                    return false;
                }

                if (current == hwnd)
                {
                    return true;
                }
            }

            return false;
        }

        private const int GwlExStyle = -20;
        private const int WsExTopmost = 0x0008;
        private const uint GwHwndPrev = 3;
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoActivate = 0x0010;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLongW(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        /// <summary>
        /// Creates a window instance using the service provider.
        /// </summary>
        private T CreateWindow<T>() where T : Window
        {
            var window = _serviceProvider.GetRequiredService<T>();
            return window;
        }

        /// <summary>
        /// Applies RTL settings to the window if the setting is enabled.
        /// </summary>
        private static void ApplyRightToLeftSettings(Window window)
        {
            if (Se.Settings.Appearance.RightToLeft)
            {
                RightToLeftHelper.SetRightToLeftForDataGridAndText(window);
            }
        }
    }
}