using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System.Collections.Concurrent;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterRunBinaryOcrTests
{
    // The OCR pipeline (BinaryOcrMatcher) needs a trained DB to actually recognize text,
    // which is too heavy for a unit test. We exercise the parallel orchestration end-to-end
    // by feeding 10 distinct images through RunBinaryOcrParallel with a fake matcher that
    // returns "hello {n}" per image. The image index is propagated to the matcher via a
    // [ThreadStatic] field set when GetBitmap(i) is called — RunBinaryOcrParallel calls
    // GetBitmap and GetCompareMatch on the same thread for a given image, so the thread-local
    // remains correct under parallel execution.

    [Fact]
    public void RunBinaryOcrParallel_TenImages_ReturnsParagraphsInOrderWithExpectedText()
    {
        var converter = new BatchConverter(new FakeNOcrCaseFixer(), new FakeBinaryOcrMatcher(), new FakeNamesList());
        var imageSubtitles = new FakeOcrSubtitle(count: 10);
        var sharedDb = new BinaryOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db"), loadCompareImages: false);

        var progressTicks = new ConcurrentBag<int>();
        var paragraphs = converter.RunBinaryOcrParallel(
            imageSubtitles,
            sharedDb,
            pixelsAreSpace: 1,
            maxErrorPercent: 0,
            onProgress: progressTicks.Add,
            cancellationToken: CancellationToken.None);

        Assert.Equal(10, paragraphs.Length);
        for (var i = 0; i < 10; i++)
        {
            Assert.NotNull(paragraphs[i]);
            Assert.Equal($"hello {i + 1}", paragraphs[i].Text);
            Assert.Equal((i + 1) * 1000, paragraphs[i].StartTime.TotalMilliseconds);
            Assert.Equal((i + 1) * 1000 + 500, paragraphs[i].EndTime.TotalMilliseconds);
        }

        Assert.Equal(10, progressTicks.Count);
        var sortedTicks = progressTicks.OrderBy(t => t).ToArray();
        Assert.Equal(Enumerable.Range(1, 10).ToArray(), sortedTicks);
    }

    [Fact]
    public void RunBinaryOcrParallel_EmptyInput_ReturnsEmptyArray()
    {
        var converter = new BatchConverter(new FakeNOcrCaseFixer(), new FakeBinaryOcrMatcher(), new FakeNamesList());
        var imageSubtitles = new FakeOcrSubtitle(count: 0);
        var sharedDb = new BinaryOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db"), loadCompareImages: false);

        var paragraphs = converter.RunBinaryOcrParallel(
            imageSubtitles,
            sharedDb,
            pixelsAreSpace: 1,
            maxErrorPercent: 0,
            onProgress: null,
            cancellationToken: CancellationToken.None);

        Assert.Empty(paragraphs);
    }

    private sealed class FakeOcrSubtitle : IOcrSubtitle
    {
        [ThreadStatic]
        internal static int CurrentImageIndex;

        public FakeOcrSubtitle(int count) => Count = count;

        public int Count { get; }

        public SKBitmap GetBitmap(int index)
        {
            CurrentImageIndex = index;

            // 100x60 transparent canvas with a 60x40 white rectangle so the OCR splitter
            // detects exactly one line and one letter (minLineHeight=20 needs >40px height).
            var bmp = new SKBitmap(100, 60, isOpaque: false);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = false };
            canvas.DrawRect(new SKRect(20, 10, 80, 50), paint);
            return bmp;
        }

        public TimeSpan GetStartTime(int index) => TimeSpan.FromSeconds(index + 1);
        public TimeSpan GetEndTime(int index) => TimeSpan.FromSeconds(index + 1) + TimeSpan.FromMilliseconds(500);
        public List<OcrSubtitleItem> MakeOcrSubtitleItems() => new();
        public bool GetIsForced(int index) => false;
        public SKPointI GetPosition(int index) => new(-1, -1);
        public SKSizeI GetScreenSize(int index) => new(-1, -1);
    }

    private sealed class FakeBinaryOcrMatcher : IBinaryOcrMatcher
    {
        // First splitter-item per image emits the full text; subsequent items (if any) emit
        // empty so concatenation in ItalicTextMerger yields exactly "hello {n}".
        [ThreadStatic]
        private static int _lastEmittedForIndex;

        [ThreadStatic]
        private static bool _hasEmitted;

        public bool IsLatinDb { get; set; }

        public BinaryOcrMatcher.CompareMatch? GetCompareMatch(
            ImageSplitterItem2 targetItem,
            out BinaryOcrMatcher.CompareMatch? secondBestGuess,
            List<ImageSplitterItem2> list,
            int listIndex,
            BinaryOcrDb binaryOcrDb,
            double maxErrorPercent)
        {
            secondBestGuess = null;

            var imageIndex = FakeOcrSubtitle.CurrentImageIndex;
            if (!_hasEmitted || _lastEmittedForIndex != imageIndex)
            {
                _hasEmitted = true;
                _lastEmittedForIndex = imageIndex;
                return new BinaryOcrMatcher.CompareMatch($"hello {imageIndex + 1}", false, 0, "fake");
            }

            return new BinaryOcrMatcher.CompareMatch(string.Empty, false, 0, "fake");
        }
    }

    private sealed class FakeNOcrCaseFixer : INOcrCaseFixer
    {
        public bool HasWarmedUp { get; set; }
        public string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result) => result.Text ?? string.Empty;
    }

    private sealed class FakeNamesList : INamesList
    {
        public void Load(string dictionaryFolder, string languageCode)
        {
        }

        public bool IsName(string candidate) => false;
        public HashSet<string> GetAbbreviations() => new();
    }
}
