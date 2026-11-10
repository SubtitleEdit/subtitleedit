using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public partial class BookmarksListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private bool _hasBookmarks;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private List<SubtitleLineViewModel> _allSubtitles = new();

    public BookmarksListViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
    }

    /// <summary>
    /// Saves the bookmarks to a text file, see <see cref="BookmarksFile"/>.
    /// </summary>
    [RelayCommand]
    private async Task Export()
    {
        if (Window == null || Subtitles.Count == 0)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            new[] { (Se.Language.General.TextFiles, ".txt") },
            "bookmarks",
            Se.Language.General.BookmarksList);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            await File.WriteAllTextAsync(fileName, BookmarksFile.ToText(Subtitles));
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.CouldNotSaveFileXErrorY, fileName, ex.Message),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Reads a file saved by <see cref="Export"/> and sets its bookmarks on the matching lines -
    /// existing bookmarks on other lines are kept.
    /// </summary>
    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.BookmarksList, Se.Language.General.TextFiles, ".txt");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            BookmarksFile.Apply(await File.ReadAllTextAsync(fileName), _allSubtitles);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.CouldNotOpenFileXErrorY, fileName, ex.Message),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var selected = SelectedSubtitle;
        Subtitles.Clear();
        Subtitles.AddRange(_allSubtitles.Where(p => p.Bookmark != null));
        SelectedSubtitle = selected != null && Subtitles.Contains(selected) ? selected : Subtitles.FirstOrDefault();
        HasBookmarks = SelectedSubtitle != null;
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

        if (Se.Settings.General.PromptBeforeDelete)
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/subtitle-grid", "bookmarks");
        }
    }

    internal void Initialize(List<SubtitleLineViewModel> allSubtitles)
    {
        _allSubtitles = allSubtitles;
        Subtitles.AddRange(allSubtitles.Where(p => p.Bookmark != null));
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