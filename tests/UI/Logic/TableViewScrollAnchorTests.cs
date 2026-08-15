using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UITests.Logic;

/// <summary>
/// The scroll anchor for variable-height TableView rows (issue #13619): breaking a subtitle
/// line in two grows that row, the virtualizing panel re-estimates the pixel extent from the
/// average realized row height, and the unchanged pixel offset then maps to a much earlier
/// row - so the grid scrolled tens of rows away from the line being edited (measured 546 ->
/// 512 with a single row gaining a second line). TableViewScrollAnchor puts the row that was
/// at the top of the viewport back where it was, without pinning ordinary scrolling.
/// </summary>
public class TableViewScrollAnchorTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class Row : INotifyPropertyChanged
    {
        private string _text = string.Empty;

        public int Number { get; set; }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private const int RowCount = 1000;

    // Far enough down the list that the estimate-driven jump is tens of rows, like the file
    // in the issue (the reporter was on line 552).
    private const int StartIndex = 551;

    private (Window window, TableView grid, ScrollViewer scrollViewer, ObservableCollection<Row> items) BuildShownGrid(bool attachAnchor = true)
    {
        // Every third row has two text lines, so realized row heights vary the way the
        // subtitle grid's do - the mix that makes the estimated extent move.
        var items = new ObservableCollection<Row>(Enumerable.Range(1, RowCount)
            .Select(i => new Row { Number = i, Text = i % 3 == 0 ? $"Line {i}\nsecond line" : $"Line {i}" }));

        var grid = new TableView
        {
            ItemsSource = items,
            Columns =
            {
                new TableViewColumn { Header = "#", Binding = new Avalonia.Data.Binding(nameof(Row.Number)) },
                new TableViewColumn { Header = "Text", Binding = new Avalonia.Data.Binding(nameof(Row.Text)) },
            },
        };

        if (attachAnchor)
        {
            TableViewScrollAnchor.Attach(grid);
        }

        var window = new Window { Content = grid, Width = 400, Height = 300 };
        _windows.Add(window);
        window.Show();
        Settle(window);

        var scrollViewer = grid.GetVisualDescendants().OfType<ScrollViewer>().First();

        grid.ScrollIntoView(StartIndex);
        Settle(window);

        return (window, grid, scrollViewer, items);
    }

    private static void Settle(Window window)
    {
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    // The TableView template pins the column header inside the ScrollViewer, so the true
    // viewport origin is the content presenter, a header height below the ScrollViewer's own.
    private static Visual ViewportOrigin(ScrollViewer scrollViewer) => (Visual?)scrollViewer.Presenter ?? scrollViewer;

    private static int FirstVisibleIndex(TableView grid, ScrollViewer scrollViewer)
    {
        var best = -1;
        var bestTop = double.MaxValue;
        foreach (var row in grid.GetRealizedContainers().OfType<TableViewRow>())
        {
            var top = ((Visual)row).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))?.Y;
            if (top == null || row.Bounds.Height <= 0 || top.Value + row.Bounds.Height <= 0.5 || top.Value >= bestTop)
            {
                continue;
            }

            bestTop = top.Value;
            best = grid.IndexFromContainer(row);
        }

        return best;
    }

    [AvaloniaFact]
    public void EditedRowGainingASecondLine_KeepsTheViewOnTheSameRow()
    {
        var (window, grid, scrollViewer, items) = BuildShownGrid();

        var before = FirstVisibleIndex(grid, scrollViewer);
        Assert.True(before > 100, "sanity: the test scrolls far enough down for the estimate to matter");

        // The row being edited is broken into two lines - auto break, split at cursor with a
        // long half, or just typing a line break in the edit box.
        items[before + 1].Text = "Byl jsem v jeho mysli, Rhiannon.\nZnam ho.";
        Settle(window);

        Assert.Equal(before, FirstVisibleIndex(grid, scrollViewer));
    }

    [AvaloniaFact]
    public void RowGainingASecondLine_KeepsTheEditedRowVisible()
    {
        var (window, grid, scrollViewer, items) = BuildShownGrid();

        var edited = FirstVisibleIndex(grid, scrollViewer) + 1;
        items[edited].Text = "Byl jsem v jeho mysli, Rhiannon.\nZnam ho.";
        Settle(window);

        // The point of the issue: the line the user is editing must still be on screen.
        var container = grid.ContainerFromIndex(edited);
        Assert.NotNull(container);

        var top = ((Visual)container!).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))?.Y;
        Assert.NotNull(top);
        Assert.InRange(top!.Value, -0.5, scrollViewer.Viewport.Height);
    }

    [AvaloniaFact]
    public void RowLosingItsSecondLine_KeepsTheViewOnTheSameRow()
    {
        // The mirror case - unbreak, or merging two lines into one - shrinks the estimate.
        var (window, grid, scrollViewer, items) = BuildShownGrid();

        var before = FirstVisibleIndex(grid, scrollViewer);
        var twoLine = Enumerable.Range(before + 1, 5).First(i => items[i].Text.Contains('\n'));

        items[twoLine].Text = "Byl jsem v jeho mysli, Rhiannon. Znam ho.";
        Settle(window);

        Assert.Equal(before, FirstVisibleIndex(grid, scrollViewer));
    }

    [AvaloniaFact]
    public void SplitInsertingARowBelow_KeepsTheViewOnTheSameRow()
    {
        var (window, grid, scrollViewer, items) = BuildShownGrid();

        var before = FirstVisibleIndex(grid, scrollViewer);
        var split = before + 1;

        // Both halves end up two lines long, so the average - and the estimate - grows.
        items[split].Text = "Byl jsem v jeho\nmysli, Rhiannon.";
        items.Insert(split + 1, new Row { Number = items[split].Number + 1, Text = "Znam\nho." });
        Settle(window);

        Assert.Equal(before, FirstVisibleIndex(grid, scrollViewer));
    }

    [AvaloniaFact]
    public void InsertAboveTheAnchor_KeepsTheSameRowInView()
    {
        var (window, grid, scrollViewer, items) = BuildShownGrid();

        var before = FirstVisibleIndex(grid, scrollViewer);
        var anchorItem = items[before];

        for (var i = 0; i < 5; i++)
        {
            items.Insert(before, new Row { Number = 0, Text = $"inserted {i}\nabove" });
        }

        Settle(window);

        // The anchor follows the item, not the index, so the same line stays at the top.
        Assert.Equal(items.IndexOf(anchorItem), FirstVisibleIndex(grid, scrollViewer));
    }

    [AvaloniaFact]
    public void OrdinarysScrolling_StillMovesTheView()
    {
        var (window, grid, scrollViewer, _) = BuildShownGrid();

        var before = FirstVisibleIndex(grid, scrollViewer);

        // A few wheel notches' worth of pixels, the same path the wheel drives.
        for (var i = 0; i < 5; i++)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y + 120);
            Settle(window);
        }

        Assert.True(FirstVisibleIndex(grid, scrollViewer) > before,
            "the anchor must follow ordinary scrolling, not pin the view");
    }

    [AvaloniaFact]
    public void ScrollIntoView_StillReachesItsTarget()
    {
        var (window, grid, scrollViewer, _) = BuildShownGrid();

        foreach (var target in new[] { 900, 100, 0, RowCount - 1 })
        {
            TableViewExtras.PrePositionScroll(grid, target);
            grid.ScrollIntoView(target);
            Settle(window);

            var container = grid.ContainerFromIndex(target);
            Assert.NotNull(container);

            var top = ((Visual)container!).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))?.Y;
            Assert.NotNull(top);
            Assert.InRange(top!.Value, -container!.Bounds.Height, scrollViewer.Viewport.Height);
        }
    }

    [AvaloniaFact]
    public void IndexScrollBar_StillPositionsRows_WithAnAnchorAttached()
    {
        // The bar's own multi-pass placement re-estimates the extent as it realizes rows;
        // it suspends the anchor so the restore does not drag the view back.
        var items = new ObservableCollection<Row>(Enumerable.Range(1, RowCount)
            .Select(i => new Row { Number = i, Text = i % 3 == 0 ? $"Line {i}\nsecond line" : $"Line {i}" }));

        var grid = new TableView
        {
            ItemsSource = items,
            Columns =
            {
                new TableViewColumn { Header = "#", Binding = new Avalonia.Data.Binding(nameof(Row.Number)) },
                new TableViewColumn { Header = "Text", Binding = new Avalonia.Data.Binding(nameof(Row.Text)) },
            },
        };

        TableViewScrollAnchor.Attach(grid);
        var wrapper = new TableViewIndexScrollBar(grid);
        var window = new Window { Content = wrapper, Width = 400, Height = 300 };
        _windows.Add(window);
        window.Show();
        Settle(window);

        var scrollViewer = grid.GetVisualDescendants().OfType<ScrollViewer>().First();

        wrapper.BarForTest.Value = 400;
        wrapper.ApplyPendingForTest();
        Settle(window);

        Assert.InRange(FirstVisibleIndex(grid, scrollViewer), 399, 401);
    }
}
