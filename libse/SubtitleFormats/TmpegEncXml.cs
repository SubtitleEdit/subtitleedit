using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
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

    public class TmpegEncXml : SubtitleFormat
    {
        public override string Extension => ".xsubtitle";

        public override string Name => "TMPGEnc VME";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if ((xmlAsString.Contains("<TMPGEncVMESubtitleTextFormat>") || xmlAsString.Contains("<SubtitleItem ")) && (xmlAsString.Contains("<Subtitle")))
            {
                return base.IsMine(lines, fileName);
            }
            return false;
        }

        internal static string Layout = @"<?xml version='1.0' encoding='UTF-8'?>
<TMPGEncVMESubtitleTextFormat>
    <Layout>
        <LayoutItem index='0'>
            <Name>
                <![CDATA[Picture bottom layout]]>
            </Name>
            <Position>[Position]</Position>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <DirectionVertical>0</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.005</BorderSize>
            <BorderColor>0</BorderColor>
            <BorderOpacity>1</BorderOpacity>
            <BackgroundActive>0</BackgroundActive>
            <BackgroundSize>0.005</BackgroundSize>
            <BackgroundColor>0</BackgroundColor>
            <BackgroundOpacity>1</BackgroundOpacity>
            <FadeInActive>0</FadeInActive>
            <FadeInTime>1000</FadeInTime>
            <FadeOutActive>0</FadeOutActive>
            <FadeOutTime>1000</FadeOutTime>
            <ScrollDirectionIndex>0</ScrollDirectionIndex>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='1'>
            <Name>
                <![CDATA[Picture top layout]]>
            </Name>
            <Position>[Position]</Position>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
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
            <BorderOpacity>1</BorderOpacity>
            <BackgroundActive>0</BackgroundActive>
            <BackgroundSize>0.005</BackgroundSize>
            <BackgroundColor>0</BackgroundColor>
            <BackgroundOpacity>1</BackgroundOpacity>
            <FadeInActive>0</FadeInActive>
            <FadeInTime>1000</FadeInTime>
            <FadeOutActive>0</FadeOutActive>
            <FadeOutTime>1000</FadeOutTime>
            <ScrollDirectionIndex>0</ScrollDirectionIndex>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='2'>
            <Name>
                <![CDATA[Picture left layout]]>
            </Name>
            <Position>[Position]</Position>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
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
            <BorderOpacity>1</BorderOpacity>
            <BackgroundActive>0</BackgroundActive>
            <BackgroundSize>0.005</BackgroundSize>
            <BackgroundColor>0</BackgroundColor>
            <BackgroundOpacity>1</BackgroundOpacity>
            <FadeInActive>0</FadeInActive>
            <FadeInTime>1000</FadeInTime>
            <FadeOutActive>0</FadeOutActive>
            <FadeOutTime>1000</FadeOutTime>
            <ScrollDirectionIndex>0</ScrollDirectionIndex>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>1</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='3'>
            <Name>
                <![CDATA[Picture right layout]]>
            </Name>
            <Position>[Position]</Position>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
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
            <BorderOpacity>1</BorderOpacity>
            <BackgroundActive>0</BackgroundActive>
            <BackgroundSize>0.005</BackgroundSize>
            <BackgroundColor>0</BackgroundColor>
            <BackgroundOpacity>1</BackgroundOpacity>
            <FadeInActive>0</FadeInActive>
            <FadeInTime>1000</FadeInTime>
            <FadeOutActive>0</FadeOutActive>
            <FadeOutTime>1000</FadeOutTime>
            <ScrollDirectionIndex>0</ScrollDirectionIndex>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>1</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='4'>
            <Name>
                <![CDATA[Picture bottom layout]]>
            </Name>
            <Position>[Position]</Position>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>0</FontBold>
            <FontItalic>1</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <DirectionVertical>0</DirectionVertical>
            <BorderActive>1</BorderActive>
            <BorderSize>0.005</BorderSize>
            <BorderColor>0</BorderColor>
            <BorderOpacity>1</BorderOpacity>
            <BackgroundActive>0</BackgroundActive>
            <BackgroundSize>0.005</BackgroundSize>
            <BackgroundColor>0</BackgroundColor>
            <BackgroundOpacity>1</BackgroundOpacity>
            <FadeInActive>0</FadeInActive>
            <FadeInTime>1000</FadeInTime>
            <FadeOutActive>0</FadeOutActive>
            <FadeOutTime>1000</FadeOutTime>
            <ScrollDirectionIndex>0</ScrollDirectionIndex>
            <TextAlign>1</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
    </Layout>
    <Subtitle>
        @
    </Subtitle>
