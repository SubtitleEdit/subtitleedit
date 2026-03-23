//using Nikse.SubtitleEdit.Core.Common;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml;

//namespace Nikse.SubtitleEdit.Core.SubtitleFormats
//{
//    public class Smil30 : SubtitleFormat
//    {
//        public override string Extension => ".xml";

//        public override string Name => "SMIL 3.0";

//        private static string ToTimeCode(TimeCode tc)
//        {
//            return tc.ToString(false).Replace(',', '.');
//        }

//        private static TimeCode DecodeTimeCode(string s)
//        {
//            var parts = s.Split(new[] { ';', ':', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
//            return DecodeTimeCodeFramesFourParts(parts);
//        }

//        public override string ToText(Subtitle subtitle, string title)
//        {
//            var xml = new XmlDocument();
//            xml.LoadXml("<smil xmlns=\"http://www.w3.org/ns/SMIL\" xmlns:epub=\"http://www.idpf.org/2007/ops\" version=\"3.0\"><body></body></smil>");

//            const string ns = "http://www.w3.org/ns/SMIL";
//            var nsmgr = new XmlNamespaceManager(xml.NameTable);
//            nsmgr.AddNamespace("smil", ns);
//            nsmgr.AddNamespace("epub", "http://www.idpf.org/2007/ops");

//            XmlNode bodyNode = xml.SelectSingleNode("//smil:body", nsmgr);
//            var count = 1;
//            foreach (var p in subtitle.Paragraphs)
//            {
//                XmlNode paragraph = xml.CreateElement("par", ns);
//                XmlNode text = xml.CreateElement("text", ns);
//                XmlNode audio = xml.CreateElement("audio", ns);
//                text.InnerText = p.Text;

//                XmlAttribute start = xml.CreateAttribute("clipBegin");
//                start.InnerText = ToTimeCode(p.StartTime);
//                audio.Attributes.Append(start);

//                XmlAttribute end = xml.CreateAttribute("clipEnd");
//                end.InnerText = ToTimeCode(p.EndTime);
//                audio.Attributes.Append(end);

//                XmlAttribute textSource = xml.CreateAttribute("src");
//                textSource.InnerText = $"text.xhtml#para{count}";
//                text.Attributes.Append(textSource);

//                XmlAttribute audioSource = xml.CreateAttribute("src");
//                audioSource.InnerText = "audio.mp3";
//                audio.Attributes.Append(audioSource);

//                bodyNode.AppendChild(paragraph);
//                paragraph.AppendChild(text);
//                paragraph.AppendChild(audio);

//                count++;
//            }

//            return ToUtf8XmlString(xml);
//        }

//        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
//        {
//            _errorCount = 0;
//            var sb = new StringBuilder();
//            lines.ForEach(line => sb.AppendLine(line));
//            var allText = sb.ToString();
//            if (!allText.Contains("<smil") || !allText.Contains("</smil>"))
//            {
//                return;
//            }

//            var xml = new XmlDocument { XmlResolver = null };
//            try
//            {
//                xml.LoadXml(allText);
//            }
//            catch
//            {
//                _errorCount = 1;
//                return;
//            }

//            const string ns = "http://www.w3.org/ns/SMIL";
//            var nsmgr = new XmlNamespaceManager(xml.NameTable);
//            nsmgr.AddNamespace("smil", ns);
//            nsmgr.AddNamespace("epub", "http://www.idpf.org/2007/ops");

//            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//smil:par", nsmgr))
//            {
//                try
//                {
//                    var textNode = node.SelectSingleNode("smil:text", nsmgr);
//                    var audioNode = node.SelectSingleNode("smil:audio", nsmgr);

//                    var text = textNode.InnerText;
//                    var start = audioNode.Attributes["clipBegin"].InnerText;
//                    var end = audioNode.Attributes["clipEnd"].InnerText;
//                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text));
//                }
//                catch (Exception ex)
//                {
//                    System.Diagnostics.Debug.WriteLine(ex.Message);
//                    _errorCount++;
//                }
//            }

//            subtitle.Renumber();
//        }
//    }
//}
