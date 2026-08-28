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

            for (var i = 0; i < paragraphs.Count; i++)
            {
                var p = paragraphs[i];
                var show = new TeletextPacket { Milliseconds = ToMilliseconds(p.StartTime.TotalMilliseconds) };
                show.DataUnits.Add(GetPageHeaderDataUnit(magazine, pageBcd, show.DataUnits.Count, erasePage: true));
                show.DataUnits.AddRange(GetTextDataUnits(p, magazine, show.DataUnits.Count));
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

        private static List<byte[]> GetTextDataUnits(Paragraph paragraph, int magazine, int firstUnitIndex)
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
            for (var i = 0; i < lines.Count; i++)
            {
                result.Add(GetDataUnit(magazine, rows[i], GetRow(lines[i], alignment, doubleHeight),
                    firstUnitIndex + i, alreadyEncoded: false));
            }

            return result;
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
        private static byte[] GetRow(string line, Alignment alignment, bool doubleHeight)
        {
            var lead = new List<byte>();
            var body = new List<byte>();
            AppendText(line, lead, body);

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
            cells.AddRange(body);
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
        private static void AppendText(string line, List<byte> lead, List<byte> body)
        {
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
                            var color = GetTeletextColor(tag);
                            if (color.HasValue)
                            {
                                if (body.Count == 0 && lead.Count == 0)
                                {
                                    lead.Add(color.Value);
                                }
                                else
                                {
                                    body.Add(color.Value);
                                }
                            }

                            i = end + 1;
                            continue;
                        }

                        if (tag.Equals("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            // Back to the start-of-row default, but only when text follows.
                            if (body.Count > 0 && end + 1 < line.Length)
                            {
                                body.Add(AlphaWhite);
                            }

                            i = end + 1;
                            continue;
                        }
                    }
                }

                foreach (var c in GetTeletextCharacters(line[i]))
                {
                    body.Add(c);
                }

                i++;
            }
        }

        /// <summary>
        /// Teletext rows hold seven bit G0 characters, so anything else is folded to its base
        /// letter ("é" to "e") and whatever is left is replaced by a question mark.
        /// </summary>
        private static IEnumerable<byte> GetTeletextCharacters(char c)
        {
            if (c >= 0x20 && c < 0x7f)
            {
                yield return (byte)c;
                yield break;
            }

            var normalized = c.ToString().Normalize(NormalizationForm.FormD);
            var any = false;
            foreach (var n in normalized)
            {
                if (n >= 0x20 && n < 0x7f)
                {
                    any = true;
                    yield return (byte)n;
                }
            }

            if (!any)
            {
                yield return (byte)'?';
            }
        }

        private static byte? GetTeletextColor(string fontTag)
        {
            var colorStart = fontTag.IndexOf("color=", StringComparison.OrdinalIgnoreCase);
            if (colorStart < 0)
            {
                return null;
            }

            var color = fontTag.Substring(colorStart + "color=".Length).TrimStart().TrimEnd('>', '/', ' ');
            color = color.Trim('"', '\'').Trim().Trim('#').ToLowerInvariant();
            if (color.Length == 0)
            {
                return null;
            }

            switch (color)
            {
                case "black": return 0x00;
                case "red": return 0x01;
                case "lime":
                case "green": return 0x02;
                case "yellow": return 0x03;
                case "blue": return 0x04;
                case "magenta":
                case "fuchsia": return 0x05;
                case "cyan":
                case "aqua": return 0x06;
                case "white": return 0x07;
            }

            if (color.Length == 6 &&
                int.TryParse(color.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
                int.TryParse(color.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
                int.TryParse(color.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                // Teletext only has the eight combinations of full red, green and blue.
                var code = (r >= 128 ? 1 : 0) | (g >= 128 ? 2 : 0) | (b >= 128 ? 4 : 0);
                return (byte)code;
            }

            return null;
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
