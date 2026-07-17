using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit;
using System.Runtime.InteropServices;

namespace UITests.Logic;

/// <summary>
/// The subtitle text box (with color tags on) is an AvaloniaEdit TextEditor. In right-to-left
/// mode its lines must start at the right edge, like every other RTL text control.
///
/// The vendored TextView opts out of Avalonia's automatic mirror transform for RightToLeft
/// controls (BypassFlowDirectionPolicies, like the framework's own text controls) and lets the
/// text formatter handle right-to-left: bidi run order plus TextAlignment.Start resolving to the
/// right edge. Any mirror transform over the TextView renders the formatter's output reversed
/// with mirror-imaged glyphs, which is why FlowDirection goes on the TextView itself - a
/// RightToLeft TextArea or TextEditor would introduce that mirror (#12386 follow-up).
///
/// These tests assert on rendered pixels (headless Skia), not on layout state, so they catch
/// transform-level regressions that state-based asserts cannot see.
/// </summary>
public class TextViewAlignmentTests
{
    private const string Arabic = "مرحبا بالعالم.";
    private const string English = "hello world";

    private const int Width = 400;
    private const int Height = 100;

    private static (int MinX, int MaxX) ShowAndMeasureInk(Control content)
    {
        var window = new Window
        {
            Content = content,
            Width = Width,
            Height = Height,
            Background = Brushes.Black,
        };
        window.Show();
        window.Measure(new Size(Width, Height));
        window.Arrange(new Rect(0, 0, Width, Height));
        Dispatcher.UIThread.RunJobs();

        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        return MeasureInk(frame!);
    }

