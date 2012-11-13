using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class TmpegEncAW5 : TmpegEncXml
    {

        public override string Name
        {
            get { return "TMPGEnc AW5"; }
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
                paragraph.InnerXml = "<Text><![CDATA[" + paragraph.InnerXml.Replace(Environment.NewLine, "\\n") + "\\n]]></Text>";

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

            return ToUtf8XmlString(xml);
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if ((xmlAsString.Contains("<TMPGEncVMESubtitleTextFormat>") || xmlAsString.Contains("<SubtitleItem ")) && (xmlAsString.Contains("<Subtitle")))
            {

                var subtitle = new Subtitle();
                LoadSubtitle(subtitle, lines, fileName);
                return subtitle.Paragraphs.Count > _errorCount;
            }
            return false;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            LoadTMpeg(subtitle, lines, true);
        }

    }
}
