using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// Manzanita transport stream parser.
    /// </summary>
    public class ManzanitaTransportStreamParser
    {
        private readonly List<DvbSubPes> _dvbSubs;

        public ManzanitaTransportStreamParser()
        {
            _dvbSubs = new List<DvbSubPes>();
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
            var dataIndices = GetDataIndicesAndPesStart(ms, out var dvbPesStartIndex);
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
            startIndex = 0;
            ms.Position = 0;
            var buffer = new byte[200_000];
            var bytesRead = ms.Read(buffer, 0, buffer.Length);
            var xml = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            const string endTag = "</private_stream_1>";
            var endIndex = xml.IndexOf(endTag, StringComparison.Ordinal);
            if (endIndex < 0)
            {
                return new List<ManzanitaDataIndex>();
            }

            startIndex = FindBinaryStartIndex(bytesRead, buffer, endTag);

            xml = xml.Substring(0, endIndex + endTag.Length);
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

        private static int FindBinaryStartIndex(int bytesRead, byte[] buffer, string endTag)
        {
            for (var i = 0; i < bytesRead - 20; i++)
            {
                // </private_stream_1> + 0x0a
                // 3C 2F 70 72 69 76 61 74 65 5F 73 74 72 65 61 6D 5F 31 3E 0A
                if (buffer[i + 0] == 0x3c &&
                    buffer[i + 1] == 0x2f &&
                    buffer[i + 2] == 0x70 &&
                    buffer[i + 3] == 0x72 &&
                    buffer[i + 4] == 0x69 &&
                    buffer[i + 5] == 0x76 &&
                    buffer[i + 6] == 0x61 &&
                    buffer[i + 7] == 0x74 &&
                    buffer[i + 8] == 0x65 &&
                    buffer[i + 9] == 0x5f &&
                    buffer[i + 10] == 0x73 &&
                    buffer[i + 11] == 0x74 &&
                    buffer[i + 12] == 0x72 &&
                    buffer[i + 13] == 0x65 &&
                    buffer[i + 14] == 0x61 &&
                    buffer[i + 15] == 0x6d &&
                    buffer[i + 16] == 0x5f &&
                    buffer[i + 17] == 0x31 &&
                    buffer[i + 18] == 0x3e &&
                    buffer[i + 19] == 0x0a)
                {
                    return i + endTag.Length + 1;
                }
            }

            return 0;
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
