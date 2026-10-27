using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;

namespace LibSETests.ContainerFormats;

public class TimedTextMxfWriterTest
{
    private static readonly Guid AssetId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid FontId = Guid.Parse("3dec6dc0-39d0-498d-97d0-928d2eb78391");
    private static readonly Guid ImageId = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000001");
    private static readonly byte[] Font = Enumerable.Range(0, 512).Select(i => (byte)i).ToArray();
    private static readonly byte[] Png = Convert.FromHexString(
        "89504e470d0a1a0a0000000d494844520000000a000000040806000000b738aecd0000001249444154789c63f8cfc0f09f18cc30700a01887b4fb11f4f85f20000000049454e44ae426082");

    // local set tags holding instance ids, references to them or dates - random/now per file
    private static readonly HashSet<int> VolatileTags = new() { 0x3c0a, 0x3b02, 0x3b03, 0x3b06, 0x3b08, 0x1901, 0x1902, 0x4403, 0x4404, 0x4405, 0x4803, 0x1001, 0x4701, 0xffff };

    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// The XML asdcplib wrapped, with its LF line breaks - a Windows checkout with core.autocrlf
    /// gives it CRLF, and the 22 extra bytes moved every offset after the XML in the written file.
    /// </summary>
    private static string ReadReferenceXml() => File.ReadAllText(FilePath("sample_mxf_429-5_asdcplib.xml")).Replace("\r\n", "\n");

    private static byte[] WriteSample(string xml)
    {
        var writer = new TimedTextMxfWriter { AssetId = AssetId };
        writer.Resources.Add(new TimedTextMxfResource(FontId, TimedTextMxfResource.MimeTypeFont, Font));
        writer.Resources.Add(new TimedTextMxfResource(ImageId, TimedTextMxfResource.MimeTypePng, Png));
        using var ms = new MemoryStream();
        writer.Write(ms, xml);
        return ms.ToArray();
    }

    /// <summary>
    /// The reference was made by asdcplib 2.13.3 (asdcp-wrap -L) from the same XML, font and PNG.
    /// Everything but the identification set and the random instance ids/dates must match - the
    /// fill after the header metadata keeps all partition offsets identical too.
    /// </summary>
    [Fact]
    public void MatchesAsdcplibReference()
    {
        var reference = ReadKlvs(File.ReadAllBytes(FilePath("sample_mxf_429-5_asdcplib.mxf")));
        var written = ReadKlvs(WriteSample(ReadReferenceXml()));

        Assert.Equal(reference.Select(k => Convert.ToHexString(k.Key)), written.Select(k => Convert.ToHexString(k.Key)));
        for (var i = 0; i < reference.Count; i++)
        {
            var key = reference[i].Key;
            var isIdentification = key[5] == 0x53 && key[13] == 0x01 && key[14] == 0x30;
            var isFill = key[4] == 0x01 && key[8] == 0x03 && key[9] == 0x01;
            if (isIdentification || isFill)
            {
                continue;
            }

            var isLocalSet = key[4] == 0x02 && key[5] == 0x53;
            if (!isLocalSet)
            {
                Assert.Equal(reference[i].Offset, written[i].Offset);
                Assert.Equal(Convert.ToHexString(reference[i].Value), Convert.ToHexString(written[i].Value));
                continue;
            }

            var isMaterialPackage = key[14] == 0x36;
            var referenceItems = ReadLocalSet(reference[i].Value);
            var writtenItems = ReadLocalSet(written[i].Value);
            Assert.Equal(referenceItems.Select(x => x.Tag), writtenItems.Select(x => x.Tag));
            for (var j = 0; j < referenceItems.Count; j++)
            {
                var tag = referenceItems[j].Tag;
                if (VolatileTags.Contains(tag) || (isMaterialPackage && tag == 0x4401))
                {
                    Assert.Equal(referenceItems[j].Value.Length, writtenItems[j].Value.Length);
                    continue;
                }

                Assert.True(referenceItems[j].Value.SequenceEqual(writtenItems[j].Value), $"set {key[14]:x2} tag {tag:x4} differs");
            }
        }
    }

    [Fact]
    public void ReadBackWithMxfParser()
    {
        var xml = ReadReferenceXml();
        var parser = new MxfParser(new MemoryStream(WriteSample(xml)));

        Assert.True(parser.IsValid);
        Assert.Equal(xml, Assert.Single(parser.GetSubtitles()));
        Assert.Equal(Png, Assert.Single(parser.GetImages()));
    }

    [Fact]
    public void XmlWithoutIdThrows()
    {
        var xml = ReadReferenceXml();
        xml = xml.Replace("<dcst:Id>urn:uuid:7be835a3-cfb4-43d0-bb4b-f0b4c95e962e</dcst:Id>", string.Empty);
        Assert.Throws<InvalidDataException>(() => WriteSample(xml));
    }

