using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic.TransportStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class TextST : SubtitleFormat
    {

        public class DialogStyleSegment
        {
            public bool PlayerStyleFlag { get; set; }
            public int NumberOfRegionStyles { get; set; }
            public int NumberOfUserStyles { get; set; }
            
            public DialogStyleSegment(byte[] buffer)
            {
                PlayerStyleFlag = (buffer[13] & Helper.B10000000) > 0;
                NumberOfRegionStyles = buffer[15];
                NumberOfUserStyles = buffer[16];
                for (int i = 0; i < NumberOfRegionStyles; i++)
                {
                }
            }
        }

        public class SubtitleRegion
        {
            public bool ContinuousPresentation { get; set; }
            public bool Forced { get; set; }
            public int RegionStyleId { get; set; }
            public List<string> Texts { get; set; }
        }

        public class DialogPresentationSegment
        {
            public DialogPresentationSegment(byte[] buffer)
            {
                StartPts = (ulong)buffer[13];
                StartPts += (ulong)buffer[12] << 8;
                StartPts += (ulong)buffer[11] << 16;
                StartPts += (ulong)buffer[10] << 24;
                StartPts += (ulong)(buffer[9] & Helper.B00000001) << 32;

                EndPts = (ulong)buffer[18];
                EndPts += (ulong)buffer[17] << 8;
                EndPts += (ulong)buffer[16] << 16;
                EndPts += (ulong)buffer[15] << 24;
                EndPts += (ulong)(buffer[14] & Helper.B00000001) << 32;

                PaletteUpdate = (buffer[19] & Helper.B10000000) > 0;
                int idx = 20;
                if (PaletteUpdate)
                {
                    NumberOfPaletteEntries = buffer[21] + (buffer[20] << 8);
                    idx += NumberOfPaletteEntries * 5;
                }

                int numberOfRegions = buffer[idx];
                idx++;
                Regions = new List<SubtitleRegion>(numberOfRegions);
                for (int i = 0; i < numberOfRegions; i++)
                {
                    var region = new SubtitleRegion();
                    region.ContinuousPresentation = (buffer[idx] & Helper.B10000000) > 0;
                    region.Forced = (buffer[idx] & Helper.B01000000) > 0;
                    idx++;
                    region.RegionStyleId = buffer[idx];
                    idx++;
                    int regionSubtitleLength = buffer[idx + 1] + (buffer[idx] << 8);
                    idx += 2;
                    int processedLength = 0;
                    region.Texts = new List<string>();
                    while (processedLength < regionSubtitleLength)
                    {
                        byte escapeCode = buffer[idx];
                        idx++;
                        byte dataType = buffer[idx];
                        idx++;
                        byte dataLength = buffer[idx];
                        idx++;
                        processedLength += 3;
                        if (dataType == 0x01) // Text
                        {
                            string text = System.Text.Encoding.UTF8.GetString(buffer, idx, dataLength);
                            region.Texts.Add(text);                            
                        }
                        else if (dataType == 0x02) // Change a font set
                        {
                        }
                        else if (dataType == 0x03) // Change a font style
                        {
                        }
                        else if (dataType == 0x04) // Change a font size
                        {
                        }
                        else if (dataType == 0x05) // Change a font color
                        {
                        }
                        else if (dataType == 0x0A) // Line break
                        {
                            region.Texts.Add(Environment.NewLine);
                        }
                        else if (dataType == 0x0B) // End of inline style
                        {
                        }
                        processedLength += dataLength;
                        idx += dataLength;
                    }
                    Regions.Add(region);
                }
            }

            public int Length { get; set; }
            public UInt64 StartPts { get; set; }
            public UInt64 EndPts { get; set; }
            public bool PaletteUpdate { get; set; }
            public int NumberOfPaletteEntries { get; set; }
            public List<SubtitleRegion> Regions { get; set; }
            public string PlainText
            {
                get
                {
                    var sb = new StringBuilder();
                    foreach (var region in Regions)
                    {
                        foreach (string text in region.Texts)
                        {
                            sb.Append(text);
                        }
                    }
                    return sb.ToString();
                }
            }

            public ulong StartPtsMilliseconds
            {
                get { return (ulong)Math.Round((StartPts) / 90.0); }
            }
            public ulong EndPtsMilliseconds
            {
                get { return (ulong)Math.Round((EndPts) / 90.0); }
            }

        }

        private const int TextSubtitleStreamPID = 0x1800;
        private const byte SegmentTypeDialogStyle = 0x81;
        private const byte SegmentTypeDialogPresentation = 0x82;
        List<Packet> SubtitlePackets;

        public override string Extension
        {
            get { return ".m2ts"; }
        }

        public override string Name
        {
            get { return "Blu-ray TextST"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || 
                !fileName.EndsWith(".m2ts", StringComparison.OrdinalIgnoreCase) ||
                !FileUtil.IsM2TransportStream(fileName))
            {
                return false;
            }

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                LoadSubtitle(subtitle, fs);
            }
        }

        public void LoadSubtitle(Subtitle subtitle, Stream ms)
        {
            var SubtitlePackets = new List<Packet>();
            const int packetLength = 188;
            bool IsM2TransportStream = DetectFormat(ms);
            var packetBuffer = new byte[packetLength];
            var m2TsTimeCodeBuffer = new byte[4];
            long position = 0;
            ms.Position = 0;

            // check for Topfield .rec file
            ms.Seek(position, SeekOrigin.Begin);
            ms.Read(m2TsTimeCodeBuffer, 0, 3);
            if (m2TsTimeCodeBuffer[0] == 0x54 && m2TsTimeCodeBuffer[1] == 0x46 && m2TsTimeCodeBuffer[2] == 0x72)
                position = 3760;

            long transportStreamLength = ms.Length;
            while (position < transportStreamLength)
            {
                ms.Seek(position, SeekOrigin.Begin);
                if (IsM2TransportStream)
                {
                    ms.Read(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                    //var tc = (m2tsTimeCodeBuffer[0]<< 24) | (m2tsTimeCodeBuffer[1] << 16) | (m2tsTimeCodeBuffer[2] << 8) | (m2tsTimeCodeBuffer[3]);
                    // should m2ts time code be used in any way?
                    position += m2TsTimeCodeBuffer.Length;
                }

                ms.Read(packetBuffer, 0, packetLength);
                byte syncByte = packetBuffer[0];
                if (syncByte == Packet.SynchronizationByte)
                {
                    var packet = new Packet(packetBuffer);
                    if (packet.PacketId == TextSubtitleStreamPID)
                    {
                        SubtitlePackets.Add(packet);
                    }
                    position += packetLength;
                }
                else
                {
                    position++;
                }
            }

            //TODO: merge ts packets

            DialogStyleSegment dss;
            foreach (var item in SubtitlePackets)
            {
                if (item.Payload != null && item.Payload.Length > 10 && VobSub.VobSubParser.IsPrivateStream2(item.Payload, 0))
                {
                    if (item.Payload[6] == SegmentTypeDialogPresentation)
                    {
                        var dps = new DialogPresentationSegment(item.Payload);
                        subtitle.Paragraphs.Add(new Paragraph(dps.PlainText.Trim(), dps.StartPtsMilliseconds, dps.EndPtsMilliseconds));
                    }
                    else if (item.Payload[6] == SegmentTypeDialogStyle)
                    {
                        dss = new DialogStyleSegment(item.Payload);
                    }
                }
            }
   
            subtitle.Renumber();
        }

        private bool DetectFormat(Stream ms)
        {
            if (ms.Length > 192 + 192 + 5)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[192 + 192 + 5];
                ms.Read(buffer, 0, buffer.Length);
                if (buffer[0] == Packet.SynchronizationByte && buffer[188] == Packet.SynchronizationByte)
                    return false;
                if (buffer[4] == Packet.SynchronizationByte && buffer[192 + 4] == Packet.SynchronizationByte && buffer[192 + 192 + 4] == Packet.SynchronizationByte)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
