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

    /// <summary>
    /// Per video stream state for cc_data parsing - kept by the caller across pictures.
    /// </summary>
    public sealed class CcDataParseState
    {
        /// <summary>
        /// MPEG-2 user data format locked to (like ffmpeg's cc_format auto): a stream may carry
        /// the same captions both as ATSC A/53 and as SCTE 20 - only the first one found is used.
        /// </summary>
        public CcUserDataFormat UserDataFormat { get; set; }

        /// <summary>
        /// True when a caption data unit was found - also if it only had padding/null bytes.
        /// </summary>
        public bool CaptionDataSeen { get; set; }
    }

    public enum CcUserDataFormat
    {
        Unknown,
        Atsc,
        Scte20,
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
        /// <param name="state">Optional per stream state</param>
        public static void ParseCcDataFromLengthPrefixedSample(ReadOnlySpan<byte> sample, bool isHevc, int nalLengthSize, List<CcData> fieldData, CcDataParseState state = null)
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
                        if (ParseCcDataFromSei(seiData, fieldData) && state != null)
                        {
                            state.CaptionDataSeen = true;
                        }
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
        /// <param name="data">Video data</param>
        /// <param name="codec">Video codec</param>
        /// <param name="fieldData">cc_data found is added here</param>
        /// <param name="state">Optional per stream state - locks the MPEG-2 user data format</param>
        public static void ParseCcDataFromStartCodeStream(ReadOnlySpan<byte> data, CcVideoCodec codec, List<CcData> fieldData, CcDataParseState state = null)
        {
            var topFieldFirst = true; // from the MPEG-2 picture coding extension, which comes before the user data
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
                ParseStartCodeUnit(data.Slice(unitStart, unitEnd - unitStart), codec, fieldData, state, ref topFieldFirst);
                position = next < 0 ? -1 : unitEnd;
            }
        }

        private static void ParseStartCodeUnit(ReadOnlySpan<byte> unit, CcVideoCodec codec, List<CcData> fieldData, CcDataParseState state, ref bool topFieldFirst)
        {
            if (unit.Length < 2)
            {
                return;
            }

            var b = unit[0];
            var isMpeg2 = codec == CcVideoCodec.Mpeg2 || codec == CcVideoCodec.Unknown;
            if (isMpeg2 && b == 0xB5 && unit.Length >= 5 && (unit[1] >> 4) == 8)
            {
                // picture coding extension: 4-bit id, 4 x 4-bit f_code, intra_dc_precision (2),
                // picture_structure (2), then top_field_first
                topFieldFirst = (unit[4] & 0x80) != 0;
            }
            else if (isMpeg2 && b == 0xB2)
            {
                ParseCcDataFromMpeg2UserData(unit.Slice(1), topFieldFirst, fieldData, state);
            }
            else if ((codec == CcVideoCodec.H264 || codec == CcVideoCodec.Unknown) && (b & 0x9F) == 0x06)
            {
                ParseCcDataFromSeiNalPayload(unit.Slice(1), fieldData, state); // H.264 SEI
            }
            else if ((codec == CcVideoCodec.H265 || codec == CcVideoCodec.Unknown) && (b & 0x81) == 0 && ((b >> 1) == 39 || (b >> 1) == 40))
            {
                ParseCcDataFromSeiNalPayload(unit.Slice(2), fieldData, state); // H.265 prefix/suffix SEI
            }
        }

        /// <summary>
        /// Parses cc_data from MPEG-2 video user data (the bytes after the 00 00 01 B2 start code) in
        /// ATSC A/53 or SCTE 20 form. With a state, the stream is locked to the first form found, so
        /// captions sent in both forms are not decoded twice.
        /// </summary>
        public static void ParseCcDataFromMpeg2UserData(ReadOnlySpan<byte> userData, bool topFieldFirst, List<CcData> fieldData, CcDataParseState state)
        {
            if (state == null)
            {
                ParseCcDataFromAtscUserData(userData, fieldData);
                ParseCcDataFromScte20UserData(userData, topFieldFirst, fieldData);
                return;
            }

            if (state.UserDataFormat != CcUserDataFormat.Scte20 && ParseCcDataFromAtscUserData(userData, fieldData))
            {
                state.UserDataFormat = CcUserDataFormat.Atsc;
                state.CaptionDataSeen = true;
            }
            else if (state.UserDataFormat != CcUserDataFormat.Atsc && ParseCcDataFromScte20UserData(userData, topFieldFirst, fieldData))
            {
                state.UserDataFormat = CcUserDataFormat.Scte20;
                state.CaptionDataSeen = true;
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

        /// <returns>True if a cc_data SEI message was found (also if it only had padding)</returns>
        public static bool ParseCcDataFromSei(byte[] buffer, List<CcData> fieldData)
        {
            var found = false;
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
                    found = true;
                }

                x += payloadSize;
            }

            return found;
        }

        /// <summary>
        /// Parses cc_data from an H.264/H.265 SEI NAL unit payload (the bytes after the NAL unit
        /// header, still containing emulation prevention bytes).
        /// </summary>
        public static void ParseCcDataFromSeiNalPayload(ReadOnlySpan<byte> escapedSeiPayload, List<CcData> fieldData, CcDataParseState state = null)
        {
            if (escapedSeiPayload.Length < 12)
            {
                return;
            }

            if (ParseCcDataFromSei(UnescapeSeiData(escapedSeiPayload), fieldData) && state != null)
            {
                state.CaptionDataSeen = true;
            }
        }

        /// <summary>
        /// Parses cc_data from MPEG-2 video user data (the bytes after the 00 00 01 B2 start code)
        /// in ATSC A/53 form: "GA94", user_data_type_code 3, flags/cc_count, em_data, triplets.
        /// </summary>
        /// <returns>True if this is ATSC A/53 cc_data (also if it only had padding)</returns>
        public static bool ParseCcDataFromAtscUserData(ReadOnlySpan<byte> userData, List<CcData> fieldData)
        {
            if (userData.Length < 10 ||
                BinaryPrimitives.ReadUInt32BigEndian(userData) != 0x47413934 || // "GA94"
                userData[4] != 0x03) // cc_data
            {
                return false;
            }

            if ((userData[5] & 0x40) != 0) // process_cc_data_flag
            {
                AddCcTriplets(userData, 7, userData[5] & 0x1F, fieldData);
            }

            return true;
        }

        /// <summary>
        /// Parses cc_data from MPEG-2 video user data (the bytes after the 00 00 01 B2 start code) in
        /// SCTE 20 form - used by US cable before ATSC A/53: user_data_type_code 3, then a 5-bit
        /// cc_count and per pair 2-bit priority, 2-bit field number, 5-bit line offset, the two
        /// caption bytes (sent least significant bit first) and a marker bit.
        /// </summary>
        /// <param name="userData">User data after the start code</param>
        /// <param name="topFieldFirst">From the picture coding extension - field numbers are in transmission order</param>
        /// <param name="fieldData">cc_data found is added here (as CEA-608 field 1/2 pairs)</param>
        /// <returns>True if this is SCTE 20 cc_data (also if it only had padding)</returns>
        public static bool ParseCcDataFromScte20UserData(ReadOnlySpan<byte> userData, bool topFieldFirst, List<CcData> fieldData)
        {
            if (userData.Length < 3 || userData[0] != 0x03 || (userData[1] & 0x7F) != 0x01)
            {
                return false;
            }

            var bitPosition = 16;
            var totalBits = userData.Length * 8;
            var ccCount = ReadBits(userData, ref bitPosition, 5);
            for (var i = 0; i < ccCount && bitPosition + 26 <= totalBits; i++)
            {
                bitPosition += 2; // priority
                var field = ReadBits(userData, ref bitPosition, 2);
                bitPosition += 5; // line offset
                var ccData1 = ReverseBits(ReadBits(userData, ref bitPosition, 8));
                var ccData2 = ReverseBits(ReadBits(userData, ref bitPosition, 8));
                bitPosition += 1; // marker

                // field 1 = odd field, 2 = even field, 3 = repeated odd field, 0 = forbidden
                if (field == 0 || !IsNonEmptyCcData(ccData1, ccData2))
                {
                    continue;
                }

                var ccType = field == 2 ? 1 : 0;
                if (!topFieldFirst)
                {
                    ccType = 1 - ccType;
                }

                fieldData.Add(new CcData(ccType, ccData1, ccData2));
            }

            return true;
        }

        /// <summary>
        /// Parses DVD style Line 21 captions from MPEG-2 video user data (the bytes after the
        /// 00 00 01 B2 start code): "CC", user_data_type_code 1, caption_block_size 0xF8, a flags
        /// byte (bit 7 = odd field first), then 6-byte caption blocks - two caption words, each a
        /// marker byte (0xFF/0xFE) and two caption bytes. One user data packet at the start of a GOP
        /// holds the captions of the whole GOP, one block per frame.
        /// </summary>
        /// <returns>The cc_data of each frame (CEA-608 field 1/2 pairs), in frame order - empty if this is not DVD caption data</returns>
        public static List<CcData[]> ParseDvdCaptionUserData(ReadOnlySpan<byte> userData)
        {
            var frames = new List<CcData[]>();
            if (userData.Length < 11 || userData[0] != 0x43 || userData[1] != 0x43 || userData[2] != 0x01 || userData[3] != 0xF8)
            {
                return frames;
            }

            // The caption_block_count in the flags byte is often wrong - count the blocks instead,
            // like ffmpeg does, and map the fields the same way.
            var oddFieldFirst = (userData[4] & 0x80) != 0;
            for (var i = 5; i + 6 <= userData.Length && (userData[i] & 0xFE) == 0xFE; i += 6)
            {
                var frame = new List<CcData>(2);
                AddDvdCaptionWord(frame, userData[i] == 0xFF && oddFieldFirst ? 0 : 1, userData[i + 1], userData[i + 2]);
                AddDvdCaptionWord(frame, userData[i + 3] == 0xFF && !oddFieldFirst ? 0 : 1, userData[i + 4], userData[i + 5]);
                frames.Add(frame.ToArray());
            }

            return frames;
        }

        private static void AddDvdCaptionWord(List<CcData> frame, int ccType, byte ccData1, byte ccData2)
        {
            if (IsNonEmptyCcData(ccData1, ccData2))
            {
                frame.Add(new CcData(ccType, ccData1, ccData2));
            }
        }

        private static int ReadBits(ReadOnlySpan<byte> data, ref int bitPosition, int count)
        {
            var value = 0;
            for (var i = 0; i < count; i++)
            {
                var bit = (data[bitPosition >> 3] >> (7 - (bitPosition & 7))) & 1;
                value = (value << 1) | bit;
                bitPosition++;
            }

            return value;
        }

        private static int ReverseBits(int b)
        {
            var result = 0;
            for (var i = 0; i < 8; i++)
            {
                result = (result << 1) | ((b >> i) & 1);
            }

            return result;
        }

        internal static void AddCcTriplets(ReadOnlySpan<byte> buffer, int pos, int count, List<CcData> fieldData)
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
