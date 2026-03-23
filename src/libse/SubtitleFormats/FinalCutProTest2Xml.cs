using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    //  - Mom, when you were my age&#13;what did you want to do?
    public class FinalCutProTest2Xml : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Final Cut Pro Test2 Xml";

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
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            int duration = 0;
            if (subtitle.Paragraphs.Count > 0)
            {
                duration = (int)Math.Round(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds * Configuration.Settings.General.CurrentFrameRate);
            }

            string seString = "Subtitle Edit at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<xmeml version=\"5\">" + Environment.NewLine +
                "   <sequence id=\"" + seString + "\">" + Environment.NewLine +
                "       <uuid>EC466A7D-8B45-4682-9978-D15D630C882E</uuid><updatebehavior>add</updatebehavior><name>" + seString + "</name><duration>" + duration + "</duration><rate><ntsc>>" + GetNtsc() + @"</ntsc><timebase>" + GetFrameRateAsString() + @"</timebase></rate><timecode><rate><ntsc>" + GetNtsc() + @"</ntsc><timebase>" + GetFrameRateAsString() + @"</timebase></rate><string>01:00:00:00</string><frame>90000</frame><source>source</source><displayformat>NDF</displayformat></timecode><in>-1</in><out>-1</out><media><video><format><samplecharacteristics><width>1920</width><height>1080</height><anamorphic>FALSE</anamorphic><pixelaspectratio>Square</pixelaspectratio><fielddominance>none</fielddominance><rate><ntsc>FALSE</ntsc><timebase>25</timebase></rate><colordepth>24</colordepth><codec><name>Apple ProRes 422</name><appspecificdata><appname>Final Cut Pro</appname><appmanufacturer>Apple Inc.</appmanufacturer><appversion>7.0</appversion><data><qtcodec><codecname>Apple ProRes 422</codecname><codectypename>Apple ProRes 422 (HQ)</codectypename><codectypecode>apch</codectypecode><codecvendorcode>appl</codecvendorcode><spatialquality>1024</spatialquality><temporalquality>0</temporalquality><keyframerate>0</keyframerate><datarate>0</datarate></qtcodec></data></appspecificdata></codec></samplecharacteristics><appspecificdata><appname>Final Cut Pro</appname><appmanufacturer>Apple Inc.</appmanufacturer><appversion>7.0</appversion><data><fcpimageprocessing><useyuv>TRUE</useyuv><usesuperwhite>FALSE</usesuperwhite><rendermode>Float10BPP</rendermode></fcpimageprocessing></data></appspecificdata></format><track><enabled>TRUE</enabled><locked>FALSE</locked></track>" + Environment.NewLine +
                "       <track></track>" +
                "</video><audio><format><samplecharacteristics><depth>16</depth><samplerate>48000</samplerate></samplecharacteristics></format><outputs><group><index>1</index><numchannels>2</numchannels><downmix>0</downmix><channel><index>1</index></channel><channel><index>2</index></channel></group></outputs><in>-1</in><out>-1</out><track><enabled>TRUE</enabled><locked>FALSE</locked><outputchannelindex>1</outputchannelindex></track><track><enabled>TRUE</enabled><locked>FALSE</locked><outputchannelindex>2</outputchannelindex></track><track><enabled>TRUE</enabled><locked>FALSE</locked><outputchannelindex>1</outputchannelindex></track><track><enabled>TRUE</enabled><locked>FALSE</locked><outputchannelindex>2</outputchannelindex></track><filter><effect><name>Audio Levels</name><effectid>audiolevels</effectid><effectcategory>audiolevels</effectcategory><effecttype>audiolevels</effecttype><mediatype>audio</mediatype><parameter><name>Level</name><parameterid>level</parameterid><valuemin>0</valuemin><valuemax>3.98109</valuemax><value>1</value></parameter></effect></filter></audio></media></sequence></xmeml>";

            const string xmlTrackStructure = "<generatoritem id=\"Text\"><name>Text</name><duration>3000</duration><rate><ntsc>FALSE</ntsc><timebase>25</timebase></rate><in>1375</in><out>1486</out><start>1504</start><end>1615</end><anamorphic>FALSE</anamorphic><alphatype>black</alphatype><logginginfo><scene/><shottake/><lognote/><good>FALSE</good></logginginfo><labels><label2/></labels><comments><mastercomment1/><mastercomment2/><mastercomment3/><mastercomment4/></comments><effect><name>Text</name><effectid>Text</effectid><effectcategory>Text</effectcategory><effecttype>generator</effecttype><mediatype>video</mediatype><parameter><parameterid>str</parameterid><name>Text</name><value><i>A finales de los años sesenta, una joven pareja, Guy y Rosemary,</i> </value></parameter><parameter><parameterid>fontname</parameterid><name>Font</name><value>Lucida Grande</value></parameter><parameter><parameterid>fontsize</parameterid><name>Size</name><valuemin>0</valuemin><valuemax>1000</valuemax><value>[FONTSIZE]</value></parameter><parameter><parameterid>fontstyle</parameterid><name>Style</name><valuemin>1</valuemin><valuemax>4</valuemax><valuelist><valueentry><name>Plain</name><value>1</value></valueentry><valueentry><name>Bold</name><value>2</value></valueentry><valueentry><name>Italic</name><value>3</value></valueentry><valueentry><name>Bold/Italic</name><value>4</value></valueentry></valuelist><value>1</value></parameter><parameter><parameterid>fontalign</parameterid><name>Alignment</name><valuemin>1</valuemin><valuemax>3</valuemax><valuelist><valueentry><name>Left</name><value>1</value></valueentry><valueentry><name>Center</name><value>2</value></valueentry><valueentry><name>Right</name><value>3</value></valueentry></valuelist><value>2</value></parameter><parameter><parameterid>fontcolor</parameterid><name>Font Color</name><value><alpha>255</alpha><red>255</red><green>255</green><blue>255</blue></value></parameter><parameter><parameterid>origin</parameterid><name>Origin</name><value><horiz>0</horiz><vert>0</vert></value></parameter><parameter><parameterid>fonttrack</parameterid><name>Tracking</name><valuemin>-200</valuemin><valuemax>200</valuemax><value>1</value></parameter><parameter><parameterid>leading</parameterid><name>Leading</name><valuemin>-100</valuemin><valuemax>100</valuemax><value>0</value></parameter><parameter><parameterid>aspect</parameterid><name>Aspect</name><valuemin>0.1</valuemin><valuemax>5</valuemax><value>1</value></parameter><parameter><parameterid>autokern</parameterid><name>Auto Kerning</name><value>TRUE</value></parameter><parameter><parameterid>subpixel</parameterid><name>Use Subpixel</name><value>TRUE</value></parameter></effect><filter><effect><name>Basic Motion</name><effectid>basic</effectid><effectcategory>motion</effectcategory><effecttype>motion</effecttype><mediatype>video</mediatype><parameter><parameterid>scale</parameterid><name>Scale</name><valuemin>0</valuemin><valuemax>1000</valuemax><value>100</value></parameter><parameter><parameterid>rotation</parameterid><name>Rotation</name><valuemin>-8640</valuemin><valuemax>8640</valuemax><value>0</value></parameter><parameter><parameterid>center</parameterid><name>Center</name><value><horiz>0.00470958</horiz><vert>0.396648</vert></value></parameter><parameter><parameterid>centerOffset</parameterid><name>Anchor Point</name><value><horiz>0</horiz><vert>0</vert></value></parameter></effect></filter><sourcetrack><mediatype>video</mediatype></sourcetrack><itemhistory><uuid>3506ED18-CB4D-41B8-A760-4D42356E4F32</uuid><uuid>1E6E96FD-94F6-4975-BDFE-7B360E909111</uuid></itemhistory></generatoritem>";

            if (string.IsNullOrEmpty(title))
            {
                title = "Subtitle Edit subtitle";
            }

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

            XmlNode trackNode = xml.DocumentElement.SelectSingleNode("sequence/media/video/track[2]");

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

                generatorItem.InnerXml = xmlTrackStructure.Replace("[NUMBER]", number.ToString()).Replace("[FONTSTYLE]", fontStyle).Replace("[FONTSIZE]", Configuration.Settings.SubtitleSettings.FcpFontSize.ToString(CultureInfo.InvariantCulture));

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

                foreach (XmlNode node in xml.SelectNodes("//video/track"))
                {
                    try
                    {
                        foreach (XmlNode generatorItemNode in node.SelectNodes("generatoritem"))
                        {
                            XmlNode rate = generatorItemNode.SelectSingleNode("rate");
                            XmlNode timebase = rate?.SelectSingleNode("timebase");
                            if (timebase != null)
                            {
                                frameRate = double.Parse(timebase.InnerText);
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
