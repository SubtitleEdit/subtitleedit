using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Logic.Config;

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

    [AvaloniaTheory]
    [InlineData("None", 0, 0)]
    [InlineData("Horizontal", 0, 1)]
    [InlineData("Vertical", 1, 0)]
    [InlineData("All", 1, 1)]
    public void ShowHistoryWindow_CellBordersFollowGridLinesSetting(string setting, double right, double bottom)
    {
        var previous = Se.Settings.Appearance.GridLinesAppearance;
        try
        {
            Se.Settings.Appearance.GridLinesAppearance = setting;

            var window = new ShowHistoryWindow(MakeViewModel(3));
            window.Show();

            var cells = GetTableView(window).GetVisualDescendants().OfType<TableViewCell>().ToList();
            Assert.NotEmpty(cells);
            foreach (var cell in cells)
            {
                Assert.Equal(right, cell.BorderThickness.Right);
                Assert.Equal(bottom, cell.BorderThickness.Bottom);
                Assert.Equal(0, cell.BorderThickness.Left);
                Assert.Equal(0, cell.BorderThickness.Top);
            }
        }
        finally
        {
            Se.Settings.Appearance.GridLinesAppearance = previous;
        }
    }

    [AvaloniaFact]
    public void ShowHistoryWindow_CellPresenterActuallyPaintsTheBorder()
    {
        // Setting TableViewCell.BorderThickness alone draws nothing: the control's built-in
        // template only template-binds Background, so UiUtil supplies a template that passes
        // the border properties to the ContentPresenter (which paints the border in Avalonia).
        // Without that, grid lines are silently missing.
        var previous = Se.Settings.Appearance.GridLinesAppearance;
        try
        {
            Se.Settings.Appearance.GridLinesAppearance = "All";

            var window = new ShowHistoryWindow(MakeViewModel(3));
            window.Show();

            var cell = GetTableView(window).GetVisualDescendants().OfType<TableViewCell>().First();
            var presenter = cell.GetVisualDescendants().OfType<ContentPresenter>().First();

            Assert.Equal(1, presenter.BorderThickness.Right);
            Assert.Equal(1, presenter.BorderThickness.Bottom);
            Assert.NotNull(presenter.BorderBrush);
        }
        finally
        {
            Se.Settings.Appearance.GridLinesAppearance = previous;
        }
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
