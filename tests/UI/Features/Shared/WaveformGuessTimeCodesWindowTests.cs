using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Shared;

/// <summary>
/// The guess-time-codes window builds its layout in code and sizes itself to its content. Three
/// stacked option boxes make it tall, so a high UI scale (or a small working area) pushes it past
/// the screen and <see cref="UiUtil"/> clamps it to the working area - which used to cut off the
/// bottom options and the OK/Cancel buttons on a window that cannot be resized.
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
        window.Height = 400;
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
}
