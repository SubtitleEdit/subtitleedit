using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The app-wide TableViewRow theme (<see cref="UiUtil.ApplyFluent2TableViewTheme"/>) replaces
/// Fluent 2's eight-visual row template with a lean one for grid performance. Selected rows are
/// filled with the accent list brush (full-row selection, as under Fluent 1), there are no template
/// row lines (SE draws grid lines on the cells), and the row's own Background (alternating-row
/// tint) reaches the row border.
/// </summary>
public class TableViewLeanRowThemeTests : IDisposable
{
    private readonly List<Window> _windows = [];

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }
    }

    private sealed record Item(string Text);

    private (Window Window, TableView Grid) ShowGrid()
    {
        var grid = TableViewExtras.MakeTableView();
        grid.Columns.Add(new TableViewColumn
        {
            Header = "Text",
            Binding = new Avalonia.Data.Binding(nameof(Item.Text)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        grid.ItemsSource = Enumerable.Range(1, 5).Select(i => new Item("Line " + i)).ToList();

        var window = new Window { Width = 400, Height = 300, Content = grid };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return (window, grid);
    }

    private static List<TableViewRow> Rows(TableView grid) =>
        grid.GetVisualDescendants().OfType<TableViewRow>().OrderBy(r => r.Bounds.Y).ToList();

    private static Border RootBorder(TableViewRow row) =>
        row.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "RootBorder");

    [AvaloniaFact]
    public void RowsUseTheLeanTemplate_WithoutTemplateGridLines()
    {
        var (_, grid) = ShowGrid();
        var row = Rows(grid).First();

        Assert.Null(row.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "GridLineBorder"));
        var root = row.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "RootBorder");
        Assert.Equal(default, root.BorderThickness);
        Assert.Single(row.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.TableViewCellsPresenter>());
    }

    [AvaloniaFact]
    public void SelectedRow_IsFilledWithTheAccentBrush()
    {
        var (window, grid) = ShowGrid();
        Assert.True(window.TryFindResource("SystemControlHighlightListAccentLowBrush", window.ActualThemeVariant, out var accent));

        grid.SelectedIndex = 2;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var rows = Rows(grid);
        for (var i = 0; i < rows.Count; i++)
        {
            if (i == 2)
            {
                Assert.Same(accent, RootBorder(rows[i]).Background);
            }
            else
            {
                Assert.NotSame(accent, RootBorder(rows[i]).Background);
            }
        }

        Assert.DoesNotContain(grid.GetVisualDescendants().OfType<Rectangle>(), r => r.Name == "PART_SelectionIndicator");
    }

    [AvaloniaFact]
    public void RowBackground_ReachesTheRowBorder()
    {
        // ApplyAlternatingRows tints rows through the row's own Background.
        var (window, grid) = ShowGrid();
        grid.SelectedIndex = 0;
        var row = Rows(grid)[3];
        row.Background = Brushes.OrangeRed;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.Same(Brushes.OrangeRed, RootBorder(row).Background);
    }
}
