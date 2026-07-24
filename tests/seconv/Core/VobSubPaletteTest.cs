using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SeConv.Core;
using SkiaSharp;
using Xunit;
using Paragraph = Nikse.SubtitleEdit.Core.Common.Paragraph;

namespace SeConvTests.Core;

/// <summary>
/// Issue #12772: seconv decoded VobSub subpictures without the .idx palette (CLUT).
/// With a null CLUT, <see cref="SubPicture.GetBitmap"/> skips both the SetColor and the
/// SetContrast (per-plane alpha) display control commands, so normally-transparent
/// outline/anti-alias planes render opaque and fuse into the glyphs — garbling OCR
/// ('-' read as '=', 'S' as '$'). These tests lock down that the palette from the .idx
/// (or a Matroska track's CodecPrivate) actually reaches the decoder, and that a '.idx'
/// file is accepted as input again (a 5.0.0 regression).
/// </summary>
public class VobSubPaletteTest : IDisposable
{
    private readonly string _tempRoot;

    public VobSubPaletteTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "VobPal_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    /// <summary>
    /// Writes a one-cue VobSub .sub/.idx pair whose pattern (glyph fill) colour is
    /// <paramref name="pattern"/>. The .idx palette line puts that colour at CLUT
    /// index 1, which is where the subpicture's SetColor command points the pattern.
    /// </summary>
    private string WritePair(SKColor pattern)
    {
        var subPath = Path.Combine(_tempRoot, "cue.sub");
        using (var writer = new VobSubWriter(
                   subPath, 720, 576, bottomMargin: 10, leftRightMargin: 10, languageStreamId: 32,
                   pattern, SKColors.Black, useInnerAntiAliasing: false, DvdSubtitleLanguage.English))
        {
            using var bmp = new SKBitmap(new SKImageInfo(120, 40, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            bmp.Erase(SKColors.Transparent);
            using (var canvas = new SKCanvas(bmp))
            using (var paint = new SKPaint { Color = pattern })
            {
                canvas.DrawRect(new SKRect(20, 10, 100, 30), paint);
            }

            var p = new Paragraph(string.Empty, 1000, 3000);
            writer.WriteParagraph(p, bmp, BluRayContentAlignment.BottomCenter);
            writer.WriteIdxFile();
        }

        return subPath;
    }

    private static bool ContainsColor(SKBitmap bmp, SKColor color)
    {
        for (var y = 0; y < bmp.Height; y++)
        {
            for (var x = 0; x < bmp.Width; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.Alpha > 200 && c.Red == color.Red && c.Green == color.Green && c.Blue == color.Blue)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [Fact]
    public void LoadVobSub_AppliesIdxPalette_ToDecodedBitmaps()
    {
        // Write a cue whose fill is pure red — a colour that only exists in the .idx
        // CLUT. Without the palette the decoder falls back to black/white defaults,
        // so a red pixel in the decoded bitmap proves the CLUT was applied.
        var red = new SKColor(255, 0, 0);
        var subPath = WritePair(red);
        var idxPath = Path.ChangeExtension(subPath, ".idx");
        Assert.True(File.Exists(idxPath));

        var items = BitmapSubtitleLoader.LoadVobSub(subPath, idxPath, isPal: true);

        Assert.Single(items);
        Assert.True(ContainsColor(items[0].Bitmap, red),
            "decoded bitmap has no pixel in the .idx palette's pattern colour — the CLUT was not applied");
        foreach (var item in items)
        {
            item.Dispose();
        }
    }

    [Fact]
    public void GetVobSubIdxPalette_ParsesCodecPrivate_AndRejectsBlank()
    {
        var codecPrivate = "size: 720x576\n"
            + "palette: 000000, ff0000, 00ff00, 0000ff, 828282, 828282, 828282, ffffff, "
            + "828282, bababa, 828282, 828282, 828282, 828282, 828282, 828282";

        var palette = BitmapSubtitleLoader.GetVobSubIdxPalette(codecPrivate);

        Assert.NotNull(palette);
        Assert.Equal(16, palette.Count);
        Assert.Equal(new SKColor(255, 0, 0), palette[1]);
        Assert.Equal(new SKColor(0, 255, 0), palette[2]);

        Assert.Null(BitmapSubtitleLoader.GetVobSubIdxPalette(null));
        Assert.Null(BitmapSubtitleLoader.GetVobSubIdxPalette(string.Empty));
        Assert.Null(BitmapSubtitleLoader.GetVobSubIdxPalette("size: 720x576"));
    }

    [Fact]
    public async Task ConvertAsync_IdxInput_RoutesToVobSubPair()
    {
        // 5.0.0 accepted the .idx of a pair as the input; rc13 rejected it with
        // "Unable to determine subtitle format". Image → image (VobSub → Blu-ray SUP)
        // exercises the routing without needing an OCR engine.
        var subPath = WritePair(new SKColor(255, 0, 0));
        var idxPath = Path.ChangeExtension(subPath, ".idx");
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [idxPath],
            Format = "Bluraysup",
            OutputFolder = outputFolder,
            Overwrite = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Single(Directory.GetFiles(outputFolder, "*.sup"));
    }

    [Fact]
    public async Task ConvertAsync_IdxInput_WithoutSub_ErrorsWithGuidance()
    {
        var idxPath = Path.Combine(_tempRoot, "orphan.idx");
        await File.WriteAllTextAsync(idxPath, "# VobSub index file, v7 (do not modify this line!)",
            TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [idxPath],
            Format = "Bluraysup",
            Overwrite = true,
        });

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("companion '.sub'"));
    }

    [Fact]
    public void LoadVobSub_ZeroDurationCue_GetsClampedEndTime()
    {
        // Issue #12772 part 3: many discs omit the subpicture's own delay command, so
        // EndTimeCode == StartTimeCode and the .sup export wrote start==end on 740 of
        // 1,115 cues. A zero-duration cue must be extended (default duration, capped at
        // the next cue's start).
        var subPath = WritePair(new SKColor(255, 0, 0));
        var idxPath = Path.ChangeExtension(subPath, ".idx");

        var items = BitmapSubtitleLoader.LoadVobSub(subPath, idxPath, isPal: true);

        foreach (var item in items)
        {
            Assert.True(item.EndTime.TotalMilliseconds > item.StartTime.TotalMilliseconds,
                $"cue at {item.StartTime} still has a zero/negative duration");
            item.Dispose();
        }
    }

    [Fact]
    public void GetVobSubIdxScreenSize_ParsesSizeLine_AndFallsBack()
    {
        Assert.Equal((720, 480), BitmapSubtitleLoader.GetVobSubIdxScreenSize("size: 720x480\npalette: 000000"));
        Assert.Equal((1920, 1080), BitmapSubtitleLoader.GetVobSubIdxScreenSize("SIZE: 1920x1080"));

        // Missing / malformed → DVD PAL default.
        Assert.Equal((720, 576), BitmapSubtitleLoader.GetVobSubIdxScreenSize(null));
        Assert.Equal((720, 576), BitmapSubtitleLoader.GetVobSubIdxScreenSize("palette: 000000"));
        Assert.Equal((720, 576), BitmapSubtitleLoader.GetVobSubIdxScreenSize("size: banana"));
    }
}
