// WORK IN PROGRESS - DO NOT REFACTOR //
// WORK IN PROGRESS - DO NOT REFACTOR //
// WORK IN PROGRESS - DO NOT REFACTOR //

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

        public class Palette
        { 
            public int PaletteEntryId { get; set; }
            public int Y { get; set; }
            public int Cr { get; set; }
            public int Cb { get; set; }
            public int T { get; set; }
        }

        public class RegionStyle
        {
            public int RegionStyleId { get; set; }
            public int RegionHorizontalPosition { get; set; }
            public int RegionVerticalPosition { get; set; }
            public int RegionWidth { get; set; }
            public int RegionHeight { get; set; }
            public int RegionBgPaletteEntryIdRef { get; set; }
            public int TextBoxHorizontalPosition { get; set; }
            public int TextBoxVerticalPosition { get; set; }
            public int TextBoxWidth { get; set; }
            public int TextBoxHeight { get; set; }
            public int TextFlow { get; set; }
            public int TextHorizontalAlignment { get; set; }
            public int TextVerticalAlignment { get; set; }
            public int LineSpace { get; set; }
            public int FontIdRef { get; set; }
            public int FontStyle { get; set; }
            public int FontSize { get; set; }
            public int FontPaletteEntryIdRef { get; set; }
            public int FontOutlinePaletteEntryIdRef { get; set; }
            public int FontOutlineThickness { get; set; }
        }

        public class UserStyle
        {
            public int UserStyleId { get; set; }
            public int RegionHorizontalPositionDirection { get; set; }
            public int RegionHorizontalPositionDelta { get; set; }
            public int RegionVerticalPositionDirection { get; set; }
            public int RegionVerticalPositionDelta { get; set; }
            public int FontSizeIncDec { get; set; }
            public int FontSizeDelta { get; set; }
            public int TextBoxHorizontalPositionDirection { get; set; }
            public int TextBoxHorizontalPositionDelta { get; set; }
            public int TextBoxVerticalPositionDirection { get; set; }
            public int TextBoxVerticalPositionDelta { get; set; }
            public int TextBoxWidthIncDec { get; set; }
            public int TextBoxWidthDelta { get; set; }
            public int TextBoxHeightIncDec { get; set; }
            public int TextBoxHeightDelta { get; set; }
            public int LineSpaceIncDec { get; set; }
            public int LineSpaceDelta { get; set; }
        }

        public class DialogStyleSegment
        {
            public bool PlayerStyleFlag { get; set; }
            public int NumberOfRegionStyles { get; set; }
            public int NumberOfUserStyles { get; set; }
            public List<RegionStyle> RegionStyles { get; set; }
            public List<UserStyle> UserStyles { get; set; }
            public List<Palette> Palettes { get; set; }
            public int NumberOfDialogPresentationSegments { get; set; }

            public DialogStyleSegment(byte[] buffer)
            {
                PlayerStyleFlag = (buffer[9] & Helper.B10000000) > 0;
                NumberOfRegionStyles = buffer[11];
                NumberOfUserStyles = buffer[12];

                int idx = 13;
                RegionStyles = new List<RegionStyle>(NumberOfRegionStyles);
                for (int i = 0; i < NumberOfRegionStyles; i++)
                {
                    var rs = new RegionStyle
                    {
                        RegionStyleId = buffer[idx],
                        RegionHorizontalPosition = (buffer[idx + 1] << 8) + buffer[idx + 2],
                        RegionVerticalPosition = (buffer[idx + 3] << 8) + buffer[idx + 4],
                        RegionWidth = (buffer[idx + 5] << 8) + buffer[idx + 6],
                        RegionHeight = (buffer[idx + 7] << 8) + buffer[idx + 8],
                        RegionBgPaletteEntryIdRef = buffer[idx + 9],
                        TextBoxHorizontalPosition = (buffer[idx + 11] << 8) + buffer[idx + 12],
                        TextBoxVerticalPosition = (buffer[idx + 13] << 8) + buffer[idx + 14],
                        TextBoxWidth = (buffer[idx + 15] << 8) + buffer[idx + 16],
                        TextBoxHeight = (buffer[idx + 17] << 8) + buffer[idx + 18],
                        TextFlow = buffer[idx + 19],
                        TextHorizontalAlignment = buffer[idx + 20],
                        TextVerticalAlignment = buffer[idx + 21],
                        LineSpace = buffer[idx + 22],
                        FontIdRef = buffer[idx + 23],
                        FontStyle = buffer[idx + 24],
                        FontSize = buffer[idx + 25],
                        FontPaletteEntryIdRef = buffer[idx + 26],
                        FontOutlinePaletteEntryIdRef = buffer[idx + 27],
                        FontOutlineThickness = buffer[idx + 28]
                    };
                    RegionStyles.Add(rs);
                    idx += 29;

                    UserStyles = new List<UserStyle>(NumberOfUserStyles);
                    for (int j= 0; j < NumberOfUserStyles; j++)
                    {
                        var us = new UserStyle
                        {
                            UserStyleId = buffer[idx],
                            RegionHorizontalPositionDirection = buffer[idx + 1] >> 7,
                            RegionHorizontalPositionDelta = ((buffer[idx + 1] & Helper.B01111111) << 8) + buffer[idx + 2],
                            RegionVerticalPositionDirection = buffer[idx + 3] >> 7,
                            RegionVerticalPositionDelta = ((buffer[idx + 3] & Helper.B01111111) << 8) + buffer[idx + 4],
                            FontSizeIncDec = buffer[idx + 5] >> 7,
                            FontSizeDelta = (buffer[idx + 5] & Helper.B01111111),
                            TextBoxHorizontalPositionDirection = buffer[idx + 6] >> 7,
                            TextBoxHorizontalPositionDelta = ((buffer[idx + 6] & Helper.B01111111) << 8) + buffer[idx + 7],
                            TextBoxVerticalPositionDirection = buffer[idx + 8] >> 7,
                            TextBoxVerticalPositionDelta = ((buffer[idx + 8] & Helper.B01111111) << 8) + buffer[idx + 9],
                            TextBoxWidthIncDec = buffer[idx + 10] >> 7,
                            TextBoxWidthDelta = ((buffer[idx + 10] & Helper.B01111111) << 8) + buffer[idx + 11],
                            TextBoxHeightIncDec = buffer[idx + 12] >> 7,
                            TextBoxHeightDelta = ((buffer[idx + 12] & Helper.B01111111) << 8) + buffer[idx + 13],
                            LineSpaceIncDec = buffer[idx + 14] >> 7,
                            LineSpaceDelta = (buffer[idx + 14] & Helper.B01111111)
                        };
                        UserStyles.Add(us);
                        idx += 15;
                    }
                }

                int numberOfPalettees = ((buffer[idx] << 8) + buffer[idx + 1]) / 5;
                Palettes = new List<Palette>(numberOfPalettees);
                idx += 2;
                for (int i = 0; i < numberOfPalettees; i++)
                {
                    var palette = new Palette
                    {
                        PaletteEntryId = buffer[idx],
                        Y = buffer[idx + 1],
                        Cr = buffer[idx + 2],
                        Cb = buffer[idx + 3],
                        T = buffer[idx + 4]
                    };
                    Palettes.Add(palette);
                    idx += 5;
                }
                NumberOfDialogPresentationSegments = (buffer[idx] << 8) + buffer[idx + 1];
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
                StartPts = buffer[13];
                StartPts += (ulong)buffer[12] << 8;
                StartPts += (ulong)buffer[11] << 16;
                StartPts += (ulong)buffer[10] << 24;
                StartPts += (ulong)(buffer[9] & Helper.B00000001) << 32;

                EndPts = buffer[18];
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
                    var region = new SubtitleRegion { ContinuousPresentation = (buffer[idx] & Helper.B10000000) > 0, Forced = (buffer[idx] & Helper.B01000000) > 0 };
                    idx++;
                    region.RegionStyleId = buffer[idx];
                    idx++;
                    int regionSubtitleLength = buffer[idx + 1] + (buffer[idx] << 8);
                    idx += 2;
                    int processedLength = 0;
                    region.Texts = new List<string>();
                    string endStyle = string.Empty;
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
                            string text = Encoding.UTF8.GetString(buffer, idx, dataLength);
                            region.Texts.Add(text);
                        }
                        else if (dataType == 0x02) // Change a font set
                        {
                            System.Diagnostics.Debug.WriteLine("font set");
                        }
                        else if (dataType == 0x03) // Change a font style
                        {
                            System.Diagnostics.Debug.WriteLine("font style");
                            var fontStyle = buffer[idx];
                            switch (fontStyle)
                            {
                                case 1: region.Texts.Add("<b>");
                                    endStyle = "</b>";
                                    break;
                                case 2: region.Texts.Add("<i>");
                                    endStyle = "</i>";
                                    break;
                                case 3: region.Texts.Add("<b><i>");
                                    endStyle = "</i></b>";
                                    break;
                                case 5: region.Texts.Add("<b>");
                                    endStyle = "</b>";
                                    break;
                                case 6: region.Texts.Add("<i>");
                                    endStyle = "</i>";
                                    break;
                                case 7: region.Texts.Add("<b><i>");
                                    endStyle = "</i></b>";
                                    break;
                            }
                        }
                        else if (dataType == 0x04) // Change a font size
                        {
                            System.Diagnostics.Debug.WriteLine("font size");
                        }
                        else if (dataType == 0x05) // Change a font color
                        {
                            System.Diagnostics.Debug.WriteLine("font color");
                        }
                        else if (dataType == 0x0A) // Line break
                        {
                            region.Texts.Add(Environment.NewLine);
                        }
                        else if (dataType == 0x0B) // End of inline style
                        {
                            System.Diagnostics.Debug.WriteLine("End inline style");
                            if (!string.IsNullOrEmpty(endStyle))
                            {
                                region.Texts.Add(endStyle);
                                endStyle = string.Empty;
                            }
                        }
                        processedLength += dataLength;
                        idx += dataLength;
                    }
                    if (!string.IsNullOrEmpty(endStyle))
                    {
                        region.Texts.Add(endStyle);
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

        private const int TextSubtitleStreamPid = 0x1800;
        private const byte SegmentTypeDialogStyle = 0x81;
        private const byte SegmentTypeDialogPresentation = 0x82;

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
            var subtitlePackets = new List<Packet>();
            const int packetLength = 188;
            bool isM2TransportStream = DetectFormat(ms);
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
                if (isM2TransportStream)
                {
                    ms.Read(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                    var tc = (m2TsTimeCodeBuffer[0] << 24) + (m2TsTimeCodeBuffer[1] << 16) + (m2TsTimeCodeBuffer[2] << 8) + (m2TsTimeCodeBuffer[3] & Helper.B00111111);
                    // should m2ts time code be used in any way?
                    var msecs = (ulong)Math.Round((tc) / 27.0); // 27 or 90?
                    TimeCode tc2 = new TimeCode(msecs);
                    System.Diagnostics.Debug.WriteLine(tc2);
                    position += m2TsTimeCodeBuffer.Length;
                }

                ms.Read(packetBuffer, 0, packetLength);
                byte syncByte = packetBuffer[0];
                if (syncByte == Packet.SynchronizationByte)
                {
                    var packet = new Packet(packetBuffer);
                    if (packet.PacketId == TextSubtitleStreamPid)
                    {
                        subtitlePackets.Add(packet);
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
            foreach (var item in subtitlePackets)
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
