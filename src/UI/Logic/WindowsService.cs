using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
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
            window.Show();
            window.Focus();

            ApplyRightToLeftSettings(window);

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

            window.WindowStartupLocation = WindowStartupLocation.CenterOwner; //TODO: does this work on mac?
            window.Show(owner);
            window.Focus();

            ApplyRightToLeftSettings(window);

            return viewModel;
        }

        /// <inheritdoc />
        public async Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
        {
            var window = CreateWindow<T>();

            configure?.Invoke(window);

            ApplyRightToLeftSettings(window);

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

            window.WindowStartupLocation = WindowStartupLocation.CenterOwner; //TODO: does this work on mac?
            configureWindow?.Invoke(window);

            ApplyRightToLeftSettings(window);

            await window.ShowDialog(owner);

            return viewModel;
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