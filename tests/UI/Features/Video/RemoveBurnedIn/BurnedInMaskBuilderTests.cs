using Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

namespace UITests.Features.Video.RemoveBurnedIn;

public class BurnedInMaskBuilderTests
{
    private const int W = 120;
    private const int H = 80;

    // Gray frame (luma == R == G == B) with a mid-gray background.
    private static byte[] Frame(byte background = 100) => Enumerable.Repeat(background, W * H).ToArray();

    // "Subtitle": a white bar with a black 2 px outline.
    private static void DrawText(byte[] luma, int x0, int y0, int w, int h)
    {
        for (var y = y0 - 2; y < y0 + h + 2; y++)
        {
            for (var x = x0 - 2; x < x0 + w + 2; x++)
            {
                var inside = x >= x0 && x < x0 + w && y >= y0 && y < y0 + h;
                luma[y * W + x] = inside ? (byte)240 : (byte)10;
            }
        }
    }

    private static void FillRect(byte[] luma, int x0, int y0, int w, int h, byte value)
    {
        for (var y = y0; y < y0 + h; y++)
        {
            for (var x = x0; x < x0 + w; x++)
            {
                luma[y * W + x] = value;
            }
        }
    }

    private static byte[] ToRgb(byte[] luma) => luma.SelectMany(v => new[] { v, v, v }).ToArray();

    private static BurnedInMaskBuilder Build(bool requireOutline, Func<int, byte[]> frame, int frames = 30)
    {
        var builder = new BurnedInMaskBuilder(W, H, 0, 40, W, 40, new[] { (0L, (long)frames - 1) }, 190, requireOutline, 2);
        for (var i = 0; i < frames; i++)
        {
            var luma = frame(i);
            var region = new byte[W * 40];
            Array.Copy(luma, 40 * W, region, 0, region.Length);
            builder.AddRegionFrame(i, region);
        }

        builder.Finish();
        return builder;
    }

    private static byte[] TextFrame(int i)
    {
        var luma = Frame();
        DrawText(luma, 30, 55, 60, 8);
        FillRect(luma, 2 + i * 3, 45, 6, 6, 250); // bright object moving through the scan area
        return luma;
    }

    [Fact]
    public void MaskCoversStaticTextAndOutline()
    {
        var builder = Build(true, TextFrame);
        var mask = new byte[W * H];
        Assert.True(builder.GetFrameMask(10, ToRgb(TextFrame(10)), mask));
        Assert.Equal(255, mask[59 * W + 60]); // text
        Assert.Equal(255, mask[54 * W + 60]); // outline (1 px above the text)
        Assert.Equal(255, mask[64 * W + 28]); // grown corner (text ends at y 62 / x 30, grow 2)
        Assert.Equal(0, mask[70 * W + 20]); // background outside text + outline + grow
        Assert.Equal(0, mask[70 * W + 60]); // background below
        Assert.Equal(0, mask[20 * W + 60]); // outside the scan area
    }

    [Fact]
    public void MovingBrightObjectIsNotMasked()
    {
        var builder = Build(false, TextFrame);
        var mask = new byte[W * H];
        builder.GetFrameMask(10, ToRgb(TextFrame(10)), mask);
        Assert.Equal(0, mask[47 * W + 34]); // where the moving object is in frame 10
    }

    [Fact]
    public void StaticBrightBackgroundNeedsOutline()
    {
        byte[] F(int i)
        {
            var luma = TextFrame(i);
            FillRect(luma, 100, 42, 15, 10, 230); // white wall patch, no outline
            return luma;
        }

        var mask = new byte[W * H];
        Build(false, F).GetFrameMask(5, ToRgb(F(5)), mask);
        Assert.Equal(255, mask[46 * W + 107]);

        Array.Clear(mask);
        Build(true, F).GetFrameMask(5, ToRgb(F(5)), mask);
        Assert.Equal(0, mask[46 * W + 107]);
        Assert.Equal(255, mask[59 * W + 60]);
    }

    [Fact]
    public void FrameWithoutTextGetsNoMask()
    {
        // Padded span: the text is only on screen in frames 5..29.
        byte[] F(int i)
        {
            var luma = Frame();
            if (i >= 5)
            {
                DrawText(luma, 30, 55, 60, 8);
            }

            return luma;
        }

        var builder = Build(true, F);
        var mask = new byte[W * H];
        Assert.False(builder.GetFrameMask(2, ToRgb(F(2)), mask));
        Assert.All(mask, v => Assert.Equal(0, v));
        Assert.True(builder.GetFrameMask(20, ToRgb(F(20)), mask));
        Assert.False(builder.GetFrameMask(40, ToRgb(F(20)), mask)); // outside every span
    }

    [Fact]
    public void NoTextGivesNoLines()
    {
        var builder = Build(true, _ => Frame());
        Assert.Equal(0, builder.LinesWithText);
        Assert.False(builder.GetFrameMask(3, ToRgb(Frame()), new byte[W * H]));
    }

    [Fact]
    public void ThickOutlineIsCovered()
    {
        // 5 px black box around the text; grow is only 2, the outline growth takes the rest.
        byte[] F(int i)
        {
            var luma = Frame();
            FillRect(luma, 25, 50, 70, 18, 10);
            FillRect(luma, 30, 55, 60, 8, 240);
            return luma;
        }

        var mask = new byte[W * H];
        Assert.True(Build(true, F).GetFrameMask(5, ToRgb(F(5)), mask));
        Assert.Equal(255, mask[50 * W + 25]); // outer corner of the box
        Assert.Equal(255, mask[67 * W + 94]);
        Assert.Equal(0, mask[77 * W + 60]); // background well below
    }

