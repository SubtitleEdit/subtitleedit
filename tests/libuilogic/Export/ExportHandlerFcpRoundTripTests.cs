using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// The "Final Cut Pro + image" export writes an xmeml whose clipitems reference the
/// exported pngs - FinalCutProImage is the matching importer (the main window routes
/// image-list xml files to the BDN OCR import through it).
/// </summary>
public class ExportHandlerFcpRoundTripTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "fcp_img_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerFcpRoundTripTests() => Directory.CreateDirectory(_dir);

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
            FramesPerSecond = 25.0,
            Alignment = ExportAlignment.BottomCenter,
            BottomTopMargin = 50,
        };
    }

    [Fact]
    public void ExportedXmemlRoundTripsThroughFinalCutProImage()
    {
        var savedRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var cues = new[]
            {
                Cue(0, "First image cue", 1000, 3000),
                Cue(1, "Second image cue", 4000, 6500),
            };

            var handler = new ExportHandlerFcp();
            handler.WriteHeader(_dir, cues[0]);
            foreach (var c in cues)
            {
                handler.WriteParagraph(c);
            }

            handler.WriteFooter();

            var xmlFile = Path.Combine(_dir, "fcpxml_export.xml");
            Assert.True(File.Exists(xmlFile));

            var format = new FinalCutProImage();
            var lines = File.ReadAllLines(xmlFile).ToList();
            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, lines, xmlFile);

            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1.0);
            Assert.Equal(3000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 1.0);
            Assert.Equal(4000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds, 1.0);

            // Each cue references its exported png - resolvable next to the xml, which is
            // how OcrSubtitleBdn loads the bitmaps for OCR (Extra first, file name fallback).
            foreach (var p in subtitle.Paragraphs)
            {
                var byExtra = Path.Combine(_dir, (p.Extra ?? string.Empty).Replace("file://", string.Empty));
                var byName = Path.Combine(_dir, p.Text);
                Assert.True(File.Exists(byExtra) || File.Exists(byName), $"image not found for cue: {p.Text} / {p.Extra}");
            }
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = savedRate;
        }
    }
}
