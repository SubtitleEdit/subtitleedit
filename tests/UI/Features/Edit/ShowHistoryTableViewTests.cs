using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;

namespace UITests.Features.Edit;

/// <summary>
/// Pilot coverage for the Avalonia 12.1 TableView control, which replaced the DataGrid in
/// this window (DataGrid is in maintenance mode upstream). Asserts the behaviour the old
/// DataGrid provided: columns declared, rows realized and virtualized, and selection
/// round-tripping to the view model.
/// </summary>
public class ShowHistoryTableViewTests
{
    private static ShowHistoryViewModel MakeViewModel(int itemCount)
    {
        var vm = new ShowHistoryViewModel();
        for (var i = 0; i < itemCount; i++)
        {
            vm.HistoryItems.Add(new ShowHistoryDisplayItem
            {
                Time = $"2026-07-21 10:00:{i:00}",
                Description = $"Change {i}",
            });
        }

        return vm;
    }

    private static TableView GetTableView(Window window) =>
        window.GetVisualDescendants().OfType<TableView>().Single();

    [AvaloniaFact]
    public void ShowHistoryWindow_UsesTableViewWithBothColumns()
    {
        var vm = MakeViewModel(3);
        var window = new ShowHistoryWindow(vm);
        window.Show();

        var tableView = GetTableView(window);

        Assert.Equal(2, tableView.Columns.Count);
        Assert.Equal(GridUnitType.Auto, tableView.Columns[0].Width.GridUnitType);
        Assert.Equal(GridUnitType.Star, tableView.Columns[1].Width.GridUnitType);
        Assert.Equal(vm.HistoryItems, tableView.ItemsSource);
    }

    [AvaloniaFact]
    public void ShowHistoryWindow_RealizesRowsAndRendersCellText()
    {
        var vm = MakeViewModel(3);
        var window = new ShowHistoryWindow(vm);
        window.Show();

        var tableView = GetTableView(window);
        var rows = tableView.GetVisualDescendants().OfType<TableViewRow>().ToList();
        Assert.Equal(3, rows.Count);

        // Cell content must actually resolve through TableViewColumn.Binding.
        var texts = rows[0].GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("2026-07-21 10:00:00", texts);
        Assert.Contains("Change 0", texts);
    }

    [AvaloniaFact]
    public void ShowHistoryWindow_SelectionUpdatesViewModelAndEnablesRollback()
    {
        var vm = MakeViewModel(3);
        var window = new ShowHistoryWindow(vm);
        window.Show();

        var tableView = GetTableView(window);
        Assert.False(vm.IsRollbackEnabled);

        tableView.SelectedIndex = 1;

        Assert.Same(vm.HistoryItems[1], vm.SelectedHistoryItem);
        Assert.True(vm.IsRollbackEnabled);
    }

    [AvaloniaFact]
    public void ShowHistoryWindow_VirtualizesLargeHistory()
    {
        // The old DataGrid virtualized; TableView derives from ListBox and must too,
        // otherwise a long undo history would realize thousands of rows.
        var vm = MakeViewModel(2000);
        var window = new ShowHistoryWindow(vm);
        window.Show();

        var tableView = GetTableView(window);
        var realized = tableView.GetVisualDescendants().OfType<TableViewRow>().Count();

        Assert.InRange(realized, 1, 200);
    }
}
