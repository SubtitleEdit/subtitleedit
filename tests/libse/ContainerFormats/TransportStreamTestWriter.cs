using System;
using System.Collections.Generic;
using System.IO;

namespace LibSETests.ContainerFormats;

/// <summary>
/// Builds small transport streams for tests - PES packets taken from sample files are re-muxed
/// with other time stamps, programs and PIDs.
/// </summary>
internal sealed class TransportStreamTestWriter
{
    public const long TimestampWrap = 1L << 33;

    private readonly MemoryStream _stream = new MemoryStream();
    private readonly Dictionary<int, int> _continuityCounters = new Dictionary<int, int>();

    public byte[] ToArray() => _stream.ToArray();

    /// <summary>
    /// Reads the PES packets of <paramref name="packetId"/> from a 188-byte transport stream.
    /// </summary>
    public static List<byte[]> ReadPesPackets(string fileName, int packetId)
    {
        var result = new List<byte[]>();
        List<byte> current = null;
        var data = File.ReadAllBytes(fileName);
        for (var i = 0; i + 188 <= data.Length; i += 188)
        {
            var pid = ((data[i + 1] & 0x1F) << 8) | data[i + 2];
            var adaptationFieldControl = (data[i + 3] >> 4) & 3;
            if (pid != packetId || (adaptationFieldControl & 1) == 0)
            {
                continue;
            }

            var payloadStart = adaptationFieldControl == 3 ? 5 + data[i + 4] : 4;
            if ((data[i + 1] & 0x40) != 0)
            {
                if (current != null)
                {
                    result.Add(Trim(current));
                }

                current = new List<byte>();
            }

            current?.AddRange(new ArraySegment<byte>(data, i + payloadStart, 188 - payloadStart));
        }

        if (current != null)
        {
            result.Add(Trim(current));
        }

        return result;
    }

    private static byte[] Trim(List<byte> pes)
    {
        var length = (pes[4] << 8) | pes[5];
        return pes.GetRange(0, length > 0 ? Math.Min(pes.Count, 6 + length) : pes.Count).ToArray();
    }

    public static long GetPts(byte[] pes)
    {
        return ((long)(pes[9] & 0x0E) << 29) | ((long)pes[10] << 22) | ((long)(pes[11] & 0xFE) << 14) | ((long)pes[12] << 7) | ((long)pes[13] >> 1);
    }

    /// <summary>
    /// Copy of <paramref name="pes"/> with the PTS set (modulo 2^33), or removed when null - the
    /// header bytes are kept and become stuffing.
    /// </summary>
    public static byte[] WithPts(byte[] pes, long? pts)
    {
        var copy = (byte[])pes.Clone();
        if (pts.HasValue)
        {
            Array.Copy(EncodePts(pts.Value), 0, copy, 9, 5);
        }
        else
        {
            copy[7] &= 0x3F; // no PTS/DTS
        }

        return copy;
    }

    public static byte[] EncodePts(long pts)
    {
        pts = ((pts % TimestampWrap) + TimestampWrap) % TimestampWrap;
        return new[]
        {
            (byte)(0x21 | ((pts >> 29) & 0x0E)),
            (byte)(pts >> 22),
            (byte)(0x01 | ((pts >> 14) & 0xFE)),
            (byte)(pts >> 7),
            (byte)(0x01 | ((pts << 1) & 0xFE)),
        };
    }

    /// <summary>
    /// A PES packet with the given stream id and PTS.
    /// </summary>
    public static byte[] MakePes(int streamId, long pts, byte[] payload)
    {
        var pes = new List<byte> { 0, 0, 1, (byte)streamId, 0, 0, 0x80, 0x80, 5 };
        pes.AddRange(EncodePts(pts));
        pes.AddRange(payload);
        var length = pes.Count - 6;
        pes[4] = (byte)(length >> 8);
        pes[5] = (byte)length;
        return pes.ToArray();
    }

    /// <summary>
    /// A video PES packet (stream id 0xE0) carrying a few bytes - enough to give the program's
    /// first video time stamp.
    /// </summary>
    public static byte[] MakeVideoPes(long pts) => MakePes(0xE0, pts, new byte[] { 0, 0, 1, 0xB3, 0, 0, 0, 0 });

