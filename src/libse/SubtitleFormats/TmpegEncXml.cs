using Nikse.SubtitleEdit.Core.Common;
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
        public static string FontName { get; set; } = "Tahoma";
        public static string FontBold { get; set; } = "0";
        public static string FontHeight { get; set; } = "0.067";
        public static string OffsetX { get; set; } = "0.001";
        public static string OffsetY { get; set; } = "0.001";


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

        internal static string GetLayout() =>
            @"<?xml version='1.0' encoding='UTF-8'?>
<TMPGEncVMESubtitleTextFormat>
    <Layout>
        <LayoutItem index='0'>
            <Name>
                <![CDATA[Picture Top Left layout]]>
            </Name>
            <Position>0</Position>
            <HorizonAlign>0</HorizonAlign>  
            <VerticalAlign>0</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            <TextAlign>0</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='1'>
            <Name>
                <![CDATA[Picture Top Center layout]]>
            </Name>
            <Position>1</Position>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>0</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
                <![CDATA[Picture Top Right layout]]>
            </Name>
            <Position>2</Position>
            <HorizonAlign>2</HorizonAlign>
            <VerticalAlign>0</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            <TextAlign>2</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='3'>
            <Name>
                <![CDATA[Picture Middle Left layout]]>
            </Name>
            <Position>3</Position>
            <HorizonAlign>0</HorizonAlign>
            <VerticalAlign>1</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            <TextAlign>0</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='4'>
            <Name>
                <![CDATA[Picture Middle Center layout]]>
            </Name>
            <Position>4</Position>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>1</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
        <LayoutItem index='5'>
            <Name>
                <![CDATA[Picture Middle Right layout]]>
            </Name>
            <Position>5</Position>
            <HorizonAlign>2</HorizonAlign>
            <VerticalAlign>1</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            <TextAlign>2</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='6'>
            <Name>
                <![CDATA[Picture Bottom Left layout]]>
            </Name>
            <Position>6</Position>
            <HorizonAlign>0</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            <TextAlign>0</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='7'>
            <Name>
                <![CDATA[Picture Bottom Center layout]]>
            </Name>
            <Position>7</Position>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
        <LayoutItem index='8'>
            <Name>
                <![CDATA[Picture Bottom Right layout]]>
            </Name>
            <Position>8</Position>
            <HorizonAlign>2</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>0</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            <TextAlign>2</TextAlign>
            <DirectionRightToLeft>0</DirectionRightToLeft>
        </LayoutItem>
        <LayoutItem index='9'>
            <Name>
                <![CDATA[Picture Bottom Center Italic layout]]>
            </Name>
            <Position>7</Position>
            <HorizonAlign>1</HorizonAlign>
            <VerticalAlign>2</VerticalAlign>
            <OffsetX>[OffsetX]</OffsetX>
            <OffsetY>[OffsetY]</OffsetY>
            <FontName>
                <![CDATA[[FontName]]]>
            </FontName>
            <FontHeight>[FontHeight]</FontHeight>
            <FontColor>17588159451135</FontColor>
            <FontBold>[FontBold]</FontBold>
            <FontItalic>1</FontItalic>
            <FontUnderline>0</FontUnderline>
            <FontStrikeOut>0</FontStrikeOut>
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
            .Replace("[FontName]", FontName)
            .Replace("[FontBold]", FontBold)
            .Replace("[FontHeight]", FontHeight)
            .Replace("[OffsetX]", OffsetX)
            .Replace("[OffsetY]", OffsetY)
            ;

        public override string ToText(Subtitle subtitle, string title)
        {
            var xmlStructure = GetLayout().Replace('\'', '"');

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
                var layoutIndexValue = GetLayoutIndexFromAssAlignment(p.Text);
                layoutIndex.InnerText = layoutIndexValue.ToString();

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
            return GetLayout().Replace("@", s);
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
            var positionCodes = new List<int>();

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

                XmlNode position = node.SelectSingleNode("Position");
                if (position != null && int.TryParse(position.InnerText, out var posCode))
                {
                    positionCodes.Add(posCode);
                }
                else
                {
                    positionCodes.Add(7); // Default to bottom center
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

                            // Add ASS alignment tag based on position code from layout
                            if (idx >= 0 && idx < positionCodes.Count)
                            {
                                var alignmentTag = GetAssAlignmentFromPositionCode(positionCodes[idx]);
                                if (!string.IsNullOrEmpty(alignmentTag))
                                {
                                    p.Text = alignmentTag + p.Text;
                                }
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

        protected static int GetLayoutIndexFromAssAlignment(string text)
        {
            // Check for ASS alignment tags (an1 to an9) - keypad layout
            // 789 (top left, center, right)
            // 456 (middle left, center, right)
            // 123 (bottom left, center, right)
            // Map to XML layoutindex: 0=Top Left, 1=Top Center, 2=Top Right,
            // 3=Middle Left, 4=Middle Center, 5=Middle Right,
            // 6=Bottom Left, 7=Bottom Center, 8=Bottom Right
            if (text.Contains("{\\an7}"))
            {
                return 0; // Top-left
            }
            if (text.Contains("{\\an8}"))
            {
                return 1; // Top-center
            }
            if (text.Contains("{\\an9}"))
            {
                return 2; // Top-right
            }
            if (text.Contains("{\\an4}"))
            {
                return 3; // Middle-left
            }
            if (text.Contains("{\\an5}"))
            {
                return 4; // Middle-center
            }
            if (text.Contains("{\\an6}"))
            {
                return 5; // Middle-right
            }
            if (text.Contains("{\\an1}"))
            {
                return 6; // Bottom-left
            }
            if (text.Contains("{\\an2}"))
            {
                return 7; // Bottom-center
            }
            if (text.Contains("{\\an3}"))
            {
                return 8; // Bottom-right
            }

            // Default to bottom-center (an2) if no alignment tag is found
            if (IsItalic(text))
            {
                return 9; // Bottom-center Italic layout
            }

            return 7;
        }

        private static bool IsItalic(string text)
        {
            var s = text.IndexOf("<i>", StringComparison.OrdinalIgnoreCase);
            return s < 20 && text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetAssAlignmentFromPositionCode(int positionCode)
        {
            // Map Position code from Layout to ASS alignment tag (keypad layout)
            // Position codes: 0=Top Left, 1=Top Center, 2=Top Right,
            // 3=Middle Left, 4=Middle Center, 5=Middle Right,
            // 6=Bottom Left, 7=Bottom Center, 8=Bottom Right
            switch (positionCode)
            {
                case 0:
                    return "{\\an7}"; // Top-left
                case 1:
                    return "{\\an8}"; // Top-center
                case 2:
                    return "{\\an9}"; // Top-right
                case 3:
                    return "{\\an4}"; // Middle-left
                case 4:
                    return "{\\an5}"; // Middle-center
                case 5:
                    return "{\\an6}"; // Middle-right
                case 6:
                    return "{\\an1}"; // Bottom-left
                case 7:
                    return string.Empty; // Bottom-center (default, don't add tag)
                case 8:
                    return "{\\an3}"; // Bottom-right
                default:
                    return string.Empty; // Default to bottom-center (no tag)
            }
        }
    }
}
