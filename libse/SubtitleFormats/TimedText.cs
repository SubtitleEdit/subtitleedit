using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedText : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Timed Text draft 2006-10"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().RemoveControlCharactersButWhiteSpace().Trim();

            if (xmlAsString.Contains("xmlns:tts=\"http://www.w3.org/2006/04"))
                return false;

            if (xmlAsString.Contains("http://www.w3.org/") &&
                xmlAsString.Contains("/ttaf1"))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);

                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);
                    XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).SelectSingleNode("ttaf1:div", nsmgr);
                    if (div == null)
                        div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;
                    int numberOfParagraphs = div.ChildNodes.Count;
                    return numberOfParagraphs > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }

        private static string ConvertToTimeString(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<tt xmlns=\"http://www.w3.org/2006/10/ttaf1\" xmlns:ttp=\"http://www.w3.org/2006/10/ttaf1#parameter\" ttp:timeBase=\"media\" xmlns:tts=\"http://www.w3.org/2006/10/ttaf1#style\" xml:lang=\"en\" xmlns:ttm=\"http://www.w3.org/2006/10/ttaf1#metadata\">" + Environment.NewLine +
                "   <head>" + Environment.NewLine +
                "       <metadata>" + Environment.NewLine +
                "           <ttm:title></ttm:title>" + Environment.NewLine +
                "      </metadata>" + Environment.NewLine +
                "       <styling>" + Environment.NewLine +
                "         <style id=\"s0\" tts:backgroundColor=\"black\" tts:fontStyle=\"normal\" tts:fontSize=\"16\" tts:fontFamily=\"sansSerif\" tts:color=\"white\" />" + Environment.NewLine +
                "      </styling>" + Environment.NewLine +
                "   </head>" + Environment.NewLine +
                "   <body tts:textAlign=\"center\" style=\"s0\">" + Environment.NewLine +
                "       <div />" + Environment.NewLine +
                "   </body>" + Environment.NewLine +
                "</tt>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", "http://www.w3.org/2006/10/ttaf1");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/2006/10/ttaf1#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/2006/10/ttaf1#style");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/2006/10/ttaf1#metadata");

            XmlNode titleNode = xml.DocumentElement.SelectSingleNode("//ttaf1:head", nsmgr).FirstChild.FirstChild;
            titleNode.InnerText = title;

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).SelectSingleNode("ttaf1:div", nsmgr);
            if (div == null)
                div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;

            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/2006/10/ttaf1");

                //string text = p.Text.Replace("<i>", "@iSTART__").Replace("</i>", "@iEND__");
                string text = p.Text.Replace(Environment.NewLine, "\n").Replace("\n", "@iNEWLINE__");
                text = HtmlUtil.RemoveHtmlTags(text);
                paragraph.InnerText = text;
                paragraph.InnerXml = paragraph.InnerXml.Replace("@iNEWLINE__", "<br />");
                //paragraph.InnerXml = paragraph.InnerXml.Replace("@iSTART__", "<i>").Replace("@iEND__", "</i>");

                XmlAttribute start = xml.CreateAttribute("begin");
                start.InnerText = ConvertToTimeString(p.StartTime);
                paragraph.Attributes.Append(start);

                XmlAttribute id = xml.CreateAttribute("id");
                id.InnerText = "p" + no;
                paragraph.Attributes.Append(id);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ConvertToTimeString(p.EndTime);
                paragraph.Attributes.Append(end);

                div.AppendChild(paragraph);
                no++;
            }

            string s = ToUtf8XmlString(xml);
            s = s.Replace(" xmlns=\"\"", string.Empty);
            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttaf1", xml.DocumentElement.NamespaceURI);

            XmlNode div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).SelectSingleNode("ttaf1:div", nsmgr);
            if (div == null)
                div = xml.DocumentElement.SelectSingleNode("//ttaf1:body", nsmgr).FirstChild;

            var styleDic = new Dictionary<string, string>();
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//ttaf1:style", nsmgr))
            {
                if (node.Attributes["tts:fontStyle"] != null && node.Attributes["xml:id"] != null)
                {
                    styleDic.Add(node.Attributes["xml:id"].Value, node.Attributes["tts:fontStyle"].Value);
                }
            }

            foreach (XmlNode node in div.ChildNodes)
            {
                try
                {
                    var pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name)
                        {
                            case "br":
                                pText.AppendLine();
                                break;
                            case "span":
                                bool italic = false;
                                if (innerNode.Attributes["style"] != null && styleDic.ContainsKey(innerNode.Attributes["style"].Value))
                                {
                                    if (styleDic[innerNode.Attributes["style"].Value].Contains("italic"))
                                    {
                                        italic = true;
                                        pText.Append("<i>");
                                    }
                                }
                                if (!italic && innerNode.Attributes != null)
                                {
                                    var fs = innerNode.Attributes.GetNamedItem("tts:fontStyle");
                                    if (fs != null && fs.Value == "italic")
                                    {
                                        italic = true;
                                        pText.Append("<i>");
                                    }
                                }
                                if (innerNode.HasChildNodes)
                                {
                                    foreach (XmlNode innerInnerNode in innerNode.ChildNodes)
                                    {
                                        if (innerInnerNode.Name == "br")
                                        {
                                            pText.AppendLine();
                                        }
                                        else
                                        {
                                            pText.Append(innerInnerNode.InnerText);
                                        }
                                    }
                                }
                                else
                                {
                                    pText.Append(innerNode.InnerText);
                                }
                                if (italic)
                                    pText.Append("</i>");
                                break;
                            case "i":
                                pText.Append("<i>" + innerNode.InnerText + "</i>");
                                break;
                            case "b":
                                pText.Append("<b>" + innerNode.InnerText + "</b>");
                                break;
                            default:
                                pText.Append(innerNode.InnerText);
                                break;
                        }
                    }

                    string start = null; // = node.Attributes["begin"].InnerText;
                    string end = null; // = node.Attributes["begin"].InnerText;
                    string dur = null; // = node.Attributes["begin"].InnerText;
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        if (attr.Name.EndsWith("begin", StringComparison.Ordinal))
                            start = attr.InnerText;
                        else if (attr.Name.EndsWith("end", StringComparison.Ordinal))
                            end = attr.InnerText;
                        else if (attr.Name.EndsWith("duration", StringComparison.Ordinal))
                            dur = attr.InnerText;
                    }
                    //string start = node.Attributes["begin"].InnerText;
                    string text = pText.ToString();
                    text = text.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
                    text = text.Replace("<i></i>", string.Empty).Trim();
                    if (end != null)
                    {
                        //string end = node.Attributes["end"].InnerText;
                        double dBegin, dEnd;
                        if (!start.Contains(':') && Utilities.CountTagInText(start, '.') == 1 &&
                            !end.Contains(':') && Utilities.CountTagInText(end, '.') == 1 &&
                            double.TryParse(start, NumberStyles.Float , CultureInfo.InvariantCulture, out dBegin) && double.TryParse(end, NumberStyles.Float, CultureInfo.InvariantCulture, out dEnd))
                        {
                            subtitle.Paragraphs.Add(new Paragraph(text, dBegin * TimeCode.BaseUnit, dEnd * TimeCode.BaseUnit));
                        }
                        else
                        {
                            if (start.Length == 8 && start[2] == ':' && start[5] == ':' &&
                                end.Length == 8 && end[2] == ':' && end[5] == ':')
                            {
                                var p = new Paragraph();
                                var parts = start.Split(new[] { ':' });
                                p.StartTime = new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0);
                                parts = end.Split(new[] { ':' });
                                p.EndTime = new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0);
                                p.Text = text;
                                subtitle.Paragraphs.Add(p);
                            }
                            else
                            {
                                subtitle.Paragraphs.Add(new Paragraph(TimedText10.GetTimeCode(start, false), TimedText10.GetTimeCode(end, false), text));
                            }
                        }
                    }
                    else if (dur != null)
                    {
                        TimeCode duration = TimedText10.GetTimeCode(dur, false);
                        TimeCode startTime = TimedText10.GetTimeCode(start, false);
                        var endTime = new TimeCode(startTime.TotalMilliseconds + duration.TotalMilliseconds);
                        subtitle.Paragraphs.Add(new Paragraph(startTime, endTime, text));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        public override List<string> AlternateExtensions
        {
            get
            {
                return new List<string> { ".tt" };
            }
        }

    }
}
