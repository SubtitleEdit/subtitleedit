using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

/// <summary>
/// Objects and windows have to be at least 8x8 and lie inside the frame, or a decoder (and
/// Scenarist) rejects the display set.
/// </summary>
public class BluRaySupPictureObjectBoundsTests
{
    private static BluRaySupParser.PcsData WriteAndParse(int bitmapWidth, int bitmapHeight, int x, int y)
    {
        using var bitmap = new SKBitmap(bitmapWidth, bitmapHeight);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        var picture = new BluRaySupPicture
        {
            StartTime = 1000,
            EndTime = 2000,
            Width = 1920,
            Height = 1080,
        };
        var sup = BluRaySupPicture.CreateSupFrame(picture, bitmap, SKColors.White, 25, 0, 0, BluRayContentAlignment.BottomCenter, new BluRayPoint(x, y));
        using var stream = new MemoryStream(sup);
        return BluRaySupParser.ParseBluRaySup(stream, new StringBuilder(), false, new Dictionary<int, List<PaletteInfo>>(), new Dictionary<int, List<BluRaySupParser.OdsData>>())[0];
    }

    [Fact]
    public void TinyBitmap_IsPaddedToEightByEight()
    {
        var pcs = WriteAndParse(3, 2, 900, 900);

        Assert.Equal(8, pcs.BitmapObjects[0][0].Width);
        Assert.Equal(8, pcs.BitmapObjects[0][0].Height);
        Assert.Equal(900, pcs.PcsObjects[0].Origin.X);
        Assert.Equal(900, pcs.PcsObjects[0].Origin.Y);

        using var decoded = pcs.GetBitmap();
        Assert.Equal(255, decoded.GetPixel(1, 1).Alpha);
        Assert.Equal(0, decoded.GetPixel(6, 6).Alpha);
    }

    [Fact]
    public void ObjectPastTheRightAndBottomEdge_IsMovedInside()
    {
        var pcs = WriteAndParse(200, 60, 1900, 1070);

        Assert.Equal(1920 - 200, pcs.PcsObjects[0].Origin.X);
        Assert.Equal(1080 - 60, pcs.PcsObjects[0].Origin.Y);
    }

    [Fact]
    public void ObjectInside_KeepsItsPosition()
    {
        var pcs = WriteAndParse(200, 60, 100, 900);

        Assert.Equal(100, pcs.PcsObjects[0].Origin.X);
        Assert.Equal(900, pcs.PcsObjects[0].Origin.Y);
    }
}
