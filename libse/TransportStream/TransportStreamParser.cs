using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    /// <summary>
    /// MPEG transport stream parser
    /// </summary>
    public class TransportStreamParser
    {
        public delegate void LoadTransportStreamCallback(long position, long total);

        public int NumberOfNullPackets { get; private set; }
        public long TotalNumberOfPackets { get; private set; }
        public long TotalNumberOfPrivateStream1 { get; private set; }
        public long TotalNumberOfPrivateStream1Continuation0 { get; private set; }
        public List<int> SubtitlePacketIds { get; private set; }
        public List<Packet> SubtitlePackets { get; private set; }
        private Dictionary<int, List<DvbSubPes>> SubtitlesLookup { get; set; }
        private Dictionary<int, List<TransportStreamSubtitle>> DvbSubtitlesLookup { get; set; }
        public Dictionary<int, StringBuilder> TeletextSubtitlesLookup { get; set; }
        public bool IsM2TransportStream { get; private set; }
        public ulong FirstVideoPts { get; private set; }

        public void Parse(string fileName, LoadTransportStreamCallback callback)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Parse(fs, callback);
            }
        }

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms">Input stream</param>
        /// <param name="callback">Optional callback event to follow progress</param>
        public void Parse(Stream ms, LoadTransportStreamCallback callback)
        {
            bool firstVideoPtsFound = false;
            IsM2TransportStream = false;
            NumberOfNullPackets = 0;
            TotalNumberOfPackets = 0;
            TotalNumberOfPrivateStream1 = 0;
            TotalNumberOfPrivateStream1Continuation0 = 0;
            SubtitlePacketIds = new List<int>();
            SubtitlePackets = new List<Packet>();
            ms.Position = 0;
            const int packetLength = 188;
            IsM2TransportStream = IsM3TransportStream(ms);
            var packetBuffer = new byte[packetLength];
            var m2TsTimeCodeBuffer = new byte[4];
            long position = 0;
            SubtitlesLookup = new Dictionary<int, List<DvbSubPes>>();
            TeletextSubtitlesLookup = new Dictionary<int, StringBuilder>();

            // check for Topfield .rec file
            ms.Seek(position, SeekOrigin.Begin);
            ms.Read(m2TsTimeCodeBuffer, 0, 3);
            if (m2TsTimeCodeBuffer[0] == 0x54 && m2TsTimeCodeBuffer[1] == 0x46 && m2TsTimeCodeBuffer[2] == 0x72)
            {
                position = 3760;
            }

            long transportStreamLength = ms.Length;
            while (position < transportStreamLength)
            {
                ms.Seek(position, SeekOrigin.Begin);

                if (IsM2TransportStream)
                {
                    ms.Read(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                    position += m2TsTimeCodeBuffer.Length;
                }

                ms.Read(packetBuffer, 0, packetLength);
                if (packetBuffer[0] == Packet.SynchronizationByte)
                {
                    var packet = new Packet(packetBuffer);

                    if (packet.IsNullPacket)
                    {
                        NumberOfNullPackets++;
                    }
                    else if (!firstVideoPtsFound && packet.IsVideoStream)
                    {
                        if (packet.Payload != null && packet.Payload.Length > 10)
                        {
                            int presentationTimestampDecodeTimestampFlags = packet.Payload[7] >> 6;
                            if (presentationTimestampDecodeTimestampFlags == Helper.B00000010 ||
                                presentationTimestampDecodeTimestampFlags == Helper.B00000011)
                            {
                                FirstVideoPts = (ulong)packet.Payload[9 + 4] >> 1;
                                FirstVideoPts += (ulong)packet.Payload[9 + 3] << 7;
                                FirstVideoPts += (ulong)(packet.Payload[9 + 2] & Helper.B11111110) << 14;
                                FirstVideoPts += (ulong)packet.Payload[9 + 1] << 22;
                                FirstVideoPts += (ulong)(packet.Payload[9 + 0] & Helper.B00001110) << 29;
                                firstVideoPtsFound = true;
                            }
                        }
                    }
                    else if (packet.IsProgramAssociationTable)
                    {

                    }
                    else if (packet.IsPrivateStream1 || SubtitlePacketIds.Contains(packet.PacketId))
                    {
                        TotalNumberOfPrivateStream1++;

                        if (!SubtitlePacketIds.Contains(packet.PacketId))
                        {
                            SubtitlePacketIds.Add(packet.PacketId);
                        }

                        if (!IsM2TransportStream && packet.PayloadUnitStartIndicator)
                        {
                            var list = MakeSubtitlePesPackets(packet.PacketId, SubtitlePackets);
                            if (list.Any(p => p.IsDvbSubpicture))
                            {
                                if (SubtitlesLookup.ContainsKey(packet.PacketId))
                                {
                                    SubtitlesLookup[packet.PacketId].AddRange(list);
                                }
                                else
                                {
                                    SubtitlesLookup.Add(packet.PacketId, list);
                                }
                            }
                            if (list.Any(p => p.IsTeletext))
                            {
                                foreach (var item in list.Where(p => p.IsTeletext))
                                {
                                    var text = item.GetTeletext(packet.PacketId);
                                    if (text.Length > 0)
                                    {
                                        if (TeletextSubtitlesLookup.ContainsKey(packet.PacketId))
                                        {
                                            TeletextSubtitlesLookup[packet.PacketId].AppendLine(text + Environment.NewLine + Environment.NewLine);
                                        }
                                        else
                                        {
                                            TeletextSubtitlesLookup.Add(packet.PacketId, new StringBuilder(text + Environment.NewLine + Environment.NewLine));
                                        }
                                    }
                                }
                            }

                            SubtitlePackets.RemoveAll(p => p.PacketId == packet.PacketId);
                        }
                        SubtitlePackets.Add(packet);


                        if (packet.ContinuityCounter == 0)
                        {
                            TotalNumberOfPrivateStream1Continuation0++;

                            //int pesExtensionlength = 0;
                            //if (12 + packet.AdaptionFieldLength < packetBuffer.Length)
                            //    pesExtensionlength = 0xFF & packetBuffer[12 + packet.AdaptionFieldLength];
                            //int pesOffset = 13 + packet.AdaptionFieldLength + pesExtensionlength;
                            //bool isTeletext = (pesExtensionlength == 0x24 && (0xFF & packetBuffer[pesOffset]) >> 4 == 1);

                            //// workaround uk freesat teletext
                            //if (!isTeletext)
                            //    isTeletext = (pesExtensionlength == 0x24 && (0xFF & packetBuffer[pesOffset]) == 0x99);

                            //if (!isTeletext)
                            //{

                            //}
                        }
                    }
                    TotalNumberOfPackets++;
                    position += packetLength;

                    if (TotalNumberOfPackets % 100000 == 0)
                    {
                        callback.Invoke(ms.Position, transportStreamLength);
                    }
                }
                else
                {
                    position++;
                }
            }

            if (IsM2TransportStream)
            {
                DvbSubtitlesLookup = new Dictionary<int, List<TransportStreamSubtitle>>();
                SubtitlesLookup = new Dictionary<int, List<DvbSubPes>>();
                foreach (int pid in SubtitlePacketIds)
                {
                    var bdMs = new MemoryStream();
                    var list = MakeSubtitlePesPackets(pid, SubtitlePackets);
                    var currentList = new List<DvbSubPes>();
                    var sb = new StringBuilder();
                    var subList = new List<TransportStreamSubtitle>();
                    for (var index = 0; index < list.Count; index++)
                    {
                        var item = list[index];
                        item.WriteToStream(bdMs);
                        currentList.Add(item);
                        if (item.DataIdentifier == 0x80)
                        {
                            bdMs.Position = 0;
                            var bdList = BluRaySupParser.ParseBluRaySup(bdMs, sb, true);
                            if (bdList.Count > 0)
                            {
                                var startMs = currentList.First().PresentationTimestampToMilliseconds();
                                var endMs = index + 1 < list.Count ? list[index + 1].PresentationTimestampToMilliseconds() : startMs + (ulong)Configuration.Settings.General.NewEmptyDefaultMs;
                                subList.Add(new TransportStreamSubtitle(bdList[0], startMs, endMs, (ulong)(FirstVideoPts / 90.0)));
                            }
                            bdMs = new MemoryStream();
                            currentList.Clear();
                        }
                    }
                    SubtitlesLookup.Add(pid, list);
                    DvbSubtitlesLookup.Add(pid, subList);
                }
                SubtitlePacketIds.Clear();
                foreach (int key in DvbSubtitlesLookup.Keys)
                {
                    SubtitlePacketIds.Add(key);
                }

                SubtitlePacketIds.Sort();
                return;
            }

            // check for SubPictureStreamId = 32
            foreach (int pid in SubtitlePacketIds)
            {
                var list = MakeSubtitlePesPackets(pid, SubtitlePackets);
                bool hasImageSubtitles = false;
                foreach (var item in list)
                {
                    if (item.IsDvbSubpicture)
                    {
                        hasImageSubtitles = true;
                        break;
                    }
                }
                if (hasImageSubtitles)
                {
                    if (SubtitlesLookup.ContainsKey(pid))
                    {
                        SubtitlesLookup[pid].AddRange(list);
                    }
                    else
                    {
                        SubtitlesLookup.Add(pid, list);
                    }
                }
                SubtitlePackets.RemoveAll(p => p.PacketId == pid);
            }

            SubtitlePacketIds.Clear();
            foreach (int key in SubtitlesLookup.Keys)
            {
                SubtitlePacketIds.Add(key);
            }

            SubtitlePacketIds.Sort();

            // Merge packets and set start/end time
            DvbSubtitlesLookup = new Dictionary<int, List<TransportStreamSubtitle>>();
            var firstVideoMs = (ulong)(FirstVideoPts / 90.0);
            foreach (int pid in SubtitlePacketIds)
            {
                var subtitles = new List<TransportStreamSubtitle>();
                var list = ParseAndRemoveEmpty(GetSubtitlePesPackets(pid));
                for (int i = 0; i < list.Count; i++)
                {
                    var pes = list[i];
                    pes.ParseSegments();
                    if (pes.ObjectDataList.Count > 0)
                    {
                        var sub = new TransportStreamSubtitle { StartMilliseconds = pes.PresentationTimestampToMilliseconds(), Pes = pes };
                        if (i + 1 < list.Count && list[i + 1].PresentationTimestampToMilliseconds() > 25)
                        {
                            sub.EndMilliseconds = list[i + 1].PresentationTimestampToMilliseconds() - 25;
                        }
                        if (sub.EndMilliseconds < sub.StartMilliseconds || sub.EndMilliseconds - sub.StartMilliseconds > (ulong)Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            sub.EndMilliseconds = sub.StartMilliseconds + (ulong)Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                        }
                        subtitles.Add(sub);
                        if (sub.StartMilliseconds < firstVideoMs)
                        {
                            firstVideoMs = sub.StartMilliseconds;
                        }
                    }
                }
                foreach (var s in subtitles)
                {
                    s.OffsetMilliseconds = firstVideoMs;
                }
                DvbSubtitlesLookup.Add(pid, subtitles);
            }
            SubtitlePacketIds.Clear();
            foreach (int key in DvbSubtitlesLookup.Keys)
            {
                if (DvbSubtitlesLookup[key].Count > 0)
                {
                    SubtitlePacketIds.Add(key);
                }
            }
            SubtitlePacketIds.Sort();
        }

        public List<TransportStreamSubtitle> GetDvbSubtitles(int packetId)
        {
            if (DvbSubtitlesLookup.ContainsKey(packetId))
            {
                return DvbSubtitlesLookup[packetId];
            }

            return null;
        }

        internal static List<DvbSubPes> MakeSubtitlePesPackets(int packetId, List<Packet> subtitlePackets)
        {
            var list = new List<DvbSubPes>();
            int last = -1;
            var packetList = new List<Packet>();
            foreach (var packet in subtitlePackets)
            {
                if (packet.PacketId == packetId)
                {
                    if (packet.PayloadUnitStartIndicator)
                    {
                        if (packetList.Count > 0)
                        {
                            AddPesPacket(list, packetList);
                        }
                        packetList = new List<Packet>();
                    }
                    if (packet.Payload != null && last != packet.ContinuityCounter)
                    {
                        packetList.Add(packet);
                    }
                    last = packet.ContinuityCounter;
                }
            }
            if (packetList.Count > 0)
            {
                AddPesPacket(list, packetList);
            }
            return list;
        }

        public List<DvbSubPes> GetSubtitlePesPackets(int packetId)
        {
            if (SubtitlesLookup.ContainsKey(packetId))
            {
                return SubtitlesLookup[packetId];
            }

            return null;
        }

        private List<DvbSubPes> ParseAndRemoveEmpty(List<DvbSubPes> list)
        {
            var newList = new List<DvbSubPes>();
            foreach (var pes in list)
            {
                pes.ParseSegments();
                if (pes.ObjectDataList.Count > 0 || pes.PresentationTimestamp > 0)
                {
                    newList.Add(pes);
                }
            }
            return newList;
        }

        private static void AddPesPacket(List<DvbSubPes> list, List<Packet> packetList)
        {
            int bufferSize = 0;
            foreach (var p in packetList)
            {
                bufferSize += p.Payload.Length;
            }

            var pesData = new byte[bufferSize];
            int pesIndex = 0;
            foreach (var p in packetList)
            {
                Buffer.BlockCopy(p.Payload, 0, pesData, pesIndex, p.Payload.Length);
                pesIndex += p.Payload.Length;
            }

            DvbSubPes pes;
            if (VobSubParser.IsMpeg2PackHeader(pesData))
            {
                pes = new DvbSubPes(pesData, Mpeg2Header.Length);
            }
            else if (VobSubParser.IsPrivateStream1(pesData, 0))
            {
                pes = new DvbSubPes(pesData, 0);
            }
            else
            {
                pes = new DvbSubPes(pesData, 0);
            }
            list.Add(pes);
        }

        public static bool IsM3TransportStream(Stream ms)
        {
            if (ms.Length > 192 + 192 + 5)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[192 + 192 + 5];
                ms.Read(buffer, 0, buffer.Length);
                if (buffer[0] == Packet.SynchronizationByte && buffer[188] == Packet.SynchronizationByte)
                {
                    return false;
                }

                if (buffer[4] == Packet.SynchronizationByte && buffer[192 + 4] == Packet.SynchronizationByte && buffer[192 + 192 + 4] == Packet.SynchronizationByte)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDvbSup(string fileName)
        {
            try
            {
                byte[] pesData = File.ReadAllBytes(fileName);
                if (pesData[0] != 0x20 || pesData[1] != 0 || pesData[2] != 0x0F)
                {
                    return false;
                }

                var pes = new DvbSubPes(0, pesData);
                return pes.SubtitleSegments.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static List<TransportStreamSubtitle> GetDvbSup(string fileName)
        {
            byte[] pesData = File.ReadAllBytes(fileName);
            var list = new List<DvbSubPes>();
            int index = 0;
            while (index < pesData.Length - 10)
            {
                var pes = new DvbSubPes(index, pesData);
                index = pes.Length + 1;
                list.Add(pes);
            }

            var subtitles = new List<TransportStreamSubtitle>();
            int seconds = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var pes = list[i];
                pes.ParseSegments();
                if (pes.ObjectDataList.Count > 0)
                {
                    var sub = new TransportStreamSubtitle();
                    sub.StartMilliseconds = (ulong)seconds * 1000UL;
                    seconds += pes.PageCompositions[0].PageTimeOut;
                    if (pes.PageCompositions.Count > 0)
                    {
                        sub.EndMilliseconds = sub.StartMilliseconds + (ulong)pes.PageCompositions[0].PageTimeOut * 1000UL;
                    }
                    else
                    {
                        sub.EndMilliseconds = sub.StartMilliseconds + 2500;
                    }

                    sub.Pes = pes;
                    subtitles.Add(sub);
                }
                if (pes.PageCompositions.Count > 0)
                {
                    seconds += pes.PageCompositions[0].PageTimeOut;
                }
            }
            return subtitles;
        }

    }
}
