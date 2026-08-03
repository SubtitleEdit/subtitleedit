using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Linq;

namespace LibSETests.ContainerFormats;

public class AribCaptionParserTest
{
    // ARIB STD-B24 hiragana set, GR invoked by default in profile A: あ = 0xA2, い = 0xA4
    private const byte HiraganaA = 0xA2;
    private const byte HiraganaI = 0xA4;

    private static byte[] MakeCaptionManagementPayload(string languageCode)
    {
        var data = new List<byte>
        {
            0x80, 0xFF, 0x00, // data_identifier, private_stream_id, PES_data_packet_header_length 0
            0x00,             // data_group_id 0 = caption management (group set A)
            0x00, 0x00,       // data_group_link_number, last_data_group_link_number
        };
        var groupData = new List<byte>
        {
            0x00, // TMD free
            0x01, // number of languages
            0x00, // language_tag 0 + DMF
            (byte)languageCode[0], (byte)languageCode[1], (byte)languageCode[2],
            0x00, // format
            0x00, 0x00, 0x00, // data_unit_loop_length
        };
        data.Add((byte)(groupData.Count >> 8));
        data.Add((byte)groupData.Count);
        data.AddRange(groupData);
        return data.ToArray();
    }

    private static byte[] MakeCaptionStatementPayload(params byte[] textBytes)
    {
        var data = new List<byte>
        {
            0x80, 0xFF, 0x00,
            0x01 << 2,  // data_group_id 1 = caption statement, first language (group set A)
            0x00, 0x00,
        };
        var groupData = new List<byte> { 0x00 }; // TMD free
        if (textBytes.Length > 0)
        {
            var unitLoopLength = 5 + textBytes.Length;
            groupData.Add((byte)(unitLoopLength >> 16));
            groupData.Add((byte)(unitLoopLength >> 8));
            groupData.Add((byte)unitLoopLength);
            groupData.Add(0x1F); // unit_separator
            groupData.Add(0x20); // data_unit_parameter: statement body
            groupData.Add((byte)(textBytes.Length >> 16));
            groupData.Add((byte)(textBytes.Length >> 8));
            groupData.Add((byte)textBytes.Length);
            groupData.AddRange(textBytes);
        }
        else
        {
            groupData.AddRange(new byte[] { 0x00, 0x00, 0x00 }); // empty data unit loop = clear screen
        }

        data.Add((byte)(groupData.Count >> 8));
        data.Add((byte)groupData.Count);
        data.AddRange(groupData);
        return data.ToArray();
    }

