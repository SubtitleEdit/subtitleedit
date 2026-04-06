using Nikse.SubtitleEdit.Logic.Ocr;

namespace UITests.Logic.Ocr;

public class ExpandedOcrGroupTests
{
    [Fact]
    public void CreateNOcrChar_UsesCombinedPreviewSizeAndRelativeTopMargin()
    {
        var sourceBitmap = new NikseBitmap2(40, 40);
        var letters = new List<ImageSplitterItem2>
        {
            CreateLetter(10, 12, 8, 4, 5),
            CreateLetter(15, 10, 6, 3, 7),
        };

        var group = ExpandedOcrGroup.Create(sourceBitmap, letters, 0, 2);

        Assert.NotNull(group);
        var actualGroup = Assert.IsType<ExpandedOcrGroup>(group);
        Assert.Equal(8, actualGroup.PreviewBitmap.Width);
        Assert.Equal(7, actualGroup.PreviewBitmap.Height);
        Assert.Equal(2, actualGroup.PreviewTopMargin);

        var nOcrChar = actualGroup.CreateNOcrChar();
        Assert.Equal(8, nOcrChar.Width);
        Assert.Equal(7, nOcrChar.Height);
        Assert.Equal(2, nOcrChar.MarginTop);
        Assert.Equal(2, nOcrChar.ExpandCount);
    }

    [Fact]
    public void CreateBinaryOcrBitmap_KeepsFirstGlyphPersistedAndStoresExpandedChildren()
    {
        var sourceBitmap = new NikseBitmap2(50, 40);
        var letters = new List<ImageSplitterItem2>
        {
            CreateLetter(10, 12, 8, 4, 5),
            CreateLetter(15, 10, 6, 3, 7),
            CreateLetter(19, 11, 7, 2, 6),
        };

        var group = ExpandedOcrGroup.Create(sourceBitmap, letters, 0, 3);

        Assert.NotNull(group);
        var actualGroup = Assert.IsType<ExpandedOcrGroup>(group);
        var firstBitmap = Assert.IsType<NikseBitmap2>(letters[0].NikseBitmap);
        Assert.True(actualGroup.PreviewBitmap.Width > firstBitmap.Width);

        var binaryOcrBitmap = actualGroup.CreateBinaryOcrBitmap();
        Assert.Equal(firstBitmap.Width, binaryOcrBitmap.Width);
        Assert.Equal(firstBitmap.Height, binaryOcrBitmap.Height);
        Assert.Equal(letters[0].X, binaryOcrBitmap.X);
        Assert.Equal(letters[0].Top, binaryOcrBitmap.Y);
        Assert.Equal(3, binaryOcrBitmap.ExpandCount);
        Assert.Equal(2, binaryOcrBitmap.ExpandedList.Count);
        Assert.Equal(letters[1].X, binaryOcrBitmap.ExpandedList[0].X);
        Assert.Equal(letters[1].Top, binaryOcrBitmap.ExpandedList[0].Y);
        Assert.Equal(letters[2].X, binaryOcrBitmap.ExpandedList[1].X);
        Assert.Equal(letters[2].Top, binaryOcrBitmap.ExpandedList[1].Y);
    }

    private static ImageSplitterItem2 CreateLetter(int x, int y, int top, int width, int height)
    {
        var item = new ImageSplitterItem2(x, y, new NikseBitmap2(width, height))
        {
            Top = top,
            ParentY = y,
        };

        return item;
    }
}
