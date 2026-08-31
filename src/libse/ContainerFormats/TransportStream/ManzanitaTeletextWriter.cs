using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// Writes subtitles as a Manzanita "private_stream_1" DVB teletext file (.dvbttx): an XML
    /// preamble holding a packet index, followed by the raw teletext PES payloads that index
    /// points into. <see cref="ManzanitaTransportStreamParser"/> reads the result back.
    /// <para>
    /// The packet layout follows ETSI EN 300 472 / ETS 300 706 and mirrors a real broadcast dump:
    /// one page header per subtitle (erase + subtitle flags set), one boxed double height row per
    /// text line, and a filler page header closing the transmission.
    /// </para>
    /// </summary>
    public class ManzanitaTeletextWriter
    {
        /// <summary>
        /// Teletext page carrying the subtitles, e.g. 888.
        /// </summary>
        public int PageNumber { get; set; } = 888;

        /// <summary>
        /// Three letter ISO 639-2 language code for the teletext descriptor.
        /// </summary>
        public string LanguageCode { get; set; } = "eng";

        public double FrameRate { get; set; } = 25.0;

        public string Creator { get; set; } = "Subtitle Edit";

        /// <summary>
        /// Written to the XML preamble - settable so tests can produce a stable file.
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Teletext transmits the payload least significant bit first, so 0x27 is stored as 0xE4.
        /// </summary>
        private const byte FramingCode = 0xE4;

        private const byte DataIdentifier = 0x10;
        private const byte DataUnitLength = 44;
        private const int DataUnitSize = DataUnitLength + 2; // data_unit_id + data_unit_length
        private const byte StuffingDataUnitId = (byte)Teletext.DataUnitT.DataUnitStuffing;
        private const byte SubtitleDataUnitId = (byte)Teletext.DataUnitT.DataUnitEbuTeletextSubtitle;

        // Page FE in the same magazine is the usual "nothing to see here" filler that terminates
        // a page transmission in serial mode.
        private const int FillerPageBcd = 0xfe;

        // A teletext page is hidden one frame before the header that replaces it (Teletext.cs
        // subtracts a fixed 40 ms), so the erase must be sent that much after the wanted end.
        private const ulong HideOffsetMs = 40;

        private const int ColumnCount = 40;
        private const int LastRow = 23;
        private const int DefaultBottomRow = 22; // a double height row covers 22 and 23

        // ETS 300 706, chapter 12.3: the enhancement packet and the triplets it holds.
        private const int EnhancementPacketNumber = 26;
        private const int TripletsPerPacket = 13;
        private const byte SetActivePositionMode = 0x04;

        // ETS 300 706, chapter 12.3.1, table 27: an X/26 foreground colour triplet names one of
        // the 32 colour map entries at a character position - the only road to the Level 2.5
        // colours beyond the eight a spacing attribute reaches. The map itself travels in a
        // packet X/28/0.
        private const byte ForegroundColorMode = 0x00;
        private const int ColorMapPacketNumber = 28;
        private const int RowAddressGroupStart = 40;
        private const int TerminationAddress = 63;
        private const byte TerminationMode = 0x1f;
        private const byte TerminationData = 0x7f;

        // The designation code of an X/26 packet is four bits wide, so a page cannot carry more
        // than sixteen of them - far past what even a full screen of accented text needs.
        private const int MaxEnhancementPackets = 16;

        // Spacing attributes, ETS 300 706 chapter 12.2.
        private const byte DoubleHeight = 0x0d;
        private const byte StartBox = 0x0b;
        private const byte EndBox = 0x0a;
        private const byte AlphaWhite = 0x07;
        private const byte Space = 0x20;

        // The first data unit of a packet claims this VBI line, the next one the line below, ...
        private const int FirstLineOffset = 7;

        private enum Alignment
        {
            Left,
            Center,
            Right
        }

        private class TeletextPacket
        {
            public ulong Milliseconds { get; set; }
            public List<byte[]> DataUnits { get; } = new List<byte[]>();
        }

        /// <summary>
        /// One cell of a teletext row: the seven bit code the row itself carries, plus the X/26
        /// triplet that overwrites it when the character needs the G2 set or a diacritical mark.
        /// </summary>
        private readonly struct Cell
        {
            public Cell(byte value) : this(value, 0, 0)
            {
            }

            public Cell(byte value, byte mode, byte data) : this(value, mode, data, -1)
            {
            }

            public Cell(byte value, byte mode, byte data, int colorEntry)
            {
                Value = value;
                Mode = mode;
                Data = data;
                ColorEntry = colorEntry;
            }

            public byte Value { get; }

            /// <summary>X/26 mode, or zero when the cell needs no enhancement.</summary>
            public byte Mode { get; }

            public byte Data { get; }

            /// <summary>
            /// The colour map entry an X/26 foreground colour triplet paints this cell (and the
            /// rest of the run) with, or -1 when the Level 1 attributes already have it right.
            /// </summary>
            public int ColorEntry { get; }
        }

        /// <summary>
        /// An X/26 triplet waiting for the row layout to settle, so it knows its column.
        /// </summary>
        private class Enhancement
        {
            public int Row { get; set; }
            public int Column { get; set; }
            public byte Mode { get; set; }
            public byte Data { get; set; }
        }

        public void Write(Subtitle subtitle, string fileName)
        {
            File.WriteAllBytes(fileName, GetBytes(subtitle));
        }

        public byte[] GetBytes(Subtitle subtitle)
        {
            var packets = BuildPackets(subtitle);

            // EN 300 472 wants a constant PES payload size - pad every packet to the widest one.
            var dataUnitsPerPacket = Math.Max(3, packets.Count == 0 ? 3 : packets.Max(p => p.DataUnits.Count));
            var payloadLength = 1 + dataUnitsPerPacket * DataUnitSize;

            var binary = new List<byte>(payloadLength * Math.Max(1, packets.Count));
            var index = new StringBuilder();
            var offset = 0;
            foreach (var packet in packets)
            {
                binary.Add(DataIdentifier);
                foreach (var dataUnit in packet.DataUnits)
                {
                    binary.AddRange(dataUnit);
                }

                for (var i = packet.DataUnits.Count; i < dataUnitsPerPacket; i++)
                {
                    binary.AddRange(GetStuffingDataUnit());
                }

                index.Append("    <packet pts=\"").Append((packet.Milliseconds * 90).ToString(CultureInfo.InvariantCulture))
                     .Append("\" offset=\"").Append(offset.ToString(CultureInfo.InvariantCulture))
                     .Append("\" length=\"").Append(payloadLength.ToString(CultureInfo.InvariantCulture))
                     .Append("\" />\n");
                offset += payloadLength;
            }

            var result = new List<byte>(offset + 2000);
            result.AddRange(Encoding.ASCII.GetBytes(GetXml(index.ToString(), payloadLength)));
            result.AddRange(binary);
            return result.ToArray();
        }

        private string GetXml(string dataIndex, int payloadLength)
        {
            // A muxer hint only: how much bandwidth the stream needs if every frame carries a
            // packet of this size, counting the PES header and the 188 byte transport packets.
            var transportPackets = (int)Math.Ceiling((payloadLength + 14) / 184.0);
            var rate = transportPackets * 188 * 8 * FrameRate;

            var magazine = PageNumber / 100 % 8; // magazine 8 is transmitted as 0
            var pageBcd = Teletext.DecToBec(PageNumber) & 0xff;

            var sb = new StringBuilder();
            sb.Append("<private_stream_1\n");
            sb.Append("  xmlns=\"http://www.manzanitasystems.com/schema/v1.03/private_stream_1\"\n");
            sb.Append("  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n");
            sb.Append("  xsi:schemaLocation=\"http://www.manzanitasystems.com/schema/v1.03/private_stream_1\"\n");
            sb.Append("  version=\"1.03\"\n");
            sb.Append("  type=\"").Append(ManzanitaTransportStreamParser.TeletextStreamType).Append("\">\n\n");
            sb.Append("  <preamble>\n");
            sb.Append("    <conversion_data\n");
            sb.Append("      creator=\"").Append(XmlEscape(Creator)).Append("\"\n");
            sb.Append("      date=\"").Append(Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append("\"\n");
            sb.Append("      source_format=\"demux\"\n");
            sb.Append("    />\n");
            sb.Append("    <descriptors>\n");
            sb.Append("      <dvb_teletext_descriptor>\n");
            sb.Append("        <dvb_teletext_content\n");
            sb.Append("          ISO_639_language_code=\"").Append(XmlEscape(GetLanguageCode())).Append("\"\n");
            sb.Append("          teletext_type=\"subtitle\"\n");
            sb.Append("          teletext_magazine_number=\"").Append(magazine.ToString(CultureInfo.InvariantCulture)).Append("\"\n");
            sb.Append("          teletext_page_number=\"").Append(pageBcd.ToString(CultureInfo.InvariantCulture)).Append("\"\n");
            sb.Append("        />\n");
            sb.Append("      </dvb_teletext_descriptor>\n");
            sb.Append("    </descriptors>\n");
            sb.Append("    <pes_header\n");
            sb.Append("      PES_priority=\"0\"\n");
            sb.Append("      copyright=\"0\"\n");
            sb.Append("      original_or_copy=\"0\"\n");
            sb.Append("      stream_id=\"189\"\n");
            sb.Append("    />\n");
            sb.Append("    <timing_information\n");
            sb.Append("      average_rate=\"").Append(rate.ToString("0.000", CultureInfo.InvariantCulture)).Append("\"\n");
            sb.Append("      max_rate=\"").Append(rate.ToString("0.000", CultureInfo.InvariantCulture)).Append("\"\n");
            sb.Append("      min_advance_time=\"3600\"\n");
            sb.Append("      max_advance_time=\"7200\"\n");
            sb.Append("    />\n");
            sb.Append("  </preamble>\n\n");
            sb.Append("  <data_index>\n");
            sb.Append(dataIndex);
            sb.Append("  </data_index>\n\n");

            // The reader finds the binary section by the end tag plus a single line feed - keep it.
            sb.Append("</private_stream_1>\n");
            return sb.ToString();
        }

        private string GetLanguageCode()
        {
            var code = (LanguageCode ?? string.Empty).Trim();
            return code.Length == 3 ? code.ToLowerInvariant() : "eng";
        }

        private static string XmlEscape(string text)
        {
            return (text ?? string.Empty)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private List<TeletextPacket> BuildPackets(Subtitle subtitle)
        {
            var packets = new List<TeletextPacket>();
            if (subtitle?.Paragraphs == null)
            {
                return packets;
            }

            var magazine = PageNumber / 100 % 8;
            var pageBcd = Teletext.DecToBec(PageNumber) & 0xff;
            var paragraphs = subtitle.Paragraphs;
            var colorMap = new TeletextColorMap();

            for (var i = 0; i < paragraphs.Count; i++)
            {
                var p = paragraphs[i];
                var show = new TeletextPacket { Milliseconds = ToMilliseconds(p.StartTime.TotalMilliseconds) };
                show.DataUnits.Add(GetPageHeaderDataUnit(magazine, pageBcd, show.DataUnits.Count, erasePage: true));
                show.DataUnits.AddRange(GetTextDataUnits(p, magazine, show.DataUnits.Count, colorMap));
                show.DataUnits.Add(GetPageHeaderDataUnit(magazine, FillerPageBcd, show.DataUnits.Count, erasePage: false));
                packets.Add(show);

                // Clearing the page is only needed when no other subtitle takes over: the header
                // of the next subtitle erases this one anyway.
                var eraseMs = ToMilliseconds(p.EndTime.TotalMilliseconds) + HideOffsetMs;
                var next = i + 1 < paragraphs.Count ? paragraphs[i + 1] : null;
                if (next != null && ToMilliseconds(next.StartTime.TotalMilliseconds) <= eraseMs)
                {
                    continue;
                }

                var erase = new TeletextPacket { Milliseconds = eraseMs };
                erase.DataUnits.Add(GetPageHeaderDataUnit(magazine, pageBcd, 0, erasePage: true));
                erase.DataUnits.Add(GetPageHeaderDataUnit(magazine, FillerPageBcd, 1, erasePage: false));
                packets.Add(erase);
            }

            // Overlapping or unsorted subtitles would otherwise put the index out of order, and
            // both the muxer and the reader expect the time stamps to grow. OrderBy is stable, so
            // a page and the erase that belongs to it keep their order.
            return packets.OrderBy(p => p.Milliseconds).ToList();
        }

        private static ulong ToMilliseconds(double milliseconds)
        {
            return milliseconds <= 0 ? 0 : (ulong)Math.Round(milliseconds, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Packet 0 of a page - page number, flags and the (suppressed) header row.
        /// </summary>
        private static byte[] GetPageHeaderDataUnit(int magazine, int pageBcd, int unitIndex, bool erasePage)
        {
            var data = new byte[ColumnCount];
            data[0] = TeletextHamming.Hamming84Encode(pageBcd & 0x0f);        // page units
            data[1] = TeletextHamming.Hamming84Encode((pageBcd >> 4) & 0x0f); // page tens
            data[2] = TeletextHamming.Hamming84Encode(0);                     // S1
            data[3] = TeletextHamming.Hamming84Encode(erasePage ? 0x08 : 0x00); // S2 + C4 erase page
            data[4] = TeletextHamming.Hamming84Encode(0);                     // S3
            data[5] = TeletextHamming.Hamming84Encode(0x08);                  // S4 + C6 subtitle
            data[6] = TeletextHamming.Hamming84Encode(0x07);                  // C7 suppress header, C8 update, C9 interrupted
            data[7] = TeletextHamming.Hamming84Encode(0x01);                  // C11 serial magazine, latin G0 charset

            // The header row is not displayed (C7), but the cells still have to be valid.
            for (var i = 8; i < ColumnCount; i++)
            {
                data[i] = TeletextHamming.OddParityEncode(Space);
            }

            return GetDataUnit(magazine, 0, data, unitIndex, alreadyEncoded: true);
        }

        private static List<byte[]> GetTextDataUnits(Paragraph paragraph, int magazine, int firstUnitIndex, TeletextColorMap colorMap)
        {
            var result = new List<byte[]>();
            var text = paragraph.Text ?? string.Empty;
            var alignment = GetAlignment(ref text, out var topAligned);
            // Teletext has no italics or positioning of its own, so only the color tags survive.
            var lines = Utilities.RemoveSsaTags(
                    HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic, HtmlUtil.TagBold, HtmlUtil.TagUnderline))
                .SplitToLines()
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
            if (lines.Count == 0)
            {
                return result;
            }

            var rows = GetRowNumbers(lines.Count, paragraph.MarginV, topAligned, out var doubleHeight);
            var enhancements = new List<Enhancement>();
            var rowData = new List<byte[]>();
            for (var i = 0; i < lines.Count; i++)
            {
                rowData.Add(GetRow(lines[i], alignment, doubleHeight, rows[i], enhancements, colorMap));
            }

            // ETS 300 706, chapter 9.4.2: the colour map rides in a packet X/28/0, transmitted
            // with every page so a decoder joining anywhere has it. Once any subtitle has put a
            // colour in a redefinable entry, every later page carries the map - entries are only
            // ever added, so a page is never sent ahead of an entry it uses.
            if (colorMap.NeedsColorMapPacket)
            {
                result.Add(GetDataUnit(magazine, ColorMapPacketNumber, colorMap.GetColorMapPacketData(),
                    firstUnitIndex + result.Count, alreadyEncoded: true));
            }

            // ETS 300 706, annex B.2.2: packets with Y = 26 are transmitted before the rows they
            // change, so a decoder has the enhancement in hand when the row arrives.
            foreach (var enhancementPacket in GetEnhancementPackets(enhancements))
            {
                result.Add(GetDataUnit(magazine, EnhancementPacketNumber, enhancementPacket,
                    firstUnitIndex + result.Count, alreadyEncoded: true));
            }

            for (var i = 0; i < rowData.Count; i++)
            {
                result.Add(GetDataUnit(magazine, rows[i], rowData[i], firstUnitIndex + result.Count,
                    alreadyEncoded: false));
            }

            return result;
        }

        /// <summary>
        /// Packs the triplets of a page into X/26 packets: every row starts with a "set active
        /// position" triplet, then one triplet per character, and the rest of the packet is
        /// filled with termination markers.
        /// </summary>
        private static List<byte[]> GetEnhancementPackets(List<Enhancement> enhancements)
        {
            if (enhancements.Count == 0)
            {
                return new List<byte[]>();
            }

            var triplets = new List<List<int>> { new List<int>() };
            foreach (var row in enhancements.GroupBy(e => e.Row).OrderBy(g => g.Key))
            {
                var ordered = row.OrderBy(e => e.Column).ToList();
                var index = 0;
                while (index < ordered.Count)
                {
                    // The active position only holds within one packet, so a row that is split
                    // over two packets has to set it again - and a set active position as the
                    // last triplet of a packet would say nothing.
                    var current = triplets[triplets.Count - 1];
                    if (TripletsPerPacket - current.Count < 2)
                    {
                        if (triplets.Count == MaxEnhancementPackets)
                        {
                            // Nothing is lost that a reader could have shown anyway: the row keeps
                            // the plain stand-in these triplets would have overwritten.
                            return GetEnhancementPacketBytes(triplets);
                        }

                        current = new List<int>();
                        triplets.Add(current);
                    }

                    var count = Math.Min(ordered.Count - index, TripletsPerPacket - current.Count - 1);
                    current.Add(GetTriplet(RowAddressGroupStart + row.Key, SetActivePositionMode, ordered[index].Column));
                    for (var i = 0; i < count; i++)
                    {
                        var enhancement = ordered[index + i];
                        current.Add(GetTriplet(enhancement.Column, enhancement.Mode, enhancement.Data));
                    }

                    index += count;
                }
            }

            return GetEnhancementPacketBytes(triplets);
        }

        private static List<byte[]> GetEnhancementPacketBytes(List<List<int>> triplets)
        {
            var packets = new List<byte[]>();
            for (var i = 0; i < triplets.Count; i++)
            {
                var data = new byte[ColumnCount];
                data[0] = TeletextHamming.Hamming84Encode(i); // designation code

                for (var t = 0; t < TripletsPerPacket; t++)
                {
                    var value = t < triplets[i].Count
                        ? triplets[i][t]
                        : GetTriplet(TerminationAddress, TerminationMode, TerminationData);
                    var encoded = TeletextHamming.Hamming2418Encode(value);
                    data[1 + t * 3] = (byte)encoded;
                    data[2 + t * 3] = (byte)(encoded >> 8);
                    data[3 + t * 3] = (byte)(encoded >> 16);
                }

                packets.Add(data);
            }

            return packets;
        }

        /// <summary>
        /// ETS 300 706, chapter 12.3.2: an X/26 triplet is an address, a mode and a data field.
        /// </summary>
        private static int GetTriplet(int address, int mode, int data)
        {
            return (address & 0x3f) | ((mode & 0x1f) << 6) | ((data & 0x7f) << 11);
        }

        /// <summary>
        /// Bottom aligned rows counted upwards, or the top of the screen for {\an7}-{\an9}. A
        /// double height row occupies the row below it too, hence the gap of two.
        /// </summary>
        private static List<int> GetRowNumbers(int lineCount, string marginV, bool topAligned, out bool doubleHeight)
        {
            doubleHeight = true;
            var rows = new List<int>();

            if (topAligned && !IsTeletextRow(marginV))
            {
                // Teletext.cs reports {\an8} again when every used row is above row 6.
                for (var i = 0; i < lineCount; i++)
                {
                    rows.Add(1 + i * 2);
                }

                if (rows[rows.Count - 1] <= LastRow)
                {
                    return rows;
                }

                rows.Clear();
            }

            var bottom = IsTeletextRow(marginV) ? int.Parse(marginV, CultureInfo.InvariantCulture) : DefaultBottomRow;
            var spacing = 2;
            if (bottom - (lineCount - 1) * spacing < 1)
            {
                // Too many lines to fit at double height - fall back to single height rows.
                spacing = 1;
                doubleHeight = false;
            }

            for (var i = 0; i < lineCount; i++)
            {
                rows.Add(Math.Max(1, bottom - (lineCount - 1 - i) * spacing));
            }

            return rows;
        }

        private static bool IsTeletextRow(string marginV)
        {
            return int.TryParse(marginV, NumberStyles.Integer, CultureInfo.InvariantCulture, out var row) &&
                   row >= 1 && row <= LastRow;
        }

        private static Alignment GetAlignment(ref string text, out bool topAligned)
        {
            topAligned = false;
            var alignment = Alignment.Center;
            if (text.Length > 5 && text[0] == '{' && text[1] == '\\' && text[5] == '}')
            {
                var tag = text.Substring(0, 6);
                switch (tag)
                {
                    case "{\\an1}":
                    case "{\\an4}":
                        alignment = Alignment.Left;
                        break;
                    case "{\\an7}":
                        alignment = Alignment.Left;
                        topAligned = true;
                        break;
                    case "{\\an3}":
                    case "{\\an6}":
                        alignment = Alignment.Right;
                        break;
                    case "{\\an9}":
                        alignment = Alignment.Right;
                        topAligned = true;
                        break;
                    case "{\\an8}":
                        topAligned = true;
                        break;
                }

                if (tag.StartsWith("{\\an", StringComparison.Ordinal))
                {
                    text = text.Substring(6);
                }
            }

            return alignment;
        }

        /// <summary>
        /// Turns one line of text into the 40 cells of a teletext row: a leading color attribute,
        /// the double height and start box attributes, the text itself and the end box markers,
        /// padded to the wanted alignment.
        /// </summary>
        private static byte[] GetRow(string line, Alignment alignment, bool doubleHeight, int row, List<Enhancement> enhancements, TeletextColorMap colorMap)
        {
            var lead = new List<byte>();
            var body = new List<Cell>();
            AppendText(line, lead, body, colorMap);

            var attributes = doubleHeight ? new List<byte> { DoubleHeight, StartBox, StartBox } : new List<byte> { StartBox, StartBox };
            var maxBody = ColumnCount - lead.Count - attributes.Count;
            if (body.Count > maxBody)
            {
                body.RemoveRange(maxBody, body.Count - maxBody);
            }

            // A row ending in the last column closes the box on its own, so the end box markers
            // only take the space that is left - that is what real broadcast rows do, and it buys
            // the two characters that would otherwise clip a full width line.
            var endBoxCount = Math.Min(2, ColumnCount - lead.Count - attributes.Count - body.Count);
            var used = lead.Count + attributes.Count + body.Count + endBoxCount;
            var leftPad = 0;
            if (alignment == Alignment.Center)
            {
                leftPad = (ColumnCount - used) / 2;
            }
            else if (alignment == Alignment.Right)
            {
                leftPad = ColumnCount - used;
            }

            var cells = new List<byte>(ColumnCount);
            cells.AddRange(Enumerable.Repeat(Space, leftPad));
            cells.AddRange(lead);
            cells.AddRange(attributes);

            // Now that the padding is known, the cells that need an X/26 triplet know their column.
            var bodyStart = cells.Count;
            for (var i = 0; i < body.Count; i++)
            {
                cells.Add(body[i].Value);

                // The foreground colour triplet goes first at its column, so that a G2 character
                // at the same cell (which only touches the text plane) cannot be affected.
                if (body[i].ColorEntry >= 0)
                {
                    enhancements.Add(new Enhancement
                    {
                        Row = row,
                        Column = bodyStart + i,
                        Mode = ForegroundColorMode,
                        Data = (byte)body[i].ColorEntry
                    });
                }

                if (body[i].Mode != 0)
                {
                    enhancements.Add(new Enhancement
                    {
                        Row = row,
                        Column = bodyStart + i,
                        Mode = body[i].Mode,
                        Data = body[i].Data
                    });
                }
            }

            for (var i = 0; i < endBoxCount; i++)
            {
                cells.Add(EndBox);
            }

            while (cells.Count < ColumnCount)
            {
                cells.Add(Space);
            }

            return cells.ToArray();
        }

        /// <summary>
        /// Splits the line into the color attribute that applies before the box starts (teletext
        /// shows a spacing attribute as a space, so a leading color is free of charge there) and
        /// the boxed cells, where every further color change costs one cell.
        /// </summary>
        private static void AppendText(string line, List<byte> lead, List<Cell> body, TeletextColorMap colorMap)
        {
            // A colour that needs a colour map entry is painted by an X/26 triplet on the first
            // character cell that follows - on the spacing attribute's own cell the Level 1
            // attribute would win, and a spacing attribute in the lead has no body column at all.
            var pendingColorEntry = -1;

            var i = 0;
            while (i < line.Length)
            {
                if (line[i] == '<')
                {
                    var end = line.IndexOf('>', i);
                    if (end > 0)
                    {
                        var tag = line.Substring(i, end - i + 1);
                        if (tag.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
                        {
                            if (TryParseColor(tag, out var rgb12))
                            {
                                var resolved = colorMap.Resolve(rgb12);
                                pendingColorEntry = resolved.Entry;
                                if (body.Count == 0 && lead.Count == 0)
                                {
                                    lead.Add(resolved.SpacingAttribute);
                                }
                                else
                                {
                                    body.Add(new Cell(resolved.SpacingAttribute));
                                }
                            }

                            i = end + 1;
                            continue;
                        }

                        if (tag.Equals("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            pendingColorEntry = -1;

                            // Back to the start-of-row default, but only when text follows.
                            if (body.Count > 0 && end + 1 < line.Length)
                            {
                                body.Add(new Cell(AlphaWhite));
                            }

                            i = end + 1;
                            continue;
                        }
                    }
                }

                foreach (var cell in GetTeletextCells(line[i]))
                {
                    if (pendingColorEntry >= 0)
                    {
                        body.Add(new Cell(cell.Value, cell.Mode, cell.Data, pendingColorEntry));
                        pendingColorEntry = -1;
                    }
                    else
                    {
                        body.Add(cell);
                    }
                }

                i++;
            }
        }

        /// <summary>
        /// Where a character has no seven bit G0 code but the row can still show it through an
        /// X/26 triplet, this is what stays in the row itself - all a Level 1.0 decoder, which
        /// ignores the enhancement, has to go on. "#" for the music note is what broadcasters
        /// send; ZDF page 777 does exactly that.
        /// </summary>
        private static readonly Dictionary<char, char> Level1Fallbacks = new Dictionary<char, char>
        {
            { '♪', '#' }, { '€', 'E' }, { '©', 'C' }, { '®', 'R' }, { '™', 'T' },
            { '‘', '\'' }, { '’', '\'' }, { '“', '"' }, { '”', '"' },
            { '¡', '!' }, { '¿', '?' }, { '―', '-' }, { '_', '-' }, { '°', 'o' }, { '×', 'x' },
            { 'Æ', 'A' }, { 'æ', 'a' }, { 'Ø', 'O' }, { 'ø', 'o' }, { 'Œ', 'O' }, { 'œ', 'o' },
            { 'ß', 's' }, { 'Ł', 'L' }, { 'ł', 'l' }, { 'Þ', 'P' }, { 'þ', 'p' },
            { 'Đ', 'D' }, { 'đ', 'd' }, { 'ð', 'd' }, { 'Ħ', 'H' }, { 'ħ', 'h' }, { 'ı', 'i' },
            { 'Ĳ', 'I' }, { 'ĳ', 'i' }, { 'Ŧ', 'T' }, { 'ŧ', 't' }, { 'Ŋ', 'N' }, { 'ŋ', 'n' },
            { 'ª', 'a' }, { 'º', 'o' }, { 'µ', 'u' }, { 'Ω', 'O' }, { 'α', 'a' }
        };

        /// <summary>
        /// Characters that neither set has a code for: the printable ASCII the Latin G0 set gives
        /// to the national options instead, and the typography a subtitle is full of.
        /// </summary>
        private static readonly Dictionary<char, string> Substitutes = new Dictionary<char, string>
        {
            { '[', "(" }, { ']', ")" }, { '{', "(" }, { '}', ")" },
            { '\\', "/" }, { '|', "/" }, { '`', "'" }, { '~', "-" },
            { '—', "-" }, { '–', "-" }, { '‒', "-" }, { '−', "-" }, { '‐', "-" },
            { '…', "..." }, { ' ', " " }, { '′', "'" }, { '″', "\"" },
            { '„', "\"" }, { '‚', "'" }, { '‹', "<" }, { '›', ">" }
        };

        /// <summary>
        /// Turns one character into the cells that carry it: its G0 code where the row can hold
        /// it, otherwise a stand-in plus the X/26 triplet that overwrites the cell with the G2
        /// character or the accented letter. What is left over is folded to its base letters
        /// ("ǽ" to "ae"), and only then replaced by a question mark.
        /// </summary>
        private static IEnumerable<Cell> GetTeletextCells(char c)
        {
            if (TeletextTables.TryGetLatinG0Code(c, out var code))
            {
                yield return new Cell(code);
                yield break;
            }

            if (TeletextTables.TryGetG2Replacement(c, out var replacement))
            {
                yield return new Cell(GetLevel1Fallback(c, replacement), replacement.Mode, replacement.Data);
                yield break;
            }

            if (Substitutes.TryGetValue(c, out var substitute))
            {
                foreach (var s in substitute)
                {
                    if (TeletextTables.TryGetLatinG0Code(s, out var substituteCode))
                    {
                        yield return new Cell(substituteCode);
                    }
                }

                yield break;
            }

            var normalized = c.ToString().Normalize(NormalizationForm.FormD);
            var any = false;
            foreach (var n in normalized)
            {
                if (TeletextTables.TryGetLatinG0Code(n, out var folded))
                {
                    any = true;
                    yield return new Cell(folded);
                }
            }

            if (!any)
            {
                yield return new Cell((byte)'?');
            }
        }

        private static byte GetLevel1Fallback(char c, TeletextTables.G2Replacement replacement)
        {
            // A diacritical mark is put on a plain letter, which is a fine stand-in by itself.
            if (replacement.Mode != TeletextTables.G2Mode)
            {
                return replacement.Data;
            }

            if (Level1Fallbacks.TryGetValue(c, out var fallback) &&
                TeletextTables.TryGetLatinG0Code(fallback, out var code))
            {
                return code;
            }

            return Space;
        }

        /// <summary>
        /// The colour of a font tag at the resolution the teletext colour map has: four bits per
        /// component, red in the high nibble.
        /// </summary>
        private static bool TryParseColor(string fontTag, out int rgb12)
        {
            rgb12 = 0;
            var colorStart = fontTag.IndexOf("color=", StringComparison.OrdinalIgnoreCase);
            if (colorStart < 0)
            {
                return false;
            }

            var color = fontTag.Substring(colorStart + "color=".Length).TrimStart().TrimEnd('>', '/', ' ');
            color = color.Trim('"', '\'').Trim().Trim('#').ToLowerInvariant();
            if (color.Length == 0)
            {
                return false;
            }

            switch (color)
            {
                case "black": rgb12 = 0x000; return true;
                case "red": rgb12 = 0xf00; return true;
                case "lime":
                case "green": rgb12 = 0x0f0; return true;
                case "yellow": rgb12 = 0xff0; return true;
                case "blue": rgb12 = 0x00f; return true;
                case "magenta":
                case "fuchsia": rgb12 = 0xf0f; return true;
                case "cyan":
                case "aqua": rgb12 = 0x0ff; return true;
                case "white": rgb12 = 0xfff; return true;
            }

            if (color.Length == 6 &&
                int.TryParse(color.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
                int.TryParse(color.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
                int.TryParse(color.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                // Round each component to the four bits the map holds - 0x11 steps, so #ff8822
                // becomes 0xf82 and reads back as exactly #ff8822.
                rgb12 = TeletextColorMap.QuantizeRgb(r, g, b);
                return true;
            }

            return false;
        }

        private static byte[] GetStuffingDataUnit()
        {
            var unit = new byte[DataUnitSize];
            unit[0] = StuffingDataUnitId;
            unit[1] = DataUnitLength;
            for (var i = 2; i < unit.Length; i++)
            {
                unit[i] = 0xff;
            }

            return unit;
        }

        /// <summary>
        /// Wraps 40 data bytes in a teletext data unit: the VBI line, the framing code, the
        /// Hamming 8/4 coded magazine and packet number, and the payload - all but the first byte
        /// stored least significant bit first, as they go on air.
        /// </summary>
        private static byte[] GetDataUnit(int magazine, int packetNumber, byte[] data, int unitIndex, bool alreadyEncoded)
        {
            var unit = new byte[DataUnitSize];
            unit[0] = SubtitleDataUnitId;
            unit[1] = DataUnitLength;

            var lineOffset = Math.Min(22, FirstLineOffset + Math.Max(0, unitIndex));
            unit[2] = (byte)(0xc0 | lineOffset); // reserved_future_use, field_parity, line_offset
            unit[3] = FramingCode;

            var address = ((packetNumber & 0x1f) << 3) | (magazine & 0x07);
            unit[4] = TeletextHamming.Reverse8[TeletextHamming.Hamming84Encode(address & 0x0f)];
            unit[5] = TeletextHamming.Reverse8[TeletextHamming.Hamming84Encode((address >> 4) & 0x0f)];

            for (var i = 0; i < ColumnCount; i++)
            {
                var value = alreadyEncoded ? data[i] : TeletextHamming.OddParityEncode(data[i]);
                unit[6 + i] = TeletextHamming.Reverse8[value];
            }

            return unit;
        }
    }
}
