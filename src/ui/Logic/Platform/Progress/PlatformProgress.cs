using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Nikse.SubtitleEdit.Logic.Platform.Progress;

public static class PlatformProgress
{
    public static readonly AttachedProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<ProgressBar, bool>("IsActive", typeof(PlatformProgress));

    private static readonly ConditionalWeakTable<Window, PlatformProgressGroup> WindowGroups = new();
    private static readonly PlatformProgressGroup ApplicationGroup = new(() =>
        OperatingSystem.IsMacOS() ? new MacDockProgress() : new LinuxLauncherProgress());

    public static ProgressBar WithPlatformProgress(this ProgressBar progressBar, Window window,
        string activeProperty, IValueConverter? converter = null)
    {
        progressBar.Bind(IsActiveProperty, new Binding(activeProperty) { Converter = converter });
        _ = new Attachment(progressBar, window);
        return progressBar;
    }

    internal static Window GetOwner(Window window)
    {
        while (window.Owner is Window owner)
            window = owner;
        return window;
    }

    private sealed class Attachment
    {
        private readonly ProgressBar _bar;
        private readonly Window _window;
        private PlatformProgressGroup? _group;

        public Attachment(ProgressBar bar, Window window)
        {
            _bar = bar;
            _window = window;
            window.Opened += OnOpened;
            window.Closed += OnClosed;
            bar.PropertyChanged += OnPropertyChanged;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            if (OperatingSystem.IsWindows())
                _group = WindowGroups.GetValue(GetOwner(_window), owner =>
                    new PlatformProgressGroup(() => new WindowsTaskbarProgress(owner)));
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
                _group = ApplicationGroup;
            Update();
        }

        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == IsActiveProperty || e.Property == ProgressBar.ValueProperty ||
                e.Property == ProgressBar.MinimumProperty || e.Property == ProgressBar.MaximumProperty ||
                e.Property == ProgressBar.IsIndeterminateProperty)
                Update();
        }

        private void Update()
        {
            if (!_bar.GetValue(IsActiveProperty))
            {
                _group?.Update(this, null, false);
                return;
            }

            var range = _bar.Maximum - _bar.Minimum;
            var percentage = range > 0 ? (_bar.Value - _bar.Minimum) * 100 / range : 0;
            _group?.Update(this, percentage, _bar.IsIndeterminate);
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _bar.PropertyChanged -= OnPropertyChanged;
            _window.Opened -= OnOpened;
            _window.Closed -= OnClosed;
            _group?.Update(this, null, false);
            _group = null;
        }
    }
}
