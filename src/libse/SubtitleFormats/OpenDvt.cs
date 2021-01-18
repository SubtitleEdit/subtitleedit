using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class OpenDvt : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "OpenDVT";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("OpenDVT"))
            {
                try
                {
                    var xml = new XmlDocument { XmlResolver = null };
                    xml.LoadXml(xmlAsString);
                    int numberOfParagraphs = xml.DocumentElement.SelectSingleNode("Lines").ChildNodes.Count;
                    return numberOfParagraphs > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var guid = new Guid().ToString();
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<OpenDVT UUID=\"{" + guid + "\" ShortID=\"" + title + "\" Type=\"Deposition\" Version=\"1.3\">" + Environment.NewLine +
                "<Information>" + Environment.NewLine +
                "  <Origination>" + Environment.NewLine +
                "    <ID>" + guid + "</ID> " + Environment.NewLine +
                "    <AppName>Subtitle Edit</AppName> " + Environment.NewLine +
                "    <AppVersion>2.9</AppVersion> " + Environment.NewLine +
                "    <VendorName>Nikse.dk</VendorName> " + Environment.NewLine +
                "    <VendorPhone></VendorPhone> " + Environment.NewLine +
                "    <VendorURL>www.nikse.dk.com</VendorURL> " + Environment.NewLine +
                "  </Origination>" + Environment.NewLine +
                "  <Case>" + Environment.NewLine +
                "    <MatterNumber /> " + Environment.NewLine +
                "  </Case>" + Environment.NewLine +
                "  <Deponent>" + Environment.NewLine +
                "    <FirstName></FirstName> " + Environment.NewLine +
                "    <LastName></LastName> " + Environment.NewLine +
                "  </Deponent>" + Environment.NewLine +
                "  <ReportingFirm>" + Environment.NewLine +
                "    <Name /> " + Environment.NewLine +
                "  </ReportingFirm>" + Environment.NewLine +
                "  <FirstPageNo>1</FirstPageNo> " + Environment.NewLine +
                "  <LastPageNo>3</LastPageNo> " + Environment.NewLine +
                "  <MaxLinesPerPage>25</MaxLinesPerPage> " + Environment.NewLine +
                "  <Volume>1</Volume> " + Environment.NewLine +
                "  <TakenOn>06/02/2010</TakenOn> " + Environment.NewLine +
                "  <TranscriptVerify></TranscriptVerify> " + Environment.NewLine +
                "  <PrintVerify></PrintVerify> " + Environment.NewLine +
                "  </Information>" + Environment.NewLine +
                "<Lines Count=\"" + subtitle.Paragraphs.Count + "\">" + Environment.NewLine +
                "</Lines>" + Environment.NewLine +
                "<Streams Count=\"0\">" + Environment.NewLine +
                "<Stream ID=\"0\">" + Environment.NewLine +
                //"<URI>C:\Users\Eric\Desktop\Player Folder\Bing\Bing.mpg</URI>
                //"<FileSize>52158464</FileSize>
                //"<FileDate>06/02/2009 10:44:37</FileDate>
                //"<DurationMs>166144</DurationMs>
                //"<VolumeLabel>OS</VolumeLabel>
                "  </Stream>" + Environment.NewLine +
                "</Streams>" + Environment.NewLine +
                "</OpenDVT>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode lines = xml.DocumentElement.SelectSingleNode("Lines");
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode line = xml.CreateElement("Line");

                XmlAttribute id = xml.CreateAttribute("ID");
                id.InnerText = no.ToString();
                line.Attributes.Append(id);

                XmlNode stream = xml.CreateElement("Stream");
                stream.InnerText = "0";
                line.AppendChild(stream);

                XmlNode timeMS = xml.CreateElement("TimeMs");
                timeMS.InnerText = p.StartTime.TotalMilliseconds.ToString();
                line.AppendChild(timeMS);

                XmlNode pageNo = xml.CreateElement("PageNo");
                pageNo.InnerText = "1";
                line.AppendChild(pageNo);

                XmlNode lineNo = xml.CreateElement("LineNo");
                lineNo.InnerText = "1";
                line.AppendChild(lineNo);

                XmlNode qa = xml.CreateElement("QA");
                qa.InnerText = "-";
                line.AppendChild(qa);

                XmlNode text = xml.CreateElement("Text");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text, true);
                line.AppendChild(text);

                lines.AppendChild(line);

                no++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());

            XmlNode div = xml.DocumentElement.SelectSingleNode("Lines");
            foreach (XmlNode node in div.ChildNodes)
            {
                try
                {
                    var p = new Paragraph();

                    XmlNode text = node.SelectSingleNode("Text");
                    if (text != null)
                    {
                        sb.Clear();
                        foreach (XmlNode innerNode in text.ChildNodes)
                        {
                            if (innerNode.Name == "br")
                            {
                                sb.AppendLine();
                            }
                            else
                            {
                                sb.Append(innerNode.InnerText);
                            }
                        }
                        p.Text = sb.ToString();
                    }

                    XmlNode timeMS = node.SelectSingleNode("TimeMs");
                    if (timeMS != null)
                    {
                        string ms = timeMS.InnerText;
                        long milliseconds;
                        if (long.TryParse(ms, out milliseconds))
                        {
                            p.StartTime = new TimeCode(milliseconds);
                        }
                    }
                    p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text));

                    subtitle.Paragraphs.Add(p);
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