    [Fact]
    public void ParseStatementWithManagementData()
    {
        var parser = new AribCaptionParser(AribB24Decoder.AribProfile.ProfileA);
        parser.ParsePesPayload(MakeCaptionManagementPayload("jpn"), 9_500);
        parser.ParsePesPayload(MakeCaptionStatementPayload(HiraganaA, HiraganaI), 10_000);
        parser.ParsePesPayload(MakeCaptionStatementPayload(), 13_000); // clear screen
        parser.Flush();

        Assert.Equal("jpn", parser.LanguageCodes[0]);
        var paragraphs = parser.ParagraphsByLanguage[0];
        Assert.Single(paragraphs);
        Assert.Equal("あい", paragraphs[0].Text);
        Assert.Equal(10_000, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(13_000, paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void StatementEndsAtNextStatement()
    {
        var parser = new AribCaptionParser(AribB24Decoder.AribProfile.ProfileA);
        parser.ParsePesPayload(MakeCaptionStatementPayload(HiraganaA), 1_000);
        parser.ParsePesPayload(MakeCaptionStatementPayload(HiraganaI), 4_000);
        parser.ParsePesPayload(MakeCaptionStatementPayload(), 6_000);
        parser.Flush();

        var paragraphs = parser.ParagraphsByLanguage[0];
        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("あ", paragraphs[0].Text);
        Assert.Equal(1_000, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(4_000, paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("い", paragraphs[1].Text);
        Assert.Equal(6_000, paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void RollupContinuationsAreMerged()
    {
        // roll-up captions repaint the screen for every added character - the
        // grow-by-prefix chain should collapse into one paragraph with the final text
        var parser = new AribCaptionParser(AribB24Decoder.AribProfile.ProfileA);
        parser.ParsePesPayload(MakeCaptionStatementPayload(HiraganaA), 1_000);
        parser.ParsePesPayload(MakeCaptionStatementPayload(HiraganaA, HiraganaI), 1_100);
        parser.ParsePesPayload(MakeCaptionStatementPayload(), 5_000);
        parser.Flush();

        var paragraphs = parser.ParagraphsByLanguage[0];
        Assert.Single(paragraphs);
        Assert.Equal("あい", paragraphs[0].Text);
        Assert.Equal(1_000, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(5_000, paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void IsAribCaptionPayloadDetection()
    {
        Assert.True(AribCaptionParser.IsAribCaptionPayload(MakeCaptionStatementPayload(HiraganaA)));
        Assert.False(AribCaptionParser.IsAribCaptionPayload(new byte[] { 0x20, 0x00, 0x0F, 0, 0, 0, 0, 0, 0, 0 })); // DVB subtitle
        Assert.False(AribCaptionParser.IsAribCaptionPayload(new byte[] { 0x10, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0 })); // teletext
        Assert.False(AribCaptionParser.IsAribCaptionPayload(null));
    }
}

public class AribB24DecoderTest
{
    private static string Decode(params byte[] bytes)
    {
        return new AribB24Decoder().Decode(bytes, 0, bytes.Length);
    }

    [Fact]
    public void DefaultKanjiViaGl()
    {
        Assert.Equal("亜", Decode(0x30, 0x21)); // JIS X 0208 ku-ten 16-01
    }

    [Fact]
    public void DefaultHiraganaViaGr()
    {
        Assert.Equal("あい", Decode(0xA2, 0xA4));
    }

    [Fact]
    public void AlphanumericFullwidthByDefaultAndHalfwidthInMiddleSize()
    {
        Assert.Equal("Ａ", Decode(0x0E, 0x41)); // LS1 -> G1 alphanumeric, normal size
        Assert.Equal("AB", Decode(0x0E, 0x89, 0x41, 0x42)); // MSZ (middle size) -> half width
    }

    [Fact]
    public void EscDesignationSwitchesCharacterSet()
    {
        // ESC 0x28 F designates a one-byte set to G0; F = 0x31 is katakana
        var expected = AribB24Tables.KatakanaTable[0x41 - 0x21];
        Assert.Equal(expected, Decode(0x1B, 0x28, 0x31, 0x41));
    }

    [Fact]
    public void GaijiRowMapsToAdditionalSymbols()
    {
        Assert.Equal("㐂", Decode(0x75, 0x21)); // row 85 cell 1 - first ARIB additional kanji
    }

    [Fact]
    public void CsiParameterBytesAreNotDecodedAsText()
    {
        // CSI SWF with parameters "62;169" - everything up to and including the
        // final byte after the 0x20 intermediate must be consumed
        Assert.Equal("あ", Decode(0x9B, 0x36, 0x32, 0x3B, 0x31, 0x36, 0x39, 0x20, 0x53, HiraganaA));
    }

    [Fact]
    public void ActivePositionControlsProduceLineBreaks()
    {
        Assert.Equal("あ" + Environment.NewLine + "い", Decode(0xA2, 0x0D, 0xA4)); // APR
        Assert.Equal("あ" + Environment.NewLine + "い", Decode(0xA2, 0x1C, 0x05, 0x05, 0xA4)); // APS with position parameters
    }

    [Fact]
    public void ClearScreenDropsEarlierText()
    {
        Assert.Equal("い", Decode(0xA2, 0x0C, 0xA4)); // CS
    }

    [Fact]
    public void AribToStringStillWorksForAribB36()
    {
        Assert.Equal("亜", AribB24Decoder.AribToString(new byte[] { 0x30, 0x21 }, 0, 2));
    }

    private const byte HiraganaA = 0xA2;
}
