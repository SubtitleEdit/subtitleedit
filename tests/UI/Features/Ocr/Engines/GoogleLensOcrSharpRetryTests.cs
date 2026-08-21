using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UITests.Features.Ocr.Engines;

// Google Lens is a public endpoint that will time out or rate limit now and then. A single failed
// image used to throw out of the batch loop, so OCR stopped mid-run and the user had to press
// "Start OCR" again - losing the unknown-words list (#13563). Failures are now retried and, when
// they persist, that one image is skipped so the rest of the subtitle still gets OCR'ed.
public class GoogleLensOcrSharpRetryTests
{
    private sealed class FakeLens : ILens
    {
        private readonly Queue<Func<LensResult>> _responses;

        public int CallCount { get; private set; }

        public FakeLens(params Func<LensResult>[] responses)
        {
            _responses = new Queue<Func<LensResult>>(responses);
        }

        public Task<LensResult> ScanByBitmap(SKBitmap bitmap, string twoLetterLanguageCode)
        {
            CallCount++;
            return Task.FromResult(_responses.Dequeue()());
        }
    }

    // Progress<T> hands the callback to the thread pool, which would race the batch loop.
    private sealed class SyncProgress : IProgress<PaddleOcrBatchProgress>
    {
        private readonly Action<PaddleOcrBatchProgress> _onReport;

        public SyncProgress(Action<PaddleOcrBatchProgress>? onReport = null)
        {
            _onReport = onReport ?? (_ => { });
        }

        public void Report(PaddleOcrBatchProgress value) => _onReport(value);
    }

    private static Func<LensResult> Text(string text) =>
        () => new LensResult("en", new List<Segment>
        {
            new(text, new BoundingBox(new double[] { 0.5, 0.5, 1, 1 }, new[] { 100, 20 })),
        });

    private static Func<LensResult> Throws(Exception exception) =>
        () => throw exception;

    private static List<PaddleOcrBatchInput> Images(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new PaddleOcrBatchInput { Index = i, Bitmap = new SKBitmap(10, 10) })
            .ToList();

    private static GoogleLensOcrSharp CreateEngine(FakeLens lens) =>
        new(lens) { RetryDelay = (_, _) => TimeSpan.Zero };

    [Fact]
    public async Task TransientFailure_IsRetriedAndSucceeds()
    {
        var lens = new FakeLens(
            Throws(new LensError("Lens returned status code 429", 429)),
            Text("HELLO THERE"));
        var images = Images(1);

        var engine = CreateEngine(lens);
        await engine.OcrBatch(images, "en", new SyncProgress(), CancellationToken.None);

        Assert.Equal(2, lens.CallCount);
        Assert.Equal("HELLO THERE", images[0].Text);
        Assert.Equal(0, engine.SkippedImageCount);
    }

    [Fact]
    public async Task PersistentFailure_SkipsOnlyThatImage()
    {
        var lens = new FakeLens(
            Text("FIRST LINE"),
            Throws(new HttpRequestException("connection reset")),
            Throws(new HttpRequestException("connection reset")),
            Throws(new HttpRequestException("connection reset")),
            Text("THIRD LINE"));
        var images = Images(3);
        var reported = new List<int>();

        var engine = CreateEngine(lens);
        await engine.OcrBatch(images, "en", new SyncProgress(p => reported.Add(p.Index)), CancellationToken.None);

        // Three tries were spent on the middle image, then the batch carried on to the last one.
        Assert.Equal(5, lens.CallCount);
        Assert.Equal("FIRST LINE", images[0].Text);
        Assert.Equal(string.Empty, images[1].Text);
        Assert.Equal("THIRD LINE", images[2].Text);
        Assert.Equal(1, engine.SkippedImageCount);
        Assert.Equal(new List<int> { 0, 2 }, reported);
    }

    [Fact]
    public async Task NonTransientFailure_IsNotRetriedButBatchContinues()
    {
        var lens = new FakeLens(
            Throws(new LensError("Lens returned status code 400", 400)),
            Text("SECOND LINE"));
        var images = Images(2);

        var engine = CreateEngine(lens);
        await engine.OcrBatch(images, "en", new SyncProgress(), CancellationToken.None);

        // A 400 will not go away by asking again, so only one try for the first image.
        Assert.Equal(2, lens.CallCount);
        Assert.Equal(string.Empty, images[0].Text);
        Assert.Equal("SECOND LINE", images[1].Text);
        Assert.Equal(1, engine.SkippedImageCount);
    }

    [Fact]
    public async Task Cancellation_StopsTheBatch()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var lens = new FakeLens(
            Text("FIRST LINE"),
            Text("NEVER REACHED"));
        var images = Images(2);

        var engine = CreateEngine(lens);
        await engine.OcrBatch(
            images,
            "en",
            new SyncProgress(_ => cancellationTokenSource.Cancel()),
            cancellationTokenSource.Token);

        Assert.Equal(1, lens.CallCount);
        Assert.Equal("FIRST LINE", images[0].Text);
        Assert.Equal(string.Empty, images[1].Text);
    }

    [Fact]
    public async Task CancellationDuringRetry_DoesNotCountAsSkipped()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var lens = new FakeLens(
            Throws(new LensError("Lens returned status code 503", 503)),
            Text("NEVER REACHED"));
        var images = Images(1);

        var engine = CreateEngine(lens);
        engine.RetryDelay = (_, _) =>
        {
            cancellationTokenSource.Cancel();
            return TimeSpan.Zero;
        };

        await engine.OcrBatch(images, "en", new SyncProgress(), cancellationTokenSource.Token);

        Assert.Equal(1, lens.CallCount);
        Assert.Equal(0, engine.SkippedImageCount);
    }
}
