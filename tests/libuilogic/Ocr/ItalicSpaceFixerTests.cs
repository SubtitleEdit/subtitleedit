using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The slanted glyph boxes of an italic line overlap, so the space detector sees few gaps in
/// it and its threshold can come out as a double space. <see cref="ItalicSpaceFixer"/> then
/// counts the blank angled columns between glyphs with its own threshold, capped relative to
/// the glyph height, so an italic "Dat werd tijd" does not stay one word.
/// </summary>
public class ItalicSpaceFixerTests
{
    private const int Height = 40;

    private static NikseBitmap2 Bars(int width, params int[] barLefts)
    {
        var bmp = new NikseBitmap2(width, Height);
        foreach (var left in barLefts)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = left; x < left + 6; x++)
                {
                    bmp.SetPixel(x, y, SKColors.White);
                }
            }
        }

        return bmp;
    }

    private static (NikseBitmap2 Parent, List<ImageSplitterItem2> Letters, List<NOcrChar> Matches) MakeLine()
    {
        // Three upright 6 px bars: 4 px between the first two (a letter gap), 14 px before the third.
        var parent = Bars(60, 0, 10, 30);
        var letters = new List<ImageSplitterItem2>();
        var matches = new List<NOcrChar>();
        foreach (var (x, text) in new[] { (0, "a"), (10, "b"), (30, "c") })
        {
            var item = new ImageSplitterItem2(x, 0, Bars(6, 0)) { Top = 0 };
            letters.Add(item);
            matches.Add(new NOcrChar(text) { Italic = true, ImageSplitterItem = item });
        }

        return (parent, letters, matches);
    }

    [Fact]
    public void DoubleSpaceThreshold_IsCappedByGlyphHeight()
    {
        var (parent, letters, matches) = MakeLine();

        // 24 px: more than any gap in the line - with it no space was ever found.
        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parent, unItalicFactor: 0, pixelsIsSpace: 24);

        Assert.Equal("<i>ab c</i>", text);
    }

    [Fact]
    public void SaneThreshold_IsKept()
    {
        var (parent, letters, matches) = MakeLine();

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parent, unItalicFactor: 0, pixelsIsSpace: 6);

        Assert.Equal("<i>ab c</i>", text);
    }

    [Fact]
    public void LetterGap_IsNoSpace()
    {
        var (parent, letters, matches) = MakeLine();

        // A 3 px threshold would split "ab" too; the cap only lowers, never raises, the threshold.
        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parent, unItalicFactor: 0, pixelsIsSpace: 3);

        Assert.Equal("<i>a b c</i>", text);
    }
}
