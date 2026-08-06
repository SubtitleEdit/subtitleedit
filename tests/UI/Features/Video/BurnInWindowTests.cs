using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Construction + layout tests for the burn-in window. The layout is built entirely in code.
/// The left settings column (subtitle + video + target size, packed in one panel spanning
/// rows 0-3) is taller than the middle column's cut/preview/audio/video-info rows, so the
/// preview row (row 1) carries a MinHeight: it keeps the packed panel inside rows 0-3 (so it
/// can never overflow into the progress-bar row, which used to draw the bar through the
/// "File size in MB" field) and keeps the preview box taller than its label + player (so the
/// player can never spill over the audio settings box when the window is shrunk).
/// </summary>
public class BurnInWindowTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            // The burn-in window posts its re-fit callback from Opened; flush it while the
            // window is still alive so it does not run against the disposed platform
            // implementation during session teardown.
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();
    }

    // WindowService only touches the provider when it creates a child window, which this
    // construction test never does.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private BurnInWindow BuildWindow()
    {
        var vm = new BurnInViewModel(
            new FolderHelper(),
            new FileHelper(),
            new WindowService(new NullServiceProvider()));
        var window = new BurnInWindow(vm);
        _windows.Add(window);
        return window;
    }

    /// <summary>Returns the window's root grid (the one holding the progress view).</summary>
    private static Grid FindRootGrid(BurnInWindow window)
    {
        var progressBar = window.GetLogicalDescendants().OfType<ProgressBar>().FirstOrDefault();
        Assert.NotNull(progressBar);
        var progressView = progressBar.Parent as Grid;
        Assert.NotNull(progressView);
        var rootGrid = progressView.Parent as Grid;
        Assert.NotNull(rootGrid);
        return rootGrid;
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void PreviewRow_HasMinimumHeight_KeepingPanelAboveProgressBar()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        vm.IsGenerating = true; // progress view is only visible while generating
        window.Show();

        var rootGrid = FindRootGrid(window);

        // The preview row (row 1, star) must carry a MinHeight: without it the window can be
        // shrunk (or restored) below the content height, the preview box gets shorter than its
        // label + player and the player spills over the audio settings box below it.
        var previewRow = rootGrid.RowDefinitions[1];
        Assert.Equal(GridUnitType.Star, previewRow.Height.GridUnitType);
        Assert.True(previewRow.MinHeight >= 350, $"Preview row MinHeight is only {previewRow.MinHeight:0.#}.");

        // The left column is one panel spanning rows 0-3. With the row minimum in place it
        // must fit entirely above the progress view - the regression this guards against drew
        // the progress bar through the "File size in MB" field while generating.
        var leftPanel = rootGrid.Children.OfType<StackPanel>().FirstOrDefault();
        Assert.NotNull(leftPanel);
        var progressView = rootGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 4);
        Assert.NotNull(progressView);

        var panelBottom = leftPanel.TranslatePoint(new Point(0, leftPanel.Bounds.Height), window)?.Y;
        var progressTop = progressView.TranslatePoint(new Point(0, 0), window)?.Y;
        Assert.NotNull(panelBottom);
        Assert.NotNull(progressTop);
        Assert.True(
            panelBottom.Value <= progressTop.Value + 1.5,
            $"Left panel bottom ({panelBottom.Value:0.#}) is below the progress view top ({progressTop.Value:0.#}).");
    }

    [AvaloniaFact]
    public void VideoPlayer_StaysAboveAudioSettingsBox_WhenWindowIsAtMinimumHeight()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        vm.IsGenerating = true; // progress view is only visible while generating
        window.Show();

        // Try to shrink the window far below the content minimum; the layout must clamp it so
        // the preview box still fits its label + player.
        window.Height = 400;
        window.UpdateLayout();

        var rootGrid = FindRootGrid(window);
        var audioSettingsView = rootGrid.Children.OfType<Border>().FirstOrDefault(b => Grid.GetRow(b) == 2 && Grid.GetColumn(b) == 1);
        Assert.NotNull(audioSettingsView);

        // The player is the bottom-most element of the preview box; its bottom must never
        // reach the audio settings box.
        var player = vm.VideoPlayerControl;
        Assert.NotNull(player);
        var playerBottom = player.TranslatePoint(new Point(0, player.Bounds.Height), window)?.Y;
        var audioTop = audioSettingsView.TranslatePoint(new Point(0, 0), window)?.Y;
        Assert.NotNull(playerBottom);
        Assert.NotNull(audioTop);
        Assert.True(
            playerBottom.Value <= audioTop.Value + 1.5,
            $"Video player bottom ({playerBottom.Value:0.#}) overlaps the audio settings box top ({audioTop.Value:0.#}).");
    }

    [AvaloniaFact]
    public void Window_EnlargesToFitProgressBar_WhenGeneratingStarts()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        window.Show();
        window.UpdateLayout();
        var before = window.ClientSize.Height;

        // Starting a generation shows the progress row; the window minimum must grow so the
        // button row is never clipped while the bar + status text are visible.
        vm.IsGenerating = true;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var rootGrid = FindRootGrid(window);
        Assert.True(
            rootGrid.Children.OfType<Grid>().Any(g => Grid.GetRow(g) == 4 && g.IsVisible),
            "Progress view is not visible while generating.");

        var buttonPanel = rootGrid.Children.OfType<StackPanel>().FirstOrDefault(s => Grid.GetRow(s) == 5);
        Assert.NotNull(buttonPanel);
        var buttonsBottom = buttonPanel.TranslatePoint(new Point(0, buttonPanel.Bounds.Height), window)?.Y;
        Assert.NotNull(buttonsBottom);
        Assert.True(
            buttonsBottom.Value <= window.ClientSize.Height + 1.5,
            $"Buttons bottom ({buttonsBottom.Value:0.#}) is clipped by the window height ({window.ClientSize.Height:0.#}).");
        Assert.True(
            window.ClientSize.Height >= before,
            $"Window shrank when generating started ({before:0.#} -> {window.ClientSize.Height:0.#}).");
    }

    // Re-locking the minimum for the progress row is height-only, but it used to clear MinWidth
    // along the way and never put it back - so starting (or finishing) a burn-in left the window
    // freely shrinkable sideways, clipping the very content the minimum protects.
    [AvaloniaFact]
    public void Window_KeepsMinimumWidth_WhenGeneratingStartsAndStops()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        var lockedMinWidth = window.MinWidth;
        Assert.True(lockedMinWidth > 0, "The window never locked a minimum width to begin with.");

        vm.IsGenerating = true;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Equal(lockedMinWidth, window.MinWidth);

        vm.IsGenerating = false;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Equal(lockedMinWidth, window.MinWidth);
    }
}
