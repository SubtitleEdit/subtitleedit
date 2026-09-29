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
        // The image profile prohibits embedded images (IMSC 1.1 section 9.4.5): each cue
        // references a png file next to the document.
        Assert.DoesNotContain("smpte:image", xml);
        Assert.Contains("smpte:backgroundImage=\"out_0001.png\"", xml);
        Assert.Contains("smpte:backgroundImage=\"out_0002.png\"", xml);
        Assert.Contains("region=\"region0\"", xml);
        Assert.Contains("begin=\"00:00:01.240\"", xml);
        Assert.Contains("end=\"00:00:06.500\"", xml);
        Assert.DoesNotContain("forcedDisplay", xml);
        // valid XML
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xml);
        Assert.NotNull(doc.DocumentElement);
    }

    [Fact]
    public void WritesOnePngPerCueNextToTheDocument()
    {
        Export(Cue(0, "Decode me", 0, 2000, 200, 60), Cue(1, "Me too", 3000, 4000, 220, 70));

        using var first = SKBitmap.Decode(Path.Combine(_dir, "out_0001.png"));
        Assert.NotNull(first);
        Assert.Equal(200, first.Width);
        Assert.Equal(60, first.Height);
        using var second = SKBitmap.Decode(Path.Combine(_dir, "out_0002.png"));
        Assert.Equal(220, second.Width);
    }

    [Fact]
    public void ForcedCue_IsMarkedForcedDisplay()
    {
        var forced = Cue(0, "Forced", 1000, 2000);
        forced.IsForced = true;

        var xml = Export(forced, Cue(1, "Normal", 3000, 4000));

        Assert.Contains("xmlns:itts=\"http://www.w3.org/ns/ttml/profile/imsc1#styling\"", xml);
        Assert.Contains("smpte:backgroundImage=\"out_0001.png\" region=\"region0\" begin=\"00:00:01.000\" end=\"00:00:02.000\" itts:forcedDisplay=\"true\"", xml);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(xml, "forcedDisplay"));
    }

    [Fact]
    public void RoundTripsThroughTimedTextImageReader()
    {
        var xml = Export(
            Cue(0, "First line", 1240, 3120),
            Cue(1, "Second line", 4000, 6500));

        // SE's Timed Text Image reader must accept our output and recover the images and timings.
        var fileName = Path.Combine(_dir, "out.ttml");
        var format = new TimedTextImage();
        Assert.True(format.IsMine(xml.SplitToLines(), fileName));

        var sub = new Subtitle();
        format.LoadSubtitle(sub, xml.SplitToLines(), fileName);

        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal("out_0001.png", sub.Paragraphs[0].Text);
        Assert.Equal(1240, sub.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3120, sub.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4000, sub.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6500, sub.Paragraphs[1].EndTime.TotalMilliseconds);

        // Not mistaken for the embedded-image variant.
        Assert.False(new TimedTextBase64Image().IsMine(xml.SplitToLines(), fileName));
    }
}
