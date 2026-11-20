using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;

namespace UITests.Logic.Ocr;

/// <summary>Which OCR image a main-grid line shows after OCR (#17066).</summary>
public class OcrImageSourceHolderTests
{
    private sealed class FakeSource : IOcrSubtitle
    {
        private readonly (int Start, int End)[] _times;

        public FakeSource(params (int Start, int End)[] times) => _times = times;

        public int Count => _times.Length;
        public SKBitmap GetBitmap(int index) => new(10, 10);
        public TimeSpan GetStartTime(int index) => TimeSpan.FromMilliseconds(_times[index].Start);
        public TimeSpan GetEndTime(int index) => TimeSpan.FromMilliseconds(_times[index].End);
        public List<OcrSubtitleItem> MakeOcrSubtitleItems() => new();
        public bool GetIsForced(int index) => false;
        public SKPointI GetPosition(int index) => new(0, 0);
        public SKSizeI GetScreenSize(int index) => new(1920, 1080);
    }

    private static TimeSpan Ms(int ms) => TimeSpan.FromMilliseconds(ms);

    [Fact]
    public void MappedLine_KeepsItsImage_AfterTimeCodesChange()
    {
        var holder = new OcrImageSourceHolder();
        var lineId = Guid.NewGuid();
        holder.Set(new FakeSource((0, 1000), (2000, 3000)), "a.sup", new Dictionary<Guid, int> { [lineId] = 1 });

        // Shifted by an offset far away from both images - the id still finds image 1.
        Assert.Equal(1, holder.GetSourceIndex(lineId, Ms(60_000), Ms(61_000)));
    }

    [Fact]
    public void UnmappedLine_UsesTheImageItOverlapsMost()
    {
        var holder = new OcrImageSourceHolder();
        holder.Set(new FakeSource((0, 1000), (2000, 3000)), "a.sup", new Dictionary<Guid, int>());

        // Second half of a split line.
        Assert.Equal(1, holder.GetSourceIndex(Guid.NewGuid(), Ms(2500), Ms(3000)));
    }

    [Fact]
    public void UnmappedLine_WithoutOverlap_HasNoImage()
    {
        var holder = new OcrImageSourceHolder();
        holder.Set(new FakeSource((0, 1000), (2000, 3000)), "a.sup", new Dictionary<Guid, int>());

        Assert.Equal(-1, holder.GetSourceIndex(Guid.NewGuid(), Ms(1100), Ms(1900)));
    }

    [Fact]
    public void Clear_DropsSourceAndRaisesChanged()
    {
        var holder = new OcrImageSourceHolder();
        var lineId = Guid.NewGuid();
        holder.Set(new FakeSource((0, 1000)), "a.sup", new Dictionary<Guid, int> { [lineId] = 0 });
        var raised = 0;
        holder.Changed += (_, _) => raised++;

        holder.Clear();
        holder.Clear(); // nothing left - no second event

        Assert.Null(holder.Source);
        Assert.Equal(1, raised);
        Assert.Equal(-1, holder.GetSourceIndex(lineId, Ms(0), Ms(1000)));
    }
}