    /// <summary>Horizontal extent of all non-black pixels.</summary>
    private static (int MinX, int MaxX) MeasureInk(WriteableBitmap frame)
    {
        int minX = int.MaxValue, maxX = -1;
        using var fb = frame.Lock();
        var row = new int[frame.PixelSize.Width];
        for (var y = 0; y < frame.PixelSize.Height; y++)
        {
            Marshal.Copy(fb.Address + y * fb.RowBytes, row, 0, row.Length);
            for (var x = 0; x < row.Length; x++)
            {
                if ((row[x] & 0x00FFFFFF) != 0)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
        }

        Assert.True(maxX >= 0, "no ink rendered");
        return (minX, maxX);
    }

    private static TextEditor MakeEditor(string text, FlowDirection flowDirection, TextAlignment? alignment = null)
    {
        var editor = new TextEditor
        {
            Text = text,
            WordWrap = true,
            FontSize = 20,
            Foreground = Brushes.White,
            Background = Brushes.Black,
        };
        // On the TextView, not the TextArea - matches RightToLeftHelper in the app.
        editor.TextArea.TextView.FlowDirection = flowDirection;
        if (alignment != null)
        {
            editor.TextArea.TextView.TextAlignment = alignment.Value;
        }

        return editor;
    }

    [AvaloniaFact]
    public void RightToLeft_RendersAtRightEdge()
    {
        var (minX, maxX) = ShowAndMeasureInk(MakeEditor(Arabic, FlowDirection.RightToLeft));

        Assert.True(maxX > Width - 30, $"ink ends at {maxX}, expected near the right edge of {Width}");
        Assert.True(minX > Width / 2, $"ink starts at {minX}, expected in the right half");
    }

    [AvaloniaFact]
    public void RightToLeft_MatchesTextBlockReference()
    {
        // TextBlock is the framework's reference for RTL rendering; the editor must place its
        // line in the same area.
        var editorInk = ShowAndMeasureInk(MakeEditor(Arabic, FlowDirection.RightToLeft));
        var textBlockInk = ShowAndMeasureInk(new TextBlock
        {
            Text = Arabic,
            FontSize = 20,
            Foreground = Brushes.White,
            FlowDirection = FlowDirection.RightToLeft,
        });

        Assert.True(System.Math.Abs(editorInk.MaxX - textBlockInk.MaxX) < 30,
            $"editor ink ends at {editorInk.MaxX}, TextBlock reference at {textBlockInk.MaxX}");
    }

    [AvaloniaFact]
    public void LeftToRight_RendersAtLeftEdge()
    {
        var (minX, maxX) = ShowAndMeasureInk(MakeEditor(English, FlowDirection.LeftToRight));

        Assert.True(minX < 30, $"ink starts at {minX}, expected near the left edge");
        Assert.True(maxX < Width / 2, $"ink ends at {maxX}, expected in the left half");
    }

    [AvaloniaFact]
    public void Center_LeftToRight_RendersCentered()
    {
        var (minX, maxX) = ShowAndMeasureInk(MakeEditor(English, FlowDirection.LeftToRight, TextAlignment.Center));

        var center = (minX + maxX) / 2.0;
        Assert.True(System.Math.Abs(center - Width / 2.0) < 20,
            $"ink center is {center}, expected near {Width / 2.0}");
    }

    [AvaloniaFact]
    public void Center_RightToLeft_RendersCentered()
    {
        var (minX, maxX) = ShowAndMeasureInk(MakeEditor(Arabic, FlowDirection.RightToLeft, TextAlignment.Center));

        var center = (minX + maxX) / 2.0;
        Assert.True(System.Math.Abs(center - Width / 2.0) < 20,
            $"ink center is {center}, expected near {Width / 2.0}");
    }

    // Selection highlighting and mouse hit-testing in RTL mode are covered by
    // RightToLeftSelectionTests, including lines with formatting tags.

    [AvaloniaFact]
    public void RightToLeft_LatinGlyphsAreNotMirrorImaged()
    {
        // A Latin run inside an RTL paragraph must render the same glyphs as in an LTR
        // paragraph - if the mirror handling is wrong the glyph outlines come out flipped
        // ("an8" reads "8na"). Compare the ink of the same text in both directions after
        // aligning the two bounding boxes.
        var ltr = RenderFrame(MakeEditor(English, FlowDirection.LeftToRight));
        var rtl = RenderFrame(MakeEditor(English, FlowDirection.RightToLeft));

        var ltrInk = MeasureInk(ltr);
        var rtlInk = MeasureInk(rtl);

        var widthLtr = ltrInk.MaxX - ltrInk.MinX;
        var widthRtl = rtlInk.MaxX - rtlInk.MinX;
        Assert.True(System.Math.Abs(widthLtr - widthRtl) < 8,
            $"ink widths differ too much: {widthLtr} vs {widthRtl}");

        var mismatch = CountMismatchedInkColumns(ltr, ltrInk.MinX, rtl, rtlInk.MinX, System.Math.Min(widthLtr, widthRtl));
        Assert.True(mismatch < 0.25, $"glyphs differ between LTR and RTL rendering (mismatch {mismatch:P0})");
    }

    private static WriteableBitmap RenderFrame(Control content)
    {
        var window = new Window
        {
            Content = content,
            Width = Width,
            Height = Height,
            Background = Brushes.Black,
        };
        window.Show();
        window.Measure(new Size(Width, Height));
        window.Arrange(new Rect(0, 0, Width, Height));
        Dispatcher.UIThread.RunJobs();

        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        return frame!;
    }

    /// <summary>
    /// Fraction of columns whose ink profile (rows with ink) differs between the two frames,
    /// comparing column x0a+i against x0b+i. Tolerant of antialiasing; a mirror-imaged
    /// rendering mismatches heavily.
    /// </summary>
    private static double CountMismatchedInkColumns(WriteableBitmap a, int x0a, WriteableBitmap b, int x0b, int width)
    {
        using var fa = a.Lock();
        using var fb = b.Lock();
        var rowA = new int[a.PixelSize.Width];
        var rowB = new int[b.PixelSize.Width];
        var inkA = new bool[width, a.PixelSize.Height];
        var inkB = new bool[width, b.PixelSize.Height];

        for (var y = 0; y < a.PixelSize.Height && y < b.PixelSize.Height; y++)
        {
            Marshal.Copy(fa.Address + y * fa.RowBytes, rowA, 0, rowA.Length);
            Marshal.Copy(fb.Address + y * fb.RowBytes, rowB, 0, rowB.Length);
            for (var i = 0; i < width; i++)
            {
                inkA[i, y] = (rowA[x0a + i] & 0x00FFFFFF) != 0;
                inkB[i, y] = (rowB[x0b + i] & 0x00FFFFFF) != 0;
            }
        }

        var mismatched = 0;
        for (var i = 0; i < width; i++)
        {
            var diff = 0;
            var total = 0;
            for (var y = 0; y < a.PixelSize.Height && y < b.PixelSize.Height; y++)
            {
                if (inkA[i, y] || inkB[i, y])
                {
                    total++;
                    if (inkA[i, y] != inkB[i, y])
                    {
                        diff++;
                    }
                }
            }

            if (total > 0 && diff > total / 2)
            {
                mismatched++;
            }
        }

        return width == 0 ? 0 : (double)mismatched / width;
    }
}
