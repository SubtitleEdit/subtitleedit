using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace LibSETests.Common;

// VobSubColorIsolation.Isolate rebuilds a black-on-white OCR bitmap by keeping only the
// glyph-fill colour plane — the most interior one (lowest background adjacency). These
// tests build tiny synthetic indexed bitmaps and assert which plane survives.
public class VobSubColorIsolationTest
{
    private static readonly SKColor Fill = new SKColor(255, 255, 255);
    private static readonly SKColor Outline = new SKColor(0, 0, 1);
    private static readonly SKColor AntiAlias = new SKColor(128, 128, 128);

    [Fact]
    public void Isolate_BlankFrame_ReturnsAllWhite()
    {
        using var source = MakeTransparent(8, 8);
        using var result = VobSubColorIsolation.Isolate(source);
        AssertPlaneKept(result, source, null);
    }

    // The classic #12772 case: the outline holds more pixels than the fill, so frequency
    // alone picks the wrong plane; interiority must win.
    [Fact]
    public void Isolate_OutlineLargerThanFill_KeepsFill()
    {
        using var source = MakeTransparent(24, 24);
        FillRect(source, 2, 2, 20, 20, Outline);   // 4-px thick ring after fill overwrite
        FillRect(source, 6, 6, 12, 12, Fill);
        using var result = VobSubColorIsolation.Isolate(source);
        AssertPlaneKept(result, source, Fill);
    }

    // PR #13481 regression: an anti-alias tier fully trapped between fill and outline
    // (e.g. bridging the gap between an i-dot and its stem) never touches the background.
    // Unsmoothed it ties with the fully-outlined fill at ratio 0.0 and the colour-key
    // tie-break picks the (lower-keyed) grey tier, wiping the text. Laplace smoothing
    // must let the larger fill win.
    [Fact]
    public void Isolate_TrappedAntiAliasTier_KeepsFill()
    {
        using var source = MakeTransparent(12, 30);
        // Outline envelope around dot and stem, bridging the gap between them.
        FillRect(source, 2, 2, 8, 26, Outline);
        // Anti-alias tier strictly inside the outline (fully trapped, borderCount = 0).
        FillRect(source, 3, 3, 6, 5, AntiAlias);   // around the dot
        FillRect(source, 3, 12, 6, 15, AntiAlias); // around the stem
        // Fill strictly inside the anti-alias tier (also borderCount = 0, but larger).
        FillRect(source, 4, 4, 4, 3, Fill);        // dot
        FillRect(source, 4, 13, 4, 13, Fill);      // stem
        using var result = VobSubColorIsolation.Isolate(source);
        AssertPlaneKept(result, source, Fill);
    }

    // PR #13481: very short text in a thin font (".") can leave the fill plane under the
    // old 16-pixel floor while the outline clears it, so only the outline was considered
    // and the text was wiped. The floor must relax so the fill can compete.
    [Fact]
    public void Isolate_ThinFontShortText_KeepsSmallFill()
    {
        using var source = MakeTransparent(10, 10);
        FillRect(source, 2, 2, 6, 6, Outline); // 36 px ring after fill overwrite
        FillRect(source, 4, 4, 3, 3, Fill);    // 9 px fill: >= 8, < 16
        using var result = VobSubColorIsolation.Isolate(source);
        AssertPlaneKept(result, source, Fill);
    }

    // Guard for the 8-pixel floor: a plane below it that is trapped inside the fill
    // (borderCount 0) would beat any background-touching fill under smoothing, so the
    // floor must keep excluding it.
    [Fact]
    public void Isolate_SpeckBelowFloorInsideFill_DoesNotStealForeground()
    {
        using var source = MakeTransparent(20, 12);
        FillRect(source, 3, 3, 14, 6, Fill);       // 84 px, touches background
        FillRect(source, 8, 5, 3, 2, AntiAlias);   // 6 px speck, fully enclosed, below floor
        using var result = VobSubColorIsolation.Isolate(source);
        AssertPlaneKept(result, source, Fill);
    }

    // When every plane is speckle-sized the frequency fallback keeps the biggest one
    // instead of blanking the frame.
    [Fact]
    public void Isolate_OnlySpeckles_FallsBackToMostFrequent()
    {
        using var source = MakeTransparent(12, 6);
        FillRect(source, 1, 1, 3, 2, Fill);      // 6 px
        FillRect(source, 7, 1, 2, 2, AntiAlias); // 4 px
        using var result = VobSubColorIsolation.Isolate(source);
        AssertPlaneKept(result, source, Fill);
    }

    private static SKBitmap MakeTransparent(int width, int height)
    {
        var bmp = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        bmp.Erase(SKColors.Transparent);
        return bmp;
    }

    private static void FillRect(SKBitmap bmp, int x, int y, int width, int height, SKColor color)
    {
        for (var yy = y; yy < y + height; yy++)
        {
            for (var xx = x; xx < x + width; xx++)
            {
                bmp.SetPixel(xx, yy, color);
            }
        }
    }

    // Every source pixel of `kept` must come out black; every other pixel white.
    private static void AssertPlaneKept(SKBitmap result, SKBitmap source, SKColor? kept)
    {
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                var s = source.GetPixel(x, y);
                var expectBlack = kept.HasValue && s.Alpha >= 128 &&
                                  s.Red == kept.Value.Red && s.Green == kept.Value.Green && s.Blue == kept.Value.Blue;
                var r = result.GetPixel(x, y);
                if (expectBlack)
                {
                    Assert.True(r.Red < 64 && r.Green < 64 && r.Blue < 64,
                        $"pixel ({x},{y}) should be kept as black ink, was {r}");
                }
                else
                {
                    Assert.True(r.Red > 192 && r.Green > 192 && r.Blue > 192,
                        $"pixel ({x},{y}) should be white background, was {r}");
                }
            }
        }
    }
}
