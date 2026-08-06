using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Jumping around the subtitle grid must cost a viewport of rows, not the distance travelled.
/// TableView's virtualizing panel locates an unrealized row by estimating its offset from the
/// average row height, and with variable-height rows that estimate drifts until the panel gives
/// up and walks - realizing every row on the way. Measured on 5000 lines before
/// <see cref="TableViewExtras.PrePositionScroll"/>: Home realized 78, 139, 184, ... 513 rows on
/// successive Home/End round trips (~1 s per keypress, getting worse) while End stayed at 17,
/// and a jump to line 100 realized all 4935 remaining rows in every second attempt.
/// </summary>
public class SubtitleGridScrollPerformanceTests : IDisposable
{
    private const int LineCount = 5000;

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

    /// <summary>
    /// A viewport holds ~17 rows. Three passes of pre-positioning plus the ScrollIntoView that
    /// follows can realize a few viewports; anything beyond this is the panel walking again.
    /// </summary>
    private const int MaxRealizedPerJump = 120;

    private readonly ITestOutputHelper _output;

    public SubtitleGridScrollPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private (Window Window, MainViewModel Vm, TableView Grid, ScrollViewer ScrollViewer) ShowMainWindowWithLines(
        int lineCount = LineCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        for (var i = 0; i < lineCount; i++)
        {
            // Every third line is two lines tall - variable row heights are what makes the
            // panel's average-height estimate drift in the first place.
            var text = $"Line {i} of the test subtitle" + (i % 3 == 0 ? Environment.NewLine + "second line here" : string.Empty);
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, i * 2000, i * 2000 + 1500), null!) { Number = i + 1 });
        }

        for (var pump = 0; pump < 10; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var grid = vm.SubtitleGrid;
        return (window, vm, grid, grid.GetVisualDescendants().OfType<ScrollViewer>().First());
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 3; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static void Drag(GridSplitter splitter, double verticalChange)
    {
        splitter.RaiseEvent(new VectorEventArgs
        {
            RoutedEvent = Thumb.DragStartedEvent,
            Vector = default,
        });
        splitter.RaiseEvent(new VectorEventArgs
        {
            RoutedEvent = Thumb.DragDeltaEvent,
            Vector = new Vector(0, verticalChange),
        });
        splitter.RaiseEvent(new VectorEventArgs
        {
            RoutedEvent = Thumb.DragCompletedEvent,
            Vector = new Vector(0, verticalChange),
        });
    }

    [AvaloniaFact]
    public void EditBoxSplitter_DragResizesEditSectionAndPreservesMinimumHeight()
    {
        var (window, _, _, _) = ShowMainWindowWithLines(0);

        try
        {
            var splitter = Assert.Single(window.GetVisualDescendants().OfType<GridSplitter>(), s =>
                Grid.GetRow(s) == 1 &&
                s.VerticalAlignment == Avalonia.Layout.VerticalAlignment.Top &&
                s.Parent is Grid { RowDefinitions.Count: 2 });
            var mainGrid = Assert.IsType<Grid>(splitter.Parent);
            var editGrid = Assert.Single(mainGrid.Children.OfType<Grid>(), g => Grid.GetRow(g) == 1);
            var initialHeight = editGrid.Bounds.Height;

            Drag(splitter, -120);
            Settle(window);
            var grownHeight = editGrid.Bounds.Height;

            Assert.True(grownHeight > initialHeight,
                $"Dragging up should grow editGrid (initial={initialHeight:F1}, grown={grownHeight:F1})");

            Drag(splitter, window.Bounds.Height);
            Settle(window);
            var minimumHeight = editGrid.Bounds.Height;

            Assert.True(minimumHeight < grownHeight,
                $"Dragging down should shrink editGrid (grown={grownHeight:F1}, shrunk={minimumHeight:F1})");

            var textBoxMinimum = editGrid.GetVisualDescendants()
                .OfType<TextBox>()
                .Max(p => p.MinHeight);
            Assert.True(minimumHeight >= textBoxMinimum,
                $"Dragging must not make editGrid smaller than its text boxes " +
                $"(grid={minimumHeight:F1}, text box minimum={textBoxMinimum:F1})");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void EditBoxSplitter_AtMinimum_TextBoxDoesNotOverflowTheLabelRow()
    {
        // The edit section is "Auto,*,Auto": the "Text" header and the "Line length /
        // Total chars" panel sit above and below the box. The section floor has to cover all
        // three - sized to the box alone, the box (which cannot shrink past its own MinHeight)
        // overflows its row and draws over the labels underneath (#10271).
        var (window, _, _, _) = ShowMainWindowWithLines(0);

        try
        {
            var splitter = Assert.Single(window.GetVisualDescendants().OfType<GridSplitter>(), s =>
                Grid.GetRow(s) == 1 &&
                s.VerticalAlignment == Avalonia.Layout.VerticalAlignment.Top &&
                s.Parent is Grid { RowDefinitions.Count: 2 });
            var textEditGrid = window.GetVisualDescendants().OfType<Grid>()
                .First(g => g.Name == "SubtitleTextEditGrid");

            Drag(splitter, window.Bounds.Height);
            Settle(window);

            var textBox = textEditGrid.GetVisualDescendants().OfType<TextBox>().First();
            var textBoxRowBottom = textEditGrid.RowDefinitions[0].ActualHeight +
                                   textEditGrid.RowDefinitions[1].ActualHeight;

            Assert.True(textBox.Bounds.Bottom <= textBoxRowBottom + 0.5,
                $"Text box overflows its row and covers the length labels " +
                $"(box bottom={textBox.Bounds.Bottom:F1}, row bottom={textBoxRowBottom:F1})");
            Assert.True(textEditGrid.RowDefinitions[2].ActualHeight > 0,
                "The length-label row collapsed to zero at the minimum section height");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HomeAndEnd_RealizeOnlyAViewportOfRows()
    {
        var (window, _, grid, scrollViewer) = ShowMainWindowWithLines();
        grid.SelectedIndex = 0;
        Dispatcher.UIThread.RunJobs();
        TableViewExtras.FocusRow(grid);
        Dispatcher.UIThread.RunJobs();

        var prepared = 0;
        grid.ContainerPrepared += (_, _) => prepared++;

        int Press(PhysicalKey key)
        {
            // Re-anchor focus on the grid before every press. KeyPressQwerty routes through the
            // application-wide focused element, and that leaks between headless tests: closing a
            // window does not clear it, so a detached row container from an earlier test can
            // still be "focused". The panel recycling this grid's focused row during a jump then
            // hands focus back to that stale row, and every later press lands there instead -
            // the test failed with realized=0 and an unchanged SelectedIndex, at a different
            // round each run. What this test measures is how many rows a jump realizes, not how
            // Avalonia routes keys, so pin the focus rather than depend on it.
            TableViewExtras.FocusRow(grid);
            Dispatcher.UIThread.RunJobs();

            prepared = 0;
            window.KeyPressQwerty(key, RawInputModifiers.None);
            Settle(window);
            return prepared;
        }

        for (var round = 0; round < 8; round++)
        {
            var end = Press(PhysicalKey.End);
            _output.WriteLine($"round {round}: End realized={end} idx={grid.SelectedIndex} " +
                              $"offset={scrollViewer.Offset.Y:F0} extent={scrollViewer.Extent.Height:F0}");
            Assert.Equal(LineCount - 1, grid.SelectedIndex);
            Assert.True(end <= MaxRealizedPerJump, $"End realized {end} rows in round {round}");

            var home = Press(PhysicalKey.Home);
            _output.WriteLine($"round {round}: Home realized={home} idx={grid.SelectedIndex} " +
                              $"offset={scrollViewer.Offset.Y:F0} extent={scrollViewer.Extent.Height:F0}");
            Assert.Equal(0, grid.SelectedIndex);
            Assert.True(home <= MaxRealizedPerJump, $"Home realized {home} rows in round {round}");
        }

        window.Close();
    }

    /// <summary>
    /// The same walk hit any long jump, not just Home: Find, Go to line number and bookmarks all
    /// land here through SelectAndScrollTo. Lines near the top were the worst, but 400 and 700
    /// hit it too, so the whole range is covered.
    /// </summary>
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(100)]
    [InlineData(400)]
    [InlineData(700)]
    [InlineData(2500)]
    public void JumpToRow_FromTheBottom_RealizesOnlyAFewViewports(int target)
    {
        var (window, vm, grid, scrollViewer) = ShowMainWindowWithLines();
        var prepared = 0;
        grid.ContainerPrepared += (_, _) => prepared++;

        var elapsed = TimeSpan.Zero;

        int JumpTo(int index)
        {
            prepared = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            vm.SelectAndScrollToSubtitle(vm.Subtitles[index]);
            Settle(window);
            elapsed = sw.Elapsed;
            return prepared;
        }

        for (var round = 0; round < 5; round++)
        {
            JumpTo(LineCount - 1);
            var realized = JumpTo(target);
            _output.WriteLine($"round {round}: jump to {target} realized={realized} in {elapsed.TotalMilliseconds:F0}ms " +
                              $"offset={scrollViewer.Offset.Y:F0} extent={scrollViewer.Extent.Height:F0}");
            Assert.True(realized <= MaxRealizedPerJump, $"Jump to {target} realized {realized} rows in round {round}");

            // ... and the row is actually on screen when it is over.
            var row = grid.ContainerFromItem(vm.Subtitles[target]);
            Assert.NotNull(row);
            var top = ((Visual)row!).TranslatePoint(new Point(0, 0), scrollViewer)!.Value.Y;
            Assert.InRange(top, -1, scrollViewer.Viewport.Height);
        }

        window.Close();
    }
}
