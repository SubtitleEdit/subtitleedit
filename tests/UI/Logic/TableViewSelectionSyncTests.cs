using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UITests.Logic;

/// <summary>
/// SelectionMode.AlwaysSelected picks row 0 when ItemsSource is assigned without ever filling the
/// control's SelectedItems collection. Subtitle Edit reads every selection through SelectedItems,
/// so the grid showed the first row highlighted while the app saw nothing selected - the edit box
/// and time codes went blank and a selection starting at row 1 left row 1 out (issue #13230).
/// <see cref="TableViewExtras.MakeTableView"/> repairs the collection on every ItemsSource change.
/// </summary>
public class TableViewSelectionSyncTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class Row
    {
        public string Text { get; set; } = string.Empty;
    }

    private sealed class Vm : INotifyPropertyChanged
    {
        private object? _selected;
        private int? _selectedIndex;

        public object? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// The main subtitle grid: multi-select, always selected, with the view model's current row and
    /// index bound two-way - and filled the way SetSubtitles fills it (detach, fill, re-attach).
    /// </summary>
    private (TableView Grid, Vm Vm, ObservableCollection<Row> Items) MakeGridWithRows(int rowCount)
    {
        var items = new ObservableCollection<Row>();
        var vm = new Vm();

        var grid = TableViewExtras.MakeTableView();
        grid.Columns.Add(new TableViewColumn { Header = "Text", Binding = new Binding(nameof(Row.Text)) });
        grid.ItemsSource = items;
        grid[!TableView.SelectedItemProperty] = new Binding(nameof(vm.Selected)) { Mode = BindingMode.TwoWay, Source = vm };
        grid[!TableView.SelectedIndexProperty] = new Binding(nameof(vm.SelectedIndex)) { Mode = BindingMode.TwoWay, Source = vm };

        var window = new Window { Width = 400, Height = 300, Content = grid };
        _windows.Add(window);
        window.Show();

        grid.ItemsSource = null;
        for (var i = 1; i <= rowCount; i++)
        {
            items.Add(new Row { Text = "line " + i });
        }

        grid.ItemsSource = items;

        return (grid, vm, items);
    }

    [AvaloniaFact]
    public void FirstRowIsInSelectedItemsAfterFillingTheGrid()
    {
        var (grid, vm, items) = MakeGridWithRows(20);

        Assert.Same(items[0], grid.SelectedItem);
        Assert.Same(items[0], vm.Selected);
        Assert.Equal(1, grid.Selection.Count);

        // Without the repair this is empty: the row is highlighted but nothing is selected as far
        // as the rest of the application can tell.
        Assert.Single(grid.SelectedItems!);
        Assert.Same(items[0], grid.SelectedItems![0]);
    }

    [AvaloniaFact]
    public void SelectingARangeFromTheFirstRowKeepsTheFirstRow()
    {
        var (grid, _, items) = MakeGridWithRows(20);

        // How the main grid applies a shift-selection (SelectGridRange).
        grid.Selection.BeginBatchUpdate();
        grid.Selection.Clear();
        grid.Selection.Select(4);
        grid.Selection.SelectRange(0, 4);
        grid.Selection.EndBatchUpdate();

        Assert.Equal(5, grid.Selection.Count);

        // Without the repair row 0 is missing here, so italic/copy/delete skipped the first line.
        var selected = grid.SelectedItems!.Cast<Row>().Select(r => r.Text).Order().ToArray();
        Assert.Equal(new[] { "line 1", "line 2", "line 3", "line 4", "line 5" }, selected);
        Assert.Contains(items[0], grid.SelectedItems!.Cast<Row>());
    }

    [AvaloniaFact]
    public void EmptyGridSelectsNothing()
    {
        var (grid, vm, _) = MakeGridWithRows(0);

        Assert.Equal(0, grid.Selection.Count);
        Assert.Empty(grid.SelectedItems!);
        Assert.Null(grid.SelectedItem);
        Assert.Null(vm.Selected);
    }

    [AvaloniaFact]
    public void RefillingTheGridKeepsSelectedItemsInSync()
    {
        var (grid, vm, _) = MakeGridWithRows(20);

        // A later reload (format change, undo, import, ...) goes through the same detach/fill/attach.
        var newItems = new ObservableCollection<Row>();
        for (var i = 1; i <= 5; i++)
        {
            newItems.Add(new Row { Text = "new " + i });
        }

        grid.ItemsSource = null;
        grid.ItemsSource = newItems;

        Assert.Same(newItems[0], grid.SelectedItem);
        Assert.Same(newItems[0], vm.Selected);
        Assert.Single(grid.SelectedItems!);
        Assert.Same(newItems[0], grid.SelectedItems![0]);
    }
}
