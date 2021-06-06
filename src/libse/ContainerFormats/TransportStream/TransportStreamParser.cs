using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
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
        public List<int> SubtitlePacketIds { get; private set; }
        public SortedDictionary<int, SortedDictionary<int, List<Paragraph>>> TeletextSubtitlesLookup { get; set; } // teletext

        private List<Packet> SubtitlePackets { get; set; }
        private SortedDictionary<int, List<DvbSubPes>> SubtitlesLookup { get; set; }
        private SortedDictionary<int, List<TransportStreamSubtitle>> DvbSubtitlesLookup { get; set; } // images
        private bool _isM2TransportStream;

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
            _isM2TransportStream = false;
            NumberOfNullPackets = 0;
            TotalNumberOfPackets = 0;
            TotalNumberOfPrivateStream1 = 0;
            SubtitlePacketIds = new List<int>();
            SubtitlePackets = new List<Packet>();
            ms.Position = 0;
            const int packetLength = 188;
            _isM2TransportStream = IsM2TransportStream(ms);
            var packetBuffer = new byte[packetLength];
            var m2TsTimeCodeBuffer = new byte[4];
            long position = 0;
            SubtitlesLookup = new SortedDictionary<int, List<DvbSubPes>>();
            TeletextSubtitlesLookup = new SortedDictionary<int, SortedDictionary<int, List<Paragraph>>>();
            var teletextPesList = new Dictionary<int, List<DvbSubPes>>();
            var teletextPages = new Dictionary<int, List<int>>();
            ulong? firstMs = null;
            ulong? firstVideoMs = null;

            // check for Topfield .rec file
            ms.Seek(position, SeekOrigin.Begin);
            ms.Read(m2TsTimeCodeBuffer, 0, 3);
            if (m2TsTimeCodeBuffer[0] == 0x54 && m2TsTimeCodeBuffer[1] == 0x46 && m2TsTimeCodeBuffer[2] == 0x72)
            {
                position = 3760;
            }

            long transportStreamLength = ms.Length;
            ms.Seek(position, SeekOrigin.Begin);
            while (position < transportStreamLength)
            {
                if (_isM2TransportStream)
                {
                    ms.Read(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                    position += m2TsTimeCodeBuffer.Length;
                }

                var bytesRead = ms.Read(packetBuffer, 0, packetLength);
                if (bytesRead < packetLength)
                {
                    break; // incomplete packet at end-of-file
                }

                if (packetBuffer[0] == Packet.SynchronizationByte)
                {
                    var packet = new Packet(packetBuffer);
                    if (packet.IsNullPacket)
                    {
                        NumberOfNullPackets++;
                    }

                    else if (!firstVideoMs.HasValue && packet.IsVideoStream)
                    {
                        if (packet.Payload != null && packet.Payload.Length > 10)
                        {
                            int presentationTimestampDecodeTimestampFlags = packet.Payload[7] >> 6;
                            if (presentationTimestampDecodeTimestampFlags == 0b00000010 ||
                                presentationTimestampDecodeTimestampFlags == 0b00000011)
                            {
                                firstVideoMs = (ulong)packet.Payload[9 + 4] >> 1;
                                firstVideoMs += (ulong)packet.Payload[9 + 3] << 7;
                                firstVideoMs += (ulong)(packet.Payload[9 + 2] & 0b11111110) << 14;
                                firstVideoMs += (ulong)packet.Payload[9 + 1] << 22;
                                firstVideoMs += (ulong)(packet.Payload[9 + 0] & 0b00001110) << 29;
                                firstVideoMs = firstVideoMs / 90;
                            }
                        }
                    }

                    else if (packet.IsPrivateStream1 || SubtitlePacketIds.Contains(packet.PacketId))
                    {
                        TotalNumberOfPrivateStream1++;

                        if (!SubtitlePacketIds.Contains(packet.PacketId))
                        {
                            SubtitlePacketIds.Add(packet.PacketId);
                        }

                        if (packet.PayloadUnitStartIndicator)
                        {
                            firstMs = ProcessPackages(packet.PacketId, teletextPages, teletextPesList, firstMs);
                            SubtitlePackets.RemoveAll(p => p.PacketId == packet.PacketId);
                        }
                        SubtitlePackets.Add(packet);
                    }
                    TotalNumberOfPackets++;
                    position += packetLength;

                    if (TotalNumberOfPackets % 100000 == 0)
                    {
                        callback?.Invoke(position, transportStreamLength);
                    }
                }
                else
                {
                    // sync byte not found - search for it (will be very slow!)
                    if (_isM2TransportStream)
                    {
                        position -= m2TsTimeCodeBuffer.Length;
                    }
                    position++;
                    ms.Seek(position, SeekOrigin.Begin);
                }
            }
            foreach (var pid in SubtitlePackets.GroupBy(p => p.PacketId))
            {
                firstMs = ProcessPackages(pid.Key, teletextPages, teletextPesList, firstMs);
            }
            SubtitlePackets.Clear();
            callback?.Invoke(transportStreamLength, transportStreamLength);

            foreach (var packetId in teletextPesList.Keys) // teletext from PES packets
            {
                if (!teletextPages.ContainsKey(packetId))
                {
                    continue;
                }

                foreach (var page in teletextPages[packetId].OrderBy(p => p))
                {
                    var pageBcd = Teletext.DecToBec(page);
                    Teletext.InitializeStaticFields(packetId, pageBcd);
                    var teletextRunSettings = new TeletextRunSettings(firstMs);
                    foreach (var pes in teletextPesList[packetId])
                    {
                        var textDictionary = pes.GetTeletext(teletextRunSettings, page, pageBcd);
                        AddToTeletextDictionary(textDictionary, packetId);
                    }

                    var lastTextDictionary = Teletext.ProcessTelxPacketPendingLeftovers(teletextRunSettings, page);
                    AddToTeletextDictionary(lastTextDictionary, packetId);
                }
            }
            if (Configuration.Settings.SubtitleSettings.TeletextItalicFix)
            {
                FixTeletextItalics(TeletextSubtitlesLookup);
            }

            foreach (var id in TeletextSubtitlesLookup.Keys)
            {
                SubtitlePacketIds = SubtitlePacketIds.Where(p => p != id).ToList();
                SubtitlesLookup.Remove(id);
            }

            DvbSubtitlesLookup = new SortedDictionary<int, List<TransportStreamSubtitle>>();
            if (_isM2TransportStream) // m2ts blu-ray images from PES packets
            {
                foreach (int pid in SubtitlesLookup.Keys)
                {
                    var bdMs = new MemoryStream();
                    var list = SubtitlesLookup[pid];
                    var currentList = new List<DvbSubPes>();
                    var sb = new StringBuilder();
                    var subList = new List<TransportStreamSubtitle>();
                    var offset = (long)(firstVideoMs ?? 0); // when to use firstMs ?
                    var lastPalettes = new Dictionary<int, List<PaletteInfo>>();
                    var lastBitmapObjects = new Dictionary<int, List<BluRaySupParser.OdsData>>();
                    for (var index = 0; index < list.Count; index++)
                    {
                        var item = list[index];
                        item.WriteToStream(bdMs);
                        currentList.Add(item);
                        if (item.DataIdentifier == 0x80)
                        {
                            bdMs.Position = 0;
                            var bdList = BluRaySupParser.ParseBluRaySup(bdMs, sb, true, lastPalettes, lastBitmapObjects);
                            if (bdList.Count > 0)
                            {
                                var startMs = currentList.First().PresentationTimestampToMilliseconds();
                                var endMs = index + 1 < list.Count ? list[index + 1].PresentationTimestampToMilliseconds() : startMs + (ulong)Configuration.Settings.General.NewEmptyDefaultMs;
                                startMs = (ulong)((long)startMs - offset);
                                endMs = (ulong)((long)endMs - offset);
                                subList.Add(new TransportStreamSubtitle(bdList[0], startMs, endMs));
                            }
                            bdMs.Dispose();
                            bdMs = new MemoryStream();
                            currentList.Clear();
                        }
                    }

                    if (subList.Count > 0)
                    {
                        DvbSubtitlesLookup.Add(pid, subList);
                        SubtitlePacketIds = SubtitlePacketIds.Where(p => p != pid).ToList();
                    }
                }
            }

            SubtitlePacketIds.Clear();
            foreach (int key in SubtitlesLookup.Keys)
            {
                SubtitlePacketIds.Add(key);
            }
            SubtitlePacketIds.Sort();

            // Merge packets and set start/end time
            foreach (int pid in SubtitlePacketIds.Where(p => !DvbSubtitlesLookup.ContainsKey(p)))
            {
                var subtitles = new List<TransportStreamSubtitle>();
                var list = ParseAndRemoveEmpty(GetSubtitlePesPackets(pid));
                var offset = (long)(firstVideoMs ?? 0); // when to use firstMs ?
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

                        if (offset <= (long)sub.StartMilliseconds || offset < 0)
                        {
                            sub.StartMilliseconds = (ulong)((long)sub.StartMilliseconds - offset);
                            sub.EndMilliseconds = (ulong)((long)sub.EndMilliseconds - offset);
                        }
                        else
                        {
                            if (subtitles.Count > 0)
                            {
                                offset = (long)sub.StartMilliseconds - (long)subtitles[subtitles.Count - 1].EndMilliseconds + 1000;
                                sub.StartMilliseconds = (ulong)((long)sub.StartMilliseconds - offset);
                                sub.EndMilliseconds = (ulong)((long)sub.EndMilliseconds - offset);
                            }
                        }
                        subtitles.Add(sub);
                    }
                }
                if (subtitles.Count > 0)
                {
                    DvbSubtitlesLookup.Add(pid, subtitles);
                }
            }

            SubtitlePacketIds.Clear();
            foreach (int key in DvbSubtitlesLookup.Keys)
            {
                if (DvbSubtitlesLookup[key].Count > 0 && !SubtitlePacketIds.Contains(key))
                {
                    SubtitlePacketIds.Add(key);
                }
            }
            SubtitlePacketIds.Sort();
        }

        /// <summary>
        /// Converts a starting '&lt;' char to italic style (can be preceded by a font tag)
        /// E.g. "&lt;Hi there." to "&lt;i&gt;Hi there.&lt;/i&gt;"
        /// </summary>
        private static void FixTeletextItalics(SortedDictionary<int, SortedDictionary<int, List<Paragraph>>> dictionary)
        {
            foreach (var dic in dictionary)
            {
                foreach (var inner in dic.Value)
                {
                    foreach (var p in inner.Value.Where(p => p.Text.Trim().Contains('<')))
                    {
                        var sb = new StringBuilder();
                        foreach (var line in p.Text.SplitToLines())
                        {
                            var s = line.Trim();
                            if (s.StartsWith("<font", StringComparison.Ordinal))
                            {
                                var fontRemoved = HtmlUtil.RemoveOpenCloseTags(s, HtmlUtil.TagFont);
                                if (!fontRemoved.StartsWith('<'))
                                {
                                    sb.AppendLine(s); // no italic, only font tag
                                    continue;
                                }

                                // italic and font tag
                                var indexOfEnd = s.IndexOf('>');
                                if (indexOfEnd > 0 && s.Length > indexOfEnd + 2 && s[indexOfEnd + 1] == '<' &&
                                    !s.Remove(0, indexOfEnd).StartsWith("<font", StringComparison.Ordinal))
                                {
                                    s = s.Remove(indexOfEnd + 1, 1);
                                    sb.Append("<i>").Append(s).AppendLine("</i>"); // italic + font tag
                                    continue;
                                }

                                sb.AppendLine(line.Trim()); // no italic, only font tag
                                continue;
                            }

                            if (s.StartsWith('<'))
                            {
                                sb.Append("<i>").Append(s.Remove(0, 1)).AppendLine("</i>");
                            }
                            else
                            {
                                sb.AppendLine(s);
                            }
                        }
                        p.Text = sb.ToString().Trim();
                    }
                }
            }
        }

        private void AddToTeletextDictionary(Dictionary<int, Paragraph> textDictionary, int packetId)
        {
            foreach (var dic in textDictionary)
            {
                if (!string.IsNullOrEmpty(dic.Value.Text))
                {
                    if (TeletextSubtitlesLookup.ContainsKey(packetId))
                    {
                        var innerDic = TeletextSubtitlesLookup[packetId];
                        if (innerDic.ContainsKey(dic.Key))
                        {
                            innerDic[dic.Key].Add(dic.Value);
                        }
                        else
                        {
                            innerDic.Add(dic.Key, new List<Paragraph> { dic.Value });
                        }
                    }
                    else
                    {
                        TeletextSubtitlesLookup.Add(packetId, new SortedDictionary<int, List<Paragraph>> { { dic.Key, new List<Paragraph> { dic.Value } } });
                    }
                }
            }
        }

        private ulong? ProcessPackages(int packetId, Dictionary<int, List<int>> teletextPages, Dictionary<int, List<DvbSubPes>> teletextPesList, ulong? firstVideoMs)
        {
            var list = MakeSubtitlePesPackets(packetId, SubtitlePackets);
            if (list.Count == 0)
            {
                return firstVideoMs;
            }

            if (!firstVideoMs.HasValue)
            {
                foreach (var pes in list)
                {
                    if (pes.PresentationTimestamp.HasValue)
                    {
                        firstVideoMs = pes.PresentationTimestampToMilliseconds();
                        break;
                    }
                }
            }

            if (_isM2TransportStream || list.Any(p => p.IsDvbSubPicture))
            {
                if (SubtitlesLookup.ContainsKey(packetId))
                {
                    SubtitlesLookup[packetId].AddRange(list);
                }
                else
                {
                    SubtitlesLookup.Add(packetId, list);
                }
            }

            foreach (var item in list.Where(p => p.IsTeletext))
            {
                foreach (var pageNumber in item.PrepareTeletext().Where(p => p > 0))
                {
                    if (!teletextPages.ContainsKey(packetId))
                    {
                        teletextPages.Add(packetId, new List<int> { pageNumber });
                    }
                    else
                    {
                        if (!teletextPages[packetId].Contains(pageNumber))
                        {
                            teletextPages[packetId].Add(pageNumber);
                        }
                    }
                }

                if (teletextPesList.ContainsKey(packetId))
                {
                    teletextPesList[packetId].Add(item);
                }
                else
                {
                    teletextPesList.Add(packetId, new List<DvbSubPes> { item });
                }
            }

            return firstVideoMs;
        }

        public List<TransportStreamSubtitle> GetDvbSubtitles(int packetId)
        {
            return DvbSubtitlesLookup.ContainsKey(packetId) ? DvbSubtitlesLookup[packetId] : null;
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

        private static List<DvbSubPes> ParseAndRemoveEmpty(List<DvbSubPes> list)
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

        public static bool IsM2TransportStream(Stream ms)
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
