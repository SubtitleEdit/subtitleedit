using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
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
/// player can never spill over the audio settings box when the window is shrunk). A window
/// shorter than its own content minimum - which UiUtil produces on screens too short for the
/// dialog - no longer clips anything: the settings grid (rows 0-3) sits in a ScrollViewer while the
/// progress bar and the button row stay outside it, so on a short screen the settings scroll and
/// "Generate" remains reachable (issues #13904, #14360).
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

    /// <summary>Returns the window's root grid (the one holding the progress view and the buttons).</summary>
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

    /// <summary>Returns the scroll viewer in row 0 of the root grid and the settings grid inside it.</summary>
    private static (ScrollViewer Scroller, Grid SettingsGrid) FindSettingsGrid(Grid rootGrid)
    {
        var scroller = rootGrid.Children.OfType<ScrollViewer>().FirstOrDefault(s => Grid.GetRow(s) == 0);
        Assert.NotNull(scroller);
        var settingsGrid = scroller.Content as Grid;
        Assert.NotNull(settingsGrid);
        return (scroller, settingsGrid);
    }

    /// <summary>Returns the middle column grid (cut / preview / audio / video info / filler rows).</summary>
    private static Grid FindMiddleColumn(Grid settingsGrid)
    {
        var middleColumn = settingsGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetColumn(g) == 1);
        Assert.NotNull(middleColumn);
        return middleColumn;
    }

    private static Grid FindProgressView(Grid rootGrid)
    {
        var progressView = rootGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 1);
        Assert.NotNull(progressView);
        return progressView;
    }

    private static StackPanel FindButtonPanel(Grid rootGrid)
    {
        var buttonPanel = rootGrid.Children.OfType<StackPanel>().FirstOrDefault(s => Grid.GetRow(s) == 2);
        Assert.NotNull(buttonPanel);
        return buttonPanel;
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void PreviewRow_HugsFixedHeightPlayer_KeepingPanelAboveProgressBar()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        vm.IsGenerating = true; // progress view is only visible while generating
        window.Show();

        var rootGrid = FindRootGrid(window);

        // The preview row (row 1) is an Auto row sized by a fixed-height player, and the star
        // filler row at the bottom takes any extra height: a stretching player made the middle
        // column taller than the settings column (forcing the window to scroll) and a centred
        // one left empty bands above and below the video.
        var middleColumn = FindMiddleColumn(FindSettingsGrid(rootGrid).SettingsGrid);
        Assert.Equal(GridUnitType.Auto, middleColumn.RowDefinitions[1].Height.GridUnitType);
        Assert.Equal(GridUnitType.Star, middleColumn.RowDefinitions[^1].Height.GridUnitType);
        Assert.NotNull(vm.VideoPlayerControl);
        Assert.Equal(360, vm.VideoPlayerControl.Height);
        Assert.Equal(VerticalAlignment.Top, vm.VideoPlayerControl.VerticalAlignment);

        // The left column is one panel in the settings grid's single row. It must fit entirely
        // above the progress view - the regression this guards against drew
        // the progress bar through the "File size in MB" field while generating.
        AssertSettingsColumnStaysAboveProgressView(window, rootGrid);
    }

    [AvaloniaFact]
    public void SettingsColumn_StaysAboveProgressBar_WhenWindowIsShorterThanItsContent()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        vm.IsGenerating = true; // progress view is only visible while generating
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        // What a screen too short for the dialog produces: UiUtil lowers the window minimum to
        // the working area, so the grid gets less height than its rows asked for and rows 0-3
        // end up shorter than the settings column. The column must contain its own overflow -
        // an unclipped panel drew the last box ("File size in MB") under the progress bar.
        window.SizeToContent = SizeToContent.Manual;
        window.MinHeight = 650;
        window.Height = 650;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        AssertSettingsColumnStaysAboveProgressView(window, FindRootGrid(window));
    }

    private static void AssertSettingsColumnStaysAboveProgressView(BurnInWindow window, Grid rootGrid)
    {
        var (scroller, settingsGrid) = FindSettingsGrid(rootGrid);
        var settingsColumn = settingsGrid.Children.FirstOrDefault(c => Grid.GetColumn(c) == 0);
        Assert.NotNull(settingsColumn);
        var progressView = FindProgressView(rootGrid);

        // The column is clipped by the scroll viewer around the settings grid, so measure from
        // there: anything the column paints past the viewport is scrolled, not drawn over the bar.
        var panelBottom = PaintedBottom(scroller, window);
        var progressTop = progressView.TranslatePoint(new Point(0, 0), window)?.Y;
        Assert.NotNull(progressTop);
        Assert.True(
            panelBottom <= progressTop.Value + 1.5,
            $"Settings column paints down to {panelBottom:0.#}, past the progress view top ({progressTop.Value:0.#}).");
    }

    /// <summary>
    /// Lowest point in the window that anything inside <paramref name="visual"/> actually paints
    /// at. The walk stops at a control that clips, since nothing below it can paint outside its
    /// bounds - which is what keeps the settings column out of the progress row.
    /// </summary>
    private static double PaintedBottom(Visual visual, Visual relativeTo)
    {
        var bottom = visual.TranslatePoint(new Point(0, visual.Bounds.Height), relativeTo)?.Y ?? 0;
        if (visual.ClipToBounds)
        {
            return bottom;
        }

        foreach (var child in visual.GetVisualChildren())
        {
            bottom = Math.Max(bottom, PaintedBottom(child, relativeTo));
        }

        return bottom;
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

        var middleColumn = FindMiddleColumn(FindSettingsGrid(FindRootGrid(window)).SettingsGrid);
        var audioSettingsView = middleColumn.Children.OfType<Border>().FirstOrDefault(b => Grid.GetRow(b) == 2);
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
        Assert.True(FindProgressView(rootGrid).IsVisible, "Progress view is not visible while generating.");

        var buttonPanel = FindButtonPanel(rootGrid);
        var buttonsBottom = buttonPanel.TranslatePoint(new Point(0, buttonPanel.Bounds.Height), window)?.Y;
        Assert.NotNull(buttonsBottom);
        Assert.True(
            buttonsBottom.Value <= window.ClientSize.Height + 1.5,
            $"Buttons bottom ({buttonsBottom.Value:0.#}) is clipped by the window height ({window.ClientSize.Height:0.#}).");
        Assert.True(
            window.ClientSize.Height >= before,
            $"Window shrank when generating started ({before:0.#} -> {window.ClientSize.Height:0.#}).");
    }

    [AvaloniaFact]
    public void ButtonRow_StaysReachable_OnAShortScreen()
    {
        var window = BuildWindow();
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        // A maximized window on a 1366x768 laptop (taskbar + title bar) leaves about 700 DIPs of
        // client height, less than the settings area needs. The reporter of issue #14360 saw the
        // "Generate" row clipped off the bottom of the screen with no way to reach it.
        window.SizeToContent = SizeToContent.Manual;
        window.MinHeight = 0;
        window.Height = 697;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var rootGrid = FindRootGrid(window);
        var buttonPanel = FindButtonPanel(rootGrid);
        var buttonsBottom = buttonPanel.TranslatePoint(new Point(0, buttonPanel.Bounds.Height), window)?.Y;
        Assert.NotNull(buttonsBottom);
        Assert.True(
            buttonsBottom.Value <= window.ClientSize.Height + 1.5,
            $"Buttons bottom ({buttonsBottom.Value:0.#}) is clipped by the window height ({window.ClientSize.Height:0.#}).");

        // The settings area itself is what gives way: it scrolls instead of being clipped.
        var (scroller, settingsGrid) = FindSettingsGrid(rootGrid);
        Assert.True(
            scroller.Extent.Height > scroller.Viewport.Height + 1.5,
            $"Settings area does not scroll (extent {scroller.Extent.Height:0.#}, viewport {scroller.Viewport.Height:0.#}).");
        Assert.Equal(GridUnitType.Star, FindMiddleColumn(settingsGrid).RowDefinitions[^1].Height.GridUnitType);
    }

    [AvaloniaFact]
    public void SettingsGrid_FillsTheWindow_WhenThereIsRoom()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        // Inside a ScrollViewer a star row would otherwise collapse to its minimum; the settings
        // grid must keep filling the viewport (extra height goes to the filler row, not the
        // player) so nothing scrolls while the window is tall enough.
        var rootGrid = FindRootGrid(window);
        var (scroller, settingsGrid) = FindSettingsGrid(rootGrid);
        var before = settingsGrid.Bounds.Height;
        var playerHeightBefore = vm.VideoPlayerControl!.Bounds.Height;

        window.SizeToContent = SizeToContent.Manual;
        window.Height = window.ClientSize.Height + 200;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.True(
            settingsGrid.Bounds.Height >= before + 190,
            $"Settings grid did not grow with the window ({before:0.#} -> {settingsGrid.Bounds.Height:0.#}); client {window.ClientSize.Height:0.#}, viewport {scroller.Viewport.Height:0.#}, extent {scroller.Extent.Height:0.#}).");
        Assert.True(
            scroller.Extent.Height <= scroller.Viewport.Height + 1.5,
            $"Settings area scrolls although the window is tall enough (extent {scroller.Extent.Height:0.#}, viewport {scroller.Viewport.Height:0.#}).");
        Assert.Equal(playerHeightBefore, vm.VideoPlayerControl.Bounds.Height, 0.5);
    }

    // The extension list is only correct if both combo boxes are wired up: the encoder box
    // rebuilds the container list, and the container box rebuilds the audio encoder list.
    [AvaloniaFact]
    public void ContainerAndAudioLists_FollowTheSelectedEncoder()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        window.Show();

        var encoderComboBox = window.GetLogicalDescendants().OfType<ComboBox>()
            .First(c => ReferenceEquals(c.ItemsSource, vm.VideoEncodings));
        var extensionComboBox = window.GetLogicalDescendants().OfType<ComboBox>()
            .First(c => ReferenceEquals(c.ItemsSource, vm.VideoExtensions));

        encoderComboBox.SelectedItem = vm.VideoEncodings.First(p => p.Codec == "libvpx-vp9");
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Contains(".webm", vm.VideoExtensions);
        Assert.DoesNotContain(".ts", vm.VideoExtensions);

        extensionComboBox.SelectedItem = ".webm";
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // WebM holds Opus/Vorbis only, so "copy" (the default) cannot survive the switch.
        Assert.Equal(".webm", vm.SelectedVideoExtension);
        Assert.DoesNotContain("copy", vm.AudioEncodings);
        Assert.DoesNotContain("aac", vm.AudioEncodings);
        Assert.Contains(vm.SelectedAudioEncoding, vm.AudioEncodings);

        encoderComboBox.SelectedItem = vm.VideoEncodings.First(p => p.Codec == "libx264");
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // H.264 cannot go into WebM, so the container - and with it the audio list - moves back.
        Assert.Contains(".ts", vm.VideoExtensions);
        Assert.DoesNotContain(".webm", vm.VideoExtensions);
        Assert.Contains(vm.SelectedVideoExtension, vm.VideoExtensions);
        Assert.Contains(vm.SelectedAudioEncoding, vm.AudioEncodings);
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

    /// <summary>
    /// An image subtitle (a Blu-ray sup from the image-based editor) is burned in as it is, so the
    /// font/color/box/effect settings mean nothing and go away - the logo still applies.
    /// </summary>
    [AvaloniaFact]
    public void ImageSubtitle_HidesTheTextSettingsAndKeepsTheLogo()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<BurnInViewModel>(window.DataContext);
        var textSettings = window.GetLogicalDescendants().OfType<Grid>().Single(p => p.Name == BurnInWindow.TextSettingsName);
        var logoButton = window.GetLogicalDescendants().OfType<Button>().Single(p => ReferenceEquals(p.Command, vm.ShowLogoCommand));
        Assert.True(textSettings.IsVisible);

        vm.InitializeImageSubtitle(string.Empty, "subs.sup");
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.False(textSettings.IsVisible);
        Assert.True(logoButton.IsVisible);
        Assert.True(vm.IsImageSubtitle);
    }
}
