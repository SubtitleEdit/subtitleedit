using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubtitleEditorProject : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Subtitle Editor Project";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<SubtitleEditorProject") &&
                xmlAsString.Contains("<subtitle "))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);

                    XmlNode div = xml.DocumentElement.SelectSingleNode("subtitles");
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

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\"?>" + Environment.NewLine +
                "<SubtitleEditorProject version=\"1.0\">" + Environment.NewLine +
                "  <player />" + Environment.NewLine +
                "  <waveform />" + Environment.NewLine +
                "  <styles />" + Environment.NewLine +
                "  <subtitles timing_mode=\"TIME\" edit_timing_mode=\"TIME\" framerate=\"25\">" + Environment.NewLine +
                "  </subtitles>" + Environment.NewLine +
                "  <subtitles-selection />" + Environment.NewLine +
                "</SubtitleEditorProject>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            //          <subtitle duration="2256" effect="" end="124581" layer="0" margin-l="0" margin-r="0" margin-v="0" name="" note="" path="0" start="122325" style="Default" text="The fever hath weakened thee." translation="" />
            XmlNode div = xml.DocumentElement.SelectSingleNode("subtitles");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("subtitle");

                XmlAttribute duration = xml.CreateAttribute("duration");
                duration.InnerText = ((int)Math.Round(p.Duration.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(duration);

                XmlAttribute effect = xml.CreateAttribute("effect");
                effect.InnerText = string.Empty;
                paragraph.Attributes.Append(effect);

                XmlAttribute end = xml.CreateAttribute("end");
                end.InnerText = ((int)Math.Round(p.EndTime.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(end);

                XmlAttribute layer = xml.CreateAttribute("layer");
                layer.InnerText = "0";
                paragraph.Attributes.Append(layer);

                XmlAttribute marginL = xml.CreateAttribute("margin-l");
                marginL.InnerText = "0";
                paragraph.Attributes.Append(marginL);

                XmlAttribute marginR = xml.CreateAttribute("margin-r");
                marginR.InnerText = "0";
                paragraph.Attributes.Append(marginR);

                XmlAttribute marginV = xml.CreateAttribute("margin-v");
                marginV.InnerText = "0";
                paragraph.Attributes.Append(marginV);

                XmlAttribute name = xml.CreateAttribute("name");
                name.InnerText = string.Empty;
                paragraph.Attributes.Append(name);

                XmlAttribute note = xml.CreateAttribute("note");
                note.InnerText = string.Empty;
                paragraph.Attributes.Append(note);

                XmlAttribute path = xml.CreateAttribute("path");
                path.InnerText = "0";
                paragraph.Attributes.Append(path);

                XmlAttribute start = xml.CreateAttribute("start");
                start.InnerText = ((int)Math.Round(p.StartTime.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
                paragraph.Attributes.Append(start);

                XmlAttribute style = xml.CreateAttribute("style");
                style.InnerText = "Default";
                paragraph.Attributes.Append(style);

                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                XmlAttribute textNode = xml.CreateAttribute("text");
                textNode.InnerText = text;
                paragraph.Attributes.Append(textNode);

                XmlAttribute translation = xml.CreateAttribute("translation");
                translation.InnerText = string.Empty;
                paragraph.Attributes.Append(translation);

                div.AppendChild(paragraph);
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

            XmlNode div = xml.DocumentElement.SelectSingleNode("subtitles");
            foreach (XmlNode node in div.ChildNodes)
            {
                try
                {
                    //<subtitle duration="2256" effect="" end="124581" layer="0" margin-l="0" margin-r="0" margin-v="0" name="" note="" path="0" start="122325" style="Default" text="The fever hath weakened thee." translation="" />
                    var p = new Paragraph { StartTime = { TotalMilliseconds = int.Parse(node.Attributes["start"].Value) } };
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + int.Parse(node.Attributes["duration"].Value);
                    p.Text = node.Attributes["text"].Value;

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
