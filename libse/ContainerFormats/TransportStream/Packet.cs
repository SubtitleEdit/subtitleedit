using System;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// MPEG transport stream packet
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// ID byte of TS Packet
        /// </summary>
        public const byte SynchronizationByte = 0x47; // 74 decimal, or 01000111 binary

        /// <summary>
        /// Null packets can ensure that the stream maintains a constant bit rate. Null packets is to be ignored
        /// </summary>
        public const int NullPacketId = 0x1FFF;

        /// <summary>
        /// Program Association Table: lists all programs available in the transport stream.
        /// </summary>
        public const int ProgramAssociationTablePacketId = 0;

        /// <summary>
        /// Start of PES data or PSI
        /// </summary>
        public bool PayloadUnitStartIndicator { get; set; }

        /// <summary>
        /// Program Identifier
        /// </summary>
        public int PacketId { get; set; }

        /// <summary>
        /// 1 = no adaptation fields (payload only), 10 = adaptation field only, 11 = adaptation field and payload
        /// </summary>
        public int AdaptationFieldControl { get; set; }

        /// <summary>
        /// Incremented only when a payload is present (AdaptationFieldExist = 10 or 11). Starts at 0.
        /// </summary>
        public int ContinuityCounter { get; set; }

        public int AdaptionFieldLength { get; set; }

        public AdaptationField AdaptationField { get; }

        public bool IsNullPacket => PacketId == NullPacketId;

        public bool IsProgramAssociationTable => PacketId == ProgramAssociationTablePacketId;

        public byte[] Payload { get; }

        public bool IsPrivateStream1
        {
            get
            {
                if (Payload == null || Payload.Length < 4)
                {
                    return false;
                }

                return Payload[0] == 0 &&
                       Payload[1] == 0 &&
                       Payload[2] == 1 &&
                       Payload[3] == 0xbd; // 0xbd == 189 - MPEG-2 Private stream 1 (non MPEG audio, sub pictures)
            }
        }

        public bool IsVideoStream
        {
            get
            {
                if (Payload == null || Payload.Length < 4)
                {
                    return false;
                }

                return Payload[0] == 0 &&
                       Payload[1] == 0 &&
                       Payload[2] == 1 &&
                       Payload[3] >= 0xE0 &&
                       Payload[3] < 0xF0;
            }
        }

        public Packet(byte[] packetBuffer)
        {
            if (packetBuffer == null || packetBuffer.Length < 30)
            {
                return;
            }

            PayloadUnitStartIndicator = 1 == (packetBuffer[1] & 64) >> 6; // and with 01000000 to get second byte - 1 means start of PES data or PSI otherwise zero
            PacketId = (packetBuffer[1] & 31) * 256 + packetBuffer[2];// and with 00011111 to get last 5 bytes
            AdaptationFieldControl = (packetBuffer[3] & 48) >> 4; // and with 00110000, 01 = no adaptation fields (payload only), 10 = adaptation field only, 11 = adaptation field and payload
            ContinuityCounter = packetBuffer[3] & 15;
            AdaptionFieldLength = AdaptationFieldControl > 1 ? (0xFF & packetBuffer[4]) + 1 : 0;

            if (AdaptationFieldControl == Helper.B00000010 || AdaptationFieldControl == Helper.B00000011)
            {
                AdaptationField = new AdaptationField(packetBuffer);
            }

            if (AdaptationFieldControl == Helper.B00000001 || AdaptationFieldControl == Helper.B00000011) // Payload exists -  binary '01' || '11'
            {
                int payloadStart = 4;
                if (AdaptationField != null)
                {
                    payloadStart += 1 + AdaptationField.Length;
                }

                // Save payload
                int payloadLength = packetBuffer.Length - payloadStart;
                if (payloadLength < 0)
                {
                    return;
                }

                Payload = new byte[payloadLength];
                Buffer.BlockCopy(packetBuffer, payloadStart, Payload, 0, Payload.Length);
            }
        }

    }
}
