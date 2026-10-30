using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic;

/// <summary>
/// An auto-sized window taller than the screen gets clamped to the working area; its content
/// must then scroll instead of clipping the bottom rows - e.g. the batch convert settings
/// window at 125% on a laptop lost its OCR settings and the OK button (#16026).
/// </summary>
public class WrapContentInScrollViewerTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private Window Show(Window window)
    {
        _windows.Add(window);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    private static void Shrink(Window window, double height)
    {
        window.SizeToContent = SizeToContent.Manual;
        window.MinHeight = 0;
        window.Height = height;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    [AvaloniaFact]
    public void BatchConvertSettings_ScrollsToTheButtons_OnAShortScreen()
    {
        var vm = new BatchConvertSettingsViewModel(new FolderHelper(), new WindowService(new NullServiceProvider()));
        var window = Show(new BatchConvertSettingsWindow(vm));
        var naturalHeight = window.ClientSize.Height;

        Shrink(window, 500);
        UiUtil.WrapContentInScrollViewer(window, horizontal: false, vertical: true);
        window.UpdateLayout();

        var scroller = Assert.IsType<ScrollViewer>(window.Content);
        Assert.Equal(ScrollBarVisibility.Disabled, scroller.HorizontalScrollBarVisibility);
        Assert.True(
            scroller.Extent.Height >= naturalHeight - 1.5,
            $"Content was squeezed instead of scrolled (extent {scroller.Extent.Height:0.#}, natural {naturalHeight:0.#}).");
        Assert.True(scroller.Viewport.Height <= 500 + 1.5);

        var okButton = window.GetLogicalDescendants().OfType<Button>()
            .First(b => b.IsDefault);
        scroller.ScrollToEnd();
        window.UpdateLayout();
        var okBottom = okButton.TranslatePoint(new Point(0, okButton.Bounds.Height), window)?.Y;
        Assert.NotNull(okBottom);
        Assert.True(okBottom.Value <= window.ClientSize.Height + 1.5, $"OK button still clipped ({okBottom.Value:0.#}).");
    }

    [AvaloniaFact]
    public void Wrap_IsIdempotent()
    {
        var window = Show(new Window { SizeToContent = SizeToContent.WidthAndHeight, Content = new StackPanel { Height = 900 } });
        UiUtil.WrapContentInScrollViewer(window, horizontal: true, vertical: true);
        var first = window.Content;
        UiUtil.WrapContentInScrollViewer(window, horizontal: true, vertical: true);

        Assert.Same(first, window.Content);
        Assert.IsType<StackPanel>(((ScrollViewer)window.Content!).Content);
    }

    [AvaloniaFact]
    public void Wrap_SkipsWindowsWithTheirOwnScrollViewer()
    {
        // e.g. burn-in: settings scroll above a fixed button bar - an outer scroller would
        // give the inner one unlimited height and push the buttons off-screen again.
        var inner = new ScrollViewer { Content = new StackPanel { Height = 900 } };
        var root = new DockPanel { Children = { new Button { [DockPanel.DockProperty] = Dock.Bottom }, inner } };
        var window = Show(new Window { SizeToContent = SizeToContent.WidthAndHeight, Content = root });

        UiUtil.WrapContentInScrollViewer(window, horizontal: false, vertical: true);

        Assert.Same(root, window.Content);
    }

    [AvaloniaFact]
    public void Wrap_StillWraps_WhenTheOnlyInnerScrollViewerHasAFixedSize()
    {
        // e.g. spell check: a 243 px suggestion list is part of the content, not a window filler.
        var root = new StackPanel { Children = { new ScrollViewer { Height = 243 }, new Border { Height = 900 } } };
        var window = Show(new Window { SizeToContent = SizeToContent.WidthAndHeight, Content = root });

        UiUtil.WrapContentInScrollViewer(window, horizontal: false, vertical: true);

        Assert.Same(root, Assert.IsType<ScrollViewer>(window.Content).Content);
    }

    [AvaloniaFact]
    public void Wrap_DoesNothing_WhenNoAutoSizedDimensionWasClamped()
    {
        var root = new StackPanel { Height = 900 };
        var window = Show(new Window { Content = root });

        UiUtil.WrapContentInScrollViewer(window, horizontal: false, vertical: false);

        Assert.Same(root, window.Content);
    }
}
