using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DlDd : SubtitleFormat
    {
        public override string Extension => ".htm";

        public override string Name => "dl dd span";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsText = sb.ToString().Trim();
            if (!xmlAsText.Contains("</dl>") || !xmlAsText.Contains(" data-time"))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("dd/span"))
                {
                    try
                    {
                        var timeCodeIn = new TimeCode(Convert.ToDouble(node.Attributes["data-time"].InnerText));
                        var timeCodeOut = new TimeCode(timeCodeIn.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(node.InnerText));
                        var p = new Paragraph(timeCodeIn, timeCodeOut, Utilities.AutoBreakLine(node.InnerText));
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        _errorCount++;
                    }
                }
                subtitle.Sort(SubtitleSortCriteria.StartTime);
                for (int index = 0; index < subtitle.Paragraphs.Count - 1; index++)
                {
                    var paragraph = subtitle.Paragraphs[index];
                    var next = subtitle.GetParagraphOrDefault(index + 1);
                    if (next.StartTime.TotalMilliseconds <= paragraph.EndTime.TotalMilliseconds)
                    {
                        paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
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
