using Nikse.SubtitleEdit.Core.Cea708;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// SMPTE 334-2 allows an optional time code section (id 0x71 + 4 bytes) between the cdp header and the
/// cc_data section (0x72). The parser used to read cc_data at a fixed offset, so such packets came out
/// as garbage/exceptions and the whole MCC file loaded empty.
/// </summary>
public class MacCaptionTimeCodeSectionTest
{
    private const int FlagsIndex = 7;
    private const int TimeCodeSectionIndex = 10;

    /// <summary>
    /// 00:00:00:28 (drop frame) as SMPTE 334-2 time code bytes: hours, minutes, seconds, drop_frame|frames.
    /// </summary>
    private static readonly byte[] TimeCode00000028 = { 0xC0, 0x80, 0x00, 0xE8 };

    private static string ReadFixtureAncData(int lineIndexWithText)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_mcc_cea608_708.mcc");
        var dataLines = File.ReadAllLines(path).Where(l => l.Length > 12 && l[2] == ':' && l.Contains('\t')).ToList();
        return dataLines[lineIndexWithText].Split('\t')[1];
    }

    private static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    private static string BytesToHex(IEnumerable<byte> bytes) => string.Concat(bytes.Select(b => b.ToString("X2")));

    /// <summary>
    /// The checksum is the last byte and makes the sum of all cdp bytes (from cdp_identifier on) zero mod 256.
    /// </summary>
    private static byte CalculateChecksum(byte[] packet)
    {
        var total = 0;
        for (var i = 3; i < packet.Length - 1; i++)
        {
            total += packet[i];
        }

        return (byte)((256 - total % 256) % 256);
    }

    private static byte[] WithTimeCodeFlag(byte[] original)
    {
        var packet = (byte[])original.Clone();
        packet[FlagsIndex] |= 0b10000000;
        packet[packet.Length - 1] = CalculateChecksum(packet);
        return packet;
    }

    private static byte[] WithTimeCodeSection(byte[] original, byte[] timeCode)
    {
        var list = original.ToList();
        list[FlagsIndex] |= 0b10000000;
        list.InsertRange(TimeCodeSectionIndex, new[] { Smpte291M.TimeCodeSectionId }.Concat(timeCode));
        list[2] += 5; // anc data count
        list[5] += 5; // cdp data count
        var packet = list.ToArray();
        packet[packet.Length - 1] = CalculateChecksum(packet);
        return packet;
    }

    private static void AssertSameCcData(List<CcData> expected, List<CcData> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Valid, actual[i].Valid);
            Assert.Equal(expected[i].Type, actual[i].Type);
            Assert.Equal(expected[i].Data1, actual[i].Data1);
            Assert.Equal(expected[i].Data2, actual[i].Data2);
        }
    }

    [Fact]
    public void FixtureLineHasNoTimeCodeSectionAndValidChecksum()
    {
        var original = HexToBytes(ReadFixtureAncData(28));

        Assert.Equal(0x43, original[FlagsIndex]);
        Assert.Equal(Smpte291M.CcDataSectionId, original[TimeCodeSectionIndex]);
        Assert.Equal(original[original.Length - 1], CalculateChecksum(original));
    }

    [Fact]
    public void ReadsCcDataAfterTimeCodeSection()
    {
        var originalHex = ReadFixtureAncData(28);
        var original = HexToBytes(originalHex);
        var withTimeCode = WithTimeCodeSection(original, TimeCode00000028);

        var expected = MacCaption10.GetAllCcData(originalHex);
        var actual = MacCaption10.GetAllCcData(BytesToHex(withTimeCode));

        Assert.Equal(20, expected.Count);
        Assert.Contains(expected, cc => cc.Valid && cc.Type == 3); // real CEA-708 payload, not just padding
        AssertSameCcData(expected, actual);

        var packet = new Smpte291M(withTimeCode);
        Assert.True(packet.CaptionDistributionPacketTimeCodeAdded);
        Assert.Equal(TimeCode00000028, packet.CaptionDistributionPacketTimeCode);
        Assert.Equal(0x74, packet.CaptionDistributionPacketFooterSection);
        Assert.Equal(packet.CaptionDistributionPacketHeaderSequenceCounter, packet.CaptionDistributionPacketHeaderSequenceCounter2);
        Assert.Equal(withTimeCode[withTimeCode.Length - 1], packet.CaptionDistributionPacketChecksum);
    }

    [Fact]
    public void ScansForwardToCcDataWhenTimeCodeFlagIsSetButSectionIsMissing()
    {
        var originalHex = ReadFixtureAncData(28);
        var original = HexToBytes(originalHex);
        var flagOnly = WithTimeCodeFlag(original);

        var expected = MacCaption10.GetAllCcData(originalHex);
        var actual = MacCaption10.GetAllCcData(BytesToHex(flagOnly));

        Assert.Equal(20, expected.Count);
        AssertSameCcData(expected, actual);
        Assert.Null(new Smpte291M(flagOnly).CaptionDistributionPacketTimeCode);
    }

    [Fact]
    public void WriterRoundTripsPacketsWithAndWithoutTimeCodeSection()
    {
        var original = HexToBytes(ReadFixtureAncData(28));
        var withTimeCode = WithTimeCodeSection(original, TimeCode00000028);

        // GetBytes appends a trailing 0xBB after the checksum, so compare the packet proper
        foreach (var input in new[] { original, withTimeCode })
        {
            var output = new Smpte291M(input).GetBytes();
            Assert.Equal(input.Length + 1, output.Length);
            Assert.Equal(BytesToHex(input), BytesToHex(output.Take(input.Length)));
        }

        var written = new Smpte291M(0, 20, new byte[0]).GetBytes();
        Assert.False(new Smpte291M(written).CaptionDistributionPacketTimeCodeAdded);
        Assert.Equal(Smpte291M.CcDataSectionId, written[TimeCodeSectionIndex]);
    }
}
