using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class ExportHandlerBdnXmlTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "bdnxml_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerBdnXmlTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private static ImageParameter Cue(int index, string text, int startMs, int endMs)
    {
        var bitmap = new SKBitmap(300, 80);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var font = new SKFont(SKTypeface.Default, 24);
            canvas.DrawText(text, 4, 40, font, paint);
        }

        return new ImageParameter
        {
            Text = text,
            Bitmap = bitmap,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs),
            Index = index,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            Alignment = ExportAlignment.BottomCenter,
            BottomTopMargin = 50,
            FramesPerSecond = 25,
        };
    }

    private string Export(bool use8BitPng)
    {
        var folder = Path.Combine(_dir, use8BitPng ? "8bit" : "32bit");
        var handler = new ExportHandlerBdnXml(use8BitPng);
        var cues = new[] { Cue(0, "Hello", 1000, 3000), Cue(1, "World", 4000, 6000) };
        handler.WriteHeader(folder, cues[0]);
        foreach (var cue in cues)
        {
            handler.WriteParagraph(cue);
        }

        handler.WriteFooter();
        return folder;
    }

    private static int ColorType(string pngFileName)
    {
        var bytes = File.ReadAllBytes(pngFileName);
        return bytes[25]; // signature (8) + length/type (8) + width/height (8) + bit depth (1)
    }

    [Fact]
    public void EightBitExportWritesPaletteIndexedPngs()
    {
        var folder = Export(true);

        Assert.True(File.Exists(Path.Combine(folder, "index.xml")));
        foreach (var image in new[] { "0001.png", "0002.png" })
        {
            var fileName = Path.Combine(folder, image);
            Assert.True(File.Exists(fileName), image + " missing");
            Assert.Equal(3, ColorType(fileName)); // 3 = palette
        }
    }

    [Fact]
    public void DefaultExportKeepsThirtyTwoBitPngs()
    {
        var folder = Export(false);

        Assert.Equal(6, ColorType(Path.Combine(folder, "0001.png"))); // 6 = truecolor with alpha
    }

    [Fact]
    public void BothVariantsWriteTheSameIndexXml()
    {
        var eightBit = File.ReadAllText(Path.Combine(Export(true), "index.xml"));
        var thirtyTwoBit = File.ReadAllText(Path.Combine(Export(false), "index.xml"));

        Assert.Equal(thirtyTwoBit, eightBit);
        Assert.Contains("<Graphic Width=\"300\" Height=\"80\"", eightBit);
        Assert.Contains("NumberofEvents=\"2\"", eightBit);
    }
}
