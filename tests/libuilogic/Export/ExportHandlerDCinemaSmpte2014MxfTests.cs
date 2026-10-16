using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class ExportHandlerDCinemaSmpte2014MxfTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "dcinema_mxf_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerDCinemaSmpte2014MxfTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private static ImageParameter Cue(int index, int startMs, int endMs)
    {
        var bitmap = new SKBitmap(200, 40);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        return new ImageParameter
        {
            Bitmap = bitmap,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs),
            Index = index,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            FramesPerSecond = 24,
            Alignment = ExportAlignment.BottomCenter,
        };
    }

    private static void Export(IExportHandler handler, string fileOrFolderName)
    {
        var cues = new[] { Cue(0, 1000, 2000), Cue(1, 3000, 4500) };
        handler.WriteHeader(fileOrFolderName, cues[0]);
        foreach (var cue in cues)
        {
            handler.WriteParagraph(cue);
        }

        handler.WriteFooter();
    }

    [Fact]
    public void WritesOneTrackFileWithEveryImageEmbedded()
    {
        var handler = new ExportHandlerDCinemaSmpte2014Mxf();
        Assert.True(handler.UseFileName);
        Assert.Equal(".mxf", handler.Extension);

        var fileName = Path.Combine(_dir, "out.mxf");
        Export(handler, fileName);

        Assert.Equal(new[] { fileName }, Directory.GetFiles(_dir));
        var parser = new MxfParser(fileName);
        var xml = Assert.Single(parser.GetSubtitles());
        Assert.Contains("<EditRate>24 1</EditRate>", xml);
        Assert.Equal(2, parser.GetImages().Count);
        Assert.Equal(2, Regex.Matches(xml, "<Image [^>]*>urn:uuid:[0-9a-f-]{36}</Image>").Count);
    }

    [Fact]
    public void ReelDurationSetsTrackDuration()
    {
        var fileName = Path.Combine(_dir, "out.mxf");
        Export(new ExportHandlerDCinemaSmpte2014Mxf { ReelDuration = TimeSpan.FromSeconds(12) }, fileName);

        // ContainerDuration (local tag 3002) of the descriptor: 12 s at 24 fps
        var bytes = File.ReadAllBytes(fileName);
        var tag = new byte[] { 0x30, 0x02, 0x00, 0x08, 0, 0, 0, 0, 0, 0, 0x01, 0x20 };
        Assert.True(bytes.AsSpan().IndexOf(tag) > 0);
    }

    [Fact]
    public void PngExportStillWritesFolder()
    {
        Export(new ExportHandlerDCinemaSmpte2014Png(), _dir);

        Assert.True(File.Exists(Path.Combine(_dir, "index.xml")));
        var pngs = Directory.GetFiles(_dir, "*.png");
        Assert.Equal(2, pngs.Length);
        var xml = File.ReadAllText(Path.Combine(_dir, "index.xml"));
        // DCP-o-matic warns on a fractional/zoned IssueDate, ClairMeta rejects encoding="utf-8"
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", xml);
        Assert.Matches("<IssueDate>\\d{4}-\\d\\d-\\d\\dT\\d\\d:\\d\\d:\\d\\d</IssueDate>", xml);
        foreach (var png in pngs)
        {
            Assert.Contains("urn:uuid:" + Path.GetFileNameWithoutExtension(png), xml);
        }
    }
}
