using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public partial class BookmarksListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private bool _hasBookmarks;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    public BookmarksListViewModel()
    {
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
    }

    [RelayCommand]
    private async Task Clear()
    {
        if (Window == null)
        {
            return;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.Clear,
            Se.Language.General.BookmarkClearQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (var subtitle in Subtitles)
        {
            subtitle.Bookmark = null;
        }

        Window?.Close();
    }

    [RelayCommand]
    private void GoTo()
    {
        GoToPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task DeleteSelectedLine(SubtitleLineViewModel subtitle)
    {
        if (subtitle == null || Window == null)
        {
            return;
        }

        if (Se.Settings.General.PromptDeleteLines)
        {
            var result = await MessageBox.Show(
                Window,
                Se.Language.General.DeleteCurrentLine,
                Se.Language.General.BookmarkDeleteSelectedQuestion,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        subtitle.Bookmark = null;
        var idx = Subtitles.IndexOf(subtitle);
        Subtitles.Remove(subtitle);
        if (Subtitles.Count > 0)
        {
            if (idx >= Subtitles.Count)
            {
                idx = Subtitles.Count - 1;
            }

            SelectedSubtitle = Subtitles[idx];
        }
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

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels)
    {
        Subtitles.AddRange(subtitleLineViewModels);
        if (Subtitles.Count > 0)
        {
            SelectedSubtitle = Subtitles[0];
        }

        HasBookmarks = SelectedSubtitle != null;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasBookmarks = SelectedSubtitle != null;
    }

    internal void OnBookmarksGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { GoTo(); });
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            if (SelectedSubtitle != null)
            {
                Dispatcher.UIThread.Invoke(async void () => { await DeleteSelectedLine(SelectedSubtitle); });
            }
        }
    }
}