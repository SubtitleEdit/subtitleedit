using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
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

            window.Show();
            window.Focus();

            return viewModel;
        }

        /// <inheritdoc />
        public async Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
        {
            var window = CreateWindow<T>();

            configure?.Invoke(window);

            ApplyRightToLeftSettings(window);
            UiTheme.ApplyScaleToWindow(window);

            // Keep the dialog above undocked tool windows (audio visualizer / video player), which
            // float on top of the main window via the same helper. Without this the dialog opens
            // behind them in undocked mode. (#11971)
            KeepTopmostWhileOwnerActive(window, owner);

            // Drop the undocked windows' topmost for the dialog's lifetime and make sure the
            // dialog really gets OS activation, not just top-of-z-order drawing (#13325).
            using var undockedSuspension = SuspendUndockedTopmost();
            ActivateWhenOpened(window);

            await YieldForPendingFlyoutDismissAsync();
            await window.ShowDialog(owner);

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

            // Keep the dialog above undocked tool windows (audio visualizer / video player), which
            // float on top of the main window via the same helper. Without this the dialog opens
            // behind them in undocked mode. (#11971)
            KeepTopmostWhileOwnerActive(window, owner);

            // Drop the undocked windows' topmost for the dialog's lifetime and make sure the
            // dialog really gets OS activation, not just top-of-z-order drawing (#13325).
            using var undockedSuspension = SuspendUndockedTopmost();
            ActivateWhenOpened(window);

            await YieldForPendingFlyoutDismissAsync();
            await window.ShowDialog(owner);

            return viewModel;
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
        /// </summary>
        public static void SuspendUndockedTopmostWhileOpen(FlyoutBase flyout)
        {
            IDisposable? suspension = null;
            flyout.Opened += (_, _) => suspension ??= SuspendUndockedTopmost();
            flyout.Closed += (_, _) =>
            {
                suspension?.Dispose();
                suspension = null;
            };
        }

        /// <summary>
        /// Same as <see cref="SuspendUndockedTopmostWhileOpen(FlyoutBase)"/> for the main menu bar.
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

        /// <summary>
        /// Explicitly activates <paramref name="window"/> once it has opened, in case the OS
        /// left foreground/keyboard focus on another window during the open (seen on Windows in
        /// undocked mode, where the dialog was drawn on top but never activated - gray buttons,
        /// no keyboard focus - #13325). Posted at Background priority so pending activation and
        /// topmost changes settle first; a no-op when the window is already active.
        /// </summary>
        public static void ActivateWhenOpened(Window window)
        {
            window.Opened += (_, _) => Dispatcher.UIThread.Post(() =>
            {
                if (window.IsVisible && !window.IsActive)
                {
                    window.Activate();
                }
            }, DispatcherPriority.Background);
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

                    child.Topmost = suppress?.Invoke() != true && (owner.IsActive || child.IsActive);
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

            child.Topmost = suppress?.Invoke() != true && (owner.IsActive || child.IsActive);
        }

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