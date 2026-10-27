using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Issue #16195: the letter splitter's path search slips between the bowl of a bold "a" and
/// the 2 px foot sticking out at its bottom right (also the leg of "R", "x", "k", "A"), so the
/// foot became a 2x2 part of its own that nothing could match - the OCR then prompted for an
/// unusable "character". A part a hundredth the area of the glyph whose box it touches is a
/// crumb of that glyph and is dropped.
/// </summary>
public class SplitterCrumbPartTests
{
    private static NikseBitmap2 Ink(int width, int height, params (int X, int Y, int W, int H)[] rects)
    {
        var bitmap = new NikseBitmap2(width, height);
        foreach (var (x0, y0, w, h) in rects)
        {
            for (var y = y0; y < y0 + h; y++)
            {
                for (var x = x0; x < x0 + w; x++)
                {
                    bitmap.SetPixel(x, y, SKColors.White);
                }
            }
        }

        return bitmap;
    }

    private static List<ImageSplitterItem2> Glyphs(NikseBitmap2 line)
    {
        return NikseBitmapImageSplitter2.SplitBitmapToLettersNew(line, 1, false, true, 10, true)
            .Where(p => p.NikseBitmap != null)
            .ToList();
    }

    [Fact]
    public void FootCutOffAGlyph_IsDropped()
    {
        // A 20x30 glyph, a 2x2 foot touching its bottom-right corner, and a second glyph.
        var line = Ink(70, 40, (2, 4, 20, 30), (22, 34, 2, 2), (44, 4, 20, 30));

        var glyphs = Glyphs(line);

        Assert.Equal(2, glyphs.Count);
        Assert.Equal(30, glyphs[0].NikseBitmap!.Height); // the foot is not merged in either
        Assert.DoesNotContain(glyphs, g => g.NikseBitmap!.Width <= 2);
    }

    [Fact]
    public void SmallMarkTouchingAGlyph_IsKept()
    {
        // A 5x5 mark (more than a hundredth of the glyph) in the same spot is a glyph of its own.
        var line = Ink(70, 40, (2, 4, 20, 30), (22, 34, 5, 5), (44, 4, 20, 30));

        var glyphs = Glyphs(line);

        Assert.Equal(3, glyphs.Count);
    }

    [Fact]
    public void TinyMarkApartFromAGlyph_IsKept()
    {
        // The same 2x2 mark two columns away from the glyph is not its crumb (a tiny period).
        var line = Ink(70, 40, (2, 4, 20, 30), (24, 34, 2, 2), (44, 4, 20, 30));

        var glyphs = Glyphs(line);

        Assert.Equal(3, glyphs.Count);
    }
}
