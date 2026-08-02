using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Controls;

/// <summary>
/// Painting must not notify the host about the scroll metrics. A line wider than the current
/// extent estimate is only discovered while it is laid out, which happens during the paint, and
/// the host reacts by resizing its scroll bars - touching a visual mid-paint makes Avalonia throw
/// "Visual was invalidated during the render pass".
/// </summary>
public class SyntaxTextEditorRenderTests : IDisposable
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

    private SyntaxTextEditor Show(string text)
    {
        var editor = new SyntaxTextEditor
        {
            Text = text,
            FontFamily = new FontFamily("Courier New"),
            FontSize = 12,
            SourceHighlighter = new SubRipSourceSyntaxHighlighting(),
        };

        var window = new Window { Content = editor, Width = 400, Height = 300 };
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();
        return editor;
    }

    /// <summary>
    /// Paints the view for real and reports whether it raised ScrollMetricsChanged while doing so.
    /// </summary>
    private static bool NotifiesScrollMetricsWhilePainting(SyntaxTextEditor editor)
    {
        var notifiedDuringPaint = false;
        var painting = false;
        editor.View.ScrollMetricsChanged += (_, _) => notifiedDuringPaint |= painting;

        using var bitmap = new RenderTargetBitmap(new PixelSize(400, 300), new Vector(96, 96));
        using var context = bitmap.CreateDrawingContext();

        painting = true;
        editor.View.Render(context);
        painting = false;

        return notifiedDuringPaint;
    }

    [AvaloniaFact]
    public void PaintingALineWiderThanTheEstimateDoesNotTouchTheScrollBars()
    {
        // Edits keep the extent estimate instead of rescanning every line for the longest one, so
        // a line that grows past it is only found out about when it is painted - which is exactly
        // the case that used to crash.
        var editor = Show(string.Join(Environment.NewLine, ["short", "lines", "only"]));
        var widthBefore = editor.View.Extent.Width;

        // Straight into the document: typing would lay the line out on the way to scrolling the
        // caret into view, and then the paint would have nothing left to discover.
        editor.Document.Insert(0, new string('W', 400));

        Assert.False(NotifiesScrollMetricsWhilePainting(editor));

        // The extent catches up on the next dispatcher turn instead.
        Dispatcher.UIThread.RunJobs();
        Assert.True(
            editor.View.Extent.Width > widthBefore,
            "the wider line should still reach the scroll bars, just not during the paint");
    }

    [AvaloniaFact]
    public void RepaintingUnchangedTextNotifiesNothing()
    {
        var editor = Show(string.Join(Environment.NewLine, ["1", "00:00:01,000 --> 00:00:03,000", "Hello"]));

        Assert.False(NotifiesScrollMetricsWhilePainting(editor));
        Assert.False(NotifiesScrollMetricsWhilePainting(editor));
    }
}
