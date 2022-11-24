using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class NinsightXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Ninsight Xml";

        public override string ToText(Subtitle subtitle, string title)
        {
            var xmlStructure = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><FILE xmlns=\"http://www.ninsight.com/Subtitle\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" Title=\"TAKEN\" Version=\"1.8\" Type=\"Protitle\"><TRACKS><TEXT_TRACK Name=\"Subtitle\"><TEXT_VERSION Lang=\"FRE\"><TEXT_HEAD><DATE>2018-05-11</DATE><MODEL BoxActive=\"true\" BoxType=\"0\" Bordered=\"true\" RowMax=\"23\" InterLine=\"4\" FontName=\"Microsoft Sans Serif\" FontSizeFull=\"31\" TopAreaSave=\"1000\" BottomAreaSave=\"1000\" LeftAreaSave=\"1000\" RightAreaSave=\"1000\" FramePerSec=\"25\" FramePerSecDivisor=\"1\" UseDropFrames=\"false\"/></TEXT_HEAD> </TEXT_VERSION></TEXT_TRACK></TRACKS></FILE>";
            var xmlTrack = "<IMG sizeX=\"720\" sizeY=\"576\" sizeYVisible=\"576\" BitCount=\"32\" Compression=\"png\"/><LIGNES><LIGNE Align=\"center\" Interline=\"4\" Position=\"519\"><SECTIONS><SECTION_TEXT TimecodeIn=\"90804\" TimecodeOut=\"90822\"><TEXT>Viens à table.</TEXT><FONT Name=\"Microsoft Sans Serif\" Size=\"31\" Color=\"$FFFFFFFF\"><EDGE Color=\"$FF000000\" Size=\"2\" Active=\"true\"/><STYLE Italic=\"false\" Bold=\"false\" Underline=\"false\" Strikeout=\"false\"/><SHADOW Active=\"false\"/></FONT><LIVE/><BACKGROUND Color=\"$FF000000\"/></SECTION_TEXT></SECTIONS></LIGNE></LIGNES><POSITION POS_X=\"72\" POS_Y=\"484\" WIDTH=\"576\" HEIGHT=\"39\"/>";
            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ns", "http://www.ninsight.com/Subtitle");
            nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            XmlNode textVersion = xml.DocumentElement.SelectSingleNode("//ns:TEXT_VERSION", nsmgr);

            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode section = xml.CreateElement( "TEXT_CLIP", "http://www.ninsight.com/Subtitle");
                section.InnerXml = xmlTrack;

                XmlAttribute st = xml.CreateAttribute("TIMECODEIN");
                st.InnerText = MillisecondsToFrames(p.StartTime.TotalMilliseconds).ToString(CultureInfo.InvariantCulture); ;
                section.Attributes.Append(st);

                XmlAttribute et = xml.CreateAttribute("TIMECODEOUT");
                et.InnerText = MillisecondsToFrames(p.EndTime.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
                section.Attributes.Append(et);

                var text = section.SelectSingleNode("//TEXT");
                text.InnerText = p.Text;

                textVersion.AppendChild(section);
            }

            var returnXml = ToUtf8XmlString(xml);
            return returnXml;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains("<TEXT_TRACK"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                var xmlText = sb.ToString();
                var startDocType = xmlText.IndexOf("<!DOCTYPE", StringComparison.Ordinal);
                if (startDocType > 0)
                {
                    var endDocType = xmlText.IndexOf('>', startDocType);
                    xmlText = xmlText.Remove(startDocType, endDocType - startDocType + 1);
                }
                xml.LoadXml(xmlText);

                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ns", "http://www.ninsight.com/Subtitle");
                nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                var clipNodes = xml.SelectNodes("//ns:TEXT_CLIP", nsmgr);
                if (clipNodes == null || clipNodes.Count == 0)
                {
                    clipNodes = xml.SelectNodes("//TEXT_CLIP");
                }

                foreach (XmlNode node in clipNodes)
                {
                    if (node.Attributes?["TIMECODEIN"] == null || node.Attributes["TIMECODEOUT"] == null)
                    {
                        continue;
                    }

                    var startTime = node.Attributes["TIMECODEIN"].InnerText;
                    var endTime = node.Attributes["TIMECODEOUT"].InnerText;
                    var text = node.InnerText;
                    var p = new Paragraph
                    {
                        StartTime =
                        {
                            TotalMilliseconds = FramesToMilliseconds(Convert.ToDouble(startTime, CultureInfo.InvariantCulture))
                        },
                        EndTime =
                        {
                            TotalMilliseconds = FramesToMilliseconds(Convert.ToDouble(endTime, CultureInfo.InvariantCulture))
                        },
                        Text = text
                    };
                    subtitle.Paragraphs.Add(p);
                }

                subtitle.Renumber();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                _errorCount = 1;
            }
        }
    }
}
