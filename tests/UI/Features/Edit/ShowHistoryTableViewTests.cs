using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
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

    /// <summary>Shows the window and settles layout - the grid-line alignment is applied on Loaded.</summary>
    private static ShowHistoryWindow ShowWindow(ShowHistoryViewModel vm)
    {
        var window = new ShowHistoryWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    private static double AbsoluteX(Visual visual, Visual relativeTo) =>
        visual.TranslatePoint(new Point(0, 0), relativeTo)!.Value.X;

    private static double AbsoluteY(Visual visual, Visual relativeTo) =>
        visual.TranslatePoint(new Point(0, 0), relativeTo)!.Value.Y;

    [AvaloniaFact]
    public void ShowHistoryWindow_HeaderAndCellGridLinesAlign()
    {
        // TableView insets the header inside a Border with hard-coded padding while rows sit flush
        // against the control edge, so the header's column divider was drawn 6px right of the cell
        // dividers below it - and in a different colour, because the built-in header draws its
        // separator as a semi-transparent rectangle inside the resize thumb.
        var previous = Se.Settings.Appearance.GridLinesAppearance;
        try
        {
            Se.Settings.Appearance.GridLinesAppearance = "All";

            var window = ShowWindow(MakeViewModel(3));
            var tableView = GetTableView(window);

            var headers = tableView.GetVisualDescendants().OfType<TableViewColumnHeader>().ToList();
            var firstRow = tableView.GetVisualDescendants().OfType<TableViewRow>().First();
            var cells = firstRow.GetVisualDescendants().OfType<TableViewCell>().ToList();
            Assert.Equal(2, headers.Count);
            Assert.Equal(2, cells.Count);

            // The divider between column 1 and 2 must be at the same x in the header and the rows.
            var headerDividerX = AbsoluteX(headers[0], tableView) + headers[0].Bounds.Width;
            var cellDividerX = AbsoluteX(cells[0], tableView) + cells[0].Bounds.Width;
            Assert.Equal(cellDividerX, headerDividerX);

            // Both draw their lines with the same brush, unlike the built-in header separator.
            Assert.Equal(headers[0].BorderBrush?.ToString(), cells[0].BorderBrush?.ToString());
        }
        finally
        {
            Se.Settings.Appearance.GridLinesAppearance = previous;
        }
    }

    [AvaloniaFact]
    public void ShowHistoryWindow_HeaderBottomLineTouchesTheFirstRow()
    {
        // The cells only draw bottom borders, so without a header bottom line the first row had no
        // line above it - and the line has to sit exactly on the row's top edge, not float above it.
        var previous = Se.Settings.Appearance.GridLinesAppearance;
        try
        {
            Se.Settings.Appearance.GridLinesAppearance = "All";

            var window = ShowWindow(MakeViewModel(3));
            var tableView = GetTableView(window);

            var header = tableView.GetVisualDescendants().OfType<TableViewColumnHeader>().First();
            var firstRow = tableView.GetVisualDescendants().OfType<TableViewRow>().First();

            Assert.Equal(1, header.BorderThickness.Bottom);
            Assert.Equal(AbsoluteY(header, tableView) + header.Bounds.Height, AbsoluteY(firstRow, tableView));
        }
        finally
        {
            Se.Settings.Appearance.GridLinesAppearance = previous;
        }
    }

    [AvaloniaFact]
    public void ShowHistoryWindow_UsesTableViewWithBothColumns()
    {
        var vm = MakeViewModel(3);
        var window = ShowWindow(vm);

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
        var window = ShowWindow(vm);

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
        var window = ShowWindow(vm);

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

            var window = ShowWindow(MakeViewModel(3));

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

            var window = ShowWindow(MakeViewModel(3));

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
    public void ShowHistoryWindow_CellsFillTheirRowSoGridLinesAreContinuous()
    {
        // TableViewRow is a ListBoxItem whose default padding sits outside the cells. Left as-is,
        // cells were 18px inside 39px rows, so the horizontal line floated above the row edge and
        // the vertical line was drawn as one short segment per row instead of a continuous column.
        var previous = Se.Settings.Appearance.GridLinesAppearance;
        try
        {
            Se.Settings.Appearance.GridLinesAppearance = "All";

            var window = ShowWindow(MakeViewModel(4));

            var tableView = GetTableView(window);
            var rows = tableView.GetVisualDescendants().OfType<TableViewRow>().ToList();
            Assert.True(rows.Count >= 2);

            foreach (var row in rows)
            {
                var cells = row.GetVisualDescendants().OfType<TableViewCell>().ToList();
                Assert.Equal(2, cells.Count);

                // Every cell spans the full row height, so its bottom/right borders land on the
                // row edges and join up with the neighbouring rows' lines.
                foreach (var cell in cells)
                {
                    Assert.Equal(row.Bounds.Height, cell.Bounds.Height);
                }

                // Cells tile horizontally without gaps.
                Assert.Equal(0, cells[0].Bounds.X);
                Assert.Equal(cells[0].Bounds.Right, cells[1].Bounds.X);
            }

            // Rows are stacked without vertical gaps, so vertical lines are unbroken.
            for (var i = 1; i < rows.Count; i++)
            {
                Assert.Equal(rows[i - 1].Bounds.Bottom, rows[i].Bounds.Y);
            }
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
        var window = ShowWindow(vm);

        var tableView = GetTableView(window);
        var realized = tableView.GetVisualDescendants().OfType<TableViewRow>().Count();

        Assert.InRange(realized, 1, 200);
    }
}
