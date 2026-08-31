using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using System.Text;

namespace LibSETests.ContainerFormats;

// A Manzanita dump can hold DVB bitmap subtitles instead of teletext; those go to OCR, so the
// parser has to hand them out as transport stream subtitles with usable time codes.
public class ManzanitaDvbSubtitleTest
{
    /// <summary>
    /// One PES payload: the DVB subtitle data identifier, an optional object data segment and the
    /// end marker. A payload without an object data segment is what ends the previous subtitle.
    /// </summary>
    private static byte[] MakeDvbPayload(bool withObject)
    {
        var payload = new List<byte> { 0x20, 0x00 };
        if (withObject)
        {
            payload.AddRange(new byte[]
            {
                0x0f,       // sync byte
                0x13,       // object data segment
                0x00, 0x00, // page id
                0x00, 0x07, // segment length
                0x00, 0x01, // object id
                0x00,       // version, coding method, non modifying color flag
                0x00, 0x00, // top field data block length
                0x00, 0x00, // bottom field data block length
            });
        }

        payload.Add(0xff); // anything but a sync byte ends the segment loop
        return payload.ToArray();
    }

    private static byte[] MakeManzanitaFile(params (ulong Milliseconds, byte[] Payload)[] packets)
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

    private static ManzanitaTransportStreamParser Parse(byte[] file)
    {
        var parser = new ManzanitaTransportStreamParser();
        using var ms = new MemoryStream(file);
        parser.Parse(ms);
        return parser;
    }

    [Fact]
    public void BitmapSubtitlesKeepTheirTimeCodes()
    {
        var file = MakeManzanitaFile(
            (1000, MakeDvbPayload(withObject: true)),
            (3000, MakeDvbPayload(withObject: false)));

        var subtitles = Parse(file).GetDvbSup();

        var subtitle = Assert.Single(subtitles);
        Assert.Equal(1000ul, subtitle.StartMilliseconds);
        Assert.Equal(3000ul, subtitle.EndMilliseconds);
        Assert.True(subtitle.IsDvbSub);
    }

    [Fact]
    public void BitmapSubtitlesAreNotMistakenForTeletext()
    {
        var file = MakeManzanitaFile(
            (1000, MakeDvbPayload(withObject: true)),
            (3000, MakeDvbPayload(withObject: false)));

        var parser = Parse(file);

        Assert.Empty(parser.TeletextPages);
        Assert.Empty(parser.GetTeletext());
    }

    [Fact]
    public void TeletextFilesHoldNoBitmapSubtitles()
    {
        var writer = new ManzanitaTeletextWriter { Date = new DateTime(2026, 1, 1) };
        var subtitle = new Nikse.SubtitleEdit.Core.Common.Subtitle();
        subtitle.Paragraphs.Add(new Nikse.SubtitleEdit.Core.Common.Paragraph("Hello", 1000, 3000));

        var parser = Parse(writer.GetBytes(subtitle));

        Assert.Empty(parser.GetDvbSup());
        Assert.Single(parser.GetTeletext());
    }
}
