using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    /// <summary>
    /// Builds the ATSC A/53 caption data of each video frame - "GA94", user_data_type_code 3 and
    /// cc_data - from CEA-608 byte pairs on 29.97 Hz slots and CEA-708 DTVCC triplets. The same
    /// bytes go in MPEG-2 picture user data and in H.264/H.265 SEI (ITU-T T.35) messages.
    /// Frames must be given in display order: decoders put the cc_data in display order, so that
    /// is the order the DTVCC bytes must have.
    /// </summary>
    public sealed class A53CcDataBuilder
    {
        /// <summary>
        /// A/53 allows 600 cc_data triplets per second (CEA-708's 9600 bit/s plus CEA-608):
        /// cc_count 20 at 29.97 fps, 10 at 59.94, 25 at 23.976.
        /// </summary>
        private const double MaxTripletsPerMillisecond = 0.6;

        private readonly Dictionary<long, SccBytePairs.TimedPair> _field1;
        private readonly Dictionary<long, SccBytePairs.TimedPair> _field2;
        private readonly List<ClosedCaptionBytes.DtvccTriplet> _dtvcc;
        private int _dtvccIndex;

        public A53CcDataBuilder(ClosedCaptionBytes captions)
        {
            _field1 = ToDictionary(captions?.Field1);
            _field2 = ToDictionary(captions?.Field2);
            _dtvcc = captions?.Dtvcc ?? new List<ClosedCaptionBytes.DtvccTriplet>();
        }

        /// <summary>
        /// The caption data of the frame shown from <paramref name="startMs"/> to
        /// <paramref name="endMs"/>: the CEA-608 slots that start while it is shown (one per frame
        /// at 29.97 fps, sometimes two at 23.976, none every other frame at 59.94), then the
        /// CEA-708 data that is due, as much as fits.
        /// </summary>
        /// <returns>"GA94", 0x03, cc_data() and marker_bits 0xFF</returns>
        public byte[] NextFrame(double startMs, double endMs)
        {
            var firstSlot = (long)Math.Ceiling(startMs / SccBytePairs.SlotMilliseconds - 0.001);
            var endSlot = (long)Math.Ceiling(endMs / SccBytePairs.SlotMilliseconds - 0.001);
            var slotCount = (int)Math.Min(15, Math.Max(0, endSlot - firstSlot));

            // CEA-708 data sent by now, up to the frame's share of the A/53 bandwidth
            var maxCcCount = Math.Min(31, Math.Max(2 * slotCount + 1, (int)(MaxTripletsPerMillisecond * (endMs - startMs))));
            var dtvccStart = _dtvccIndex;
            while (_dtvccIndex < _dtvcc.Count && _dtvcc[_dtvccIndex].Milliseconds < endMs && 2 * slotCount + _dtvccIndex - dtvccStart < maxCcCount)
            {
                _dtvccIndex++;
            }

            return MakeCcData(firstSlot, slotCount, dtvccStart, _dtvccIndex - dtvccStart);
        }

        private static Dictionary<long, SccBytePairs.TimedPair> ToDictionary(IReadOnlyList<SccBytePairs.TimedPair> pairs)
        {
            var result = new Dictionary<long, SccBytePairs.TimedPair>();
            if (pairs != null)
            {
                foreach (var pair in pairs)
                {
                    result[pair.Slot] = pair;
                }
            }

            return result;
        }

        /// <summary>
        /// "GA94", user_data_type_code 3, process_cc_data_flag + cc_count, em_data, cc_count x
        /// (marker/cc_valid/cc_type, two bytes), marker_bits. CEA-608 comes first, one field 1 and
        /// one field 2 pair per slot - a slot with no byte pair sends the null pair 0x80 0x80. A
        /// frame without slot or CEA-708 data gets padding with cc_valid = 0.
        /// </summary>
        private byte[] MakeCcData(long firstSlot, int slotCount, int dtvccStart, int dtvccCount)
        {
            var padding = slotCount == 0 && dtvccCount == 0;
            var ccCount = padding ? 2 : 2 * slotCount + dtvccCount;
            var data = new byte[7 + ccCount * 3 + 1];
            data[0] = (byte)'G';
            data[1] = (byte)'A';
            data[2] = (byte)'9';
            data[3] = (byte)'4';
            data[4] = 0x03;
            data[5] = (byte)(0xC0 | ccCount); // reserved, process_cc_data_flag, additional_data_flag = 0
            data[6] = 0xFF; // em_data
            var index = 7;
            if (padding)
            {
                AddCcData(data, ref index, 0xF8, 0x80, 0x80);
                AddCcData(data, ref index, 0xF9, 0x80, 0x80);
            }

            for (var slot = firstSlot; slot < firstSlot + slotCount; slot++)
            {
                var hasPair1 = _field1.TryGetValue(slot, out var pair1);
                AddCcData(data, ref index, 0xFC, hasPair1 ? pair1.Data1 : (byte)0x80, hasPair1 ? pair1.Data2 : (byte)0x80);
                var hasPair2 = _field2.TryGetValue(slot, out var pair2);
                AddCcData(data, ref index, 0xFD, hasPair2 ? pair2.Data1 : (byte)0x80, hasPair2 ? pair2.Data2 : (byte)0x80);
            }

            for (var i = dtvccStart; i < dtvccStart + dtvccCount; i++)
            {
                var triplet = _dtvcc[i];
                AddCcData(data, ref index, (byte)(0xFC | triplet.Type), triplet.Data1, triplet.Data2);
            }

            data[index] = 0xFF; // marker_bits
            return data;
        }

        private static void AddCcData(byte[] data, ref int index, byte header, byte data1, byte data2)
        {
            data[index++] = header;
            data[index++] = data1;
            data[index++] = data2;
        }
    }
}
