using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

/// <summary>
/// A PG segment carries a 16 bit length, so a bitmap bigger than that is written as a run of ODS
/// segments: the first one holds 0xffe4 RLE bytes (it also carries the 11 byte object header), the
/// following ones 0xffeb each. Only the last fragment may set last_in_sequence, and no fragment
/// should be empty.
/// </summary>
public class BluRaySupPictureOdsFragmentTests
{
    private const int FirstOdsPayload = 0xffe4; // 65508
    private const int NextOdsPayload = 0xffeb;  // 65515

    private static readonly SKColor ColorA = new SKColor(255, 0, 0);
    private static readonly SKColor ColorB = new SKColor(0, 255, 0);
    private static readonly SKColor ColorC = new SKColor(0, 0, 255);

    /// <summary>
    /// Builds a bitmap whose RLE encoding is exactly <paramref name="targetRleLength"/> bytes, so the
    /// fragment boundaries can be hit dead on. Each row is a stretch of alternating pixels (the
    /// encoder writes those as one byte each) followed by one run, and ends with the two byte EOL.
    /// </summary>
    private static SKBitmap BitmapWithRleLength(int targetRleLength)
    {
        const int width = 1000;
        const int fullRow = width + 2;

        var fullRows = targetRleLength / fullRow;
        var rest = targetRleLength % fullRow;
        while (rest > 0 && rest < 6 && fullRows > 0)
        {
            fullRows--;
            rest += fullRow;
        }

        var alternatingPerRow = new List<int>();
        for (var i = 0; i < fullRows; i++)
        {
            alternatingPerRow.Add(width);
        }

        if (rest > 0)
        {
            // "00 cx yy cc" (4 bytes) for a run of 64+ pixels, "00 8x cc" (3 bytes) below that.
            alternatingPerRow.Add(rest <= width - 58 ? rest - 6 : rest - 5);
        }

        var bitmap = new SKBitmap(new SKImageInfo(width, alternatingPerRow.Count, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        for (var y = 0; y < alternatingPerRow.Count; y++)
        {
            var alternating = alternatingPerRow[y];
            for (var x = 0; x < alternating; x++)
            {
                bitmap.SetPixel(x, y, (x & 1) == 0 ? ColorA : ColorB);
            }

            for (var x = alternating; x < width; x++)
            {
                bitmap.SetPixel(x, y, ColorC);
            }
        }

        return bitmap;
    }

    private sealed record Ods(bool First, bool Last, int DataLength);

    private static (byte[] Sup, List<Ods> Fragments) Write(SKBitmap bitmap)
    {
        var pic = new BluRaySupPicture
        {
            StartTime = 1000,
            EndTime = 3000,
            Width = 1920,
            Height = 1080,
            CompositionNumber = 2,
        };

        var sup = BluRaySupPicture.CreateSupFrame(pic, bitmap, SKColors.White, 25, 20, 10, BluRayContentAlignment.BottomCenter);

        var fragments = new List<Ods>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            Assert.Equal(0x50, sup[position]);
            Assert.Equal(0x47, sup[position + 1]);
            var size = (sup[position + 11] << 8) + sup[position + 12];
            if (sup[position + 10] == 0x15) // ODS
            {
                var flags = sup[position + 13 + 3];
                var first = (flags & 0x80) == 0x80;
                fragments.Add(new Ods(first, (flags & 0x40) == 0x40, size - (first ? 11 : 4)));
            }

            position += 13 + size;
        }

        Assert.Equal(sup.Length, position); // segments chain exactly to the end
        return (sup, fragments);
    }

    private static void AssertFragmentRulesHold(List<Ods> fragments)
    {
        Assert.NotEmpty(fragments);
        Assert.All(fragments, f => Assert.NotEqual(0, f.DataLength));
        Assert.True(fragments[0].First);
        Assert.All(fragments.Skip(1), f => Assert.False(f.First));

        // last_in_sequence belongs on the final fragment only
        Assert.True(fragments[fragments.Count - 1].Last);
        Assert.All(fragments.Take(fragments.Count - 1), f => Assert.False(f.Last));
    }

    [Theory]
    [InlineData(20000, 1)]                                        // comfortably inside one segment
    [InlineData(FirstOdsPayload, 1)]                              // exactly fills the first segment
    [InlineData(FirstOdsPayload + 1, 2)]                          // one byte over
    [InlineData(FirstOdsPayload + NextOdsPayload, 2)]             // exactly fills two segments
    [InlineData(FirstOdsPayload + NextOdsPayload + 1, 3)]         // one byte over
    [InlineData(250000, 4)]                                       // three continuation fragments
    public void SplitsTheRleBufferIntoTheFewestFragments(int rleLength, int expectedFragments)
    {
        using var bitmap = BitmapWithRleLength(rleLength);
        var (_, fragments) = Write(bitmap);

        Assert.Equal(expectedFragments, fragments.Count);
        Assert.Equal(rleLength, fragments.Sum(f => f.DataLength));
        AssertFragmentRulesHold(fragments);
    }

    [Theory]
    [InlineData(20000)]
    [InlineData(FirstOdsPayload + NextOdsPayload)]
    [InlineData(250000)]
    public void FragmentedBitmapsRoundTripThroughTheParser(int rleLength)
    {
        using var bitmap = BitmapWithRleLength(rleLength);
        var (sup, _) = Write(bitmap);

        using var stream = new MemoryStream(sup);
        var parsed = BluRaySupParser.ParseBluRaySup(stream, new StringBuilder(), false, new Dictionary<int, List<PaletteInfo>>(), new Dictionary<int, List<BluRaySupParser.OdsData>>());

        var withImage = parsed.Where(p => p.PcsObjects.Count > 0).ToList();
        Assert.Single(withImage);
        using var decoded = withImage[0].GetBitmap();
        Assert.Equal(bitmap.Width, decoded.Width);
        Assert.Equal(bitmap.Height, decoded.Height);

        // The palette is 8 bit YCbCr, so colours shift a little - but every pixel must be opaque
        // and the bottom row must have survived the last fragment.
        for (var x = 0; x < decoded.Width; x += 97)
        {
            Assert.Equal(255, decoded.GetPixel(x, decoded.Height - 1).Alpha);
        }
    }
}
