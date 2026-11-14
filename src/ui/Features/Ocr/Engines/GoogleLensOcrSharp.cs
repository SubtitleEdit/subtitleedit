using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

public class GoogleLensOcrSharp
{
    /// <summary>
    /// Number of tries per image before giving up on it and moving on to the next one.
    /// </summary>
    private const int MaxAttemptsPerImage = 3;

    private IProgress<PaddleOcrBatchProgress>? _batchProgress;
    private ILens _lens;

    /// <summary>
    /// Images that could not be read even after retrying - they were skipped, not OCR'ed.
    /// </summary>
    public int SkippedImageCount { get; private set; }

    /// <summary>
    /// How long to wait before the next try. Overridable so tests do not have to sleep.
    /// </summary>
    internal Func<Exception, int, TimeSpan> RetryDelay { get; set; } = GetRetryDelay;

    public GoogleLensOcrSharp(ILens lens)
    {
        _lens = lens;
    }

    private Lock _lockObject = new Lock();

    public async Task OcrBatch(List<PaddleOcrBatchInput> input, string language, IProgress<PaddleOcrBatchProgress> progress, CancellationToken cancellationToken)
    {
        _batchProgress = progress;
        SkippedImageCount = 0;

        foreach (var bmpInput in input)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (bmpInput.Bitmap == null)
            {
                continue;
            }

            var result = await ScanWithRetry(bmpInput, language, cancellationToken);
            if (result == null)
            {
                // Google Lens is a public endpoint that will now and then time out or rate limit us.
                // Letting that bubble up aborted the whole run and the user had to press "Start OCR"
                // again (losing the unknown-words list), so skip just this image and keep going (#13563).
                continue;
            }

            var lines = OcrLoneDashFixer.FixLoneDashes(result.Segments.Select(x => x.Text).ToList());

            //join "..." with line
            for (int i = 0; i < lines.Count; i++)
            {
                //it has previous line not ending with .
                if (lines[i] == "..." && i - 1 >= 0 && !lines[i-1].EndsWith("."))
                {
                    lines[i-1] = $"{lines[i - 1]} ...";
                    lines.RemoveAt(i);
                } 
                else if (lines[i] == "..." && i + 1 < lines.Count)
                {
                    lines[i] = $"... {lines[i + 1]}";
                    lines.RemoveAt(i + 1);
                }
            }

            bmpInput.Text = string.Join(Environment.NewLine, lines).Trim();
            lock (_lockObject)
            {
                var progressReport = new PaddleOcrBatchProgress
                {
                    Index = bmpInput.Index,
                    Text = bmpInput.Text,
                    Item = bmpInput.Item,
                };
                _batchProgress?.Report(progressReport);
            }
        }
    }

    /// <summary>
    /// Scans one image, retrying transient failures. Returns null when the image has to be skipped.
    /// </summary>
    private async Task<LensResult?> ScanWithRetry(PaddleOcrBatchInput bmpInput, string language, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await _lens.ScanByBitmap(bmpInput.Bitmap!, language);
            }
            catch (Exception exception)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (attempt >= MaxAttemptsPerImage || !IsTransient(exception))
                {
                    SkippedImageCount++;
                    Se.LogError(exception, $"Google Lens OCR gave up on image with index {bmpInput.Index} - skipping it");
                    return null;
                }

                try
                {
                    await Task.Delay(RetryDelay(exception, attempt), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// True for failures that tend to fix themselves - trying the same image again may well work.
    /// </summary>
    private static bool IsTransient(Exception exception)
    {
        if (exception is LensError lensError)
        {
            // 408 request timeout, 429 too many requests, 5xx = something broke on Google's side.
            return lensError.Code == 408 || lensError.Code == 429 || lensError.Code >= 500;
        }

        // Connection reset/dropped, or our own 30 second HttpClient timeout.
        return exception is HttpRequestException || exception is TaskCanceledException || exception is TimeoutException;
    }

    private static TimeSpan GetRetryDelay(Exception exception, int attempt)
    {
        // Back off a lot harder when Google is rate limiting us, a second is plenty for the rest.
        var seconds = exception is LensError { Code: 429 } ? 5 * attempt : attempt;
        return TimeSpan.FromSeconds(seconds);
    }
}