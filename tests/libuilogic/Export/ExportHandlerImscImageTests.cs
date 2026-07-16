using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class ExportHandlerImscImageTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "imsc_img_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerImscImageTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private static ImageParameter Cue(int index, string text, int startMs, int endMs, int w = 300, int h = 80)
    {
        var bitmap = new SKBitmap(w, h);
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
        };
    }

    private string Export(params ImageParameter[] cues)
    {
        var handler = new ExportHandlerImscImage();
        var file = Path.Combine(_dir, "out.ttml");
        handler.WriteHeader(file, cues[0]);
        foreach (var c in cues)
        {
            handler.WriteParagraph(c);
        }

        handler.WriteFooter();
        return File.ReadAllText(file);
    }

    [Fact]
    public void ProducesConformantImscImageProfileShape()
    {
        var xml = Export(
            Cue(0, "First line", 1240, 3120),
            Cue(1, "Second line", 4000, 6500));

        Assert.Contains("ttp:profile=\"http://www.w3.org/ns/ttml/profile/imsc1/image\"", xml);
        Assert.Contains("ttp:timeBase=\"media\"", xml);
        Assert.Contains("<smpte:image xml:id=\"img0\" imagetype=\"PNG\" encoding=\"Base64\">", xml);
        Assert.Contains("<smpte:image xml:id=\"img1\" imagetype=\"PNG\" encoding=\"Base64\">", xml);
        Assert.Contains("smpte:backgroundImage=\"#img0\"", xml);
        Assert.Contains("region=\"region0\"", xml);
        Assert.Contains("begin=\"00:00:01.240\"", xml);
        Assert.Contains("end=\"00:00:06.500\"", xml);
        // Base64 PNG payload present (PNG magic "iVBOR..." is the base64 of \x89PNG)
        Assert.Contains("iVBOR", xml);
        // valid XML
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xml);
        Assert.NotNull(doc.DocumentElement);
    }

    [Fact]
    public void RoundTripsThroughBase64ImageReader()
    {
        var xml = Export(
            Cue(0, "First line", 1240, 3120),
            Cue(1, "Second line", 4000, 6500));

        // SE's Timed Text Base64 Image reader must accept our output and recover the cue timings.
        var format = new TimedTextBase64Image();
        Assert.True(format.IsMine(xml.SplitToLines(), "out.ttml"));

        var sub = new Subtitle();
        format.LoadSubtitle(sub, xml.SplitToLines(), "out.ttml");

        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(1240, sub.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3120, sub.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4000, sub.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6500, sub.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void EmbeddedImagesAreDecodablePng()
    {
        var xml = Export(Cue(0, "Decode me", 0, 2000, 200, 60));

        var start = xml.IndexOf("Base64\">", StringComparison.Ordinal) + "Base64\">".Length;
        var end = xml.IndexOf("</smpte:image>", start, StringComparison.Ordinal);
        var base64 = xml.Substring(start, end - start);

        var bytes = Convert.FromBase64String(base64);
        using var decoded = SKBitmap.Decode(bytes);
        Assert.NotNull(decoded);
        Assert.Equal(200, decoded.Width);
        Assert.Equal(60, decoded.Height);
    }
}
