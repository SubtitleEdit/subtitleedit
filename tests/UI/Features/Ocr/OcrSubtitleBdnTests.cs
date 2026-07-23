using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using SkiaSharp;

namespace UITests.Features.Ocr;

public class OcrSubtitleBdnTests : IDisposable
{
    private readonly string _dir;

    public OcrSubtitleBdnTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "se_bdn_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dir, true);
        }
        catch
        {
            // best-effort
        }
    }

    private void WritePng(string name, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(1, 1, width - 2, height - 2, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.Create(Path.Combine(_dir, name));
        data.SaveTo(fs);
    }

    private string WriteBdnXml(string videoFormat = "1080p")
    {
        WritePng("0001.png", 400, 60);
        WritePng("0002.png", 300, 60);

        var path = Path.Combine(_dir, "index.xml");
        File.WriteAllText(path, $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <BDN Version="0.93">
            <Description>
            <Name Title="subtitle_exp" Content=""/>
            <Language Code="eng"/>
            <Format VideoFormat="{videoFormat}" FrameRate="25" DropFrame="false"/>
            <Events Type="Graphic" FirstEventInTC="00:00:01:00" LastEventOutTC="00:00:06:00" NumberofEvents="2"/>
            </Description>
            <Events>
            <Event InTC="00:00:01:00" OutTC="00:00:03:00" Forced="False">
            <Graphic Width="400" Height="60" X="760" Y="930">0001.png</Graphic>
            </Event>
            <Event InTC="00:00:04:00" OutTC="00:00:06:00" Forced="True">
            <Graphic Width="300" Height="60" X="810" Y="930">0002.png</Graphic>
            </Event>
            </Events>
            </BDN>
            """);
        return path;
    }

    private static (Subtitle Subtitle, OcrSubtitleBdn Ocr) Load(string path)
    {
        var lines = FileUtil.ReadAllLinesShared(path, LanguageAutoDetect.GetEncodingFromFile(path));
        var bdnXml = new BdnXml();
        Assert.True(bdnXml.IsMine(lines, path));

        var subtitle = new Subtitle();
        bdnXml.LoadSubtitle(subtitle, lines, path);
        return (subtitle, new OcrSubtitleBdn(subtitle, path, isSon: false));
    }

    [Fact]
    public void ReadsGraphicsTimingsAndForcedFlag()
    {
        var (subtitle, ocr) = Load(WriteBdnXml());

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(2, ocr.Count);

        Assert.Equal(TimeSpan.FromSeconds(1), ocr.GetStartTime(0));
        Assert.Equal(TimeSpan.FromSeconds(3), ocr.GetEndTime(0));
        Assert.False(ocr.GetIsForced(0));
        Assert.True(ocr.GetIsForced(1));

        using var bitmap = ocr.GetBitmap(0);
        Assert.Equal(400, bitmap.Width);
        Assert.Equal(60, bitmap.Height);
        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha); // transparent border survives the round trip
        Assert.Equal(255, bitmap.GetPixel(200, 30).Alpha);
    }

    // Both used to be hard-coded to -1, so a BDN xml converted to Blu-ray sup lost every position.
    [Fact]
    public void ReadsPositionAndScreenSize()
    {
        var (_, ocr) = Load(WriteBdnXml());

        Assert.Equal(760, ocr.GetPosition(0).X);
        Assert.Equal(930, ocr.GetPosition(0).Y);
        Assert.Equal(810, ocr.GetPosition(1).X);

        Assert.Equal(1920, ocr.GetScreenSize(0).Width);
        Assert.Equal(1080, ocr.GetScreenSize(0).Height);
    }

    [Fact]
    public void ReportsNoScreenSizeWhenVideoFormatIsUnknown()
    {
        var (_, ocr) = Load(WriteBdnXml("something-else"));

        Assert.Equal(-1, ocr.GetScreenSize(0).Width);
        Assert.Equal(-1, ocr.GetScreenSize(0).Height);
    }

    // OcrSubtitleBdn is shared with the Dost/TimedTextImage import, where Extra is an image
    // path rather than an "X,Y" pair - that must not be mistaken for a position.
    [Fact]
    public void IgnoresNonPositionExtra()
    {
        var (subtitle, ocr) = Load(WriteBdnXml());
        subtitle.Paragraphs[0].Extra = "file://0001.png";

        Assert.Equal(-1, ocr.GetPosition(0).X);
        Assert.Equal(-1, ocr.GetPosition(0).Y);
    }

    [Fact]
    public void ReturnsNoPositionForOutOfRangeIndex()
    {
        var (_, ocr) = Load(WriteBdnXml());

        Assert.Equal(-1, ocr.GetPosition(-1).X);
        Assert.Equal(-1, ocr.GetPosition(99).X);
    }
}
