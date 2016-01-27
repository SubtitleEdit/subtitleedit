using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedTextImage : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Timed Text Image"; }
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

            if (xmlAsString.Contains("=\"http://www.w3.org/ns/ttml#parameter") && (xmlAsString.Contains(".png") || xmlAsString.Contains(".jpg") || xmlAsString.Contains(".bmp")))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);

                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("tt", xml.DocumentElement.NamespaceURI);
                    XmlNode body = xml.DocumentElement.SelectSingleNode("//tt:body", nsmgr);
                    if (body == null)
                        body = xml.DocumentElement.SelectSingleNode("//tt:body", nsmgr);
                    int numberOfParagraphs = body.ChildNodes.Count;
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

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("tt", xml.DocumentElement.NamespaceURI);

            XmlNode body = xml.DocumentElement.SelectSingleNode("//tt:body", nsmgr);
            if (body == null)
                body = xml.DocumentElement.SelectSingleNode("//tt:body", nsmgr);
            int numberOfParagraphs = body.ChildNodes.Count;

            bool couldBeFrames = true;
            bool couldBeMillisecondsWithMissingLastDigit = true;
            foreach (XmlNode node in body.ChildNodes)
            {
                try
                {
                    var pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name.Replace("tt:", string.Empty))
                        {
                            case "image":
                                var src = innerNode.Attributes["src"];
                                if (src != null)
                                    pText.Append(src.InnerText);
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
                        if (end.Length != 11 || end.Substring(8, 1) != ":" || start == null ||
                            start.Length != 11 || start.Substring(8, 1) != ":")
                        {
                            couldBeFrames = false;
                        }

                        if (couldBeMillisecondsWithMissingLastDigit && (end.Length != 11 || start == null || start.Length != 11 || end.Substring(8, 1) != "." | start.Substring(8, 1) != "."))
                        {
                            couldBeMillisecondsWithMissingLastDigit = false;
                        }

                        //string end = node.Attributes["end"].InnerText;
                        double dBegin, dEnd;
                        if (!start.Contains(':') && Utilities.CountTagInText(start, '.') == 1 &&
                            !end.Contains(':') && Utilities.CountTagInText(end, '.') == 1 &&
                            double.TryParse(start, NumberStyles.Float, CultureInfo.InvariantCulture, out dBegin) && double.TryParse(end, NumberStyles.Float, CultureInfo.InvariantCulture, out dEnd))
                        {
                            subtitle.Paragraphs.Add(new Paragraph(text, dBegin * TimeCode.BaseUnit, dEnd * TimeCode.BaseUnit));
                        }
                        else
                        {
                            if (start.Length == 8 && start[2] == ':' && start[5] == ':' &&
                                end.Length == 8 && end[2] == ':' && end[5] == ':')
                            {
                                var p = new Paragraph();
                                var parts = start.Split(SplitCharColon);
                                p.StartTime = new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0);
                                parts = end.Split(SplitCharColon);
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
                        if (dur.Length != 11 || dur.Substring(8, 1) != ":" || start == null ||
                           start.Length != 11 || start.Substring(8, 1) != ":")
                        {
                            couldBeFrames = false;
                        }

                        if (couldBeMillisecondsWithMissingLastDigit && (dur.Length != 11 || start == null || start.Length != 11 || dur.Substring(8, 1) != "." | start.Substring(8, 1) != "."))
                        {
                            couldBeMillisecondsWithMissingLastDigit = false;
                        }

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

            if (couldBeFrames)
            {
                bool all30OrBelow = true;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.StartTime.Milliseconds > 30 || p.EndTime.Milliseconds > 30)
                        all30OrBelow = false;
                }
                if (all30OrBelow)
                {
                    foreach (Paragraph p in subtitle.Paragraphs)
                    {
                        p.StartTime.Milliseconds = SubtitleFormat.FramesToMillisecondsMax999(p.StartTime.Milliseconds);
                        p.EndTime.Milliseconds = SubtitleFormat.FramesToMillisecondsMax999(p.EndTime.Milliseconds);
                    }
                }
            }
            else if (couldBeMillisecondsWithMissingLastDigit)
            {
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    p.StartTime.Milliseconds *= 10;
                    p.EndTime.Milliseconds *= 10;
                }
            }

            subtitle.Renumber();
        }

    }
}
