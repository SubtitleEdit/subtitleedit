using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

    //<TMPGEncVMESubtitleTextFormat>
    //  ...
    //  <Subtitle>
    //    <SubtitleItem layoutindex="0" enable="1" starttime="00:01:57,269" endtime="00:01:59,169">
    //        <Text>
    //            <![CDATA[These hills here are full of Apaches.]]>
    //        </Text>
    //    </SubtitleItem>
    //    ...

    class TmpegEncXml : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xsubtitle"; }
        }

        public override string Name
        {
            get { return "TMPGEnc VME"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if ((xmlAsString.Contains("<TMPGEncVMESubtitleTextFormat>") || xmlAsString.Contains("<SubtitleItem ")) && (xmlAsString.Contains("<Subtitle")))
            {
                var xml = new XmlDocument();
                try
                {
                    xml.LoadXml(xmlAsString);
                    var paragraphs = xml.DocumentElement.SelectNodes("Subtitle/SubtitleItem");
                    return paragraphs != null && paragraphs.Count > 0 && xml.DocumentElement.Name == "TMPGEncVMESubtitleTextFormat";
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
            string xmlStructure = @"<?xml version='1.0' encoding='utf-8' ?>
<TMPGEncVMESubtitleTextFormat>
    <Layout>
        <LayoutItem index='0'>
            <Name>
                <![CDATA[Picture bottom layout]]>
            </Name>
            <Position>4</Position>
            <FontName>
                <![CDATA[Tahoma]]>
            </FontName>
            <FontHeight>0.069</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <DirectionVertical>0</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.00345</BorderSize>
            <BorderColor>0</BorderColor>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='1'>
            <Name>
                <![CDATA[Picture top layout]]>
            </Name>
            <Position>4</Position>
            <FontName>
                <![CDATA[Tahoma]]>
            </FontName>
            <FontHeight>0.1</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>0</VerticalAlign>
            <DirectionVertical>0</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.005</BorderSize>
            <BorderColor>0</BorderColor>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='2'>
            <Name>
                <![CDATA[Picture left layout]]>
            </Name>
            <Position>4</Position>
            <FontName>
                <![CDATA[Tahoma]]>
            </FontName>
            <FontHeight>0.1</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>0</HorizonAlign>
            <VerticalAlign>1</VerticalAlign>
            <DirectionVertical>1</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.005</BorderSize>
            <BorderColor>0</BorderColor>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='3'>
            <Name>
                <![CDATA[Picture right layout]]>
            </Name>
            <Position>4</Position>
            <FontName>
                <![CDATA[Tahoma]]>
            </FontName>
            <FontHeight>0.1</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>2</HorizonAlign>
            <VerticalAlign>1</VerticalAlign>
            <DirectionVertical>1</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.005</BorderSize>
            <BorderColor>0</BorderColor>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>1</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='4'>
            <Name>
                <![CDATA[Picture bottom layout]]>
            </Name>
            <Position>4</Position>
            <FontName>
                <![CDATA[Tahoma]]>
            </FontName>
            <FontHeight>0.069</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>1</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <DirectionVertical>0</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.00345</BorderSize>
            <BorderColor>0</BorderColor>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
    </Layout>
    <Subtitle>
    </Subtitle>
</TMPGEncVMESubtitleTextFormat>".Replace("'", "\"");

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement.SelectSingleNode("Subtitle");
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("SubtitleItem");

                string text = Utilities.RemoveHtmlTags(p.Text);
                paragraph.InnerText = text;
                paragraph.InnerXml = "<Text><![CDATA[" + paragraph.InnerXml.Replace(Environment.NewLine, "\\n") + "]]></Text>";

                XmlAttribute layoutIndex = xml.CreateAttribute("layoutindex");
                if (p.Text.Trim().StartsWith("<i>") && p.Text.Trim().EndsWith("</i>"))
                    layoutIndex.InnerText = "4";
                else
                    layoutIndex.InnerText = "0";

                paragraph.Attributes.Append(layoutIndex);

                XmlAttribute enable = xml.CreateAttribute("enable");
                enable.InnerText = "1";
                paragraph.Attributes.Append(enable);

                XmlAttribute start = xml.CreateAttribute("starttime");
                start.InnerText = p.StartTime.ToString();
                paragraph.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("endtime");
                end.InnerText = p.EndTime.ToString();
                paragraph.Attributes.Append(end);

                div.AppendChild(paragraph);
                no++;
            }

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            double startSeconds = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument();
            xml.LoadXml(sb.ToString());
            var italicStyles = new List<bool>();

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Layout/LayoutItem"))
            {
                XmlNode fontItalic = node.SelectSingleNode("FontItalic");
                if (fontItalic != null && fontItalic.InnerText == "1")
                    italicStyles.Add(true);
                else
                    italicStyles.Add(false);
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Subtitle/SubtitleItem"))
            {
                try
                {
                    var pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.SelectSingleNode("Text").ChildNodes)
                    {
                        switch (innerNode.Name.ToString())
                        {
                            case "br":
                                pText.AppendLine();
                                break;
                            default:
                                pText.Append(innerNode.InnerText.Trim());
                                break;
                        }
                    }

                    string start = string.Empty;
                    if (node.Attributes["starttime"] != null)
                    {
                        start = node.Attributes["starttime"].InnerText;
                    }

                    string end = string.Empty;
                    if (node.Attributes["endtime"] != null)
                    {
                        end = node.Attributes["endtime"].InnerText;
                    }

                    var startCode = new TimeCode(TimeSpan.FromSeconds(startSeconds));
                    if (start != string.Empty)
                    {
                        startCode = GetTimeCode(start);
                    }

                    TimeCode endCode;
                    if (end != string.Empty)
                    {
                        endCode = GetTimeCode(end);
                    }
                    else
                    {
                        endCode = new TimeCode(TimeSpan.FromMilliseconds(startCode.TotalMilliseconds + 3000));
                    }
                    startSeconds = endCode.TotalSeconds;
                    var p = new Paragraph(startCode, endCode, pText.ToString().Trim().Replace("<Text>", string.Empty).Replace("</Text>", string.Empty).Replace("\\n", Environment.NewLine));
                    if (node.Attributes["layoutindex"] != null)
                    {
                        int idx;
                        if (int.TryParse(node.Attributes["layoutindex"].InnerText, out idx))
                        {
                            if (idx >= 0 && idx < italicStyles.Count && italicStyles[idx])
                                p.Text = "<i>" + p.Text + "</i>";
                        }
                    }
                    subtitle.Paragraphs.Add(p);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCode(string s)
        {
            if (s.EndsWith("s"))
            {
                s = s.TrimEnd('s');
                var ts = TimeSpan.FromSeconds(double.Parse(s));
                return new TimeCode(ts);
            }
            else
            {
                string[] parts = s.Split(new char[] { ':', '.', ',' });
                var ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                return new TimeCode(ts);
            }
        }
    }
}


