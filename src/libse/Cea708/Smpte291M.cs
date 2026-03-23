using System.Linq;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class Smpte291M
    {
        public int DataId { get; set; }
        public int SecondaryDataId { get; set; }
        public int DataCount { get; set; }
        public int CaptionDistributionPacketId { get; set; }
        public int CaptionDistributionPacketDataCount { get; set; }
        public int CaptionDistributionPacketFramingRate { get; set; }
        public bool CaptionDistributionPacketTimeCodeAdded { get; set; }
        public bool CaptionDistributionPacketDataBlockAdded { get; set; }
        public bool CaptionDistributionPacketServiceInfoAdded { get; set; }
        public bool CaptionDistributionPacketServiceInfoStart { get; set; }
        public bool CaptionDistributionPacketServiceInfoChanged { get; set; }
        public bool CaptionDistributionPacketServiceInfoEnd { get; set; }
        public bool CaptionDistributionPacketContainsCaptions { get; set; }
        /// <summary>
        /// Should start with zero and count one up for each packet.
        /// </summary>
        public int CaptionDistributionPacketHeaderSequenceCounter { get; set; }
        public CcDataSection CcDataSectionCcData { get; set; }
        public CcServiceInfoSection CcServiceInfoSection { get; set; }
        public int CaptionDistributionPacketFooterSection { get; set; }
        public int CaptionDistributionPacketHeaderSequenceCounter2 { get; set; }
        public int CaptionDistributionPacketChecksum { get; set; }

        public string GetFrameRateDisplay()
        {
            switch (CaptionDistributionPacketFramingRate)
            {
                case 1: return "24000 / 1001 (23.976)";
                case 2: return "24";
                case 3: return "25";
                case 4: return "30000 / 1001 (29.97)";
                case 5: return "30";
                case 6: return "50";
                case 7: return "60000 / 1001 (59.94)";
                case 8: return "60";
                default:
                    return "unknown";
            }
        }

        public Smpte291M(int sequenceCount, int ccDataCount, byte[] bytes)
        {
            DataId = 0x61;
            SecondaryDataId = 1;
            CaptionDistributionPacketId = 0x9669;
            CaptionDistributionPacketDataCount = 0x59;
            CaptionDistributionPacketFramingRate = 4; // 29.97
            CaptionDistributionPacketTimeCodeAdded = false;
            CaptionDistributionPacketDataBlockAdded = true;
            CaptionDistributionPacketServiceInfoAdded = true;
            CaptionDistributionPacketServiceInfoStart = true;
            CaptionDistributionPacketServiceInfoChanged = true;
            CaptionDistributionPacketServiceInfoEnd = true;
            CaptionDistributionPacketContainsCaptions = true;
            CaptionDistributionPacketHeaderSequenceCounter = sequenceCount;
            CaptionDistributionPacketHeaderSequenceCounter2 = sequenceCount;

            CcDataSectionCcData = new CcDataSection(ccDataCount, bytes, sequenceCount);
            CcServiceInfoSection = new CcServiceInfoSection();

            DataCount = 8 + CcDataSectionCcData.GetLength() + CcServiceInfoSection.GetLength();
        }

        public Smpte291M(byte[] bytes)
        {
            DataId = bytes[0];
            SecondaryDataId = bytes[1];
            DataCount = bytes[2];
            CaptionDistributionPacketId = (bytes[3] << 8) + bytes[4];
            CaptionDistributionPacketDataCount = bytes[5];
            CaptionDistributionPacketFramingRate = bytes[6] >> 4;
            CaptionDistributionPacketTimeCodeAdded = (bytes[7] & 0b10000000) > 0;
            CaptionDistributionPacketDataBlockAdded = (bytes[7] & 0b01000000) > 0;
            CaptionDistributionPacketServiceInfoAdded = (bytes[7] & 0b00100000) > 0;
            CaptionDistributionPacketServiceInfoStart = (bytes[7] & 0b00010000) > 0;
            CaptionDistributionPacketServiceInfoChanged = (bytes[7] & 0b00001000) > 0;
            CaptionDistributionPacketServiceInfoEnd = (bytes[7] & 0b00000100) > 0;
            CaptionDistributionPacketContainsCaptions = (bytes[7] & 0b00000010) > 0;
            CaptionDistributionPacketHeaderSequenceCounter = (bytes[8] << 8) + bytes[9];

            CcDataSectionCcData = new CcDataSection(bytes, 10);

            var idx = 9 + CcDataSectionCcData.GetLength();

            if (CaptionDistributionPacketServiceInfoAdded)
            {
                CcServiceInfoSection = new CcServiceInfoSection(bytes, idx + 1);
                idx += CcServiceInfoSection.GetLength();
            }

            CaptionDistributionPacketFooterSection = bytes[1 + idx];
            CaptionDistributionPacketHeaderSequenceCounter2 = (bytes[2 + idx] << 8) + bytes[3 + idx];
            CaptionDistributionPacketChecksum = bytes[4 + idx];
        }

        public string GetText(int lineIndex, bool flush, CommandState state)
        {
            return CcDataSectionCcData.GetText(lineIndex, state, flush);
        }

        public byte[] GetBytes()
        {
            var flags = (byte)((CaptionDistributionPacketTimeCodeAdded ? 0b10000000 : 0) |
                     (CaptionDistributionPacketDataBlockAdded ? 0b01000000 : 0) |
                     (CaptionDistributionPacketServiceInfoAdded ? 0b00100000 : 0) |
                     (CaptionDistributionPacketServiceInfoStart ? 0b00010000 : 0) |
                     (CaptionDistributionPacketServiceInfoChanged ? 0b00001000 : 0) |
                     (CaptionDistributionPacketServiceInfoEnd ? 0b00000100 : 0) |
                     (CaptionDistributionPacketContainsCaptions ? 0b00000010 : 0) |
                     0b00000001);

            var result = new[]
            {
                (byte)DataId,
                (byte)SecondaryDataId,
                (byte)DataCount,
                (byte)(CaptionDistributionPacketId >> 8),
                (byte)(CaptionDistributionPacketId & 0b0000000011111111),
                (byte)CaptionDistributionPacketDataCount,
                (byte)((CaptionDistributionPacketFramingRate << 4) + 0b00001111),
                flags,
                (byte)(CaptionDistributionPacketHeaderSequenceCounter >> 8),
                (byte)(CaptionDistributionPacketHeaderSequenceCounter & 0b0000000011111111),
            }
                .Concat(CcDataSectionCcData.GetBytes())
                .Concat(CcServiceInfoSection.GetBytes())
                .Concat(new byte[]
                {
                    0x74, // footer id
                    (byte)(CaptionDistributionPacketHeaderSequenceCounter2 >> 8),
                    (byte)(CaptionDistributionPacketHeaderSequenceCounter2 & 0b11111111),
                    0, // checksum - will be replaced below
                    0xbb
                }).ToArray();

            // Data count
            result[2] = (byte)(result.Length - 4);
            result[5] = (byte)(result.Length - 4);

            // Calculate number that will make the checksum zero
            long total = 0;
            for (var i = 3; i < result.Length - 2; i++)
            {
                total += result[i];
            }
            var checksumToZero = (byte)(256 - total % 256);
            result[result.Length - 2] = checksumToZero;

            return result;
        }
    }
}
