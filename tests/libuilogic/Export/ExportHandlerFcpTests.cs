using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class ExportHandlerFcpTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "fcp_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerFcpTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private static ImageParameter Cue(int index, int startMs, int endMs, double fps, bool fullFrame = false)
    {
        var bitmap = new SKBitmap(300, 80);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, 300, 80, paint);
        }

        return new ImageParameter
        {
            Text = "Hello",
            Bitmap = bitmap,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs),
            Index = index,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            Alignment = ExportAlignment.BottomCenter,
            BottomTopMargin = 50,
            LeftRightMargin = 40,
            FramesPerSecond = fps,
            IsFullFrame = fullFrame,
        };
    }

    private string Export(double fps, bool fullFrame = false)
    {
        var folder = Path.Combine(_dir, $"{fps}_{fullFrame}");
        var handler = new ExportHandlerFcp();
        var cues = new[] { Cue(0, 1000, 3000, fps, fullFrame), Cue(1, 4000, 6000, fps, fullFrame) };
        handler.WriteHeader(folder, cues[0]);
        foreach (var cue in cues)
        {
            handler.WriteParagraph(cue);
        }

        handler.WriteFooter();
        return folder;
    }

    /// <summary>
    /// Nothing assigned the handler's FrameRate property, so every time code was written at the
    /// 25 fps default no matter which frame rate was picked in the export window.
    /// </summary>
    [Fact]
    public void FrameRate_ComesFromTheImageParameters()
    {
        var xml = File.ReadAllText(Path.Combine(Export(30), "fcpxml_export.xml"));

        // 1000 ms in at 30 fps is frame 30, not frame 25.
        Assert.Contains("<in>30</in>", xml);
        Assert.Contains("<start>30</start>", xml);
        Assert.Contains("<end>180</end>", xml);
    }

    [Fact]
    public void FrameRate_25_KeepsTheNonNtscTimeBase()
    {
        var xml = File.ReadAllText(Path.Combine(Export(25), "fcpxml_export.xml"));

        Assert.Contains("<timebase>25</timebase>", xml);
        Assert.DoesNotContain("<ntsc>TRUE</ntsc>", xml);
        Assert.Contains("<displayformat>NDF</displayformat>", xml);
    }

    [Fact]
    public void FrameRate_2997_UsesThe30TimeBaseWithNtscEverywhere()
    {
        var xml = File.ReadAllText(Path.Combine(Export(29.97), "fcpxml_export.xml"));

        Assert.Contains("<timebase>30</timebase>", xml);
        Assert.DoesNotContain("<timebase>25</timebase>", xml);
        Assert.DoesNotContain("<ntsc>FALSE</ntsc>", xml);
        Assert.Contains("<displayformat>DF</displayformat>", xml);
    }

    [Fact]
    public void FrameRate_23976_UsesThe24TimeBaseWithNtsc()
    {
        var xml = File.ReadAllText(Path.Combine(Export(23.976), "fcpxml_export.xml"));

        Assert.Contains("<timebase>24</timebase>", xml);
        Assert.DoesNotContain("<ntsc>FALSE</ntsc>", xml);
    }

    [Fact]
    public void Sequence_UsesTheExportedResolution()
    {
        var xml = File.ReadAllText(Path.Combine(Export(25), "fcpxml_export.xml"));

        Assert.Contains("<width>1280</width>", xml);
        Assert.Contains("<height>720</height>", xml);
        Assert.DoesNotContain("1920", xml);
        Assert.DoesNotContain("1080", xml);
    }

    [Fact]
    public void Xml_HasNoUnreplacedPlaceholders()
    {
        var xml = File.ReadAllText(Path.Combine(Export(25), "fcpxml_export.xml"));

        // "<!DOCTYPE xmeml[]>" has the only other square brackets in the file.
        foreach (var placeholder in new[] { "[TIMEBASE]", "[NTSC]", "[DISPLAYFORMAT]", "[WIDTH]", "[HEIGHT]", "[OUT]", "[IN]", "[START]", "[END]", "[DURATION]" })
        {
            Assert.DoesNotContain(placeholder, xml);
        }
    }

    [Fact]
    public void Images_AreCroppedToTheSubtitleByDefault()
    {
        var pngFileName = Directory.GetFiles(Export(25), "*.png").First();

        using var bitmap = SKBitmap.Decode(pngFileName);
        Assert.Equal(300, bitmap.Width);
        Assert.Equal(80, bitmap.Height);
    }

    [Fact]
    public void Images_AreFrameSizedWhenFullFrameIsOn()
    {
        var pngFileName = Directory.GetFiles(Export(25, fullFrame: true), "*.png").First();

        using var bitmap = SKBitmap.Decode(pngFileName);
        Assert.Equal(1280, bitmap.Width);
        Assert.Equal(720, bitmap.Height);

        // Bottom centered, 50 px up from the bottom edge - and transparent everywhere else.
        Assert.Equal(SKColors.White, bitmap.GetPixel(1280 / 2, 720 - 50 - 1));
        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha);
    }
}
