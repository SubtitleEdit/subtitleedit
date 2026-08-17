using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The OCR loops used to pass a hardcoded minLineHeight of 20 to the splitter. A DVD-sized
/// two-line subtitle (per-line ink height under ~21px) then swallowed the blank gap between
/// its lines and OCR'ed both lines as a handful of merged blobs. These tests use synthetic
/// block "text" so they are font-independent, and pin the behavior the adaptive
/// OcrLineHeightTracker restores.
/// </summary>
public class SplitBitmapAdaptiveLineHeightTests
{
    /// <summary>Two lines of 3 blocks each; each block 10x14 px, 6 px apart, lines 4 px apart.</summary>
    private static NikseBitmap2 MakeTwoLineBitmap()
    {
        const int blockW = 10, blockH = 14, gapX = 6, gapY = 4, blocks = 3;
        var width = blocks * blockW + (blocks - 1) * gapX + 2;
        var height = blockH * 2 + gapY;
        var data = new byte[width * height * 4];

        void Block(int left, int top)
        {
            for (var y = top; y < top + blockH; y++)
            {
                for (var x = left; x < left + blockW; x++)
                {
                    var i = (x + y * width) * 4;
                    data[i] = data[i + 1] = data[i + 2] = data[i + 3] = 255;
                }
            }
        }

        for (var b = 0; b < blocks; b++)
        {
            Block(1 + b * (blockW + gapX), 0);           // line 1
            Block(1 + b * (blockW + gapX), blockH + gapY); // line 2
        }

        return new NikseBitmap2(width, height, data);
    }

    [Fact]
    public void DvdSizedTwoLines_AdaptiveMinLineHeight_KeepsTheLineBreak()
    {
        var bmp = MakeTwoLineBitmap();

        // 12 = the tracker's pre-adaptation fallback for DVD-sized sources.
        var items = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(bmp, 3, false, true, 12, true);

        Assert.Equal(6, items.Count(p => p.NikseBitmap != null));
        Assert.Equal(1, items.Count(p => p.SpecialCharacter == Environment.NewLine));
    }

    [Fact]
    public void DvdSizedTwoLines_HardcodedTwenty_MergesThem()
    {
        // Documents the defect the adaptive height fixes: with the old hardcoded 20, the same
        // bitmap loses its line break because each line's 14px ink height fits under the limit.
        var bmp = MakeTwoLineBitmap();

        var items = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(bmp, 3, false, true, 20, true);

        Assert.Equal(0, items.Count(p => p.SpecialCharacter == Environment.NewLine));
    }

    [Fact]
    public void SplitToLinesNew_SmallMinLineHeight_DoesNotThrow()
    {
        // The up/down slides in SplitToLinesNew could read out of bounds once minLineHeight
        // goes below ~10 (now reachable via the adaptive tracker).
        var bmp = MakeTwoLineBitmap();
        foreach (var minLineHeight in new[] { 4, 5, 6, 7, 8, 9, 10 })
        {
            var items = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(bmp, 3, false, true, minLineHeight, true);
            Assert.NotEmpty(items);
        }
    }
}
