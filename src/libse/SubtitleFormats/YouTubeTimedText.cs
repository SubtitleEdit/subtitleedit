using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// YouTube timed text, aka srv3 - what yt-dlp writes for --sub-format srv3 and what
    /// YTSubConverter produces as .ytt. Text can be split over several timed s runs inside
    /// each p element.
    /// </summary>
    internal class YouTubeTimedText : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "YouTube timed text srv3";

        // No parenthesis in the name above - GetSubtitleFormatByFriendlyName truncates there.

        public override List<string> AlternateExtensions => new List<string> { ".ytt", ".srv3" };

        public override List<string> AlternateNames => new List<string> { "Unknown 82" };

   public override string ToText(Subtitle subtitle, string title)
        {
            const string xmpTemplate = @"<?xml version='1.0' encoding='utf-8'?>
<timedtext format='3'>
    <body />
</timedtext>";

            var xml = new XmlDocument();
            xml.LoadXml(xmpTemplate.Replace('\'', '"'));
            var paragraphInsertNode = xml.DocumentElement.SelectSingleNode("body");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("p");
                paragraph.InnerText = p.Text;//.Replace(Environment.NewLine, " ");

                XmlAttribute tAttribute = xml.CreateAttribute("t");
                tAttribute.InnerText = Convert.ToInt64(p.StartTime.TotalMilliseconds).ToString();
                paragraph.Attributes.Append(tAttribute);

                XmlAttribute dAttribute = xml.CreateAttribute("d");
                dAttribute.InnerText = Convert.ToInt64(p.DurationTotalMilliseconds).ToString();
                paragraph.Attributes.Append(dAttribute);

                paragraphInsertNode.AppendChild(paragraph);
            }
            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var xmlAsText = JoinLinesTrimmed(lines);
            if (!xmlAsText.Contains("</timedtext>") || !xmlAsText.Contains("<p "))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("body/p"))
                {
                    try
                    {
                        var timeCodeIn = new TimeCode(Convert.ToDouble(node.Attributes["t"].InnerText, CultureInfo.InvariantCulture));
                        var timeCodeOut = new TimeCode(timeCodeIn.TotalMilliseconds + Convert.ToDouble(node.Attributes["d"].InnerText, CultureInfo.InvariantCulture));
                        var p = new Paragraph(timeCodeIn, timeCodeOut, node.InnerText);
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
            catch (Exception)
            {
                _errorCount++;
            }
        }

    }
}
