using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Reads the xml file inside a ".prproj" file (gzipped zml) from Adobe Premiere (Essential Graphics).
    /// </summary>
    public class AdobePremierePrProj : SubtitleFormat
    {

        public class TimeLookup
        {
            public string ObjectId { get; set; } // lookup to time code via VideoClipTrackItem
            public string ObjectRefId { get; set; } // lookup to subtitle text
            public string Start { get; set; }
            public string End { get; set; }
        }

        public override string Extension => ".xml";

        public override string Name => "Adobe Premiere PrProj Xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unpacks a gzipped .prproj into its XML text (uncompressed project files are
        /// returned as-is). Returns null when the file is neither.
        /// </summary>
        public static string LoadFromZipFile(string fileName)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var header = new byte[2];
                    var read = fileStream.Read(header, 0, 2);
                    fileStream.Position = 0;
                    if (read == 2 && header[0] == 0x1f && header[1] == 0x8b)
                    {
                        using (var decompressionStream = new GZipStream(fileStream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressionStream, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }

                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string xmlString = sb.ToString();
            if (!xmlString.Contains("<PremiereData "))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            // make time code lookup
            var dictionary = new List<TimeLookup>();
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//VideoComponentChain"))
            {
                var objectIdNode = node.Attributes["ObjectID"];
                if (objectIdNode != null)
                {
                    var objectId = node.Attributes["ObjectID"].InnerText;
                    foreach (XmlNode innerNode in node.SelectNodes("ComponentChain/Components/Component"))
                    {
                        var attr = innerNode.Attributes["ObjectRef"];
                        if (attr != null)
                        {
                            dictionary.Add(new TimeLookup { ObjectId = objectId, ObjectRefId = attr.InnerText });
                        }
                    }
                }
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//VideoClipTrackItem"))
            {
                foreach (XmlNode innerNode in node.SelectNodes("ClipTrackItem/ComponentOwner/Components"))
                {
                    var attr = innerNode.Attributes["ObjectRef"];
                    if (attr != null)
                    {
                        var objectRef = attr.InnerText;
                        var startNode = node.SelectSingleNode("ClipTrackItem/TrackItem/Start");
                        var endNode = node.SelectSingleNode("ClipTrackItem/TrackItem/End");
                        if (objectRef != null && startNode != null && endNode != null)
                        {
                            foreach (var f in dictionary.Where(p => p.ObjectId == objectRef))
                            {
                                f.Start = startNode.InnerText;
                                f.End = endNode.InnerText;
                            }
                        }
                    }
                }
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//VideoFilterComponent"))
            {
                try
                {
                    var objectId = node.Attributes["ObjectID"].InnerText;
                    var displayName = node.SelectSingleNode("Component/DisplayName");
                    if (displayName != null && displayName.InnerText == "Text")
                    {
                        var instanceName = node.SelectSingleNode("Component/InstanceName");
                        if (instanceName != null)
                        {
                            var p = new Paragraph(instanceName.InnerText, 0, 0);
                            var f = dictionary.FirstOrDefault(t => t.ObjectRefId == objectId);
                            if (f != null)
                            {
                                if (long.TryParse(f.Start, NumberStyles.Integer, CultureInfo.InvariantCulture, out var start) &&
                                    long.TryParse(f.End, NumberStyles.Integer, CultureInfo.InvariantCulture, out var end))
                                {
                                    // Premiere stores 254016000000 ticks per second, i.e.
                                    // 254016000 ticks per millisecond (a 4-second clip start
                                    // is 1016064000000 in a real .prproj)
                                    p.StartTime.TotalMilliseconds = start / 254016000.0;
                                    p.EndTime.TotalMilliseconds = end / 254016000.0;
                                }
                            }
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }
    }
}
