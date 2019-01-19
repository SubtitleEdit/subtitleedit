using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Xif : SubtitleFormat
    {
        public override string Extension => ".xif";

        public override string Name => "XIF";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string xmpTemplate = @"<XIF version='1.0' filename='file:///filename.xif'>
  <FileHeader>
    <OriginatorSoftware Name='FileConverter' Version='1.01'/>
    <GlobalFileInfo ProgrammeName='NAME' Format='Open' Language='English' StartTime='00:00:00:00' StopTime='00:00:00:00' TotalLength='00:00:00:00' ProgrammeTitle='TITLE' NumberOfCaptions='405' RevisionNumber='0' CountryIndex='0' CharacterCodeTable='0' MaxColumns='0' EpisodeNumber='0' BlinkOption='yes' CaptionService='0' CurrentVideo='2' Videosize='0' BaseOffset='0' TopOffset='0' DefaultSave='0' DisplaySafwWidth='576' SafeAreaWidthPercent='80' DisplaySafeHoriz='0' SafeAreaHorizPos='0' CurrentStyle='None' StyleDate='None' StyleTime='None' SAOLeft='72' SAORight='0' VideoStartTC='00:00:00:00' SpellLanguage='English' CGConfig='0'/>
    <ReadingSpeeds>
      <ReadingSpeed ID='0' WordsPerMinute='180' CharsPerMinute='720' CharsPerSecond='12' PTS='WPM'/>
    </ReadingSpeeds>
    <Canvases>
      <Canvas ID='0' Width='720' Height='576' SAWidth='576' SAHeight='460' SATop='57' SALeft='72'/>
    </Canvases>
    <FormattingInfo TabWidth='3'/>
    <TimingStandards>
      <TimingStandard ID='0' Type='SMPTE25'/>
    </TimingStandards>
    <Fonts>
      <Font ID='0' Type='True Type' Typeface='Courier New' Height='0' AstonFountSize='185' Ascent='268' Descent='97' IntLead='43'/>
      <Font ID='1' Type='True Type' Typeface='Courier New' Height='0' AstonFountSize='185' Ascent='268' Descent='97' IntLead='43'/>
      <Font ID='2' Type='True Type' Typeface='Hairy Helix' Height='0' AstonFountSize='12' Ascent='15' Descent='3' IntLead='2'/>
      <Font ID='3' Type='True Type' Typeface='Hairy Helix Italics' Height='0' AstonFountSize='12' Ascent='15' Descent='3' IntLead='2'/>
      <Font ID='4' Type='True Type' Typeface='Arial' Height='35' AstonFountSize='22' Ascent='28' Descent='7' IntLead='5'/>
      <Font ID='5' Type='True Type' Typeface='Arial Narrow' Height='35' AstonFountSize='25' Ascent='30' Descent='5' IntLead='0'/>
      <Font/>
    </Fonts>
    <Colours>
      <Colour ID='0' value='0x000000'/>
      <Colour ID='1' value='0xFF0000'/>
      <Colour ID='2' value='0x00FF00'/>
      <Colour ID='3' value='0xFFFF00'/>
      <Colour ID='4' value='0x0000FF'/>
      <Colour ID='5' value='0xFF00FF'/>
      <Colour ID='6' value='0x00FFFF'/>
      <Colour ID='7' value='0xFFFFFF'/>
    </Colours>
    <Threads>
      <Thread ID='0' CanvasID='0' TimingStandardID='0' ReadingSpeedID='0' Medium='Open' Name='Default'/>
    </Threads>
    <WordLists/>
    <TxStreams>
      <TxStream ID='0' Active='yes'/>
    </TxStreams>
  </FileHeader>
  <FileBody>
    <ProgrammeBlock NoOfAssociatedIDs='0'>
      <SceneBaseBlock NewsID='0' Position='0' NewsRoomInternalID='0'/>
    </ProgrammeBlock>
    <StoryBlock TimingType='Manual'>
      <SceneBaseBlock NewsID='0' Position='0' NewsRoomInternalID='0'/>
    </StoryBlock>

  </FileBody>
</XIF>";

            const string paragraphTemplate = @"
      <ContentBlock TXStreamID='0' uid='B4EB0AE5-B82F-48ea-9FD4-C4194D004041'>
      <ThreadedObject ID='0' Type='BasicSubtitle' VPos='448'>
        <Note/>
        <TimingObject>
          <TimeIn value='10:00:03:06'/>
          <TimeOut value='10:00:05:09'/>
        </TimingObject>
        <Content Override='0' RaisedRowUp='0' RaisedRowDown='0' DirectionState='0' PositionState='1' ConfidenceValue='0'>
          <SubtitleText>
            <Paragraph Type='Open' Justification='Centre' LineLimit='15' CharactersPerRow='99' Alignment='None' Language='ENG' VideoLanguage='ENG' Xpercent='0.0' Ypercent='0.0'>
              <StartState Foreground='7' Background='0' Flash='No' Underline='No' Italic='No' Bold='No' Indent='0' SBOnOff='Yes' FontID='4'/>
              <Boxing Colour='0' Transparency='0' Type='None'/>
              <AntiAliasing Style='Default'/>
            </Paragraph>
          </SubtitleText>
        </Content>
      </ThreadedObject>
    </ContentBlock>";

            var xml = new XmlDocument();
            var lastTimeCode = new TimeCode();
            if (subtitle.Paragraphs.Count > 0)
            {
                lastTimeCode = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].StartTime;
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Unknown";
            }

            xml.LoadXml(xmpTemplate.Replace('\'', '"'));
            var globalFileInfoNode = xml.DocumentElement.SelectSingleNode("FileHeader/GlobalFileInfo");
            globalFileInfoNode.Attributes["ProgrammeName"].InnerText = title;
            globalFileInfoNode.Attributes["ProgrammeTitle"].InnerText = title;
            globalFileInfoNode.Attributes["StopTime"].InnerText = lastTimeCode.ToHHMMSSFF();
            globalFileInfoNode.Attributes["NumberOfCaptions"].InnerText = subtitle.Paragraphs.Count.ToString(CultureInfo.InvariantCulture);

            var fileBodyNode = xml.DocumentElement.SelectSingleNode("FileBody");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode content = xml.CreateElement("ContentBlock");
                content.InnerXml = paragraphTemplate;
                content.SelectSingleNode("ContentBlock/ThreadedObject/TimingObject/TimeIn").InnerText = p.StartTime.ToHHMMSSFF();
                content.SelectSingleNode("ContentBlock/ThreadedObject/TimingObject/TimeOut").InnerText = p.EndTime.ToHHMMSSFF();

                var paragraphNode = content.SelectSingleNode("ContentBlock/ThreadedObject/Content/SubtitleText/Paragraph");
                var lines = HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines();
                for (int i = 1; i < lines.Count + 1; i++)
                {
                    var rowNode = xml.CreateElement("Row");

                    var attrNumber = xml.CreateAttribute("Number");
                    attrNumber.InnerText = i.ToString(CultureInfo.InvariantCulture);
                    rowNode.Attributes.Append(attrNumber);

                    var attrJust = xml.CreateAttribute("JustificationOverride");
                    attrJust.InnerText = "Centre";
                    rowNode.Attributes.Append(attrJust);

                    var attrHighlight = xml.CreateAttribute("Highlight");
                    attrHighlight.InnerText = "0";
                    rowNode.Attributes.Append(attrHighlight);

                    paragraphNode.AppendChild(rowNode);
                }
                for (int index = 0; index < lines.Count; index++)
                {
                    var line = lines[index];

                    if (index > 0)
                    {
                        var hardReturnNode = xml.CreateElement("HardReturn");
                        paragraphNode.AppendChild(hardReturnNode);
                    }

                    var foregroundColorNode = xml.CreateElement("ForegroundColour");
                    var attrColor = xml.CreateAttribute("Colour");
                    attrColor.InnerText = "7";
                    foregroundColorNode.Attributes.Append(attrColor);
                    paragraphNode.AppendChild(foregroundColorNode);

                    var textNode = xml.CreateElement("Text");
                    textNode.InnerText = line;
                    paragraphNode.AppendChild(textNode);
                }
                fileBodyNode.AppendChild(content.SelectSingleNode("ContentBlock"));
            }
            return ToUtf8XmlString(xml, true).Replace(" xmlns=\"\"", string.Empty);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsText = sb.ToString().Trim();
            if (!xmlAsText.Contains("<XIF") || !xmlAsText.Contains("<SubtitleText"))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("FileBody/ContentBlock"))
                {
                    try
                    {
                        var timeCodeIn = DecodeTimeCodeFrames(node.SelectSingleNode("ThreadedObject/TimingObject/TimeIn").Attributes["value"].InnerText, SplitCharColon);
                        var timeCodeOut = DecodeTimeCodeFrames(node.SelectSingleNode("ThreadedObject/TimingObject/TimeOut").Attributes["value"].InnerText, SplitCharColon);
                        sb.Clear();
                        foreach (XmlNode paragraphNode in node.SelectSingleNode("ThreadedObject/Content/SubtitleText/Paragraph").ChildNodes)
                        {
                            if (paragraphNode.Name == "Text")
                            {
                                sb.Append(" " + paragraphNode.InnerText);
                            }
                            else if (paragraphNode.Name == "HardReturn")
                            {
                                sb.AppendLine();
                            }
                        }
                        var p = new Paragraph(timeCodeIn, timeCodeOut, sb.ToString().Replace("  ", " ").Trim());
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
