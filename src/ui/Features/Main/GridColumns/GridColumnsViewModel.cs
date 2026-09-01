using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.GridColumns;

public partial class GridColumnsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<GridColumnDisplay> _columns;
    [ObservableProperty] private GridColumnDisplay? _selectedColumn;

    /// <summary>The flattened column key order after OK, ready for SubtitleGridColumnOrder.</summary>
    public List<string> ResultColumnOrder { get; private set; } = new();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public bool HasChanges { get; private set; }

    private List<GridColumnDisplay> _defaultOrder = new();
    private List<string> _initialOrder = new();
    private List<bool> _initialVisibility = new();

    public GridColumnsViewModel()
    {
        Columns = new ObservableCollection<GridColumnDisplay>();
    }

    /// <summary>
    /// <paramref name="entries"/> must be in the built-in default column order;
    /// <paramref name="currentOrder"/> is the saved key order (may be empty = default,
    /// and may be from an older version that lacks some of today's keys).
    /// </summary>
    public void Initialize(List<GridColumnDisplay> entries, List<string> currentOrder)
    {
        _defaultOrder = entries;

        // Same merge as TableViewColumnManager.ApplyOrder: saved keys first, then any
        // entry the saved order does not know at its default position.
        var ordered = new List<GridColumnDisplay>();
        foreach (var key in currentOrder)
        {
            var entry = entries.FirstOrDefault(x => x.Keys.Contains(key));
            if (entry != null && !ordered.Contains(entry))
            {
                ordered.Add(entry);
            }
        }

        for (var i = 0; i < entries.Count; i++)
        {
            if (!ordered.Contains(entries[i]))
            {
                ordered.Insert(System.Math.Min(i, ordered.Count), entries[i]);
            }
        }

        Columns.Clear();
        foreach (var entry in ordered)
        {
            Columns.Add(entry);
        }

        _initialOrder = ordered.SelectMany(x => x.Keys).ToList();
        _initialVisibility = ordered.Select(x => x.IsVisible).ToList();

        SelectedColumn = Columns.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
    {
        ResultColumnOrder = Columns.SelectMany(x => x.Keys).ToList();
        HasChanges = !ResultColumnOrder.SequenceEqual(_initialOrder) ||
                     !Columns.Select(x => x.IsVisible).SequenceEqual(_initialVisibility);
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void MoveUp()
    {
        Move(-1);
    }

    [RelayCommand]
    private void MoveDown()
    {
        Move(1);
    }

    private void Move(int delta)
    {
        if (SelectedColumn == null)
        {
            return;
        }

        var index = Columns.IndexOf(SelectedColumn);
        var newIndex = index + delta;
        if (newIndex < 0 || newIndex >= Columns.Count)
        {
            return;
        }

        var selected = SelectedColumn;
        Columns.Move(index, newIndex);
        SelectedColumn = selected;
    }

    [RelayCommand]
    private void Reset()
    {
        var selected = SelectedColumn;
        Columns.Clear();
        foreach (var entry in _defaultOrder)
        {
            Columns.Add(entry);
        }

        SelectedColumn = selected ?? Columns.FirstOrDefault();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
