using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

        public void ParseTsFile(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                    Packet packet = new Packet(packetBuffer);

                    if (packet.IsNullPacket)
                    {
                        NumberOfNullPackets++;
                    }
                    else if (packet.IsProgramAssociationTable)
                    {
                        StringBuilder sb = new StringBuilder();
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

                     //   MessageBox.Show(sb.ToString());
                    }
                    else if (packet.IsPrivateStream1 || SubtitlePacketIds.Contains(packet.PacketId))
                    {
                        TotalNumberOfPrivateStream1++;

                        SubtitlePackets.Add(packet);

                        if (!SubtitlePacketIds.Contains(packet.PacketId))
                        {
                            SubtitlePacketIds.Add(packet.PacketId);
//                            MessageBox.Show("Subtitle packet id=" + packet.PacketId);
                        }
                        if (packet.ContinuityCounter == 0)
                        {
                            TotalNumberOfPrivateStream1Continuation0++;

                            int pes_extensionlength = 0xFF &  packetBuffer[12 + packet.AdaptionFieldLength];
                            int pes_offset = 13 + packet.AdaptionFieldLength + pes_extensionlength;
						    bool isTeletext = (pes_extensionlength == 0x24 && (0xFF & packetBuffer[pes_offset])>>4 == 1);

							// workaround uk freesat teletext
							if (!isTeletext)
                                isTeletext = (pes_extensionlength == 0x24 && (0xFF & packetBuffer[pes_offset]) == 0x99);

                            //if (isTeletext)
                            //    MessageBox.Show(TotalNumberOfPackets.ToString() +  ": Teletext!");
                                //if (!isTeletext)
                                //    pes_subID = ((0xFF & ts_packet[pes_offset]) == 0x20 && (0xFF & ts_packet[pes_offset + 1]) == 0 && (0xFF & ts_packet[pes_offset + 2]) == 0xF) ? 0x20 : 0;

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
            //MessageBox.Show("Done parsing " + Environment.NewLine +
            //                Environment.NewLine +
            //                "#Packets: " + TotalNumberOfPackets.ToString() + Environment.NewLine +
            //                "#Null packets: " + NumberOfNullPackets.ToString() + Environment.NewLine +
            //                "#Private stream 1's: " + TotalNumberOfPrivateStream1.ToString() + Environment.NewLine +
            //                "#Private stream 1's with continuation=0: " + TotalNumberOfPrivateStream1Continuation0.ToString());
        }

        public List<PacketizedElementaryStream> GetSubtitlePesPackets(int packetId)
        { 
            var list = new List<PacketizedElementaryStream>();
            var buffer = new byte[20000];
            int index =0;
            foreach (Packet packet in SubtitlePackets)
            {
                if (packet.ContinuityCounter == 0)
                {
                    if (index > 0)
                    {
                        var pes = new PacketizedElementaryStream(buffer, 0);
                        list.Add(pes);
                    }
                    index = 0;
                }
                Buffer.BlockCopy(packet.Payload, 0, buffer, index, packet.Payload.Length);
                index += packet.Payload.Length;
            }
            return list;
        }

        private int DeterminePacketLength(Stream ms)
        {
            return 188;
        }    

    }
}
