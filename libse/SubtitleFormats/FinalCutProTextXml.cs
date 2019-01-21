using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    //  - Mom, when you were my age&#13;what did you want to do?
    public class FinalCutProTestXml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Final Cut Pro Test Xml";

        public static string GetFrameRateAsString()
        {
            if (Configuration.Settings.General.CurrentFrameRate < 24)
            {
                return "24"; // ntsc 23.976
            }

            if (Configuration.Settings.General.CurrentFrameRate < 25)
            {
                return "24";
            }

            if (Configuration.Settings.General.CurrentFrameRate < 29)
            {
                return "25";
            }

            if (Configuration.Settings.General.CurrentFrameRate < 30)
            {
                return "30"; // ntsc 29.97
            }

            if (Configuration.Settings.General.CurrentFrameRate < 40)
            {
                return "30";
            }

            if (Configuration.Settings.General.CurrentFrameRate < 60)
            {
                return "60"; // ntsc 59.94
            }

            return "60";
        }

        public static string GetNtsc()
        {
            if (Configuration.Settings.General.CurrentFrameRate < 24)
            {
                return "TRUE"; // ntsc 23.976
            }

            if (Configuration.Settings.General.CurrentFrameRate < 25)
            {
                return "FALSE";
            }

            return "TRUE";
            //if (Configuration.Settings.General.CurrentFrameRate < 29)
            //    return "FALSE";
            //if (Configuration.Settings.General.CurrentFrameRate < 29)
            //    return "FALSE";
            //if (Configuration.Settings.General.CurrentFrameRate < 30)
            //    return "TRUE"; // ntsc 29.97
            //if (Configuration.Settings.General.CurrentFrameRate < 40)
            //    return "TRUE";
            //if (Configuration.Settings.General.CurrentFrameRate < 40)
            //    return "TRUE";
            //if (Configuration.Settings.General.CurrentFrameRate < 60)
            //    return "TRUE"; // ntsc 59.94
            //return "FALSE";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            int duration = 0;
            if (subtitle.Paragraphs.Count > 0)
            {
                duration = (int)Math.Round(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds * Configuration.Settings.General.CurrentFrameRate);
            }

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<xmeml version=\"5\">" + Environment.NewLine +
                "<sequence id=\"X\">" + Environment.NewLine +
  @"    <uuid>5B3B0C07-9A9D-42AA-872C-C953923F97D8</uuid>
    <updatebehavior>add</updatebehavior>
    <name>X</name>
    <duration>" + duration + @"</duration>
    <rate>
      <ntsc>" + GetNtsc() + @"</ntsc>
      <timebase>" + GetFrameRateAsString() + @"</timebase>
    </rate>
    <timecode>
      <rate>
        <ntsc>" + GetNtsc() + @"</ntsc>
        <timebase>" + GetFrameRateAsString() + @"</timebase>
      </rate>
      <string>00:00:00:00</string>
      <frame>0</frame>
      <source>source</source>
      <displayformat>NDF</displayformat>
    </timecode>
    <in>0</in>
    <out>" + duration + @"</out>
    <media>
      <video>
        <format>
          <samplecharacteristics>
            <width>1920</width>
            <height>1080</height>
            <anamorphic>FALSE</anamorphic>
            <pixelaspectratio>Square</pixelaspectratio>
            <fielddominance>none</fielddominance>
            <rate>
              <ntsc>" + GetNtsc() + @"</ntsc>
              <timebase>" + GetFrameRateAsString() + @"</timebase>
            </rate>
            <colordepth>24</colordepth>
            <codec>
              <name>Apple ProRes 422</name>
              <appspecificdata>
                <appname>Final Cut Pro</appname>
                <appmanufacturer>Apple Inc.</appmanufacturer>
                <appversion>7.0</appversion>
                <data>
                  <qtcodec>
                    <codecname>Apple ProRes 422</codecname>
                    <codectypename>Apple ProRes 422 (HQ)</codectypename>
                    <codectypecode>apch</codectypecode>
                    <codecvendorcode>appl</codecvendorcode>
                    <spatialquality>1024</spatialquality>
                    <temporalquality>0</temporalquality>
                    <keyframerate>0</keyframerate>
                    <datarate>0</datarate>
                  </qtcodec>
                </data>
              </appspecificdata>
            </codec>
          </samplecharacteristics>
          <appspecificdata>
            <appname>Final Cut Pro</appname>
            <appmanufacturer>Apple Inc.</appmanufacturer>
            <appversion>7.0</appversion>
            <data>
              <fcpimageprocessing>
                <useyuv>TRUE</useyuv>
                <usesuperwhite>FALSE</usesuperwhite>
                <rendermode>Float10BPP</rendermode>
              </fcpimageprocessing>
            </data>
          </appspecificdata>
        </format>
        <track>
        </track>
      </video>
    </media>
  </sequence>
</xmeml>";

            if (string.IsNullOrEmpty(title))
            {
                title = "Subtitle Edit subtitle";
            }

            string xmlTrackStructure = "<generatoritem id='Outline Text[NUMBER]'><name>Text</name><duration>0</duration><rate><ntsc>" + GetNtsc() + @"</ntsc><timebase>" + GetFrameRateAsString() + @"</timebase></rate><start>0</start><end>46</end><effect><name>Text</name><effectid>Text</effectid><effectcategory>Text</effectcategory><effecttype>generator</effecttype><mediatype>video</mediatype><parameter><parameterid>str</parameterid><name>Text</name><value>[TEXT]</value></parameter><parameter><parameterid>fontname</parameterid><name>Font</name><value>Arial</value></parameter><parameter><parameterid>fontsize</parameterid><name>Size</name><valuemin>0</valuemin><valuemax>1000</valuemax><value>32</value></parameter><parameter><parameterid>fontstyle</parameterid><name>Style</name><valuemin>1</valuemin><valuemax>4</valuemax><valuelist><valueentry><name>Plain</name><value>1</value></valueentry><valueentry><name>Bold</name><value>2</value></valueentry><valueentry><name>Italic</name><value>3</value></valueentry><valueentry><name>Bold/Italic</name><value>1</value></valueentry></valuelist><value>[FONTSTYLE]</value></parameter><parameter><parameterid>fontalign</parameterid><name>Alignment</name><valuemin>1</valuemin><valuemax>3</valuemax><valuelist><valueentry><name>Left</name><value>1</value></valueentry><valueentry><name>Center</name><value>2</value></valueentry><valueentry><name>Right</name><value>3</value></valueentry></valuelist><value>2</value></parameter><parameter><parameterid>fontcolor</parameterid><name>Font Color</name><value><alpha>1</alpha><red>255</red><green>255</green><blue>255</blue></value></parameter><parameter><parameterid>origin</parameterid><name>Origin</name><value><horiz>0</horiz><vert>0.233854</vert></value></parameter></effect></generatoritem>";
            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.DocumentElement.SelectSingleNode("sequence").Attributes["id"].Value = title;
            xml.DocumentElement.SelectSingleNode("sequence/name").InnerText = title;

            xml.DocumentElement.SelectSingleNode("sequence/uuid").InnerText = Guid.NewGuid().ToString().ToUpperInvariant();
            if (!string.IsNullOrEmpty(subtitle.Header))
            {
                var header = new XmlDocument();
                try
                {
                    header.LoadXml(subtitle.Header);
                    var node = header.DocumentElement.SelectSingleNode("sequence/uuid");
                    if (node != null)
                    {
                        xml.DocumentElement.SelectSingleNode("sequence/uuid").InnerText = node.InnerText;
                    }
                }
                catch
                {
                }
            }

            XmlNode trackNode = xml.DocumentElement.SelectSingleNode("sequence/media/video/track");

            const string newLine = "_____@___";
            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode generatorItem = xml.CreateElement("generatoritem");
                string fontStyle = "1"; //1==plain
                var s = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont).Trim();
                if ((s.StartsWith("<i><b>") && s.EndsWith("</b></i>")) || (s.StartsWith("<b><i>") && s.EndsWith("</i></b>")))
                {
                    fontStyle = "4"; //4==bold/italic
                }
                else if (s.StartsWith("<i>") && s.EndsWith("</i>"))
                {
                    fontStyle = "3"; //3==italic
                }

                generatorItem.InnerXml = xmlTrackStructure.Replace("[NUMBER]", number.ToString()).Replace("[FONTSTYLE]", fontStyle);

                double frameRate = Configuration.Settings.General.CurrentFrameRate;
                XmlNode start = generatorItem.SelectSingleNode("generatoritem/start");
                start.InnerText = ((int)Math.Round(p.StartTime.TotalSeconds * frameRate)).ToString();

                XmlNode end = generatorItem.SelectSingleNode("generatoritem/end");
                end.InnerText = ((int)Math.Round(p.EndTime.TotalSeconds * frameRate)).ToString();

                XmlNode text = generatorItem.SelectSingleNode("generatoritem/effect/parameter[parameterid='str']/value");
                text.InnerText = HtmlUtil.RemoveHtmlTags(p.Text);
                text.InnerXml = text.InnerXml.Replace(Environment.NewLine, newLine);

                trackNode.AppendChild(generatorItem.SelectSingleNode("generatoritem"));
                number++;
            }

            string xmlAsText = ToUtf8XmlString(xml);
            xmlAsText = xmlAsText.Replace("xmeml[]", "xmeml");
            xmlAsText = xmlAsText.Replace(newLine, "&#13;");
            return xmlAsText;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(sb.ToString().Trim());

                var header = new XmlDocument { XmlResolver = null };
                header.LoadXml(sb.ToString());
                if (header.SelectSingleNode("sequence/media/video/track") != null)
                {
                    header.RemoveChild(header.SelectSingleNode("sequence/media/video/track"));
                }

                subtitle.Header = header.OuterXml;

                if (xml.DocumentElement.SelectSingleNode("sequence/rate") != null && xml.DocumentElement.SelectSingleNode("sequence/rate/timebase") != null)
                {
                    try
                    {
                        frameRate = double.Parse(xml.DocumentElement.SelectSingleNode("sequence/rate/timebase").InnerText);
                    }
                    catch
                    {
                        frameRate = Configuration.Settings.General.CurrentFrameRate;
                    }
                }

                foreach (XmlNode node in xml.SelectNodes("//media/video/track"))
                {
                    try
                    {
                        foreach (XmlNode generatorItemNode in node.SelectNodes("generatoritem"))
                        {
                            XmlNode rate = generatorItemNode.SelectSingleNode("rate");
                            if (rate != null)
                            {
                                XmlNode timebase = rate.SelectSingleNode("timebase");
                                if (timebase != null)
                                {
                                    frameRate = double.Parse(timebase.InnerText);
                                }
                            }

                            double startFrame = 0;
                            double endFrame = 0;
                            XmlNode startNode = generatorItemNode.SelectSingleNode("start");
                            if (startNode != null)
                            {
                                startFrame = double.Parse(startNode.InnerText);
                            }

                            XmlNode endNode = generatorItemNode.SelectSingleNode("end");
                            if (endNode != null)
                            {
                                endFrame = double.Parse(endNode.InnerText);
                            }

                            string text = string.Empty;
                            foreach (XmlNode parameterNode in generatorItemNode.SelectNodes("effect/parameter[parameterid='str']"))
                            {
                                XmlNode valueNode = parameterNode.SelectSingleNode("value");
                                if (valueNode != null)
                                {
                                    text += valueNode.InnerText;
                                }
                            }

                            bool italic = false;
                            bool bold = false;
                            foreach (XmlNode parameterNode in generatorItemNode.SelectNodes("effect/parameter[parameterid='style']"))
                            {
                                XmlNode valueNode = parameterNode.SelectSingleNode("value");
                                var valueEntries = parameterNode.SelectNodes("valuelist/valueentry");
                                if (valueNode != null)
                                {
                                    int no;
                                    if (int.TryParse(valueNode.InnerText, out no))
                                    {
                                        no--;
                                        if (no < valueEntries.Count)
                                        {
                                            var styleNameNode = valueEntries[no].SelectSingleNode("name");
                                            if (styleNameNode != null)
                                            {
                                                string styleName = styleNameNode.InnerText.ToLowerInvariant().Trim();
                                                italic = styleName == "italic" || styleName == "bold/italic";
                                                bold = styleName == "bold" || styleName == "bold/italic";
                                            }
                                        }
                                    }
                                }
                            }
                            if (!bold && !italic)
                            {
                                foreach (XmlNode parameterNode in generatorItemNode.SelectNodes("effect/parameter[parameterid='fontstyle']"))
                                {
                                    XmlNode valueNode = parameterNode.SelectSingleNode("value");
                                    var valueEntries = parameterNode.SelectNodes("valuelist/valueentry");
                                    if (valueNode != null)
                                    {
                                        int no;
                                        if (int.TryParse(valueNode.InnerText, out no))
                                        {
                                            no--;
                                            if (no < valueEntries.Count)
                                            {
                                                var styleNameNode = valueEntries[no].SelectSingleNode("name");
                                                if (styleNameNode != null)
                                                {
                                                    string styleName = styleNameNode.InnerText.ToLowerInvariant().Trim();
                                                    italic = styleName == "italic" || styleName == "bold/italic";
                                                    bold = styleName == "bold" || styleName == "bold/italic";
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (text.Length > 0)
                            {
                                if (!text.Contains(Environment.NewLine))
                                {
                                    text = text.Replace("\r", Environment.NewLine);
                                }

                                if (bold)
                                {
                                    text = "<b>" + text + "</b>";
                                }

                                if (italic)
                                {
                                    text = "<i>" + text + "</i>";
                                }

                                subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble((startFrame / frameRate) * 1000), Convert.ToDouble((endFrame / frameRate) * 1000)));
                            }
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
                return;
            }
            Configuration.Settings.General.CurrentFrameRate = frameRate;
        }

    }
}
