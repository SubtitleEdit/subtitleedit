using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// Manzanita transport stream parser.
    /// </summary>
    public class ManzanitaTransportStreamParser
    {
        /// <summary>
        /// The "type" attribute of a Manzanita file carrying DVB teletext (as opposed to
        /// "dvb_subtitle", which carries the bitmap subtitles handled by <see cref="GetDvbSup"/>).
        /// </summary>
        public const string TeletextStreamType = "dvb_teletext";

        // Teletext state in Teletext.cs is keyed by the transport stream packet id; a Manzanita
        // dump holds a single elementary stream, so any fixed id will do.
        private const int TeletextPacketId = 1;

        // "</private_stream_1>" - the XML preamble ends here and the binary payloads follow.
        private static readonly byte[] EndTag = Encoding.ASCII.GetBytes("</private_stream_1>");

        // How much of the preamble is read at a time; it is read in full however long it is.
        private const int PreambleChunkSize = 200_000;

        // Backstop for a file that has no end tag at all, so it is not read into memory whole.
        // No real preamble comes near this: 64 MB is over half a million packet lines.
        private const int MaxPreambleSize = 64 * 1024 * 1024;

        private readonly List<DvbSubPes> _dvbSubs;
        private readonly List<DvbSubPes> _teletextPes;

        /// <summary>
        /// Teletext page numbers (decimal, e.g. 888) seen in the parsed file.
        /// </summary>
        public List<int> TeletextPages { get; }

        /// <summary>
        /// The ISO 639-2 language code of the teletext descriptor in the XML preamble, or an
        /// empty string when the file does not carry one.
        /// </summary>
        public string LanguageCode { get; private set; } = string.Empty;

        public ManzanitaTransportStreamParser()
        {
            _dvbSubs = new List<DvbSubPes>();
            _teletextPes = new List<DvbSubPes>();
            TeletextPages = new List<int>();
        }

        public void Parse(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Parse(fs);
            }
        }

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms">Input stream</param>
        public void Parse(Stream ms)
        {
            var dataIndices = GetDataIndicesAndPesStart(ms, out var dvbPesStartIndex, out var streamType, out var languageCode);
            LanguageCode = languageCode;
            if (dvbPesStartIndex <= 0)
            {
                return;
            }

            ms.Position = 0;
            foreach (var dataIndex in dataIndices)
            {
                ms.Seek(dvbPesStartIndex + dataIndex.Offset, SeekOrigin.Begin);
                var pesData = new byte[dataIndex.Length];
                var bytesRead = ms.Read(pesData, 0, pesData.Length);
                if (bytesRead < pesData.Length)
                {
                    break; // incomplete packet at end-of-file
                }

                // A teletext dump holds the bare PES payload - data_identifier plus data units -
                // so it has no PES header for the constructors below to read.
                if (streamType == TeletextStreamType || DvbSubPes.IsTeletextPayload(pesData))
                {
                    if (!DvbSubPes.IsTeletextPayload(pesData))
                    {
                        continue; // a teletext stream, but not in a shape this can decode
                    }

                    var teletextPes = DvbSubPes.FromTeletextPayload(pesData, dataIndex.Pts);
                    foreach (var page in teletextPes.PrepareTeletext()) // also flips the bit order - call once per packet
                    {
                        // Page numbers are binary coded decimals, so the filler pages FF and FE
                        // that close a transmission read back as 965 and 964 - out of range for a
                        // real page (magazine 1-8, page 00-99) and empty anyway.
                        if (page >= 100 && page <= 899 && !TeletextPages.Contains(page))
                        {
                            TeletextPages.Add(page);
                        }
                    }

                    _teletextPes.Add(teletextPes);
                    continue;
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
                    pes = new DvbSubPes(0, pesData);
                }

                pes.PresentationTimestamp = dataIndex.Pts;
                _dvbSubs.Add(pes);
            }
        }

        public static IEnumerable<ManzanitaDataIndex> GetDataIndicesAndPesStart(Stream ms, out int startIndex)
        {
            return GetDataIndicesAndPesStart(ms, out startIndex, out _);
        }

        /// <summary>
        /// Same as <see cref="GetDataIndicesAndPesStart(Stream, out int)"/>, but also reports the
        /// "type" attribute of the root element ("dvb_teletext", "dvb_subtitle", ...).
        /// </summary>
        public static IEnumerable<ManzanitaDataIndex> GetDataIndicesAndPesStart(Stream ms, out int startIndex, out string streamType)
        {
            return GetDataIndicesAndPesStart(ms, out startIndex, out streamType, out _);
        }

        private static IEnumerable<ManzanitaDataIndex> GetDataIndicesAndPesStart(Stream ms, out int startIndex, out string streamType, out string languageCode)
        {
            startIndex = 0;
            streamType = string.Empty;
            languageCode = string.Empty;
            ms.Position = 0;
            var buffer = ReadPreamble(ms, out var bytesRead, out var endIndex);
            if (endIndex < 0)
            {
                return new List<ManzanitaDataIndex>();
            }

            startIndex = GetBinaryStartIndex(buffer, bytesRead, endIndex);

            var xml = Encoding.UTF8.GetString(buffer, 0, endIndex + EndTag.Length);
            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.LoadXml(xml);
            const string ns = "http://www.manzanitasystems.com/schema/v1.03/private_stream_1";
            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("ns", ns);

            var result = new List<ManzanitaDataIndex>();
            if (xmlDoc.DocumentElement == null)
            {
                return result;
            }

            streamType = xmlDoc.DocumentElement.Attributes?["type"]?.Value ?? string.Empty;

            var teletextContentNode = xmlDoc.DocumentElement.SelectSingleNode("//ns:dvb_teletext_content", namespaceManager);
            languageCode = teletextContentNode?.Attributes?["ISO_639_language_code"]?.Value ?? string.Empty;

            var dataIndexNode = xmlDoc.DocumentElement.SelectSingleNode("ns:data_index", namespaceManager);
            if (dataIndexNode == null)
            {
                return result;
            }

            foreach (XmlNode node in dataIndexNode.SelectNodes("ns:packet", namespaceManager))
            {
                if (node.Attributes == null)
                {
                    continue;
                }

                var dataIndex = new ManzanitaDataIndex();

                var pts = node.Attributes["pts"];
                if (pts != null && ulong.TryParse(pts.Value, out var ptsNumber))
                {
                    dataIndex.Pts = ptsNumber;
                }

                var offset = node.Attributes["offset"];
                if (offset != null && long.TryParse(offset.Value, out var offsetNumber))
                {
                    dataIndex.Offset = offsetNumber;
                }

                var length = node.Attributes["length"];
                if (length != null && long.TryParse(length.Value, out var lengthNumber))
                {
                    dataIndex.Length = lengthNumber;
                    result.Add(dataIndex);
                }
            }

            return result;
        }

        /// <summary>
        /// The binary section starts right after the end tag and the line feed that closes its
        /// line (<c>&lt;/private_stream_1&gt;</c> + 0x0a). Zero when that line feed is not there:
        /// the offsets in the data index are counted from it, so there is nothing to slice.
        /// </summary>
        private static int GetBinaryStartIndex(byte[] buffer, int bytesRead, int endTagIndex)
        {
            var afterEndTag = endTagIndex + EndTag.Length;
            return afterEndTag < bytesRead && buffer[afterEndTag] == 0x0a ? afterEndTag + 1 : 0;
        }

        /// <summary>
        /// Reads the file up to and including the XML preamble, growing the buffer until the end
        /// tag turns up.
        /// </summary>
        /// <remarks>
        /// The preamble carries one <c>&lt;packet ... /&gt;</c> line per teletext or DVB packet,
        /// so it grows with the file - a .dvbttx written from some 1650 subtitles already needs
        /// more than the 200 KB that used to be read in one go. A preamble that did not fit was
        /// reported as "no packets at all" and the file then opened as nothing, with no error.
        /// </remarks>
        /// <param name="ms">The stream to read from, positioned at the start of the file.</param>
        /// <param name="bytesRead">Bytes actually in the returned buffer.</param>
        /// <param name="endTagIndex">Index of the end tag, or -1 when there is none.</param>
        private static byte[] ReadPreamble(Stream ms, out int bytesRead, out int endTagIndex)
        {
            var buffer = new byte[PreambleChunkSize];
            bytesRead = 0;
            endTagIndex = -1;

            while (true)
            {
                if (bytesRead == buffer.Length)
                {
                    if (buffer.Length >= MaxPreambleSize)
                    {
                        return buffer; // not a Manzanita file - give up rather than read it all
                    }

                    Array.Resize(ref buffer, Math.Min(MaxPreambleSize, buffer.Length * 2));
                }

                var read = ms.Read(buffer, bytesRead, buffer.Length - bytesRead);
                if (read <= 0)
                {
                    return buffer; // end of file
                }

                if (endTagIndex < 0)
                {
                    // Only the new bytes need looking at, less the overlap an end tag split
                    // across two reads would sit in.
                    var searchFrom = Math.Max(0, bytesRead - (EndTag.Length - 1));
                    bytesRead += read;
                    endTagIndex = IndexOfEndTag(buffer, bytesRead, searchFrom);
                }
                else
                {
                    bytesRead += read;
                }

                // One byte past the end tag as well: that is the line feed GetBinaryStartIndex
                // needs, and a read can stop right on the tag.
                if (endTagIndex >= 0 && bytesRead > endTagIndex + EndTag.Length)
                {
                    return buffer;
                }
            }
        }

        private static int IndexOfEndTag(byte[] buffer, int bytesRead, int startAt)
        {
            for (var i = startAt; i <= bytesRead - EndTag.Length; i++)
            {
                var match = true;
                for (var j = 0; j < EndTag.Length; j++)
                {
                    if (buffer[i + j] != EndTag[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Decodes every teletext page found while parsing.
        /// </summary>
        /// <returns>Page number (decimal, e.g. 888) to the subtitles on that page.</returns>
        public Dictionary<int, List<Paragraph>> GetTeletext()
        {
            var result = new Dictionary<int, List<Paragraph>>();
            foreach (var page in TeletextPages.OrderBy(p => p))
            {
                var pageBcd = Teletext.DecToBec(page);
                Teletext.InitializeStaticFields(TeletextPacketId, pageBcd);
                // Unlike a transport stream there is no video track to measure the start of the
                // programme against, so the time stamps in the index are used as they are - a
                // file whose first subtitle sits ten minutes in must not slide to zero.
                var runSettings = new TeletextRunSettings(null);
                var paragraphs = new List<Paragraph>();
                foreach (var pes in _teletextPes)
                {
                    foreach (var kvp in pes.GetTeletext(runSettings, page, pageBcd))
                    {
                        paragraphs.Add(kvp.Value);
                    }
                }

                // The last page has no following page header to close it - flush it by hand.
                foreach (var kvp in Teletext.ProcessTelxPacketPendingLeftovers(runSettings, page))
                {
                    paragraphs.Add(kvp.Value);
                }

                if (paragraphs.Count > 0)
                {
                    result.Add(page, paragraphs);
                }
            }

            return result;
        }

        public List<TransportStreamSubtitle> GetDvbSup()
        {
            var subtitles = new List<TransportStreamSubtitle>();
            foreach (var pes in _dvbSubs)
            {
                pes.ParseSegments();
                if (pes.PresentationTimestamp == null)
                {
                    continue;
                }

                if (pes.ObjectDataList.Count > 0)
                {
                    subtitles.Add(new TransportStreamSubtitle
                    {
                        StartMilliseconds = pes.PresentationTimestamp.Value / 90,
                        Pes = pes
                    });
                }
                else if (subtitles.Count > 0 && subtitles[subtitles.Count - 1].EndMilliseconds == 0)
                {
                    subtitles[subtitles.Count - 1].EndMilliseconds = pes.PresentationTimestamp.Value / 90;

                }
            }

            FixEmptyDurations(subtitles);
            return subtitles;
        }

        private static void FixEmptyDurations(List<TransportStreamSubtitle> subtitles)
        {
            for (var i = 0; i < subtitles.Count; i++)
            {
                var p = subtitles[i];
                if (p.EndMilliseconds != 0)
                {
                    continue;
                }

                p.EndMilliseconds = p.StartMilliseconds + (ulong)Configuration.Settings.General.NewEmptyDefaultMs;
                if (i < subtitles.Count - 1)
                {
                    var next = subtitles[i + 1];
                    if (p.EndMilliseconds >= next.StartMilliseconds)
                    {
                        p.EndMilliseconds = next.StartMilliseconds - (ulong)Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
            }
        }
    }
}
