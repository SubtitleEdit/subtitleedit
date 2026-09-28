using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    /// <summary>
    /// Video codec whose elementary stream carries cc_data.
    /// </summary>
    public enum CcVideoCodec
    {
        /// <summary>Unknown - H.264, H.265 and MPEG-2 cc_data are all looked for.</summary>
        Unknown,
        Mpeg2,
        H264,
        H265,
    }

    public static class GetCcDataHelper
    {
        public static List<CcData> GetCcData(Stream fs, ulong startPos, ulong size)
        {
            return GetCcData(fs, startPos, size, isHevc: false, nalLengthSize: 4);
        }

        /// <summary>
        /// Reads cc_data from the SEI NAL units of one MP4 video sample (length-prefixed NAL units).
        /// </summary>
        /// <param name="fs">Input stream</param>
        /// <param name="startPos">Sample position</param>
        /// <param name="size">Sample size</param>
        /// <param name="isHevc">H.265 (2-byte NAL header, SEI types 39/40) instead of H.264</param>
        /// <param name="nalLengthSize">Size of the NAL unit length prefix (1, 2 or 4) from avcC/hvcC</param>
        public static List<CcData> GetCcData(Stream fs, ulong startPos, ulong size, bool isHevc, int nalLengthSize)
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
                ParseCcDataFromLengthPrefixedSample(sample.AsSpan(0, read), isHevc, nalLengthSize, fieldData);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sample);
            }

            return fieldData;
        }

        /// <summary>
        /// Parses cc_data from the SEI NAL units of one video sample/frame stored as length-prefixed
        /// NAL units (MP4 "avc1"/"hvc1", Matroska V_MPEG4/ISO/AVC and V_MPEGH/ISO/HEVC).
        /// </summary>
        /// <param name="sample">The sample/frame</param>
        /// <param name="isHevc">H.265 (2-byte NAL header, SEI types 39/40) instead of H.264</param>
        /// <param name="nalLengthSize">Size of the NAL unit length prefix (1, 2 or 4) from avcC/hvcC</param>
        /// <param name="fieldData">cc_data found is added here</param>
        public static void ParseCcDataFromLengthPrefixedSample(ReadOnlySpan<byte> sample, bool isHevc, int nalLengthSize, List<CcData> fieldData)
        {
            if (nalLengthSize != 1 && nalLengthSize != 2 && nalLengthSize != 4)
            {
                nalLengthSize = 4;
            }

            var nalHeaderSize = isHevc ? 2 : 1;
            var read = sample.Length;
            var i = 0;
            while (i + nalLengthSize + nalHeaderSize < read)
            {
                var nalSize = ReadNalLength(sample, i, nalLengthSize);
                var flag = sample[i + nalLengthSize];
                var isSei = isHevc
                    ? IsHevcSeiNalUnitType((flag >> 1) & 0x3F)
                    : IsRbspNalUnitType(flag & 0x1F);
                if (isSei && nalSize < 10_000)
                {
                    // SEI payload spans from after the NAL header to the NAL end minus its
                    // rbsp trailing byte, clamped to what was read
                    var seiStart = i + nalLengthSize + nalHeaderSize;
                    var seiEnd = (int)Math.Min((long)i + nalLengthSize + nalSize - 1, read);
                    if (seiEnd > seiStart)
                    {
                        var seiData = UnescapeSeiData(sample.Slice(seiStart, seiEnd - seiStart));
                        ParseCcDataFromSei(seiData, fieldData);
                    }
                }

                // nalSize is unsigned and unvalidated here; widen so a bogus size
                // cannot overflow the index into a negative value and loop forever
                var advance = (long)nalSize + nalLengthSize;
                if (i + advance > read)
                {
                    break;
                }

                i += (int)advance;
            }
        }

        private static readonly byte[] StartCode = { 0, 0, 1 };

        /// <summary>
        /// Parses cc_data from start code delimited video data (a transport stream PES payload, a
        /// Matroska V_MPEG2 frame): H.264/H.265 SEI NAL units and MPEG-2 user data (ATSC A/53).
        /// </summary>
        public static void ParseCcDataFromStartCodeStream(ReadOnlySpan<byte> data, CcVideoCodec codec, List<CcData> fieldData)
        {
            var position = data.IndexOf(StartCode);
            while (position >= 0)
            {
                var unitStart = position + 3;
                if (unitStart >= data.Length)
                {
                    return;
                }

                var next = data.Slice(unitStart).IndexOf(StartCode);
                var unitEnd = next < 0 ? data.Length : unitStart + next;
                ParseStartCodeUnit(data.Slice(unitStart, unitEnd - unitStart), codec, fieldData);
                position = next < 0 ? -1 : unitEnd;
            }
        }

        private static void ParseStartCodeUnit(ReadOnlySpan<byte> unit, CcVideoCodec codec, List<CcData> fieldData)
        {
            if (unit.Length < 2)
            {
                return;
            }

            var b = unit[0];
            if ((codec == CcVideoCodec.Mpeg2 || codec == CcVideoCodec.Unknown) && b == 0xB2)
            {
                ParseCcDataFromAtscUserData(unit.Slice(1), fieldData);
            }
            else if ((codec == CcVideoCodec.H264 || codec == CcVideoCodec.Unknown) && (b & 0x9F) == 0x06)
            {
                ParseCcDataFromSeiNalPayload(unit.Slice(1), fieldData); // H.264 SEI
            }
            else if ((codec == CcVideoCodec.H265 || codec == CcVideoCodec.Unknown) && (b & 0x81) == 0 && ((b >> 1) == 39 || (b >> 1) == 40))
            {
                ParseCcDataFromSeiNalPayload(unit.Slice(2), fieldData); // H.265 prefix/suffix SEI
            }
        }

        private static bool IsRbspNalUnitType(int unitType)
        {
            return unitType == 0x06;
        }

        private static bool IsHevcSeiNalUnitType(int unitType)
        {
            return unitType == 39 || unitType == 40; // prefix / suffix SEI
        }

        private static uint ReadNalLength(ReadOnlySpan<byte> buffer, int index, int nalLengthSize)
        {
            switch (nalLengthSize)
            {
                case 1:
                    return buffer[index];
                case 2:
                    return BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(index, 2));
                default:
                    return BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(index, 4));
            }
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
                    AddCcTriplets(buffer, pos, buffer[pos - 2] & 0x1F, fieldData);
                }

                x += payloadSize;
            }
        }

        /// <summary>
        /// Parses cc_data from an H.264/H.265 SEI NAL unit payload (the bytes after the NAL unit
        /// header, still containing emulation prevention bytes).
        /// </summary>
        public static void ParseCcDataFromSeiNalPayload(ReadOnlySpan<byte> escapedSeiPayload, List<CcData> fieldData)
        {
            if (escapedSeiPayload.Length < 12)
            {
                return;
            }

            ParseCcDataFromSei(UnescapeSeiData(escapedSeiPayload), fieldData);
        }

        /// <summary>
        /// Parses cc_data from MPEG-2 video user data (the bytes after the 00 00 01 B2 start code)
        /// in ATSC A/53 form: "GA94", user_data_type_code 3, flags/cc_count, em_data, triplets.
        /// </summary>
        public static void ParseCcDataFromAtscUserData(ReadOnlySpan<byte> userData, List<CcData> fieldData)
        {
            if (userData.Length < 10 ||
                BinaryPrimitives.ReadUInt32BigEndian(userData) != 0x47413934 || // "GA94"
                userData[4] != 0x03 || // cc_data
                (userData[5] & 0x40) == 0) // process_cc_data_flag
            {
                return;
            }

            AddCcTriplets(userData, 7, userData[5] & 0x1F, fieldData);
        }

        private static void AddCcTriplets(ReadOnlySpan<byte> buffer, int pos, int count, List<CcData> fieldData)
        {
            var end = pos + count * 3;
            for (var i = pos; i < end && i + 2 < buffer.Length; i += 3)
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
