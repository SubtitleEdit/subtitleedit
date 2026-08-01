using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

public partial class ErrorListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ErrorListItem> _subtitles;
    [ObservableProperty] private ErrorListItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasErrors;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    public ErrorListViewModel()
    {
        Subtitles = new ObservableCollection<ErrorListItem>();
    }
    
    [RelayCommand]
    private void GoTo()
    {
        GoToPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    /// <summary>
    /// The items come ready-made from the caller: it is the only place that still knows each
    /// line's real neighbours, which the gap/overlap wording needs.
    /// </summary>
    internal void Initialize(List<ErrorListItem> errorListItems)
    {
        foreach (var item in errorListItems)
        {
            Subtitles.Add(item);
        }
    }

    /// <summary>
    /// Follows the selection itself instead of the grid's SelectionChanged event: the row the
    /// grid selects on its own (AlwaysSelected) never raises that event, which left Go to
    /// disabled while row 0 looked selected.
    /// </summary>
    partial void OnSelectedSubtitleChanged(ErrorListItem? value)
    {
        HasErrors = value != null;
    }

    internal void OnBookmarksGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(GoTo);
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            GoTo();
        }
    }
}