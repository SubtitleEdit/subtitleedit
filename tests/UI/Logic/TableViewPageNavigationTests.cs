using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// PageUp/PageDown in a TableView are handled by SE, not by the control: the virtualizing
/// panel ignores them, so they used to scroll and leave the selection behind (#13060).
/// <see cref="TableViewExtras.GetPageTarget"/> picks the row to move to from the rows that
/// are actually on screen - these tests pin the edge-first behavior and that the target row
/// is always visible, which is what makes it survive variable row heights.
/// </summary>
public class TableViewPageNavigationTests : IDisposable
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

    private TableView Show(IList<string> items, double windowHeight = 300)
    {
        var grid = TableViewExtras.MakeTableView(multiSelect: false);
        grid.Columns.Add(new SeTableViewColumn { Binding = new Binding(".") });
        grid.ItemsSource = items;

        var window = new Window { Content = grid, Width = 400, Height = windowHeight };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return grid;
    }

    private static List<string> Lines(int count, Func<int, int>? lineCount = null)
    {
        return Enumerable.Range(0, count)
            .Select(i => string.Join(Environment.NewLine, Enumerable.Repeat($"Line {i}", lineCount?.Invoke(i) ?? 1)))
            .ToList();
    }

    private static bool IsRealized(TableView grid, int index)
    {
        return grid.ContainerFromIndex(index) != null;
    }

    [AvaloniaFact]
    public void PageDown_MovesDownButStaysOnScreen()
    {
        var grid = Show(Lines(500));

        var target = TableViewExtras.GetPageTarget(grid, 0, down: true);

        Assert.True(target > 0, $"expected to move down, got {target}");
        Assert.True(target < 499, $"expected less than the whole list, got {target}");
        Assert.True(IsRealized(grid, target), "the target row should already be on screen");
    }

    [AvaloniaFact]
    public void PageDown_FromTheEdgeRowMovesOnAnotherPage()
    {
        var grid = Show(Lines(500));

        var first = TableViewExtras.GetPageTarget(grid, 0, down: true);
        var second = TableViewExtras.GetPageTarget(grid, first, down: true);

        Assert.True(second > first, $"expected {second} to be past {first}");
        Assert.True(IsRealized(grid, second), "the target row should already be on screen");
    }

    [AvaloniaFact]
    public void PageUp_UndoesAPageDownAndStopsAtTheTop()
    {
        var grid = Show(Lines(500));

        var down = TableViewExtras.GetPageTarget(grid, 0, down: true);
        var back = TableViewExtras.GetPageTarget(grid, down, down: false);

        Assert.Equal(0, back);
        Assert.Equal(0, TableViewExtras.GetPageTarget(grid, 0, down: false));
    }

    [AvaloniaFact]
    public void PageDown_WithVariableRowHeightsTakesSmallerSteps()
    {
        // Every row three text lines tall - a page then holds roughly a third of the rows,
        // which a fixed row-count step would get wrong.
        var tall = Show(Lines(500, _ => 3));
        var flat = Show(Lines(500));

        var tallStep = TableViewExtras.GetPageTarget(tall, 0, down: true);
        var flatStep = TableViewExtras.GetPageTarget(flat, 0, down: true);

        Assert.True(tallStep > 0, $"expected to move down, got {tallStep}");
        Assert.True(tallStep < flatStep, $"expected a smaller step than {flatStep}, got {tallStep}");
        Assert.True(IsRealized(tall, tallStep), "the target row should already be on screen");
    }

    [AvaloniaFact]
    public void PageDown_OnAListThatFitsGoesToTheLastRow()
    {
        var grid = Show(Lines(3), windowHeight: 600);

        Assert.Equal(2, TableViewExtras.GetPageTarget(grid, 0, down: true));
        Assert.Equal(2, TableViewExtras.GetPageTarget(grid, 2, down: true));
        Assert.Equal(0, TableViewExtras.GetPageTarget(grid, 2, down: false));
    }

    [AvaloniaFact]
    public void GetPageTarget_OnAnEmptyGridReturnsMinusOne()
    {
        var grid = Show(new List<string>());

        Assert.Equal(-1, TableViewExtras.GetPageTarget(grid, 0, down: true));
    }
}
