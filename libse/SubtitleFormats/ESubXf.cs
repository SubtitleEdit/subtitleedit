using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://www.fab-online.com/pdf/ESUB-XF.pdf
    /// </summary>
    public class ESubXf : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "ESUB-XF";

        private const string NameSpaceUri = "urn:esub-xf";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
               "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                "<esub-xf xmlns=\"" + NameSpaceUri + "\" framerate=\"" + Configuration.Settings.General.CurrentFrameRate.ToString(CultureInfo.InvariantCulture) + "\" timebase=\"smpte\">" + Environment.NewLine +
                "  <subtitlelist language=\"eng\" langname=\"English\" type=\"translation\">" + Environment.NewLine +
                "  </subtitlelist>" + Environment.NewLine +
                "</esub-xf>";

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("esub-xf", NameSpaceUri);
            var subtitleList = xml.DocumentElement.SelectSingleNode("//esub-xf:subtitlelist", ns);
            foreach (var p in subtitle.Paragraphs)
            {
                var paragraph = xml.CreateElement("subtitle", NameSpaceUri);

                var start = xml.CreateAttribute("display");
                start.InnerText = p.StartTime.ToHHMMSSFF();
                paragraph.Attributes.Append(start);

                var end = xml.CreateAttribute("clear");
                end.InnerText = p.EndTime.ToHHMMSSFF();
                paragraph.Attributes.Append(end);

                var hRegion = xml.CreateElement("hregion", NameSpaceUri);
                paragraph.AppendChild(hRegion);

                foreach (var line in p.Text.SplitToLines())
                {
                    var lineNode = xml.CreateElement("line", NameSpaceUri);
                    hRegion.AppendChild(lineNode);
                    lineNode.InnerText = line;
                }
                subtitleList.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlString = sb.ToString();
            if (!xmlString.Contains("<subtitlelist"))
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

            char[] timeCodeSeparators = new[] { ':' };
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("esub-xf", NameSpaceUri);
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//esub-xf:subtitlelist/esub-xf:subtitle", ns))
            {
                try
                {
                    string start = node.Attributes["display"].InnerText;
                    string end = node.Attributes["clear"].InnerText;
                    sb = new StringBuilder();
                    foreach (XmlNode lineNode in node.SelectNodes("esub-xf:hregion/esub-xf:line", ns))
                    {
                        sb.AppendLine(lineNode.InnerText);
                    }
                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCodeFrames(start, timeCodeSeparators), DecodeTimeCodeFrames(end, timeCodeSeparators), sb.ToString().TrimEnd()));
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
