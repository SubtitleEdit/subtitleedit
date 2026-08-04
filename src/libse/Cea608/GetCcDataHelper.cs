using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public static class GetCcDataHelper
    {
        public static List<CcData> GetCcData(Stream fs, ulong startPos, ulong size)
        {
            var fieldData = new List<CcData>();
            if (size < 6 || size > int.MaxValue)
            {
                return fieldData;
            }

            var length = (int)size;

            // Read the sample once instead of seeking and reading 5 bytes per NAL unit.
            // The walk below only ever moves forward inside the sample, so a single
            // sequential read is equivalent - and on a file with a few hundred thousand
            // video samples it replaces millions of tiny reads with one read per sample.
            var sample = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                fs.Seek((long)startPos, SeekOrigin.Begin);
                var read = fs.ReadFully(sample, 0, length);

                var i = 0;
                while (i + 5 < read)
                {
                    var nalSize = BinaryPrimitives.ReadUInt32BigEndian(sample.AsSpan(i, 4));
                    var flag = sample[i + 4];
                    if (IsRbspNalUnitType(flag & 0x1F) && nalSize < 10_000)
                    {
                        // SEI payload spans [i + 5, i + nalSize + 3), clamped to what was read
                        var seiStart = i + 5;
                        var seiEnd = (int)Math.Min((long)i + nalSize + 3, read);
                        if (seiEnd > seiStart)
                        {
                            var seiData = UnescapeSeiData(sample.AsSpan(seiStart, seiEnd - seiStart));
                            ParseCcDataFromSei(seiData, fieldData);
                        }
                    }

                    // nalSize is unsigned and unvalidated here; widen so a bogus size
                    // cannot overflow the index into a negative value and loop forever
                    var advance = (long)nalSize + 4;
                    if (i + advance > read)
                    {
                        break;
                    }

                    i += (int)advance;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sample);
            }

            return fieldData;
        }

        private static bool IsRbspNalUnitType(int unitType)
        {
            return unitType == 0x06;
        }

        public static byte[] GetSeiData(Stream fs, ulong startPos, ulong endPos)
        {
            if (endPos <= startPos || endPos - startPos > int.MaxValue)
            {
                return Array.Empty<byte>();
            }

            var buffer = new byte[endPos - startPos];
            fs.Seek((long)startPos, SeekOrigin.Begin);
            var read = fs.ReadFully(buffer, 0, buffer.Length);

            return UnescapeSeiData(buffer.AsSpan(0, read));
        }

        /// <summary>
        /// Strips H.264 emulation prevention bytes: the 0x03 in any 00 00 03 sequence.
        /// </summary>
        private static byte[] UnescapeSeiData(ReadOnlySpan<byte> source)
        {
            if (source.Length == 0)
            {
                return Array.Empty<byte>();
            }

            // Output is never longer than the input, so one exact-sized buffer is
            // enough - no growing List<byte> plus a copy in ToArray().
            var data = new byte[source.Length];
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (i + 2 < source.Length && source[i] == 0x00 && source[i + 1] == 0x00 && source[i + 2] == 0x03)
                {
                    data[count++] = 0x00;
                    data[count++] = 0x00;
                    i += 2;
                }
                else
                {
                    data[count++] = source[i];
                }
            }

            if (count == data.Length)
            {
                return data;
            }

            var trimmed = new byte[count];
            Array.Copy(data, trimmed, count);
            return trimmed;
        }

        public static void ParseCcDataFromSei(byte[] buffer, List<CcData> fieldData)
        {
            var x = 0;
            while (x < buffer.Length -1)
            {
                var payloadType = 0;
                var payloadSize = 0;
                int now;

                do
                {
                    now = buffer[x++];
                    payloadType += now;
                } while (now == 0xFF && x < buffer.Length);

                if (x >= buffer.Length)
                {
                    break;
                }

                do
                {
                    now = buffer[x++];
                    payloadSize += now;
                } while (now == 0xFF && x < buffer.Length -1);

                if (IsStartOfCcDataHeader(payloadType, buffer, x))
                {
                    var pos = x + 10;
                    var ccCount = pos + (buffer[pos - 2] & 0x1F) * 3;
                    for (var i = pos; i < ccCount && i + 2 < buffer.Length; i += 3)
                    {
                        var b = buffer[i];
                        if ((b & 0x4) > 0)
                        {
                            var ccType = b & 0x3;
                            if (IsCcType(ccType))
                            {
                                var ccData1 = buffer[i + 1];
                                var ccData2 = buffer[i + 2];
                                // The "non-empty" filter is a CEA-608 convention
                                // (high bit is parity; both bytes need non-zero
                                // low-7-bits to be a meaningful char pair). For
                                // CEA-708 packet data (types 2/3), 0x80 / 0x00
                                // are legal payload bytes — e.g. a window-bitmap
                                // argument — and dropping them corrupts the
                                // DTVCC packet. Scope the filter to CEA-608.
                                var isCea608 = ccType == 0 || ccType == 1;
                                if (!isCea608 || IsNonEmptyCcData(ccData1, ccData2))
                                {
                                    fieldData.Add(new CcData(ccType, ccData1, ccData2));
                                }
                            }
                        }
                    }
                }

                x += payloadSize;
            }
        }

        private static bool IsCcType(int type)
        {
            // 0 = NTSC CEA-608 field 1, 1 = NTSC CEA-608 field 2,
            // 2 = DTVCC packet data, 3 = DTVCC packet start. Callers
            // filter by CcData.Type to route 0/1 → CEA-608 and 2/3 → CEA-708.
            return type >= 0 && type <= 3;
        }

        private static bool IsNonEmptyCcData(int ccData1, int ccData2)
        {
            return (ccData1 & 0x7f) > 0 || (ccData2 & 0x7f) > 0;
        }

        private static bool IsStartOfCcDataHeader(int payloadType, byte[] buffer, int pos)
        {
            // pos + 8 (not + 7): on success the caller also reads the cc-count byte at
            // buffer[pos + 8], so a buffer ending right after the two magic words must not match
            return payloadType == 4 &&
                   pos + 8 < buffer.Length &&
                   BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(pos)) == 3036688711 &&
                   BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(pos + 4)) == 1094267907;
        }
    }
}
