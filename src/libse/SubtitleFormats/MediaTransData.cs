using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    internal class MediaTransData : SubtitleFormat
    {
        public override string Extension => ".imtpro";

        public override string Name => "MediaTransData";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string xmpTemplate = @"<?xml version='1.0' encoding='UNICODE'?>
<MediaTransData Version='1'>
    <Settings Version='1'>
        <Type></Type>
        <Description Version='1'>
            <Count>2</Count>
            <Item1>For editing SDTV video</Item1>
            <Item2>Standard PAL video (4:3interlaced)</Item2>
        </Description>
        <General Version='1'>
            <EditingMode>SDTV</EditingMode>
            <Timebase>25.00 fps</Timebase>
            <DropFrame>True</DropFrame>
            <Horizontal>720</Horizontal>
            <Vertical>400</Vertical>
            <ScreenAspectRatio>720/400</ScreenAspectRatio>
            <PixelAspectRatio>D1/DV PAL (1.067)</PixelAspectRatio>
            <Editor>
                <LineSeparator>|</LineSeparator>
            </Editor>
        </General>
        <Render Version='1'>
            <Location>Default</Location>
            <Name>Default</Name>
            <Type>24Bit PNG</Type>
            <Index>0001</Index>
            <ExportPartial>False</ExportPartial>
            <ServerData Version='1'>
                <Track>0</Track>
                <Field>1</Field>
                <PAC>False</PAC>
                <Category>Default</Category>
                <Subtitle>FOX-HD-Malaysia</Subtitle>
                <Timecode>Default</Timecode>
                <NBJ>False</NBJ>
            </ServerData>
        </Render>
        <TimecodeBase Version='1'>
            <FramesPerSecTimesOneHundred>2500</FramesPerSecTimesOneHundred>
            <UseTimecodeBaseOfFile>FALSE</UseTimecodeBaseOfFile>
            <TimecodeBaseOfFile>00:00:00:00</TimecodeBaseOfFile>
            <RepeatOfTimecodeBaseOfFile>1</RepeatOfTimecodeBaseOfFile>
            <TimecodeBaseUserSupplied>09:59:35:00</TimecodeBaseUserSupplied>
            <RepeatOfTimecodeBaseUserSupplied>1</RepeatOfTimecodeBaseUserSupplied>
        </TimecodeBase>
    </Settings>
    <Movie Version='1'>
        <Name>C:\Project\uknown.mpg</Name>
        <Format>29.97DF</Format>
    </Movie>
    <Tracks Version='1'>
        <Count>1</Count>
        <Track1 Version='1'>
            <Owner></Owner>
            <Name>Track 1</Name>
            <Enabled>True</Enabled>
            <Locked>False</Locked>
            <Password></Password>
            <Columns Version='1'>
                <Count>7</Count>
                <Column1>
                    <Name>No</Name>
                    <Length>40</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column1>
                <Column2>
                    <Name>In</Name>
                    <Length>64</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column2>
                <Column3>
                    <Name>Out</Name>
                    <Length>66</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column3>
                <Column4>
                    <Name>Style</Name>
                    <Length>15</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column4>
                <Column5>
                    <Name>StyleEx</Name>
                    <Length>96</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column5>
                <Column6>
                    <Name>Comment</Name>
                    <Length>77</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column6>
                <Column7>
                    <Name>Language 01</Name>
                    <Length>349</Length>
                    <Locked>False</Locked>
                    <Password></Password>
                    <NonText>False</NonText>
                </Column7>
            </Columns>
            <Rows Version='1'>
                <Count>4096</Count>
            </Rows>
            <Data Version='1'>
                <Count>[COUNT]</Count>
            </Data>
            <Highlights Version='1'>
                <Count>0</Count>
            </Highlights>
        </Track1>
    </Tracks>
</MediaTransData>";

            const string paragraphTemplate = @"
<In>10:21:15:06</In>
<Out>10:21:16:18</Out>
<Style></Style>
<StyleEx></StyleEx>
<Comment></Comment>
<Fields Version=""2"">
    <Field1>
        <Type>Text</Type>
        <Data>Line1|Line2</Data>
    </Field1>
</Fields>";

            var xml = new XmlDocument();
            xml.LoadXml(xmpTemplate.Replace("[COUNT]", subtitle.Paragraphs.Count.ToString()));
            var paragraphInsertNode = xml.DocumentElement.SelectSingleNode("Tracks/Track1/Data");
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string nodeName = "Data" + count++;
                XmlNode paragraph = xml.CreateElement(nodeName);
                paragraph.InnerXml = paragraphTemplate;
                paragraph.SelectSingleNode("In").InnerText = p.StartTime.ToHHMMSSFF();
                paragraph.SelectSingleNode("Out").InnerText = p.EndTime.ToHHMMSSFF();
                paragraph.SelectSingleNode("Fields/Field1/Data").InnerText = string.Join("|", p.Text.SplitToLines());
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
            if (!xmlAsText.Contains("<Type>Text</Type>") || !xmlAsText.Contains("</MediaTransData>"))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(xmlAsText);
                foreach (XmlNode node in xml.DocumentElement.SelectNodes("//Fields/Field1"))
                {
                    try
                    {
                        var nodeType = node.SelectSingleNode("Type");
                        if (nodeType?.InnerText == "Text")
                        {
                            var timeCodeIn = DecodeTimeCodeFrames(node.ParentNode.ParentNode.SelectSingleNode("In").InnerText, SplitCharColon);
                            var timeCodeOut = DecodeTimeCodeFrames(node.ParentNode.ParentNode.SelectSingleNode("Out").InnerText, SplitCharColon);
                            var text = node.SelectSingleNode("Data").InnerText.Replace("|", Environment.NewLine);
                            var p = new Paragraph(timeCodeIn, timeCodeOut, text);
                            subtitle.Paragraphs.Add(p);
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
            catch (Exception)
            {
                _errorCount++;
            }
        }

    }
}
