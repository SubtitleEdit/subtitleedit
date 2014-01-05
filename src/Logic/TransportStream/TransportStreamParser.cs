using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{
    /// <summary>
    /// MPEG transport stream parser
    /// </summary>
    public class TransportStreamParser
    {
        public int NumberOfNullPackets { get; private set; }
        public long TotalNumberOfPackets { get; private set; }
        public long TotalNumberOfPrivateStream1 { get; private set; }
        public long TotalNumberOfPrivateStream1Continuation0 { get; private set; }
        public List<int> SubtitlePacketIds { get; private set; }
        public List<Packet> SubtitlePackets { get; private set; }
        public List<Packet> ProgramAssociationTables { get; private set; }

        public void ParseTsFile(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ParseTsFile(fs);
            fs.Close();
        }

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms">Input stream</param>
        public void ParseTsFile(Stream ms)
        {
            NumberOfNullPackets = 0;
            TotalNumberOfPackets = 0;
            TotalNumberOfPrivateStream1 = 0;
            TotalNumberOfPrivateStream1Continuation0 = 0;
            SubtitlePacketIds = new List<int>();
            SubtitlePackets = new List<Packet>();
            ProgramAssociationTables = new List<Packet>();
            ms.Position = 0;
            int packetLength = DeterminePacketLength(ms);
            var packetBuffer = new byte[packetLength];
            long position = 0;
            while (position < ms.Length)
            {
                ms.Seek(position, SeekOrigin.Begin);
                ms.Read(packetBuffer, 0, packetLength);
                byte syncByte = packetBuffer[0];
                if (syncByte == Packet.SynchronizationByte)
                {
                    var packet = new Packet(packetBuffer);

                    if (packet.IsNullPacket)
                    {
                        NumberOfNullPackets++;
                    }
                    else if (packet.IsProgramAssociationTable)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("PacketNo: " + TotalNumberOfPackets + 1);
                        sb.AppendLine("PacketId: " + packet.PacketId);
                        sb.AppendLine();
                        sb.AppendLine("TransportErrorIndicator: " + packet.TransportErrorIndicator);
                        sb.AppendLine("PayloadUnitStartIndicator: " + packet.PayloadUnitStartIndicator);
                        sb.AppendLine("TransportPriority: " + packet.TransportPriority);
                        sb.AppendLine("ScramblingControl: " + packet.ScramblingControl);
                        sb.AppendLine("AdaptationFieldExist: " + packet.AdaptationFieldControl);
                        sb.AppendLine("ContinuityCounter: " + packet.ContinuityCounter);
                        sb.AppendLine();
                        if (packet.AdaptationField != null)
                        {
                            sb.AppendLine("AdaptationFieldLength: " + packet.AdaptationField.Length);
                            sb.AppendLine("DiscontinuityIndicator: " + packet.AdaptationField.DiscontinuityIndicator);
                            sb.AppendLine("RandomAccessIndicator: " + packet.AdaptationField.RandomAccessIndicator);
                            sb.AppendLine("ElementaryStreamPriorityIndicator: " + packet.AdaptationField.ElementaryStreamPriorityIndicator);
                            sb.AppendLine("PcrFlag: " + packet.AdaptationField.PcrFlag);
                            sb.AppendLine("OpcrFlag: " + packet.AdaptationField.OpcrFlag);
                            sb.AppendLine("SplicingPointFlag: " + packet.AdaptationField.SplicingPointFlag);
                            sb.AppendLine("TransportPrivateDataFlag: " + packet.AdaptationField.TransportPrivateDataFlag);
                            sb.AppendLine("AdaptationFieldExtensionFlag: " + packet.AdaptationField.AdaptationFieldExtensionFlag);
                            sb.AppendLine();
                        }
                        sb.AppendLine("TableId: " + packet.ProgramAssociationTable.TableId);
                        sb.AppendLine("SectionLength: " + packet.ProgramAssociationTable.SectionLength);
                        sb.AppendLine("TransportStreamId: " + packet.ProgramAssociationTable.TransportStreamId);
                        sb.AppendLine("VersionNumber: " + packet.ProgramAssociationTable.VersionNumber);
                        sb.AppendLine("CurrentNextIndicator: " + packet.ProgramAssociationTable.CurrentNextIndicator);
                        sb.AppendLine("SectionNumber: " + packet.ProgramAssociationTable.SectionNumber);
                        sb.AppendLine("LastSectionNumber: " + packet.ProgramAssociationTable.LastSectionNumber);
                        ProgramAssociationTables.Add(packet);
                    }
                    else if (packet.IsPrivateStream1 || SubtitlePacketIds.Contains(packet.PacketId))
                    {
                        TotalNumberOfPrivateStream1++;

                        SubtitlePackets.Add(packet);

                        if (!SubtitlePacketIds.Contains(packet.PacketId))
                        {
                            SubtitlePacketIds.Add(packet.PacketId);
                        }
                        if (packet.ContinuityCounter == 0)
                        {
                            TotalNumberOfPrivateStream1Continuation0++;

                            int pes_extensionlength = 0;
                            if (12 + packet.AdaptionFieldLength < packetBuffer.Length)
                                pes_extensionlength = 0xFF & packetBuffer[12 + packet.AdaptionFieldLength];
                            int pes_offset = 13 + packet.AdaptionFieldLength + pes_extensionlength;
                            bool isTeletext = (pes_extensionlength == 0x24 && (0xFF & packetBuffer[pes_offset]) >> 4 == 1);

                            // workaround uk freesat teletext
                            if (!isTeletext)
                                isTeletext = (pes_extensionlength == 0x24 && (0xFF & packetBuffer[pes_offset]) == 0x99);

                            if (!isTeletext)
                            {

                            }
                        }
                    }
                    TotalNumberOfPackets++;
                    position += packetLength;
                }
                else
                {
                    position++;
                    break;
                }
            }
        }

        public List<DvbSubPes> GetSubtitlePesPackets(int packetId)
        {
            var list = new List<DvbSubPes>();
            int last = -1;
            List<Packet> packetList = new List<Packet>();
            foreach (Packet packet in SubtitlePackets)
            {
                if (packet.PacketId == packetId)
                {
                    if (packet.PayloadUnitStartIndicator)
                    {
                        if (packetList.Count > 0)
                            AddPesPacket(list, packetList);
                        packetList = new List<Packet>();
                    }
                    if (packet.Payload != null && last != packet.ContinuityCounter)
                        packetList.Add(packet);
                    last = packet.ContinuityCounter;
                }
            }
            if (packetList.Count > 0)
                AddPesPacket(list, packetList);
            return list;
        }

        private static void AddPesPacket(List<DvbSubPes> list, List<Packet> packetList)
        {
            int bufferSize = 0;
            foreach (Packet p in packetList)
                bufferSize += p.Payload.Length;
            var pesData = new byte[bufferSize];
            int pesIndex = 0;
            foreach (Packet p in packetList)
            {
                Buffer.BlockCopy(p.Payload, 0, pesData, pesIndex, p.Payload.Length);
                pesIndex += p.Payload.Length;
            }

            DvbSubPes pes;
            if (Nikse.SubtitleEdit.Logic.VobSub.VobSubParser.IsMpeg2PackHeader(pesData))
            {
                pes = new DvbSubPes(pesData, Nikse.SubtitleEdit.Logic.VobSub.Mpeg2Header.Length);
            }
            else if (Nikse.SubtitleEdit.Logic.VobSub.VobSubParser.IsPrivateStream1(pesData, 0))
            {
                pes = new DvbSubPes(pesData, 0);
            }
            else
            {
                pes = new DvbSubPes(pesData, 0);
            }
            list.Add(pes);
        }

        private int DeterminePacketLength(Stream ms)
        {
            return 188;
        }

    }
}
