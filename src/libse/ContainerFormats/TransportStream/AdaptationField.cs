using System;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class AdaptationField
    {
        /// <summary>
        /// Number of bytes in the adaptation field immediately following the Length
        /// </summary>
        public int Length { get; set; }

        public bool DiscontinuityIndicator { get; set; }
        public bool RandomAccessIndicator { get; set; }
        public bool ElementaryStreamPriorityIndicator { get; set; }

        /// <summary>
        /// '1' indicates that the adaptation field contains a PCR field coded in two parts
        /// </summary>
        public bool PcrFlag { get; set; }

        /// <summary>
        /// '1' indicates that the adaptation field contains an OPCR field coded in two parts
        /// </summary>
        public bool OpcrFlag { get; set; }

        /// <summary>
        /// '1' indicates that a splice countdown field shall be present in the associated adaptation field
        /// </summary>
        public bool SplicingPointFlag { get; set; }

        /// <summary>
        /// 1' indicates that the adaptation field contains one or more private data bytes
        /// </summary>
        public bool TransportPrivateDataFlag { get; set; }

        /// <summary>
        /// '1' indicates the presence of an adaptation field extension
        /// </summary>
        public bool AdaptationFieldExtensionFlag { get; set; }

        public ulong ProgramClockReferenceBase { get; set; }
        public int ProgramClockReferenceExtension { get; set; }

        public ulong OriginalProgramClockReferenceBase { get; set; }
        public int OriginalProgramClockReferenceExtension { get; set; }

        public int SpliceCountdown { get; set; }

        public int TransportPrivateDataLength { get; set; }
        public byte[] TransportPrivateData { get; set; }

        public int AdaptationFieldExtensionLength { get; set; }

        public AdaptationField(byte[] packetBuffer)
        {
            Length = packetBuffer[4];
            if (Length == 0)
            {
                return; // a single stuffing byte - no flags
            }

            DiscontinuityIndicator = (packetBuffer[5] & 0b1000_0000) > 0;
            RandomAccessIndicator = (packetBuffer[5] & 0b0100_0000) > 0;
            ElementaryStreamPriorityIndicator = (packetBuffer[5] & 0b0010_0000) > 0;
            PcrFlag = (packetBuffer[5] & 0b0001_0000) > 0;
            OpcrFlag = (packetBuffer[5] & 0b0000_1000) > 0;
            SplicingPointFlag = (packetBuffer[5] & 0b0000_0100) > 0;
            TransportPrivateDataFlag = (packetBuffer[5] & 0b0000_0010) > 0;
            AdaptationFieldExtensionFlag = (packetBuffer[5] & 0b0000_0001) > 0;

            // the adaptation field ends at packet byte 4 + Length
            var end = Math.Min(packetBuffer.Length, 5 + Length);
            var index = 6;
            if (PcrFlag && index + 6 <= end)
            {
                ProgramClockReferenceBase = ReadClockReferenceBase(packetBuffer, index);
                ProgramClockReferenceExtension = (packetBuffer[index + 4] & 0b0000_0001) * 256 + packetBuffer[index + 5];
                index += 6;
            }

            if (OpcrFlag && index + 6 <= end)
            {
                OriginalProgramClockReferenceBase = ReadClockReferenceBase(packetBuffer, index);
                OriginalProgramClockReferenceExtension = (packetBuffer[index + 4] & 0b0000_0001) * 256 + packetBuffer[index + 5];
                index += 6;
            }

            if (SplicingPointFlag && index < end)
            {
                SpliceCountdown = packetBuffer[index];
                index++;
            }

            if (TransportPrivateDataFlag && index < end)
            {
                TransportPrivateDataLength = packetBuffer[index];
                index++;
                TransportPrivateData = new byte[TransportPrivateDataLength];

                if (index + TransportPrivateDataLength <= end)
                {
                    Buffer.BlockCopy(packetBuffer, index, TransportPrivateData, 0, TransportPrivateDataLength);
                    index += TransportPrivateDataLength;
                }
            }

            if (AdaptationFieldExtensionFlag && index < end)
            {
                AdaptationFieldExtensionLength = packetBuffer[index];
            }
        }

        /// <summary>
        /// 33-bit clock reference base (90 kHz) - followed by 6 reserved bits and a 9-bit extension.
        /// </summary>
        private static ulong ReadClockReferenceBase(byte[] buffer, int index)
        {
            return ((ulong)buffer[index] << 25) |
                   ((ulong)buffer[index + 1] << 17) |
                   ((ulong)buffer[index + 2] << 9) |
                   ((ulong)buffer[index + 3] << 1) |
                   ((ulong)buffer[index + 4] >> 7);
        }
    }
}
