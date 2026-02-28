using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Shared.FindText;

public partial class FindTextViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _isOkEnabled;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; internal set; }

    private List<SubtitleLineViewModel> _allSubtitles;
    private bool _dirty;
    private System.Timers.Timer _searchTimer;
    private Lock _updateLock;

    public FindTextViewModel()
    {
        _allSubtitles = new List<SubtitleLineViewModel>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        Title = string.Empty;
        SubtitleGrid = new DataGrid();
        SearchText = string.Empty;
        _updateLock = new Lock();
        _searchTimer = new System.Timers.Timer();
        _searchTimer.Interval = 250;
        _searchTimer.Elapsed += _searchTimer_Elapsed;
    }

    private void _searchTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (_updateLock)
        {
            if (!_dirty)
            {
                return;
            }

            _dirty = false;
            Dispatcher.UIThread.Invoke(() =>
            {
                Subtitles.Clear();
                if (SearchText.Length == 0)
                {
                    Subtitles.AddRange(_allSubtitles);
                    return;
                }

                Subtitles.AddRange(_allSubtitles.Where(p => p.Text.Contains(SearchText, System.StringComparison.InvariantCultureIgnoreCase)));
            });
        }
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitleLines, string title)
    {
        Title = title;
        _allSubtitles = subtitleLines;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>(subtitleLines);
        _searchTimer.Start();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
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

    internal void SearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        lock (_updateLock)
        {
            _dirty = true;
        }
    }

    internal void SubtitleGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsOkEnabled = SelectedSubtitle != null;
    }

    internal void OnSubtitleGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem != null)
        {
            if (grid.SelectedItem is SubtitleLineViewModel selectedItem)
            {
                SelectedSubtitle = selectedItem;
                Ok();
            }
        }
    }

    internal void SubtitleGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && IsOkEnabled)
        {
            e.Handled = true;
            Ok();
        }
    }
}