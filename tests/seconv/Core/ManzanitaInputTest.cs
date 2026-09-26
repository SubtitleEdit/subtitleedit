using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SeConv.Core;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// A Manzanita "private_stream_1" dump can have any extension - .dvbttx, .stl, even .idx. A
/// .idx dump was taken for a VobSub index ("no companion .sub"), and a bitmap (dvb_subtitle)
/// dump had no OCR route at all.
/// </summary>
public class ManzanitaInputTest : IDisposable
{
    private readonly string _tempRoot;

    public ManzanitaInputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Manzanita_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    /// <summary>A DVB subtitle PES payload; an object data segment makes it a displayed subtitle.</summary>
    private static byte[] MakeDvbPayload(bool withObject)
    {
        var payload = new List<byte> { 0x20, 0x00 };
        if (withObject)
        {
            payload.AddRange(new byte[]
            {
                0x0f, 0x13, 0x00, 0x00, 0x00, 0x07, // sync, object data segment, page id, length
                0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, // object id, flags, field block lengths
            });
        }

        payload.Add(0xff);
        return payload.ToArray();
    }

    private static byte[] MakeBitmapDump(params (ulong Milliseconds, byte[] Payload)[] packets)
    {
        var index = new StringBuilder();
        var binary = new List<byte>();
        foreach (var packet in packets)
        {
            index.Append($"    <packet pts=\"{packet.Milliseconds * 90}\" offset=\"{binary.Count}\" length=\"{packet.Payload.Length}\" />\n");
            binary.AddRange(packet.Payload);
        }

        var xml = "<private_stream_1\n" +
                  "  xmlns=\"http://www.manzanitasystems.com/schema/v1.03/private_stream_1\"\n" +
                  "  version=\"1.03\"\n" +
                  "  type=\"dvb_subtitle\">\n\n" +
                  "  <data_index>\n" + index + "  </data_index>\n\n" +
                  "</private_stream_1>\n";

        var result = new List<byte>(Encoding.ASCII.GetBytes(xml));
        result.AddRange(binary);
        return result.ToArray();
    }

    private async Task<ConversionResult> ConvertAsync(string input, bool timeCodesOnly)
    {
        return await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = Path.Combine(_tempRoot, "out"),
            Overwrite = true,
            TimeCodesOnly = timeCodesOnly,
        });
    }

    [Fact]
    public async Task BitmapDumpNamedIdx_TimeCodesOnly_ProducesTimeCodes()
    {
        var input = Path.Combine(_tempRoot, "dump.idx");
        await File.WriteAllBytesAsync(input, MakeBitmapDump(
            (1000, MakeDvbPayload(withObject: true)),
            (3000, MakeDvbPayload(withObject: false))), TestContext.Current.CancellationToken);

        var result = await ConvertAsync(input, timeCodesOnly: true);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var output = Assert.Single(Directory.GetFiles(Path.Combine(_tempRoot, "out"), "*.srt"));
        var content = await File.ReadAllTextAsync(output, TestContext.Current.CancellationToken);
        Assert.Contains("00:00:01,000 --> 00:00:03,000", content);
    }

    [Fact]
    public async Task TeletextDumpNamedIdx_ProducesText()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 3000));
        var input = Path.Combine(_tempRoot, "teletext.idx");
        await File.WriteAllBytesAsync(input, new ManzanitaTeletextWriter { Date = new DateTime(2026, 1, 1) }.GetBytes(subtitle), TestContext.Current.CancellationToken);

        var result = await ConvertAsync(input, timeCodesOnly: false);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var output = Assert.Single(Directory.GetFiles(Path.Combine(_tempRoot, "out"), "*.srt"));
        var content = await File.ReadAllTextAsync(output, TestContext.Current.CancellationToken);
        Assert.Contains("Hello", content);
    }
}
