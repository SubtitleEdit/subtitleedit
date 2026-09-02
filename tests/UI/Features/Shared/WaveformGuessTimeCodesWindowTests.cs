using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// The guess-time-codes window builds its layout in code and sizes itself to its content. Its
/// three option boxes used to be stacked in one column, which made it tall enough that a high UI
/// scale (or a small working area) pushed it past the screen - <see cref="UiUtil"/> then clamps it
/// to the working area, which cut off the bottom options and the OK/Cancel buttons on a window
/// that cannot be resized. Two columns keep it on screen, the scroll viewer keeps the buttons
/// reachable when even that is not enough.
/// </summary>
public class WaveformGuessTimeCodesWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        UiTheme.SetLayoutScale(1.0);
    }

    private WaveformGuessTimeCodesWindow BuildWindow()
    {
        var window = new WaveformGuessTimeCodesWindow(new WaveformGuessTimeCodesViewModel());
        _windows.Add(window);
        return window;
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaTheory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void OkAndCancel_StayInsideTheWindow_WhenItIsClampedToASmallScreen(double layoutScale)
    {
        UiTheme.SetLayoutScale(layoutScale);

        var window = BuildWindow();
        var vm = (WaveformGuessTimeCodesViewModel)window.DataContext!;
        UiTheme.ApplyScaleToWindow(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // What UiUtil.ClampToWorkingArea does when the content is taller than the screen.
        window.SizeToContent = SizeToContent.Manual;
        window.Height = window.Bounds.Height / 2;
        Dispatcher.UIThread.RunJobs();

        var buttonOk = Assert.Single(
            window.GetLogicalDescendants().OfType<Button>(),
            b => ReferenceEquals(b.Command, vm.OkCommand));
        var bottom = buttonOk.TranslatePoint(new Point(0, buttonOk.Bounds.Height), window);

        Assert.NotNull(bottom);
        Assert.True(bottom!.Value.Y <= window.Bounds.Height,
            $"OK button bottom {bottom.Value.Y} is below the window height {window.Bounds.Height}");

        var scrollViewer = OptionsScrollViewer(window);
        Assert.True(scrollViewer.Extent.Height > scrollViewer.Viewport.Height, "the clamped options are not scrollable");
    }

    private static ScrollViewer OptionsScrollViewer(Window window)
    {
        // By name: the numeric up/downs bring scroll viewers of their own in their templates.
        return Assert.Single(
            window.GetVisualDescendants().OfType<ScrollViewer>(),
            s => s.Name == WaveformGuessTimeCodesWindow.OptionsScrollViewerName);
    }

    /// <summary>
    /// Scrolling is the fallback for a clamped window - left to itself the window must still size
    /// to the whole content, at any UI scale (the scaled size is what the clamp measures).
    /// </summary>
    [AvaloniaTheory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    public void Window_SizesToItsWholeContent_WhenTheScreenIsBigEnough(double layoutScale)
    {
        UiTheme.SetLayoutScale(layoutScale);

        var window = BuildWindow();
        UiTheme.ApplyScaleToWindow(window);
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = OptionsScrollViewer(window);
        Assert.True(scrollViewer.Viewport.Height >= scrollViewer.Extent.Height,
            $"viewport {scrollViewer.Viewport.Height} does not fit the content {scrollViewer.Extent.Height}");

        var buttonBottom = window.GetLogicalDescendants().OfType<Button>()
            .Where(b => b.Command != null)
            .Select(b => b.TranslatePoint(new Point(0, b.Bounds.Height), window)?.Y ?? 0)
            .Max();
        Assert.True(buttonBottom <= window.Bounds.Height, $"buttons at {buttonBottom} exceed window height {window.Bounds.Height}");
    }

    /// <summary>
    /// The point of the two-column layout: the window has to fit a small working area even at a
    /// high UI scale. 1280x720 is what a 1920x1080 screen leaves at 150% OS scaling.
    /// </summary>
    [AvaloniaFact]
    public void Window_FitsASmallWorkingArea_AtAHighUiScale()
    {
        UiTheme.SetLayoutScale(1.5);

        var window = BuildWindow();
        UiTheme.ApplyScaleToWindow(window);
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.True(window.Bounds.Width <= 1280, $"window width {window.Bounds.Width} does not fit 1280");
        Assert.True(window.Bounds.Height <= 720, $"window height {window.Bounds.Height} does not fit 720");
    }

    /// <summary>
    /// The settings box sits beside the two "which lines" boxes, not below them.
    /// </summary>
    [AvaloniaFact]
    public void OptionBoxes_AreLaidOutInTwoColumns()
    {
        var window = BuildWindow();
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var settings = Assert.Single(
            window.GetLogicalDescendants().OfType<Label>(),
            c => Equals(c.Content, Se.Language.General.Settings));
        var startFrom = Assert.Single(
            window.GetLogicalDescendants().OfType<Label>(),
            c => Equals(c.Content, Se.Language.General.StartFrom));

        var settingsLeft = settings.TranslatePoint(new Point(0, 0), window)!.Value.X;
        var startFromRight = startFrom.TranslatePoint(new Point(startFrom.Bounds.Width, 0), window)!.Value.X;
        Assert.True(settingsLeft > startFromRight,
            $"the settings box at x={settingsLeft} is not to the right of the start-from box ending at x={startFromRight}");
    }
}
