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
        /// Incremented only when a payload is present (AdaptationFieldExist = 10 or 11).
        /// Starts at 0, after 15 the CC re-starts from 0.
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

        /// <summary>
        /// Program Identifier, read straight from the packet header - the same value
        /// <see cref="PacketId"/> ends up with, without constructing a <see cref="Packet"/>.
        /// </summary>
        public static int PeekPacketId(byte[] packetBuffer)
        {
            return (packetBuffer[1] & 31) * 256 + packetBuffer[2];
        }

        /// <summary>
        /// True when this packet's payload starts with the PES marker for private stream 1
        /// (0xbd - subtitles), computed straight from the raw packet buffer. Deliberately does
        /// NOT also match video packets (0xE0-0xEF): <see cref="TransportStreamParser"/>'s main
        /// loop only needs this check after <c>firstVideoMs</c> is already known, at which point
        /// video packets are never inspected again - matching them here would allocate a
        /// <see cref="Packet"/> for every video frame's first packet for nothing.
        /// <para>
        /// <see cref="Packet"/>'s constructor always copies the payload into a fresh byte[], and
        /// almost every packet in a real file (audio/video content packets) was paying for that
        /// copy just to be inspected via <see cref="IsPrivateStream1"/> and discarded immediately.
        /// </para>
        /// </summary>
        public static bool PeekIsPrivateStream1(byte[] packetBuffer)
        {
            var adaptationFieldControl = (packetBuffer[3] & 48) >> 4;
            if (adaptationFieldControl != 0b00000001 && adaptationFieldControl != 0b00000011)
            {
                return false; // no payload
            }

            var payloadStart = 4;
            if (adaptationFieldControl == 0b00000011)
            {
                // Matches the constructor below exactly: payloadStart = 4 + 1 + AdaptationField.Length,
                // where AdaptationField.Length is the raw byte at offset 4 (no extra +1 here - that
                // +1 belongs only to the separate, differently-used AdaptionFieldLength property).
                payloadStart += 1 + (0xFF & packetBuffer[4]);
            }

            if (payloadStart + 3 >= packetBuffer.Length)
            {
                return false;
            }

            return packetBuffer[payloadStart] == 0 &&
                   packetBuffer[payloadStart + 1] == 0 &&
                   packetBuffer[payloadStart + 2] == 1 &&
                   packetBuffer[payloadStart + 3] == 0xbd;
        }

        /// <summary>
        /// Reads the PTS (90 kHz) of a video PES (stream id 0xE0-0xEF) that starts in this packet,
        /// straight from the raw packet buffer.
        /// </summary>
        public static bool TryPeekVideoPresentationTimestamp(byte[] packetBuffer, out ulong presentationTimestamp)
        {
            presentationTimestamp = 0;
            if ((packetBuffer[1] & 0x40) == 0)
            {
                return false; // no PES starts here
            }

            var payloadStart = GetPayloadStart(packetBuffer);
            if (payloadStart < 0 || payloadStart + 14 > packetBuffer.Length)
            {
                return false;
            }

            var p = payloadStart;
            if (packetBuffer[p] != 0 || packetBuffer[p + 1] != 0 || packetBuffer[p + 2] != 1 ||
                packetBuffer[p + 3] < 0xE0 || packetBuffer[p + 3] > 0xEF ||
                (packetBuffer[p + 7] & 0x80) == 0)
            {
                return false;
            }

            presentationTimestamp = ReadTimestamp(packetBuffer, p + 9);
            return true;
        }

        /// <summary>
        /// Reads the PTS (90 kHz) of any PES that starts in this packet - for a PID whose kind the
        /// program map table already tells (VC-1 video and Blu-ray audio use stream ids outside
        /// the MPEG video/audio ranges).
        /// </summary>
        public static bool TryPeekPresentationTimestamp(byte[] packetBuffer, out ulong presentationTimestamp)
        {
            presentationTimestamp = 0;
            if ((packetBuffer[1] & 0x40) == 0)
            {
                return false; // no PES starts here
            }

            var payloadStart = GetPayloadStart(packetBuffer);
            if (payloadStart < 0 || payloadStart + 14 > packetBuffer.Length)
            {
                return false;
            }

            var p = payloadStart;
            if (packetBuffer[p] != 0 || packetBuffer[p + 1] != 0 || packetBuffer[p + 2] != 1 || (packetBuffer[p + 7] & 0x80) == 0)
            {
                return false;
            }

            presentationTimestamp = ReadTimestamp(packetBuffer, p + 9);
            return true;
        }

        /// <summary>
        /// Reads the PTS (90 kHz) of an audio PES that starts in this packet: MPEG audio/AAC
        /// (stream id 0xC0-0xDF), or AC-3/E-AC-3/DTS in private stream 1 (0xBD) or an extended
        /// stream (0xFD, Blu-ray) - told apart from the subtitles and teletext that share private
        /// stream 1 by the sync word that starts the payload.
        /// </summary>
        public static bool TryPeekAudioPresentationTimestamp(byte[] packetBuffer, out ulong presentationTimestamp)
        {
            presentationTimestamp = 0;
            if ((packetBuffer[1] & 0x40) == 0)
            {
                return false; // no PES starts here
            }

            var payloadStart = GetPayloadStart(packetBuffer);
            if (payloadStart < 0 || payloadStart + 14 > packetBuffer.Length)
            {
                return false;
            }

            var p = payloadStart;
            if (packetBuffer[p] != 0 || packetBuffer[p + 1] != 0 || packetBuffer[p + 2] != 1 || (packetBuffer[p + 7] & 0x80) == 0)
            {
                return false;
            }

            var streamId = packetBuffer[p + 3];
            if (streamId == 0xBD || streamId == 0xFD)
            {
                var data = p + 9 + packetBuffer[p + 8];
                var isAc3 = data + 2 <= packetBuffer.Length && packetBuffer[data] == 0x0B && packetBuffer[data + 1] == 0x77;
                var isDts = data + 4 <= packetBuffer.Length && packetBuffer[data] == 0x7F && packetBuffer[data + 1] == 0xFE &&
                            packetBuffer[data + 2] == 0x80 && packetBuffer[data + 3] == 0x01;
                if (!isAc3 && !isDts)
                {
                    return false;
                }
            }
            else if (streamId < 0xC0 || streamId > 0xDF)
            {
                return false;
            }

            presentationTimestamp = ReadTimestamp(packetBuffer, p + 9);
            return true;
        }

        /// <summary>
        /// Reads the program clock reference base (90 kHz) from the adaptation field, if present.
        /// </summary>
        public static bool TryPeekProgramClockReference(byte[] packetBuffer, out ulong programClockReference)
        {
            programClockReference = 0;
            if ((packetBuffer[3] & 0x20) == 0 || packetBuffer[4] < 7 || (packetBuffer[5] & 0x10) == 0)
            {
                return false;
            }

            programClockReference = ((ulong)packetBuffer[6] << 25) |
                                    ((ulong)packetBuffer[7] << 17) |
                                    ((ulong)packetBuffer[8] << 9) |
                                    ((ulong)packetBuffer[9] << 1) |
                                    ((ulong)packetBuffer[10] >> 7);
            return true;
        }

        /// <summary>
        /// Decodes a 33-bit PES time stamp (PTS/DTS) stored in five bytes.
        /// </summary>
        public static ulong ReadTimestamp(byte[] buffer, int index)
        {
            return ((ulong)(buffer[index] & 0b00001110) << 29) |
                   ((ulong)buffer[index + 1] << 22) |
                   ((ulong)(buffer[index + 2] & 0b11111110) << 14) |
                   ((ulong)buffer[index + 3] << 7) |
                   ((ulong)buffer[index + 4] >> 1);
        }

        private static int GetPayloadStart(byte[] packetBuffer)
        {
            var adaptationFieldControl = (packetBuffer[3] & 48) >> 4;
            if (adaptationFieldControl == 0b00000001)
            {
                return 4;
            }

            if (adaptationFieldControl == 0b00000011)
            {
                return 5 + packetBuffer[4];
            }

            return -1; // no payload
        }

        /// <summary>
        /// Program clock reference (90 kHz) of the packet's program when the packet was read - the
        /// time stamp of last resort for PES packets sent without a PTS (e.g. teletext from some
        /// Topfield recorders).
        /// </summary>
        public ulong? ArrivalTimestamp { get; set; }

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

            if (AdaptationFieldControl == 0b00000010 || AdaptationFieldControl == 0b00000011)
            {
                AdaptationField = new AdaptationField(packetBuffer);
            }

            if (AdaptationFieldControl == 0b00000001 || AdaptationFieldControl == 0b00000011) // Payload exists -  binary '01' || '11'
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
