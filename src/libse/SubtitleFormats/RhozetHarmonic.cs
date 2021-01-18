using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class RhozetHarmonic : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Rhozet Harmonic";

        private static string ToTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //<TitlerData>
            // <Data StartTimecode='00:00:02;21' EndTimecode='00:00:05;21' Title='CAPTIONING PROVIDED BY
            //CharSize='0.2' PosX='0.5' PosY='0.75' ColorR='245' ColorG='245' ColorB='245' Transparency='0.0' ShadowSize='0.5' BkgEnable='1' BkgExtraWidth='0.02' BkgExtraHeight='0.02'/>

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<TitlerData></TitlerData>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode paragraph = xml.CreateElement("Data");

            XmlAttribute charSize = xml.CreateAttribute("CharSize");
            charSize.InnerText = HtmlUtil.RemoveHtmlTags("0.2");
            paragraph.Attributes.Append(charSize);

            XmlAttribute posX = xml.CreateAttribute("PosX");
            posX.InnerText = "0.5";
            paragraph.Attributes.Append(posX);

            XmlAttribute posY = xml.CreateAttribute("PosY");
            posY.InnerText = "0.75";
            paragraph.Attributes.Append(posY);

            XmlAttribute colorR = xml.CreateAttribute("ColorR");
            colorR.InnerText = "245";
            paragraph.Attributes.Append(colorR);

            XmlAttribute colorG = xml.CreateAttribute("ColorG");
            colorG.InnerText = "245";
            paragraph.Attributes.Append(colorG);

            XmlAttribute colorB = xml.CreateAttribute("ColorB");
            colorB.InnerText = "245";
            paragraph.Attributes.Append(colorB);

            XmlAttribute transparency = xml.CreateAttribute("Transparency");
            transparency.InnerText = "0.0";
            paragraph.Attributes.Append(transparency);

            XmlAttribute shadowSize = xml.CreateAttribute("ShadowSize");
            shadowSize.InnerText = "0.5";
            paragraph.Attributes.Append(shadowSize);

            XmlAttribute bkgEnable = xml.CreateAttribute("BkgEnable");
            bkgEnable.InnerText = "1";
            paragraph.Attributes.Append(bkgEnable);

            XmlAttribute bkgExtraWidth = xml.CreateAttribute("BkgExtraWidth");
            bkgExtraWidth.InnerText = "0.02";
            paragraph.Attributes.Append(bkgExtraWidth);

            XmlAttribute bkgExtraHeight = xml.CreateAttribute("BkgExtraHeight");
            bkgExtraHeight.InnerText = "0.02";
            paragraph.Attributes.Append(bkgExtraHeight);

            xml.DocumentElement.AppendChild(paragraph);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                paragraph = xml.CreateElement("Data");

                XmlAttribute start = xml.CreateAttribute("StartTimecode");
                start.InnerText = ToTimeCode(p.StartTime);
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("EndTimecode");
                end.InnerText = ToTimeCode(p.EndTime);
                paragraph.Attributes.Append(end);

                XmlAttribute text = xml.CreateAttribute("Title");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text);
                paragraph.Attributes.Append(text);

                xml.DocumentElement.AppendChild(paragraph);
            }

            string s = "<?xml version=\"1.0\"?>" + Environment.NewLine + ToUtf8XmlString(xml, true).Replace("\"", "__@____").Replace("'", "&apos;").Replace("__@____", "'").Replace(" />", "/>");
            while (s.Contains(Environment.NewLine + " "))
            {
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            }

            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            string allText = sb.ToString();
            if (!allText.Contains("<TitlerData") || !allText.Contains("<Data"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(allText);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount = 1;
                return;
            }

            if (xml.DocumentElement == null)
            {
                _errorCount = 1;
                return;
            }

            char[] splitChars = { ':', ';' };
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Data"))
            {
                try
                {
                    if (node.Attributes != null)
                    {
                        string text = node.Attributes.GetNamedItem("Title").InnerText.Trim();
                        string start = node.Attributes.GetNamedItem("StartTimecode").InnerText;
                        string end = node.Attributes.GetNamedItem("EndTimecode").InnerText;
                        subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCodeFrames(start, splitChars), DecodeTimeCodeFrames(end, splitChars), text));
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
