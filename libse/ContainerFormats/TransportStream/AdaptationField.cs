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

        public int ProgramClockReferenceBase { get; set; }
        public int ProgramClockReferenceExtension { get; set; }

        public int OriginalProgramClockReferenceBase { get; set; }
        public int OriginalProgramClockReferenceExtension { get; set; }

        public int SpliceCountdown { get; set; }

        public int TransportPrivateDataLength { get; set; }
        public byte[] TransportPrivateData { get; set; }

        public int AdaptationFieldExtensionLength { get; set; }

        public AdaptationField(byte[] packetBuffer)
        {
            Length = packetBuffer[4];
            DiscontinuityIndicator = 1 == packetBuffer[5] >> 7;
            RandomAccessIndicator = (packetBuffer[5] & 64) > 0; // and with 01000000 to get second byte
            ElementaryStreamPriorityIndicator = (packetBuffer[5] & 32) > 0; // and with 00100000 to get third byte
            PcrFlag = (packetBuffer[5] & 16) > 0; // and with 00010000 to get fourth byte
            OpcrFlag = (packetBuffer[5] & 8) > 0; // and with 00001000 to get fifth byte
            SplicingPointFlag = (packetBuffer[5] & 4) > 0; // and with 00000100 to get sixth byte
            TransportPrivateDataFlag = (packetBuffer[5] & 4) > 0; // and with 00000100 to get seventh byte
            AdaptationFieldExtensionFlag = (packetBuffer[5] & 2) > 0; // and with 00000010 to get 8th byte

            int index = 6;
            if (PcrFlag)
            {
                ProgramClockReferenceBase = (packetBuffer[index] * 256 + packetBuffer[index + 1]) << 1;
                ProgramClockReferenceBase += packetBuffer[index + 2] >> 7;
                ProgramClockReferenceExtension = (packetBuffer[index + 2] & Helper.B00000001) * 256 + packetBuffer[index + 3];
                index += 4;
            }

            if (OpcrFlag)
            {
                OriginalProgramClockReferenceBase = (packetBuffer[index] * 256 + packetBuffer[index + 1]) << 1;
                OriginalProgramClockReferenceBase += packetBuffer[index + 2] >> 7;
                OriginalProgramClockReferenceExtension = (packetBuffer[index + 2] & Helper.B00000001) * 256 + packetBuffer[index + 3];
                index += 4;
            }

            if (SplicingPointFlag)
            {
                SpliceCountdown = packetBuffer[index];
                index++;
            }

            if (TransportPrivateDataFlag)
            {
                TransportPrivateDataLength = packetBuffer[index];
                index++;
                TransportPrivateData = new byte[TransportPrivateDataLength];

                if (index + TransportPrivateDataLength <= packetBuffer.Length)
                {
                    Buffer.BlockCopy(packetBuffer, index, TransportPrivateData, 0, TransportPrivateDataLength);
                    index += TransportPrivateDataLength;
                }
            }

            if (AdaptationFieldExtensionFlag && index < packetBuffer.Length)
            {
                AdaptationFieldExtensionLength = packetBuffer[index];
            }
        }
    }
}
