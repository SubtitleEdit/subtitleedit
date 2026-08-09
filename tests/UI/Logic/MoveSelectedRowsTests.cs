using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Logic;

/// <summary>
/// The context-menu reordering in the ASSA/SSA styles dialogs (#13056) moves rows one
/// <see cref="ObservableCollection{T}.Move"/> at a time and then puts the selection back
/// on the same rows. These cover the part <see cref="ListReorderTests"/> cannot: that the
/// TableView really ends up selecting the moved rows, and that SelectedItem - which the
/// view models bind their "current style" to - still points at the anchor row afterwards.
/// </summary>
public class MoveSelectedRowsTests : IDisposable
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
        public Row(string name) => Name = name;

        public string Name { get; }

        public override string ToString() => Name;
    }

    private (TableView Grid, ObservableCollection<Row> Items) MakeGrid()
    {
        var items = new ObservableCollection<Row>(
            new[] { "a", "b", "c", "d", "e" }.Select(n => new Row(n)));

        var grid = TableViewExtras.MakeTableView();
        grid.Columns.Add(new SeTableViewColumn { Header = "Name" });
        grid.ItemsSource = items;

        var window = new Window { Content = grid, Width = 400, Height = 300 };
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return (grid, items);
    }

    /// <summary>
    /// Batched, because <see cref="TableViewExtras.MakeTableView"/> keeps
    /// SelectionMode.AlwaysSelected: a bare Clear() lets the control put row 0 back before
    /// the intended rows are selected, which silently widens the selection.
    /// </summary>
    private static void Select(TableView grid, params int[] indices)
    {
        grid.Selection.BeginBatchUpdate();
        grid.Selection.Clear();
        foreach (var index in indices)
        {
            grid.Selection.Select(index);
        }

        grid.Selection.EndBatchUpdate();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(indices.Length, grid.SelectedItems!.Count);
    }

    private static string Order(ObservableCollection<Row> items) =>
        string.Concat(items.Select(i => i.Name));

    private static string SelectedNames(TableView grid) =>
        string.Concat(grid.SelectedItems!.OfType<Row>().OrderBy(r => r.Name).Select(r => r.Name));

    [AvaloniaFact]
    public void Up_MovesTheRowAndKeepsItSelected()
    {
        var (grid, items) = MakeGrid();
        Select(grid, 2); // "c"

        TableViewExtras.MoveSelectedRows(grid, items, ListMoveDirection.Up);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("acbde", Order(items));
        Assert.Equal("c", SelectedNames(grid));
        Assert.Equal("c", ((Row)grid.SelectedItem!).Name);
        Assert.Equal(1, grid.SelectedIndex);
    }

    [AvaloniaFact]
    public void Down_KeepsAMultiRowSelectionIntact()
    {
        var (grid, items) = MakeGrid();
        Select(grid, 1, 2); // "b", "c"

        TableViewExtras.MoveSelectedRows(grid, items, ListMoveDirection.Down);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("adbce", Order(items));
        Assert.Equal("bc", SelectedNames(grid));
        Assert.Equal(2, grid.SelectedItems!.Count);
    }

    [AvaloniaFact]
    public void Bottom_KeepsScatteredSelectionAndItsAnchor()
    {
        var (grid, items) = MakeGrid();
        Select(grid, 1, 3); // "b", "d" - "b" is the anchor (selected first)

        TableViewExtras.MoveSelectedRows(grid, items, ListMoveDirection.Bottom);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("acebd", Order(items));
        Assert.Equal("bd", SelectedNames(grid));
        Assert.Equal("b", ((Row)grid.SelectedItem!).Name);
    }

    [AvaloniaFact]
    public void Top_MovesEverySelectedRowToTheFront()
    {
        var (grid, items) = MakeGrid();
        Select(grid, 1, 3);

        TableViewExtras.MoveSelectedRows(grid, items, ListMoveDirection.Top);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("bdace", Order(items));
        Assert.Equal("bd", SelectedNames(grid));
    }

    [AvaloniaFact]
    public void Up_AtTheTopChangesNothingAndKeepsTheSelection()
    {
        var (grid, items) = MakeGrid();
        Select(grid, 0);

        TableViewExtras.MoveSelectedRows(grid, items, ListMoveDirection.Up);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("abcde", Order(items));
        Assert.Equal("a", SelectedNames(grid));
    }
}
