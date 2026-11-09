using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Export;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Lines out of time order - an ASSA script grouped by what the author worked on - still give a
/// Blu-ray sup that follows the timeline (issue #16714): a decoder drops what comes after the
/// timeline has passed it.
/// </summary>
public class ImageOutputTimeOrderTest : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "ImgOutOrder_" + Guid.NewGuid().ToString("N"));

    public ImageOutputTimeOrderTest() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    [Fact]
    public void BluRaySup_UnsortedOverlappingLines_PtsNeverGoBackwards()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("{\\an8}Breaking news", 10000, 20000));
        subtitle.Paragraphs.Add(new Paragraph("First line", 11000, 13000));
        subtitle.Paragraphs.Add(new Paragraph("Second line", 14000, 16000));
        subtitle.Paragraphs.Add(new Paragraph("Before the sign", 2000, 4000));

        var fileName = Path.Combine(_dir, "out.sup");
        ImageOutputWriter.Write(subtitle, fileName, new ExportHandlerBluRaySup(), new ConversionOptions
        {
            Patterns = [],
            Format = "bluraysup",
            Resolution = (1920, 1080),
            ImageStyle = new ImageExportStyle(),
        });

        var sup = File.ReadAllBytes(fileName);
        var pts = new List<long>();
        var objectCounts = new List<int>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            Assert.Equal((byte)'P', sup[position]);
            Assert.Equal((byte)'G', sup[position + 1]);
            pts.Add(((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5]);
            if (sup[position + 10] == 0x16)
            {
                objectCounts.Add(sup[position + 13 + 10]);
            }

            position += 13 + (sup[position + 11] << 8 | sup[position + 12]);
        }

        Assert.Equal(sup.Length, position);
        Assert.Equal(pts.OrderBy(p => p), pts);
        Assert.Contains(2, objectCounts);

        // Only the export is sorted.
        Assert.Equal("Before the sign", subtitle.Paragraphs[3].Text);
    }
}
