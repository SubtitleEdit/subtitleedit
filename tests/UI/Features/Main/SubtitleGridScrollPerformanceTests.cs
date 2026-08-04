using Avalonia;
using Avalonia.Controls;
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
public class SubtitleGridScrollPerformanceTests
{
    private const int LineCount = 5000;

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

    private static (Window Window, MainViewModel Vm, TableView Grid, ScrollViewer ScrollViewer) ShowMainWindowWithLines()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        for (var i = 0; i < LineCount; i++)
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
