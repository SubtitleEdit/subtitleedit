using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Shared state and commands for the select-lines dialogs (detect speakers, skip noise lines):
/// a collection of checkable rows, Ok/Cancel, select-all/invert, and Escape-to-close.
/// </summary>
public abstract partial class SelectLinesViewModelBase<TRow> : ObservableObject where TRow : SelectLinesRowBase
{
    [ObservableProperty] private ObservableCollection<TRow> _rows;
    [ObservableProperty] private string _rowsInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    protected SelectLinesViewModelBase()
    {
        Rows = new ObservableCollection<TRow>();
        RowsInfo = string.Empty;
    }

    /// <summary>Collects the checked rows into the dialog's result, just before OK closes it.</summary>
    protected abstract void CollectResult();

    [RelayCommand]
    private void Ok()
    {
        CollectResult();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = true;
        }
    }

    [RelayCommand]
    private void InverseSelection()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = !row.IsSelected;
        }
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
