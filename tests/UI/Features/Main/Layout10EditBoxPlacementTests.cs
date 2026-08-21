using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Layout 10 is the Aegisub-style layout: video top-left, waveform top-right with the edit
/// box directly below it, and the subtitle grid alone across the bottom - as the layout
/// picker thumbnail shows. The edit box was docked under the grid instead (issue #13940).
/// </summary>
public class Layout10EditBoxPlacementTests
{
    [AvaloniaFact]
    public void Layout10_PutsTheEditBoxUnderTheWaveform_NotUnderTheGrid()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 10);
            Dispatcher.UIThread.RunJobs();

            var contentGrid = Assert.IsType<Grid>(vm.ContentGrid.Children[0]);
            var topContent = FindRowBorder(contentGrid, 0);
            var bottomContent = FindRowBorder(contentGrid, 2);
            var editGrid = FindEditGrid(window);

            // The bottom section holds the subtitle grid and nothing of the edit box.
            Assert.Contains(bottomContent, vm.SubtitleGrid!.GetLogicalAncestors());
            Assert.DoesNotContain(bottomContent, editGrid.GetLogicalAncestors());

            // The edit box lives in the top section, in the same right-hand column as the waveform.
            Assert.Contains(topContent, editGrid.GetLogicalAncestors());
            Assert.NotNull(vm.AudioVisualizer);
            var waveformColumn = vm.AudioVisualizer!.GetLogicalAncestors().OfType<Border>()
                .First(b => Grid.GetColumn(b) == 2 && b.GetLogicalParent() is Grid g && g.ColumnDefinitions.Count == 3);
            Assert.Contains(waveformColumn, editGrid.GetLogicalAncestors());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Layout1_StillDocksTheEditBoxWithTheGrid()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 1);
            Dispatcher.UIThread.RunJobs();

            var editGrid = FindEditGrid(window);

            // Docked layouts keep the edit box in the same two-row grid as the subtitle grid,
            // with the resize splitter between them.
            var sharedGrid = Assert.IsType<Grid>(vm.SubtitleGrid!.GetLogicalAncestors().OfType<Grid>()
                .First(g => g.RowDefinitions.Count == 2));
            Assert.Contains(sharedGrid, editGrid.GetLogicalAncestors());
            Assert.Contains(sharedGrid.Children, c => c is GridSplitter);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static Border FindRowBorder(Grid contentGrid, int row)
    {
        return contentGrid.Children.OfType<Border>().First(b => Grid.GetRow(b) == row);
    }

    private static Grid FindEditGrid(Window window)
    {
        // RightToLeftHelper locates the edit section by this name too, so it doubles as a
        // structure canary: exactly one edit grid must exist after a layout rebuild.
        return Assert.Single(window.GetLogicalDescendants().OfType<Grid>(), g => g.Name == "SubtitleTextEditGrid");
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.SuppressSaveChangesPromptOnClose(vm);
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
