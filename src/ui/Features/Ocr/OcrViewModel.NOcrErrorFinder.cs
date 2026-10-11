using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// "Find errors in nOCR database" (nOCR settings, right-click): checks the current nOCR
/// database against other OCR engines on the loaded subtitle images.
/// </summary>
public partial class OcrViewModel
{
    private async Task FindNOcrDbErrors()
    {
        if (Window == null || IsOcrRunning || !InitNOcrDb() || _nOcrDb == null || OcrSubtitleItems.Count == 0)
        {
            return;
        }

        var db = _nOcrDb;
        await _windowService.ShowDialogAsync<NOcrErrorFinderWindow, NOcrErrorFinderViewModel>(Window, vm =>
        {
            vm.Initialize(db, OcrSubtitleItems.ToList(), GetOcrCompareEngines(), ReadNOcrGlyphs,
                (owner, item) => InspectLineNOcr(item, owner));
        });
        _isCtrlDown = false;
    }

    /// <summary>The installed engines nOCR can be compared with, using the languages picked for them in this window.</summary>
    private List<OcrCompareEngine> GetOcrCompareEngines()
    {
        var engines = new List<OcrCompareEngine>();

        if (OperatingSystem.IsMacOS() && AppleVisionOcr.IsAvailable())
        {
            var languageCode = SelectedAppleVisionLanguage?.Code ?? string.Empty;
            engines.Add(new OcrCompareEngine(AppleVisionOcr.StaticName, (images, cancellationToken) =>
            {
                var result = new Dictionary<int, string>();
                foreach (var (index, bitmap) in images)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    result[index] = AppleVisionOcr.Ocr(bitmap, languageCode, fast: false, cancellationToken);
                }

                return Task.FromResult(result);
            }));
        }

        var tesseractLanguage = SelectedTesseractDictionaryItem ?? TesseractDictionaryItems.FirstOrDefault();
        if (tesseractLanguage != null && File.Exists(TesseractOcr.GetExecutablePath()))
        {
            var language = tesseractLanguage.Code;
            var engineMode = SelectedTesseractEngineMode?.Oem ?? 3;
            engines.Add(new OcrCompareEngine("Tesseract", async (images, cancellationToken) =>
            {
                var tesseractOcr = new TesseractOcr();
                var result = new Dictionary<int, string>();
                foreach (var (index, bitmap) in images)
                {
                    var text = await tesseractOcr.Ocr(bitmap, language, Se.TesseractModelFolder, cancellationToken, engineMode);
                    if (!string.IsNullOrEmpty(text) || string.IsNullOrEmpty(tesseractOcr.Error))
                    {
                        result[index] = text;
                    }
                }

                return result;
            }));
        }

        var hasPaddleStandalone = File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")) ||
                                  File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin"));
        if (hasPaddleStandalone && Directory.Exists(Se.PaddleOcrModelsFolder))
        {
            var language = SelectedPaddleOcrLanguage?.Code ?? "en";
            var mode = Se.Settings.Ocr.PaddleOcrMode;
            engines.Add(new OcrCompareEngine("Paddle OCR", async (images, cancellationToken) =>
            {
                var paddleOcr = new PaddleOcr();
                var texts = new ConcurrentDictionary<int, string>();
                var input = images.Select(p => new PaddleOcrBatchInput { Index = p.Index, Bitmap = p.Bitmap }).ToList();
                await paddleOcr.OcrBatch(OcrEngineType.PaddleOcrStandalone, input, language, mode,
                    new SynchronousProgress(p => texts[p.Index] = p.Text), cancellationToken);
                return new Dictionary<int, string>(texts);
            }));
        }

        return engines;
    }

    /// <summary>Reads one image like "Inspect line" does, keeping the database entry behind each glyph.</summary>
    private List<NOcrGlyphRead> ReadNOcrGlyphs(OcrSubtitleItem item)
    {
        var parentBitmap = new NikseBitmap2(item.GetSkBitmap());
        parentBitmap.MakeTwoColor(OcrTwoColorThreshold.Get(parentBitmap));
        parentBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters = SplitNOcrLetters(parentBitmap, out _);
        _lineHeightTracker.Update(letters);

        var glyphs = new List<NOcrGlyphRead>();
        var index = 0;
        while (index < letters.Count)
        {
            var splitterItem = letters[index];
            if (splitterItem.NikseBitmap == null)
            {
                glyphs.Add(new NOcrGlyphRead { Text = splitterItem.SpecialCharacter ?? string.Empty });
                index++;
                continue;
            }

            // No italic factor: a straightened italic match is a copy, not the database entry.
            var match = _nOcrDb!.GetMatch(parentBitmap, letters, splitterItem, splitterItem.Top, true, SelectedNOcrMaxWrongPixels);
            glyphs.Add(match == null
                ? new NOcrGlyphRead { Text = "*" }
                : new NOcrGlyphRead { Entry = match, Text = match.Text, Bitmap = splitterItem.NikseBitmap });
            index += match is { ExpandCount: > 1 } ? match.ExpandCount : 1;
        }

        return glyphs;
    }

    /// <summary>Progress&lt;T&gt; posts to the thread pool, so reports could land after the batch returned.</summary>
    private sealed class SynchronousProgress(Action<PaddleOcrBatchProgress> report) : IProgress<PaddleOcrBatchProgress>
    {
        public void Report(PaddleOcrBatchProgress value) => report(value);
    }
}
