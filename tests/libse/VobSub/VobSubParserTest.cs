using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.IO;

namespace LibSETests.VobSub;

public class VobSubParserTest
{
    private static readonly byte[] PackHeader = [0, 0, 1, 0xBA, 0x44, 0, 0x04, 0, 0x04, 0x01, 0x01, 0x89, 0xC3, 0xF8];

    private static byte[] MakePack(bool subtitle, byte fill)
    {
        var pack = new byte[2048];
        PackHeader.CopyTo(pack, 0);
        pack.AsSpan(20).Fill(fill);
        byte[] pes = subtitle
            ? [0, 0, 1, 0xBD, 0x07, 0xEC, 0x81, 0x80, 0x05, 0x21, 0x00, 0x01, 0x00, 0x01, 0x20]
            : [0, 0, 1, 0xE0, 0x07, 0xEC];
        pes.CopyTo(pack, 14);
        return pack;
    }

    // A raw .vob is mostly video packs; the parser resyncs past each one to the next subtitle
    // pack, also over junk that is not a multiple of the pack size and across its read chunks.
    [Fact]
    public void OpenSkipsVideoPacksAndJunkToEverySubtitlePack()
    {
        using var ms = new MemoryStream();
        var expected = 0;
        for (var i = 0; i < 80; i++)
        {
            var isSubtitle = i % 7 == 3;
            ms.Write(MakePack(isSubtitle, (byte)(i + 1)));
            if (isSubtitle)
            {
                expected++;
            }

            if (i % 11 == 5)
            {
                // odd-sized junk, including a false start code and a partial one
                ms.Write(new byte[] { 0, 0, 1, 0xBA, 9, 9, 9, 0, 0, 1 });
            }
        }

        var parser = new VobSubParser(true);
        parser.Open(new MemoryStream(ms.ToArray()));

        Assert.Equal(expected, parser.VobSubPacks.Count);
        foreach (var pack in parser.VobSubPacks)
        {
            Assert.Equal(0x20, pack.PacketizedElementaryStream.SubPictureStreamId);
        }
    }

    [Fact]
    public void OpenFindsSubtitlePackWhoseStartCodeStraddlesTheReadChunk()
    {
        foreach (var junkLength in new[] { 65535 - 2, 65535 - 1, 65535, 65535 + 1, 65536 + 2 })
        {
            using var ms = new MemoryStream();
            ms.Write(MakePack(false, 7));
            ms.Write(new byte[junkLength]);
            ms.Write(MakePack(true, 8));

            var parser = new VobSubParser(true);
            parser.Open(new MemoryStream(ms.ToArray()));

            Assert.Single(parser.VobSubPacks);
        }
    }
}
