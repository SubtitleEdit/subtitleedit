using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace UITests;

/// <summary>
/// An <see cref="IWindowService"/> for view models that only open a window from a command the test
/// never runs - the EBU save options dialog opens the font picker, for instance. Every member
/// throws, so a test that does take such a path fails loudly instead of silently doing nothing.
/// </summary>
public sealed class StubWindowService : IWindowService
{
    public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window
        => throw new NotSupportedException();

    public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
        where T : Window where TViewModel : class
        => throw new NotSupportedException();

    public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
        where T : Window where TViewModel : class
        => throw new NotSupportedException();

    public Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
        => throw new NotSupportedException();

    public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
        Window owner,
        Action<TViewModel>? configureViewModel = null,
        Action<TWindow>? configureWindow = null)
        where TWindow : Window where TViewModel : class
        => throw new NotSupportedException();
}
