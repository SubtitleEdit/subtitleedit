using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Issue #14262 (matter H): Arabic in the waveform was shaped with a left to right paragraph
/// direction, so the neutral characters around the letters - the dialogue dash, the ellipsis,
/// brackets - took the paragraph direction instead of the direction of the letters next to them
/// and ended up on the wrong side of the line. SE 4 got this right, SE 5 beta 27 did not.
/// </summary>
public class AudioVisualizerRtlTextTests
{
    // "- Hello my friend..." - a dialogue dash before the letters and an ellipsis after them,
    // the two neutrals from the issue's screenshots.
    private const string ArabicLine = "- مرحبا يا صديقي...";

    [AvaloniaFact]
    public void ArabicParagraphIsShapedRightToLeft()
    {
        var av = new AudioVisualizer();

        var prepared = av.GetPreparedParagraphText(ArabicLine);

        Assert.True(prepared.RightToLeft);
        Assert.Equal(FlowDirection.RightToLeft, av.GetCachedParagraphText(prepared.Unwrapped, prepared.RightToLeft).FlowDirection);
    }

    [AvaloniaFact]
    public void LatinParagraphKeepsLeftToRight()
    {
        var av = new AudioVisualizer();

        var prepared = av.GetPreparedParagraphText("- Where is Arturo?" + Environment.NewLine + "- He was here.");

        Assert.False(prepared.RightToLeft);
        Assert.Equal(FlowDirection.LeftToRight, av.GetCachedParagraphText(prepared.Unwrapped, prepared.RightToLeft).FlowDirection);
    }

    /// <summary>
    /// The direction is decided for the whole paragraph, not line by line: a second line holding
    /// only neutrals, digits or a Latin name belongs to the same block as the first one and must
    /// not flip back to left to right in the middle of a subtitle.
    /// </summary>
    [AvaloniaFact]
    public void ASecondLineWithoutRightToLeftLettersFollowsTheParagraph()
    {
        var av = new AudioVisualizer();

        var prepared = av.GetPreparedParagraphText("مرحبا يا صديقي" + Environment.NewLine + "- (Gino)...");

        Assert.True(prepared.RightToLeft);
        Assert.Equal(2, prepared.Lines.Count);
    }

    /// <summary>
    /// The symptom itself, measured on the shaped line: laid out left to right, the dialogue dash
    /// is the leftmost thing on the line and the trailing "..." the rightmost - both on the wrong
    /// side of a right to left line, which is exactly what the issue's SE 5 screenshot shows.
    /// (Avalonia hands out no highlight geometry for a right to left line - it comes back empty -
    /// so the right to left half of this is measured by rendering, below.)
    /// </summary>
    [AvaloniaFact]
    public void LeftToRightLeavesTheNeutralsOnTheWrongSideOfAnArabicLine()
    {
        var av = new AudioVisualizer();
        var leftToRight = av.GetCachedParagraphText(ArabicLine, false);

        var dash = leftToRight.BuildHighlightGeometry(new Point(0, 0), 0, 1)!.Bounds;
        var ellipsis = leftToRight.BuildHighlightGeometry(new Point(0, 0), ArabicLine.Length - 3, 3)!.Bounds;

        Assert.True(dash.Left < leftToRight.Width / 2, "the dash should start the line when it is laid out left to right");
        Assert.True(ellipsis.Left > leftToRight.Width / 2, "the ellipsis should end the line when it is laid out left to right");
    }

    /// <summary>
    /// And the direction really reaches the shaping: the same string drawn right to left comes out
    /// as a different picture (same width - the direction only reorders the glyphs).
    /// </summary>
    [AvaloniaFact]
    public void TheDirectionChangesWhatIsDrawn()
    {
        var av = new AudioVisualizer();
        var rightToLeft = av.GetCachedParagraphText(ArabicLine, true);
        var leftToRight = av.GetCachedParagraphText(ArabicLine, false);

        Assert.Equal(leftToRight.Width, rightToLeft.Width, 3);
        Assert.NotEqual(Render(leftToRight), Render(rightToLeft));
    }

    private static byte[] Render(FormattedText text)
    {
        using var bitmap = new RenderTargetBitmap(new PixelSize(200, 40), new Vector(96, 96));
        using (var context = bitmap.CreateDrawingContext())
        {
            context.FillRectangle(Brushes.Black, new Rect(0, 0, 200, 40));
            context.DrawText(text, new Point(0, 0));
        }

        var pixels = new byte[200 * 40 * 4];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(0, 0, 200, 40), handle.AddrOfPinnedObject(), pixels.Length, 200 * 4);
        }
        finally
        {
            handle.Free();
        }

        return pixels;
    }
}