    [Fact]
    public void FrameMaskTakesThisFramesDarkShadow()
    {
        // The line mask is built over 30 frames; frame 20 also has a dark shadow band right
        // under the text that the line average does not see. Left on the rim, the models
        // would fill the hole dark - the per-frame growth must take it.
        var builder = Build(true, TextFrame);
        var luma = TextFrame(20);
        FillRect(luma, 30, 65, 60, 2, 20); // directly under the outline (outline ends at y 64)
        var mask = new byte[W * H];
        Assert.True(builder.GetFrameMask(20, ToRgb(luma), mask));
        Assert.Equal(255, mask[66 * W + 60]);
        Assert.Equal(255, mask[67 * W + 60]); // + 1 px rim
        Assert.Equal(0, mask[75 * W + 60]);
    }

    [Fact]
    public void ShortLineInLongPaddingIsFound()
    {
        // 40 frames incl. 15 frames of padding on each side: the text is only on screen for the
        // 10 frames of the line itself - still found (stability counts the line's own frames).
        byte[] F(int i)
        {
            var luma = Frame();
            if (i >= 15 && i < 25)
            {
                DrawText(luma, 30, 55, 60, 8);
            }

            return luma;
        }

        var builder = new BurnedInMaskBuilder(W, H, 0, 40, W, 40, new[] { (0L, 39L) }, 190, true, 2, padFrames: 15);
        for (var i = 0; i < 40; i++)
        {
            var region = new byte[W * 40];
            Array.Copy(F(i), 40 * W, region, 0, region.Length);
            builder.AddRegionFrame(i, region);
        }

        builder.Finish();
        Assert.Equal(1, builder.LinesWithText);
        var mask = new byte[W * H];
        Assert.True(builder.GetFrameMask(20, ToRgb(F(20)), mask));
        Assert.Equal(255, mask[59 * W + 60]);
    }

    [Fact]
    public void SemiTransparentShadowOnBrightBackgroundIsCovered()
    {
        // Bright background (220), shadow band at 120: above the absolute dark threshold, but
        // well below 65% of the background - must be grown into.
        byte[] F(int i)
        {
            var luma = Frame(220);
            DrawText(luma, 30, 55, 60, 8);
            return luma;
        }

        var builder = new BurnedInMaskBuilder(W, H, 0, 40, W, 40, new[] { (0L, 29L) }, 230, true, 2);
        for (var i = 0; i < 30; i++)
        {
            var region = new byte[W * 40];
            Array.Copy(F(i), 40 * W, region, 0, region.Length);
            builder.AddRegionFrame(i, region);
        }

        builder.Finish();
        var luma = F(5);
        FillRect(luma, 30, 65, 62, 3, 120);
        var mask = new byte[W * H];
        Assert.True(builder.GetFrameMask(5, ToRgb(luma), mask));
        Assert.Equal(255, mask[67 * W + 60]);
        Assert.Equal(0, mask[76 * W + 60]);
    }

    [Fact]
    public void MaskFollowsTheTextBeyondInexactOcrTiming()
    {
        // OCR says frames 0..29, the text is still on screen at frame 45: with a 30-frame apply
        // margin the mask follows it there (text visible), but not to a frame without text.
        var builder = new BurnedInMaskBuilder(W, H, 0, 40, W, 40, new[] { (0L, 29L) }, 190, true, 2, applyMarginFrames: 30);
        for (var i = 0; i < 30; i++)
        {
            var region = new byte[W * 40];
            Array.Copy(TextFrame(i), 40 * W, region, 0, region.Length);
            builder.AddRegionFrame(i, region);
        }

        builder.Finish();
        var mask = new byte[W * H];
        Assert.True(builder.GetFrameMask(45, ToRgb(TextFrame(10)), mask));
        Assert.Equal(255, mask[59 * W + 60]);
        Assert.False(builder.GetFrameMask(45, ToRgb(Frame()), new byte[W * H]));
        Assert.False(builder.GetFrameMask(70, ToRgb(TextFrame(10)), new byte[W * H]));
    }

    [Fact]
    public void RegionIsSnappedToEvenValues()
    {
        // 346 * 33.2% = 115 rows: ffmpeg crops 4:2:0 video to 114, so the builder must too.
        var builder = new BurnedInMaskBuilder(640, 346, 1, 231, 639, 115, new[] { (0L, 10L) }, 190, true, 2);
        Assert.Equal(0, builder.RegionX);
        Assert.Equal(230, builder.RegionY);
        Assert.Equal(638, builder.RegionWidth);
        Assert.Equal(114, builder.RegionHeight);
    }

    [Fact]
    public void SpansArePaddedBySampleRate()
    {
        var spans = BurnedInSubtitleRemover.GetSpans(new BurnedInRemovalOptions
        {
            FrameRate = 25,
            OcrFramesPerSecond = 5,
            Lines = new List<(TimeSpan, TimeSpan)> { (TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)) },
        });
        Assert.Equal((43L, 107L), spans[0]); // 50 - (5 + 2), 100 + (5 + 2)
    }
}