</TMPGEncVMESubtitleTextFormat>"
            .Replace("[FontName]", Configuration.Settings.SubtitleSettings.TmpegEncXmlFontName)
            .Replace("[FontHeight]", Configuration.Settings.SubtitleSettings.TmpegEncXmlFontHeight)
            .Replace("[Position]", Configuration.Settings.SubtitleSettings.TmpegEncXmlPosition);


        public override string ToText(Subtitle subtitle, string title)
        {
            var xmlStructure = Layout.Replace('\'', '"');

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            XmlNode div = xml.DocumentElement.SelectSingleNode("Subtitle");
            div.InnerXml = string.Empty;
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("SubtitleItem");

                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                paragraph.InnerText = text;
                paragraph.InnerXml = "<Text><![CDATA[" + paragraph.InnerXml.Replace(Environment.NewLine, "\\n") + "]]></Text>";

                XmlAttribute layoutIndex = xml.CreateAttribute("layoutindex");
                if (p.Text.TrimStart().StartsWith("<i>", StringComparison.OrdinalIgnoreCase) && p.Text.TrimEnd().EndsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    layoutIndex.InnerText = "4";
                }
                else
                {
                    layoutIndex.InnerText = "0";
                }

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

            string s = ToUtf8XmlString(xml);
            var startPos = s.IndexOf("<Subtitle>", StringComparison.Ordinal) + 10;
            s = s.Substring(startPos, s.IndexOf("</Subtitle>", StringComparison.Ordinal) - startPos).Trim();
            return Layout.Replace("@", s);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            LoadTMpeg(subtitle, lines, false);
        }

        internal void LoadTMpeg(Subtitle subtitle, List<string> lines, bool mustHaveLineBreakAsEnd)
        {
            _errorCount = 0;
            double startSeconds = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());
            var italicStyles = new List<bool>();

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Layout/LayoutItem"))
            {
                XmlNode fontItalic = node.SelectSingleNode("FontItalic");
                if (fontItalic != null && fontItalic.InnerText == "1")
                {
                    italicStyles.Add(true);
                }
                else
                {
                    italicStyles.Add(false);
                }
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Subtitle/SubtitleItem"))
            {
                try
                {
                    var pText = new StringBuilder();
                    foreach (XmlNode innerNode in node.SelectSingleNode("Text").ChildNodes)
                    {
                        if (innerNode.Name == "br")
                        {
                            pText.AppendLine();
                        }
                        else
                        {
                            pText.Append(innerNode.InnerText.Trim());
                        }
                    }

                    var start = string.Empty;
                    if (node.Attributes["starttime"] != null)
                    {
                        start = node.Attributes["starttime"].InnerText;
                    }

                    var end = string.Empty;
                    if (node.Attributes["endtime"] != null)
                    {
                        end = node.Attributes["endtime"].InnerText;
                    }

                    var startCode = TimeCode.FromSeconds(startSeconds);
                    if (start.Length > 0)
                    {
                        startCode = GetTimeCode(start);
                    }

                    TimeCode endCode;
                    if (end.Length > 0)
                    {
                        endCode = GetTimeCode(end);
                    }
                    else
                    {
                        endCode = new TimeCode(startCode.TotalMilliseconds + 3000);
                    }
                    startSeconds = endCode.TotalSeconds;
                    if (mustHaveLineBreakAsEnd)
                    {
                        if (!pText.ToString().EndsWith("\\n", StringComparison.Ordinal))
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        if (pText.ToString().EndsWith("\\n", StringComparison.Ordinal))
                        {
                            _errorCount++;
                        }
                    }

                    var p = new Paragraph(startCode, endCode, pText.ToString().Trim().Replace("<Text>", string.Empty).Replace("</Text>", string.Empty).Replace("\\n", Environment.NewLine).TrimEnd());
                    if (node.Attributes["layoutindex"] != null)
                    {
                        if (int.TryParse(node.Attributes["layoutindex"].InnerText, out var idx))
                        {
                            if (idx >= 0 && idx < italicStyles.Count && italicStyles[idx])
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }
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
            subtitle.Renumber();
        }

        private static TimeCode GetTimeCode(string s)
        {
            if (s.EndsWith('s'))
            {
                s = s.TrimEnd('s');
                return TimeCode.FromSeconds(double.Parse(s));
            }
            string[] parts = s.Split(':', '.', ',');
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }
    }
}
