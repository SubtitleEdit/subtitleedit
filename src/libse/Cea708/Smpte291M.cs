using Nikse.SubtitleEdit.Core.Cea708.Commands;

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
        public int CaptionDistributionPacketHeaderSequenceCounter { get; set; }
        public int CaptionDistributionPacketDataSection { get; set; }
        public CaptionDistributionPacket CaptionDistributionPacketCcData { get; set; }
        public CcServiceInfoSection CcServiceInfoSection { get; set; }
        public int CaptionDistributionPacketFooterSection { get; set; }
        public int CaptionDistributionPacketHeaderSequenceCounter2 { get; set; }
        public int CaptionDistributionPacketChecksum { get; set; }

        private CommandState _state;

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

        public Smpte291M(byte[] bytes)
        {
            DataId = bytes[0];
            SecondaryDataId = bytes[1];
            DataCount = bytes[2];
            CaptionDistributionPacketId = (bytes[3] << 8) + bytes[4];
            CaptionDistributionPacketDataCount = bytes[5];
            CaptionDistributionPacketFramingRate = (bytes[6] >> 4);
            CaptionDistributionPacketTimeCodeAdded = (bytes[7] & 0b10000000) > 0;
            CaptionDistributionPacketDataBlockAdded = (bytes[7] & 0b01000000) > 0;
            CaptionDistributionPacketServiceInfoAdded = (bytes[7] & 0b00100000) > 0;
            CaptionDistributionPacketServiceInfoStart = (bytes[7] & 0b00010000) > 0;
            CaptionDistributionPacketServiceInfoChanged = (bytes[7] & 0b00001000) > 0;
            CaptionDistributionPacketServiceInfoEnd = (bytes[7] & 0b00000100) > 0;
            CaptionDistributionPacketContainsCaptions = (bytes[7] & 0b00000010) > 0;
            CaptionDistributionPacketHeaderSequenceCounter = (bytes[8] << 8) + bytes[9];
            CaptionDistributionPacketDataSection = bytes[10];

            CaptionDistributionPacketCcData = new CaptionDistributionPacket(bytes, 10);

            var idx = 9 + CaptionDistributionPacketCcData.GetLength();

            if (CaptionDistributionPacketServiceInfoAdded)
            {
                CcServiceInfoSection = new CcServiceInfoSection(bytes, idx + 1);
                idx += CcServiceInfoSection.GetLength();
            }

            CaptionDistributionPacketFooterSection = bytes[1 + idx];
            CaptionDistributionPacketHeaderSequenceCounter2 = (bytes[2 + idx] << 8) + bytes[3 + idx];
            CaptionDistributionPacketChecksum = bytes[4 + idx];

            _state = new CommandState();
        }

        public string GetText(bool flush)
        {
            return CaptionDistributionPacketCcData.GetText(_state, flush);
        }
    }
}