    public void WritePes(int packetId, byte[] pes)
    {
        var offset = 0;
        while (offset < pes.Length)
        {
            var packet = new byte[188];
            packet[0] = 0x47;
            packet[1] = (byte)((offset == 0 ? 0x40 : 0) | (packetId >> 8));
            packet[2] = (byte)packetId;
            var remaining = pes.Length - offset;
            var continuityCounter = NextContinuityCounter(packetId);
            int headerLength;
            if (remaining >= 184)
            {
                packet[3] = (byte)(0x10 | continuityCounter);
                headerLength = 4;
            }
            else
            {
                // adaptation field stuffing to fill the packet
                packet[3] = (byte)(0x30 | continuityCounter);
                var adaptationLength = 183 - remaining;
                packet[4] = (byte)adaptationLength;
                if (adaptationLength > 0)
                {
                    packet[5] = 0;
                    for (var i = 6; i < 5 + adaptationLength; i++)
                    {
                        packet[i] = 0xFF;
                    }
                }

                headerLength = 5 + adaptationLength;
            }

            var count = 188 - headerLength;
            Array.Copy(pes, offset, packet, headerLength, count);
            offset += count;
            _stream.Write(packet, 0, packet.Length);
        }
    }

    /// <summary>
    /// A packet with only an adaptation field carrying a PCR.
    /// </summary>
    public void WriteProgramClockReference(int packetId, long pcr)
    {
        pcr = ((pcr % TimestampWrap) + TimestampWrap) % TimestampWrap;
        var packet = new byte[188];
        packet[0] = 0x47;
        packet[1] = (byte)(packetId >> 8);
        packet[2] = (byte)packetId;
        packet[3] = (byte)(0x20 | (_continuityCounters.TryGetValue(packetId, out var cc) ? cc : 0)); // adaptation field only
        packet[4] = 183;
        packet[5] = 0x10; // PCR flag
        packet[6] = (byte)(pcr >> 25);
        packet[7] = (byte)(pcr >> 17);
        packet[8] = (byte)(pcr >> 9);
        packet[9] = (byte)(pcr >> 1);
        packet[10] = (byte)(((pcr & 1) << 7) | 0x7E);
        packet[11] = 0;
        for (var i = 12; i < 188; i++)
        {
            packet[i] = 0xFF;
        }

        _stream.Write(packet, 0, packet.Length);
    }

    /// <summary>
    /// Program association table: program number -> PMT PID.
    /// </summary>
    public void WriteProgramAssociationTable(params (int ProgramNumber, int PmtPid)[] programs)
    {
        var section = new List<byte> { 0x00, 0, 0, 0x00, 0x01, 0xC1, 0, 0 };
        foreach (var program in programs)
        {
            section.Add((byte)(program.ProgramNumber >> 8));
            section.Add((byte)program.ProgramNumber);
            section.Add((byte)(0xE0 | (program.PmtPid >> 8)));
            section.Add((byte)program.PmtPid);
        }

        WriteSection(0, section);
    }

    /// <summary>
    /// Program map table listing (stream_type, PID) pairs.
    /// </summary>
    public void WriteProgramMapTable(int pmtPid, int programNumber, int pcrPid, params (int StreamType, int Pid)[] streams)
    {
        var section = new List<byte>
        {
            0x02, 0, 0, (byte)(programNumber >> 8), (byte)programNumber, 0xC1, 0, 0,
            (byte)(0xE0 | (pcrPid >> 8)), (byte)pcrPid, 0xF0, 0,
        };
        foreach (var stream in streams)
        {
            section.Add((byte)stream.StreamType);
            section.Add((byte)(0xE0 | (stream.Pid >> 8)));
            section.Add((byte)stream.Pid);
            section.Add(0xF0);
            section.Add(0);
        }

        WriteSection(pmtPid, section);
    }

    private void WriteSection(int packetId, List<byte> section)
    {
        var sectionLength = section.Count - 3 + 4; // after the length field, including the CRC
        section[1] = (byte)(0xB0 | (sectionLength >> 8));
        section[2] = (byte)sectionLength;
        var crc = Crc32Mpeg2(section);
        section.Add((byte)(crc >> 24));
        section.Add((byte)(crc >> 16));
        section.Add((byte)(crc >> 8));
        section.Add((byte)crc);

        var packet = new byte[188];
        packet[0] = 0x47;
        packet[1] = (byte)(0x40 | (packetId >> 8));
        packet[2] = (byte)packetId;
        packet[3] = (byte)(0x10 | NextContinuityCounter(packetId));
        packet[4] = 0; // pointer field
        section.CopyTo(packet, 5);
        for (var i = 5 + section.Count; i < 188; i++)
        {
            packet[i] = 0xFF;
        }

        _stream.Write(packet, 0, packet.Length);
    }

    private int NextContinuityCounter(int packetId)
    {
        _continuityCounters.TryGetValue(packetId, out var value);
        _continuityCounters[packetId] = (value + 1) & 0x0F;
        return value;
    }

    private static uint Crc32Mpeg2(List<byte> data)
    {
        var crc = 0xFFFFFFFF;
        foreach (var b in data)
        {
            crc ^= (uint)b << 24;
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 0x80000000) != 0 ? (crc << 1) ^ 0x04C11DB7 : crc << 1;
            }
        }

        return crc;
    }
}
