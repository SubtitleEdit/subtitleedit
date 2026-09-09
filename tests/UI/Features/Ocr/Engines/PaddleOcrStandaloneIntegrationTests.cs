using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;

namespace UITests.Features.Ocr.Engines;

/// <summary>
/// End-to-end run against the real bundled PaddleOCR standalone binary: renders subtitle-like
/// bitmaps, OCRs them through <see cref="PaddleOcr.OcrBatch"/>, and checks every line comes
/// back against its own index.
/// <para>
/// Skipped unless the engine and models are actually installed - they are a ~450 MB download,
/// so CI and a fresh clone just skip. Install them from Subtitle Edit (OCR window > download
/// Paddle OCR) or unpack the release archives into <c>Se.PaddleOcrFolder</c>.
/// </para>
/// </summary>
public class PaddleOcrStandaloneIntegrationTests
{
    private static readonly string[] Lines =
    {
        "My mommy always said",
        "there were no monsters",
        "No real ones",
        "but there are.",
    };

    [Fact]
    public async Task OcrBatch_Standalone_ReportsEveryLineAgainstItsOwnIndex()
    {
        SkipIfEngineMissing();

        // A blank image among the real ones: PaddleOCR still writes a result file for it, which
        // is what keeps the poll loop from idling out waiting on a line that has no text.
        var inputs = new List<PaddleOcrBatchInput>();
        for (var i = 0; i < Lines.Length; i++)
        {
            inputs.Add(new PaddleOcrBatchInput { Index = i, Bitmap = RenderLine(Lines[i]) });
        }

        inputs.Add(new PaddleOcrBatchInput { Index = Lines.Length, Bitmap = RenderLine(string.Empty) });

        var reported = new List<PaddleOcrBatchProgress>();
        var progress = new SyncProgress<PaddleOcrBatchProgress>(p =>
        {
            lock (reported)
            {
                reported.Add(p);
            }
        });

        var ocr = new PaddleOcr();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await ocr.OcrBatch(OcrEngineType.PaddleOcrStandalone, inputs, "en",
            Se.Settings.Ocr.PaddleOcrMode, progress, cts.Token);

        Assert.Equal(string.Empty, ocr.Error);

        // Every image reported exactly once, and the text landed on the index it belongs to -
        // the engine's worker pool does not finish images in order, so this is the property
        // that matters, not the order the reports arrived in.
        Assert.Equal(inputs.Count, reported.Count);
        Assert.Equal(inputs.Select(i => i.Index).OrderBy(i => i), reported.Select(r => r.Index).OrderBy(i => i));

        foreach (var (expected, index) in Lines.Select((text, i) => (text, i)))
        {
            var result = reported.Single(r => r.Index == index);
            Assert.Equal(expected, result.Text.Trim());
            Assert.True(result.Confidence > 0.5, $"line {index} confidence was {result.Confidence}");
        }

        Assert.Equal(string.Empty, reported.Single(r => r.Index == Lines.Length).Text.Trim());
    }

    [Fact]
    public async Task OcrBatch_Standalone_TextFreeImageStillReportsAndDoesNotError()
    {
        SkipIfEngineMissing();

        // A 1x1 transparent image gives the detector nothing to find. PaddleOCR still writes a
        // result file for it, so the run comes back reported-and-clean rather than idling out in
        // the poll loop and then failing as "no results".
        var inputs = new List<PaddleOcrBatchInput>
        {
            new() { Index = 0, Bitmap = new SKBitmap(1, 1, true) },
        };

        var reported = new List<PaddleOcrBatchProgress>();
        var ocr = new PaddleOcr();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await ocr.OcrBatch(OcrEngineType.PaddleOcrStandalone, inputs, "en",
            Se.Settings.Ocr.PaddleOcrMode, new SyncProgress<PaddleOcrBatchProgress>(reported.Add), cts.Token);

        Assert.Single(reported);
        Assert.Equal(string.Empty, reported[0].Text);
        Assert.Equal(string.Empty, ocr.Error);
    }

    /// <summary>
    /// The contract batch convert's RunPaddleOcr is built on, over a real Blu-ray SUP: every
    /// input index comes back exactly once, so pre-creating a cue per image and filling it by
    /// index cannot leave a gap or land text on the wrong line.
    /// </summary>
    [Fact]
    public async Task OcrBatch_Standalone_RealBluRaySup_ReportsEveryIndexExactlyOnce()
    {
        SkipIfEngineMissing();

        var supFile = Path.Combine(AppContext.BaseDirectory, "Files", "sample_BDSUP_multi_image.sup");
        if (!File.Exists(supFile))
        {
            Assert.Skip($"Sample Blu-ray SUP not found at {supFile}");
        }

        var pcsData = BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder());
        var imageSubtitle = new OcrSubtitleBluRay(pcsData);
        Assert.True(imageSubtitle.Count > 1, "sample should hold more than one image");

        // Exactly how BatchConverter.RunPaddleOcr builds its batch.
        var batchImages = new List<PaddleOcrBatchInput>(imageSubtitle.Count);
        for (var i = 0; i < imageSubtitle.Count; i++)
        {
            batchImages.Add(new PaddleOcrBatchInput { Index = i, Bitmap = imageSubtitle.GetBitmap(i) });
        }

        var reported = new List<PaddleOcrBatchProgress>();
        var ocr = new PaddleOcr();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await ocr.OcrBatch(OcrEngineType.PaddleOcrStandalone, batchImages, "en",
            Se.Settings.Ocr.PaddleOcrMode,
            new SyncProgress<PaddleOcrBatchProgress>(p =>
            {
                lock (reported)
                {
                    reported.Add(p);
                }
            }),
            cts.Token);

        Assert.Equal(string.Empty, ocr.Error);
        Assert.Equal(imageSubtitle.Count, reported.Count);
        Assert.Equal(Enumerable.Range(0, imageSubtitle.Count), reported.Select(r => r.Index).OrderBy(i => i));

        // Reported in line order, and a real Blu-ray subtitle is not blank.
        Assert.Equal(reported.Select(r => r.Index), reported.Select(r => r.Index).OrderBy(i => i));
        Assert.Contains(reported, r => !string.IsNullOrWhiteSpace(r.Text));
    }

    private static void SkipIfEngineMissing()
    {
        var exe = Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe");
        var bin = Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin");
        if (!File.Exists(exe) && !File.Exists(bin))
        {
            Assert.Skip($"PaddleOCR standalone engine not installed in {Se.PaddleOcrFolder}");
        }

        if (!Directory.Exists(Se.PaddleOcrModelsFolder))
        {
            Assert.Skip($"PaddleOCR models not installed in {Se.PaddleOcrModelsFolder}");
        }
    }

    /// <summary>
    /// White text on black, the way a decoded VobSub/Blu-ray subtitle bitmap looks. Rendered
    /// generously: OcrBatch pads every image with a 40 px border before handing it to Paddle,
    /// and the small detection model starts missing lines when the text is small next to that.
    /// </summary>
    private static SKBitmap RenderLine(string text)
    {
        var bitmap = new SKBitmap(1280, 140);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Black);

        if (!string.IsNullOrEmpty(text))
        {
            using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                                 ?? SKTypeface.Default;
            using var font = new SKFont(typeface, 52);
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            canvas.DrawText(text, 40, 95, font, paint);
        }

        canvas.Flush();
        return bitmap;
    }

    private sealed class SyncProgress<T> : IProgress<T>
    {
        private readonly Action<T> _onReport;

        public SyncProgress(Action<T> onReport) => _onReport = onReport;

        public void Report(T value) => _onReport(value);
    }
}