    /// <summary>
    /// A DCP reel's assets must share one duration - DCP-o-matic's verifier rejects a subtitle
    /// track ending at its last TimeOut when the picture runs longer.
    /// </summary>
    [Fact]
    public void ContainerDurationOverridesLastTimeOut()
    {
        var xml = ReadReferenceXml();
        var writer = new TimedTextMxfWriter { AssetId = AssetId, ContainerDuration = 300 };
        writer.Resources.Add(new TimedTextMxfResource(FontId, TimedTextMxfResource.MimeTypeFont, Font));
        writer.Resources.Add(new TimedTextMxfResource(ImageId, TimedTextMxfResource.MimeTypePng, Png));
        using var ms = new MemoryStream();
        writer.Write(ms, xml);

        var durations = ReadKlvs(ms.ToArray())
            .Where(k => k.Key[4] == 0x02 && k.Key[5] == 0x53)
            .SelectMany(k => ReadLocalSet(k.Value))
            .Where(x => x.Tag is 0x0202 or 0x3002)
            .Select(x => Convert.ToHexString(x.Value))
            .Distinct();
        Assert.Equal("000000000000012C", Assert.Single(durations)); // 300 in every sequence, component and the descriptor
    }

    [Fact]
    public void ResourceMissingFromTrackThrows()
    {
        var xml = ReadReferenceXml();
        var writer = new TimedTextMxfWriter();
        writer.Resources.Add(new TimedTextMxfResource(FontId, TimedTextMxfResource.MimeTypeFont, Font)); // no PNG

        var exception = Assert.Throws<InvalidDataException>(() => writer.Write(new MemoryStream(), xml));
        Assert.Contains(ImageId.ToString(), exception.Message);
    }

    [Fact]
    public void TooManyResourcesThrows()
    {
        // 4096 sub-descriptor ids no longer fit the 16-bit local set length of the batch
        var xml = ReadReferenceXml();
        var writer = new TimedTextMxfWriter();
        writer.Resources.Add(new TimedTextMxfResource(FontId, TimedTextMxfResource.MimeTypeFont, Font));
        writer.Resources.Add(new TimedTextMxfResource(ImageId, TimedTextMxfResource.MimeTypePng, Png));
        while (writer.Resources.Count <= TimedTextMxfWriter.MaxResources)
        {
            writer.Resources.Add(new TimedTextMxfResource(Guid.NewGuid(), TimedTextMxfResource.MimeTypePng, Png));
        }

        var exception = Assert.Throws<InvalidOperationException>(() => writer.Write(new MemoryStream(), xml));
        Assert.Contains("4095", exception.Message);
        Assert.Equal(4095, TimedTextMxfWriter.MaxResources);
    }

    [Fact]
    public void GetLoadFontIds()
    {
        var xml = ReadReferenceXml();
        Assert.Equal(FontId, Assert.Single(TimedTextMxfWriter.GetLoadFontIds(xml)));
    }

    [Theory]
    [InlineData("00:00:12:00", 24, 288)]
    [InlineData("01:00:00:01", 25, 90001)]
    public void TryParseTimeCode(string timeCode, int frameRate, long expected)
    {
        Assert.True(TimedTextMxfWriter.TryParseTimeCode(timeCode, frameRate, out var frames));
        Assert.Equal(expected, frames);
        Assert.False(TimedTextMxfWriter.TryParseTimeCode("12.5", frameRate, out _));
    }

    [Theory]
    [InlineData("00:60:00:00", 24)]
    [InlineData("00:00:60:00", 24)]
    [InlineData("00:00:00:24", 24)]
    [InlineData("00:00:00:25", 25)]
    [InlineData("00:00:01:00", 0)]
    public void TryParseTimeCodeOutOfRange(string timeCode, int frameRate)
    {
        Assert.False(TimedTextMxfWriter.TryParseTimeCode(timeCode, frameRate, out _));
    }

    [Fact]
    public void ToTimeCode()
    {
        Assert.Equal("00:00:12:00", TimedTextMxfWriter.ToTimeCode(TimeSpan.FromSeconds(12), 24));
        Assert.Equal("01:02:03:12", TimedTextMxfWriter.ToTimeCode(new TimeSpan(0, 1, 2, 3, 500), 24));
        Assert.Equal("00:59:56:10", TimedTextMxfWriter.ToTimeCode(86314L, 24)); // 1 h of 23.976 picture
    }

    private static List<(long Offset, byte[] Key, byte[] Value)> ReadKlvs(byte[] data)
    {
        var list = new List<(long, byte[], byte[])>();
        var i = 0;
        while (i + 17 <= data.Length)
        {
            var key = data[i..(i + 16)];
            int length = data[i + 16];
            var lengthSize = 1;
            if (length >= 0x80)
            {
                var count = length & 0x7f;
                length = 0;
                for (var j = 0; j < count; j++)
                {
                    length = length * 256 + data[i + 17 + j];
                }

                lengthSize += count;
            }

            var start = i + 16 + lengthSize;
            list.Add((i, key, data[start..(start + length)]));
            i = start + length;
        }

        return list;
    }

    private static List<(int Tag, byte[] Value)> ReadLocalSet(byte[] value)
    {
        var list = new List<(int, byte[])>();
        var i = 0;
        while (i + 4 <= value.Length)
        {
            var tag = (value[i] << 8) | value[i + 1];
            var length = (value[i + 2] << 8) | value[i + 3];
            list.Add((tag, value[(i + 4)..(i + 4 + length)]));
            i += 4 + length;
        }

        return list;
    }
}
