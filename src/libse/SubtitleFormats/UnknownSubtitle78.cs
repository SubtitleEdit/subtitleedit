using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    internal class UnknownSubtitle78 : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Unknown 78";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string xmpTemplate = @"<?xml version='1.0' encoding='utf-8'?>
<Document version='1.0'>
  <FileInfo>
    <FileID>CCTV Subtitle Sequence File</FileID>
    <FileVersion>1.0</FileVersion>
    <CodePage>1</CodePage>
    <Program>CG1</Program>
    <Author>Jetsen</Author>
    <Description>CCTV Subtitle Sequence Generate by Jetsen</Description>
    <CreationDate>[YYYY-MM-DD]</CreationDate>
    <RevisionDate>[YYYY-MM-DD]</RevisionDate>
    <RevisionNumber>1</RevisionNumber>
    <Language>
      <Primary>0</Primary>
      <Secondary>0</Secondary>
    </Language>
    <VideoStandard>HD_1080_25i</VideoStandard>
    <SectionCount>1</SectionCount>
  </FileInfo>
  <TextSection>
    <SectionInfo>
      <ScreenCount>1008</ScreenCount>
      <BlockCount>2</BlockCount>
      <TimeCodeMode>2</TimeCodeMode>
      <StartTimeCode>[TIME_CODE_FIRST]</StartTimeCode>
      <EndTimeCode>[TIME_CODE_LAST]</EndTimeCode>
      <TrimCodeIn>0</TrimCodeIn>
      <TrimCodeOut>66373</TrimCodeOut>
        <DisplayParameters>
        <BlockParameters version='1.0'>
          <Language></Language>
          <UnicodeBitField></UnicodeBitField>
          <Position X='125' Y='908' Width='390' Height='60'/>
          <Bound X='125' Y='908' Width='390' Height='60'/>
          <Font Name='黑体' Width='102' Height='62' Bold='0' Italic='0' Underline='0'/>
          <FontLatin Name='Arial' Width='102' Height='62' Bold='0' Italic='0' Underline='0'/>
          <LineAlign Align='0'/>
          <Layout CharSpace='1' LineSpace='2' Alignment='0' Direction='0'/>
          <TextColor R='229' G='229' B='229' A='255'/>
          <Side Width='3' Direction='8'/>
          <SideColor R='16' G='16' B='16' A='255'/>
          <Edge Angle='1' Width='0'/>
          <EdgeColor R='235' G='235' B='235' A='255'/>
          <Shadow OffsetX='0' OffsetY='0' Blur='0'/>
          <ShadowColor R='235' G='235' B='235' A='255'/>
          <Background Mode='0'/>
          <BackgroundColor R='0' G='0' B='0' A='0'/>
        </BlockParameters>
        <BlockParameters version='1.0'>
          <Language></Language>
          <UnicodeBitField></UnicodeBitField>
          <Position X='130' Y='974' Width='266' Height='47'/>
          <Bound X='130' Y='974' Width='266' Height='47'/>
          <Font Name='黑体' Width='54' Height='38' Bold='0' Italic='0' Underline='0'/>
          <FontLatin Name='Arial' Width='54' Height='38' Bold='0' Italic='0' Underline='0'/>
          <LineAlign Align='0'/>
          <Layout CharSpace='2' LineSpace='2' Alignment='0' Direction='0'/>
          <TextColor R='235' G='235' B='235' A='255'/>
          <Side Width='3' Direction='8'/>
          <SideColor R='16' G='16' B='16' A='255'/>
          <Edge Angle='0' Width='0'/>
          <EdgeColor R='235' G='235' B='235' A='255'/>
          <Shadow OffsetX='0' OffsetY='0' Blur='0'/>
          <ShadowColor R='235' G='235' B='235' A='255'/>
          <Background Mode='0'/>
          <BackgroundColor R='0' G='0' B='0' A='0'/>
        </BlockParameters>
      </DisplayParameters>
      <Private>
        <FreeCG>IsContinuousClip=FALSE</FreeCG>
      </Private>
      <ActionIn>
        <TCIn>0</TCIn>
        <TCOut>0</TCOut>
        <Type>0</Type>
      </ActionIn>
      <ActionStay>
        <TCIn>0</TCIn>
        <TCOut>52</TCOut>
        <Type>0</Type>
      </ActionStay>
      <ActionOut>
        <TCIn>52</TCIn>
        <TCOut>53</TCOut>
        <Type>0</Type>
      </ActionOut>
    </SectionInfo>
  </TextSection>
</Document>";

            const string paragraphTemplate = @"
      <TimeCodeIn>00:00:15:09</TimeCodeIn>
      <TimeCodeOut>00:00:16:14</TimeCodeOut>
      <TextBlock>
        <String></String>
      </TextBlock>
      <TextBlock>
        <String></String>
      </TextBlock>
      <ActionIn>
        <TCIn>0</TCIn>
        <TCOut>0</TCOut>
        <Type>0</Type>
      </ActionIn>
      <ActionStay>
        <TCIn>0</TCIn>
        <TCOut>29</TCOut>
        <Type>0</Type>
      </ActionStay>
      <ActionOut>
        <TCIn>29</TCIn>
        <TCOut>30</TCOut>
        <Type>0</Type>
      </ActionOut>";
            var xml = new XmlDocument();
            var firstTimeCode = new TimeCode();
            var lastTimeCode = new TimeCode();
            if (subtitle.Paragraphs.Count > 0)
            {
                firstTimeCode = subtitle.Paragraphs[0].StartTime;
                lastTimeCode = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].StartTime;
            }
            string today = DateTime.Now.ToString("YYYY-mm-DD");
            xml.LoadXml(xmpTemplate.Replace('\'', '"').Replace("[YYYY-MM-DD]", today).Replace("[TIME_CODE_FIRST]", firstTimeCode.ToHHMMSSFF()).Replace("[TIME_CODE_LAST]", lastTimeCode.ToHHMMSSFF()));

            var paragraphInsertNode = xml.DocumentElement.SelectSingleNode("TextSection");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode paragraph = xml.CreateElement("TextScreen");
                paragraph.InnerXml = paragraphTemplate;
                paragraph.SelectSingleNode("TimeCodeIn").InnerText = p.StartTime.ToHHMMSSFF();
                paragraph.SelectSingleNode("TimeCodeOut").InnerText = p.EndTime.ToHHMMSSFF();
                var textBlockNodes = paragraph.SelectNodes("TextBlock");
                textBlockNodes[0].SelectSingleNode("String").InnerText = p.Text;
                paragraphInsertNode.AppendChild(paragraph);
            }
            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsText = sb.ToString().Trim();
            if (!xmlAsText.Contains("<TextSection>") || !xmlAsText.Contains("<TextScreen>"))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("TextSection/TextScreen"))
                {
                    try
                    {
                        var timeCodeIn = DecodeTimeCodeFrames(node.SelectSingleNode("TimeCodeIn").InnerText, SplitCharColon);
                        var timeCodeOut = DecodeTimeCodeFrames(node.SelectSingleNode("TimeCodeOut").InnerText, SplitCharColon);
                        sb.Clear();
                        foreach (XmlNode textBlockNode in node.SelectNodes("TextBlock"))
                        {
                            sb.AppendLine(textBlockNode.InnerText);
                        }
                        var p = new Paragraph(timeCodeIn, timeCodeOut, sb.ToString().Trim());
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
