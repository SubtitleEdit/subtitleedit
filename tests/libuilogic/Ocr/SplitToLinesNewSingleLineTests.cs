using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Issue #14292: a one-line DVD subtitle whose leftmost glyph has a descender, e.g. "(", came
/// out of nOCR with an extra unknown character. The VobSub decoder leaves up to seven
/// transparent rows under the text, and that padded height cleared the 2.2 x minLineHeight
/// gate in front of the "allows for up/down" line splitter. Its path then ran down the outer
/// edge of the "(", eroded it, and left the crumbs as a bogus second line.
/// </summary>
public class SplitToLinesNewSingleLineTests
{
    // Left edge of subtitle 125 of the reporter's file after MakeTwoColor: the "(" of "(SINGT".
    private static readonly string[] Paren =
    [
        "............",
        "........###.",
        ".......###..",
        ".......###..",
        "......###...",
        "......###...",
        "......###...",
        ".....###....",
        ".....###....",
        ".....###....",
        ".....###....",
        ".....###....",
        ".....###....",
        ".....###....",
        ".....###....",
        ".....###....",
        "......###...",
        "......###...",
        "......###...",
        "......###...",
        ".......###..",
        ".......###..",
        "........###.",
    ];

    /// <summary>
    /// The "(" followed by four 10x14 blocks whose tops sit one row above the "(", as the caps do in
    /// the real line, with the decoder's transparent bottom margin under it all.
    /// </summary>
    private static NikseBitmap2 MakeOneLineWithParenAndBottomMargin()
    {
        const int blockW = 10, blockH = 14, gapX = 6, blocks = 4, bottomMargin = 8;
        var width = Paren[0].Length + 4 + blocks * blockW + (blocks - 1) * gapX + 2;
        var height = Paren.Length + bottomMargin;
        var data = new byte[width * height * 4];

        void Set(int x, int y)
        {
            var i = (x + y * width) * 4;
            data[i] = data[i + 1] = data[i + 2] = data[i + 3] = 255;
        }

        for (var y = 0; y < Paren.Length; y++)
        {
            for (var x = 0; x < Paren[y].Length; x++)
            {
                if (Paren[y][x] == '#')
                {
                    Set(x, y);
                }
            }
        }

        for (var b = 0; b < blocks; b++)
        {
            var left = Paren[0].Length + 4 + b * (blockW + gapX);
            for (var y = 0; y < blockH; y++)
            {
                for (var x = left; x < left + blockW; x++)
                {
                    Set(x, y);
                }
            }
        }

        return new NikseBitmap2(width, height, data);
    }

    [Fact]
    public void OneLineWithDescenderAndBottomMargin_IsNotSplitIntoTwoLines()
    {
        var bmp = MakeOneLineWithParenAndBottomMargin();

        // 14 is what the adaptive tracker settles on for the reporter's DVD font; the padded
        // bitmap height (31) is above 2.2 x 14 while the ink height (23) is not.
        var items = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(bmp, 3, false, true, 14, true, 15.0);

        Assert.Equal(0, items.Count(p => p.SpecialCharacter == Environment.NewLine));
        Assert.Equal(5, items.Count(p => p.NikseBitmap != null));
    }
}
