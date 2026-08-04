using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;

namespace LibSETests.VobSub;

public class SubPictureTest
{
    // A SetColor (0x03) command as the very last byte used to read its two missing
    // argument bytes past the end of the data and throw.
    [Fact]
    public void TruncatedSetColorCommandDoesNotThrow()
    {
        var data = new byte[]
        {
            0x00, 0x09, // sub picture data size
            0x00, 0x04, // display control sequence table address
            0x00, 0x00, // delay
            0x00, 0x00, // next sequence address (ends the sequence loop)
            0x03,       // SetColor with both argument bytes missing
        };

        var colorLookUpTable = Enumerable.Repeat(SKColors.White, 16).ToList();
        var subPicture = new SubPicture(data);
        using var bmp = subPicture.GetBitmap(colorLookUpTable, SKColors.Black, SKColors.White, SKColors.Gray, SKColors.Green, false);

        Assert.NotNull(bmp);
    }

    [Fact]
    public void TruncatedSetContrastCommandDoesNotThrow()
    {
        var data = new byte[]
        {
            0x00, 0x09,
            0x00, 0x04,
            0x00, 0x00,
            0x00, 0x00,
            0x04,       // SetContrast with both argument bytes missing
        };

        var colorLookUpTable = Enumerable.Repeat(SKColors.White, 16).ToList();
        var subPicture = new SubPicture(data);
        using var bmp = subPicture.GetBitmap(colorLookUpTable, SKColors.Black, SKColors.White, SKColors.Gray, SKColors.Green, false);

        Assert.NotNull(bmp);
    }
}
