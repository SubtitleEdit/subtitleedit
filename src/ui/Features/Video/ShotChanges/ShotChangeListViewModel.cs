using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public partial class ShotChangeListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ShotChangeItem> _shotChanges;
    [ObservableProperty] private ShotChangeItem? _selectedShotChange;
    [ObservableProperty] private bool _hasShotChanges;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }
    public bool OKProssed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private string _videoFileName = string.Empty;

    public ShotChangeListViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        ShotChanges = new ObservableCollection<ShotChangeItem>();
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
            Se.Language.Video.ShotChanges.ShotChangesClearQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        ShotChanges.Clear();
        OKProssed = true;

        Window?.Close();
    }

    /// <summary>
    /// Saves the shot changes to a file of the user's choosing, in the same format as the
    /// .shotchanges files in the data folder - those carry a hash of the video in their name, so
    /// they are hard to pick out for use outside Subtitle Edit.
    /// </summary>
    [RelayCommand]
    private async Task Export()
    {
        if (Window == null || ShotChanges.Count == 0)
        {
            return;
        }

        var suggestedFileName = string.IsNullOrEmpty(_videoFileName)
            ? "shot-changes"
            : Path.Combine(Path.GetDirectoryName(_videoFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(_videoFileName));

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            new[]
            {
                (Se.Language.Video.ShotChanges.ShotChanges, ".shotchanges"),
                (Se.Language.General.TextFiles, ".txt"),
            },
            suggestedFileName,
            Se.Language.Video.ShotChanges.ExportShotChanges);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            await File.WriteAllTextAsync(fileName, ShotChangesHelper.ToText(ShotChanges.Select(p => p.Seconds)));
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.CouldNotSaveFileXErrorY, fileName, ex.Message),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private void GoTo()
    {
        GoToPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task DeleteSelectedLine(ShotChangeItem shotChange)
    {
        if (shotChange == null || Window == null)
        {
            return;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.DeleteCurrentLine,
            Se.Language.Video.ShotChanges.DeleteSelectedShotChangeQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        ShotChanges.Remove(shotChange);
        OKProssed = true;
    }

    /// <summary>
    /// Copies the given rows as "number TAB time" lines, the way the DataGrid this list used to
    /// be copied them on Ctrl+C - the TableView it became has no copy of its own.
    /// </summary>
    internal async Task CopyToClipboard(IEnumerable<ShotChangeItem> items)
    {
        if (Window == null)
        {
            return;
        }

        var sb = new StringBuilder();
        foreach (var item in items.OrderBy(p => p.Index))
        {
            sb.Append(item.Index).Append('\t').AppendLine(item.TimeText);
        }

        if (sb.Length > 0)
        {
            await ClipboardHelper.SetTextAsync(Window, sb.ToString());
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OKProssed = false;
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
            UiUtil.ShowHelp("features/shot-changes");
        }
    }

    internal void Initialize(List<double> shotChanges, string? videoFileName = null)
    {
        _videoFileName = videoFileName ?? string.Empty;
        foreach (var time in shotChanges)
        {
            ShotChanges.Add(new ShotChangeItem(ShotChanges.Count, time));
        }
    }

    /// <summary>
    /// Follows the selection itself instead of the grid's SelectionChanged event: the row the
    /// grid selects on its own (AlwaysSelected) never raises that event, which left Go to and
    /// Clear disabled while row 0 looked selected.
    /// </summary>
    partial void OnSelectedShotChangeChanged(ShotChangeItem? value)
    {
        HasShotChanges = value != null;
    }

    internal void OnShotChangeGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            GoTo();
        });
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            if (SelectedShotChange != null)
            {
                Dispatcher.UIThread.Invoke(async void() =>
                {
                    await DeleteSelectedLine(SelectedShotChange);
                });
            }
        }
    }
}