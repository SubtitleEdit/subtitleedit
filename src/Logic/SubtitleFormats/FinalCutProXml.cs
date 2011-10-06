using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class FinalCutProXml : SubtitleFormat
    {
        public double FrameRate { get; set; }

        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "Final Cut Pro Xml"; }
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
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        private string IsNtsc()
        {
            if (FrameRate >= 29.976 && FrameRate <= 30.0)
                return "TRUE";
            return "FALSE";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            if (Configuration.Settings.General.CurrentFrameRate > 26)
                FrameRate = 30;
            else
                FrameRate = 25;

            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<!DOCTYPE xmeml>" + Environment.NewLine +
                "<xmeml version=\"3\">" + Environment.NewLine +
                "   <sequence id=\"Subtitles\">" + Environment.NewLine +
                "       <name>Subtitles</name>" + Environment.NewLine +
                "       <media>" + Environment.NewLine +
                "           <video />" + Environment.NewLine +
                "       </media>" + Environment.NewLine +
                "   </sequence>" + Environment.NewLine +
                "</xmeml>";

            string xmlTrackStructure =
                "<generatoritem>" + Environment.NewLine +
                "    <name>Text</name>" + Environment.NewLine +
                "    <rate>" + Environment.NewLine +
                "        <ntsc>" + IsNtsc() + "</ntsc>" + Environment.NewLine +
                "        <timebase>" + (int)Math.Round(FrameRate) + "</timebase>" + Environment.NewLine +
                "    </rate>" + Environment.NewLine +
                "    <start></start>" + Environment.NewLine + // start frame?
                "    <end></end>" + Environment.NewLine + // end frame?
                "    <enabled>TRUE</enabled>" + Environment.NewLine +
                "    <anamorphic>FALSE</anamorphic>" + Environment.NewLine +
                "    <alphatype>black</alphatype>" + Environment.NewLine +
                "    <effect id=\"subtitle\">" + Environment.NewLine +
                "        <name>Text</name>" + Environment.NewLine +
//                "        <effectid>Text</effectid>" + Environment.NewLine +
                "        <effectcategory>Text</effectcategory>" + Environment.NewLine +
                "        <effecttype>generator</effecttype>" + Environment.NewLine +
                "        <mediatype>video</mediatype>" + Environment.NewLine +
                "        <parameter>" + Environment.NewLine +
                "            <parameterid>str</parameterid>" + Environment.NewLine +
                "            <name>Text</name>" + Environment.NewLine +
                "            <value />" + Environment.NewLine + // TEXT GOES HERE
                "        </parameter>" + Environment.NewLine +
                "    </effect>" + Environment.NewLine +
                "</generatoritem>";

            string xmlTrackStructure2 =
                @"<generatoritem id='Subtitle Edit'>
                    <name>edit 'B subtitle</name>
                    <duration>3000</duration>
                    <rate>
                        <ntsc>FALSE</ntsc>
                        <timebase>25</timebase>
                    </rate>
                    <in>1375</in>
                    <out>1383</out>
                    <start>0</start>
                    <end>8</end>
                    <enabled>TRUE</enabled>
                    <anamorphic>TRUE</anamorphic>
                    <alphatype>black</alphatype>
                    <logginginfo>
                        <scene>
                        </scene>
                        <shottake>
                        </shottake>
                        <lognote>
                        </lognote>
                        <good>FALSE</good>
                    </logginginfo>
                    <labels>
                        <label2>
                        </label2>
                    </labels>
                    <comments>
                        <mastercomment1>
                        </mastercomment1>
                        <mastercomment2>
                        </mastercomment2>
                        <mastercomment3>
                        </mastercomment3>
                        <mastercomment4>
                        </mastercomment4>
                    </comments>
                    <effect>
                        <name>edit 'B subtitle</name>
                        <effectid>edit 'B subtitle</effectid>
                        <effectcategory>Text</effectcategory>
                        <effecttype>generator</effecttype>
                        <mediatype>video</mediatype>
                        <parameter>
                            <parameterid>part1</parameterid>
                            <name>Text Settings</name>
                            <value>0</value>
                        </parameter>
                        <parameter>
                            <parameterid>str</parameterid>
                            <name>Text</name>
                            <value></value>
                        </parameter>
                        <parameter>
                            <parameterid>fontname</parameterid>
                            <name>Font</name>
                            <value>Arial Narrow</value>
                        </parameter>
                        <parameter>
                            <parameterid>fontstyle</parameterid>
                            <name>Style</name>
                            <valuemin>1</valuemin>
                            <valuemax>4</valuemax>
                            <valuelist>
                                <valueentry>
                                    <name>Plain</name>
                                    <value>1</value>
                                </valueentry>
                                <valueentry>
                                    <name>Bold</name>
                                    <value>2</value>
                                </valueentry>
                                <valueentry>
                                    <name>Italic</name>
                                    <value>3</value>
                                </valueentry>
                                <valueentry>
                                    <name>Bold/Italic</name>
                                    <value>4</value>
                                </valueentry>
                            </valuelist>
                            <value>1</value>
                        </parameter>
                        <parameter>
                            <parameterid>fontalign</parameterid>
                            <name>Alignment</name>
                            <valuemin>1</valuemin>
                            <valuemax>3</valuemax>
                            <valuelist>
                                <valueentry>
                                    <name>Left</name>
                                    <value>1</value>
                                </valueentry>
                                <valueentry>
                                    <name>Center</name>
                                    <value>2</value>
                                </valueentry>
                                <valueentry>
                                    <name>Right</name>
                                    <value>3</value>
                                </valueentry>
                            </valuelist>
                            <value>2</value>
                        </parameter>
                        <parameter>
                            <parameterid>fontsize</parameterid>
                            <name>Size</name>
                            <valuemin>20</valuemin>
                            <valuemax>80</valuemax>
                            <value>28</value>
                        </parameter>
                        <parameter>
                            <parameterid>origin</parameterid>
                            <name>Origin</name>
                            <value>
                                <horiz>0</horiz>
                                <vert>0.3056</vert>
                            </value>
                        </parameter>
                        <parameter>
                            <parameterid>aspect</parameterid>
                            <name>Aspect</name>
                            <valuemin>0</valuemin>
                            <valuemax>2</valuemax>
                            <value>1</value>
                        </parameter>
                        <parameter>
                            <parameterid>textopacity</parameterid>
                            <name>Text Opacity</name>
                            <valuemin>0</valuemin>
                            <valuemax>100</valuemax>
                            <value>100</value>
                        </parameter>
                        <parameter>
                            <parameterid>textcolor</parameterid>
                            <name>Text Color</name>
                            <value>
                                <alpha>255</alpha>
                                <red>255</red>
                                <green>255</green>
                                <blue>255</blue>
                            </value>
                        </parameter>
                        <parameter>
                            <parameterid>fonttrack</parameterid>
                            <name>Tracking</name>
                            <valuemin>0</valuemin>
                            <valuemax>10</valuemax>
                            <value>1</value>
                        </parameter>
                        <parameter>
                            <parameterid>leading</parameterid>
                            <name>Leading</name>
                            <valuemin>-40</valuemin>
                            <valuemax>40</valuemax>
                            <value>-13</value>
                        </parameter>
                        <parameter>
                            <parameterid>autokern</parameterid>
                            <name>Auto Kerning</name>
                            <value>TRUE</value>
                        </parameter>
                        <parameter>
                            <parameterid>part2</parameterid>
                            <name>Outline Settings</name>
                            <value>0</value>
                        </parameter>
                        <parameter>
                            <parameterid>linewidth</parameterid>
                            <name>Width</name>
                            <valuemin>0</valuemin>
                            <valuemax>50</valuemax>
                            <value>8</value>
                        </parameter>
                        <parameter>
                            <parameterid>linesoft</parameterid>
                            <name>Soft</name>
                            <valuemin>0</valuemin>
                            <valuemax>100</valuemax>
                            <value>38</value>
                        </parameter>
                        <parameter>
                            <parameterid>linecolor</parameterid>
                            <name>Color</name>
                            <value>
                                <alpha>255</alpha>
                                <red>0</red>
                                <green>0</green>
                                <blue>0</blue>
                            </value>
                        </parameter>
                        <parameter>
                            <parameterid>part3</parameterid>
                            <name>Shadow Settings</name>
                            <value>0</value>
                        </parameter>
                        <parameter>
                            <parameterid>shadowoffsetx</parameterid>
                            <name>Offset X</name>
                            <valuemin>-20</valuemin>
                            <valuemax>20</valuemax>
                            <value>3</value>
                        </parameter>
                        <parameter>
                            <parameterid>shadowoffsety</parameterid>
                            <name>Offset y</name>
                            <valuemin>-20</valuemin>
                            <valuemax>20</valuemax>
                            <value>3</value>
                        </parameter>
                        <parameter>
                            <parameterid>shadowopacity</parameterid>
                            <name>Opacity</name>
                            <valuemin>0</valuemin>
                            <valuemax>100</valuemax>
                            <value>75</value>
                        </parameter>
                        <parameter>
                            <parameterid>shadowsoft</parameterid>
                            <name>Softness</name>
                            <valuemin>0</valuemin>
                            <valuemax>100</valuemax>
                            <value>2</value>
                        </parameter>
                        <parameter>
                            <parameterid>shadowcolor</parameterid>
                            <name>Color</name>
                            <value>
                                <alpha>255</alpha>
                                <red>0</red>
                                <green>0</green>
                                <blue>0</blue>
                            </value>
                        </parameter>
                    </effect>
                    <sourcetrack>
                        <mediatype>video</mediatype>
                    </sourcetrack>
                </generatoritem>";

//            string xmlTrackStructure3a =
//@"<generatoritem>
//    <name>Text</name>
//    <duration>3600</duration>
//    <rate>
//        <ntsc>TRUE</ntsc>
//        <timebase>30</timebase>
//    </rate>
//    <in>1650</in>
//    <out>1784</out>
//    <start>0</start>
//    <end>134</end>
//    <enabled>TRUE</enabled>
//    <anamorphic>FALSE</anamorphic>
//    <alphatype>black</alphatype>
//    <effect id = 'subtitle'>
//        <name>Text</name>
//        <effectid>Text</effectid>
//        <effectcategory>Text</effectcategory>
//        <effecttype>generator</effecttype>
//        <mediatype>video</mediatype>
//        <parameter>
//            <parameterid>str</parameterid>
//            <name>Text</name>
//            <value>If you look at the Lindy Hop&#13;you'll see a couple just moving</value>
//        </parameter>
//        <parameter>
//            <parameterid>fontname</parameterid>
//            <name>Font</name>
//            <value>Futura</value>
//        </parameter>
//        <parameter>
//            <parameterid>fontsize</parameterid>
//            <name>Size</name>
//            <valuemin>0</valuemin>
//            <valuemax>1000</valuemax>
//            <value>36</value></parameter>
//        <parameter>
//            <parameterid>fontstyle</parameterid>
//            <name>Style</name>
//            <valuemin>1</valuemin>
//            <valuemax>4</valuemax>
//            <valuelist>
//                <valueentry>
//                    <name>Plain</name>
//                    <value>1</value>
//                </valueentry>
//                <valueentry>
//                    <name>Bold</name>
//                    <value>2</value>
//                </valueentry>
//                <valueentry>
//                    <name>Italic</name>
//                    <value>3</value>
//                </valueentry>
//                <valueentry>
//                    <name>Bold/Italic</name>
//                    <value>4</value>
//                </valueentry>
//            </valuelist>
//            <value>3</value>
//        </parameter>
//        <parameter>
//            <parameterid>fontalign</parameterid>
//            <name>Alignment</name>
//            <valuemin>1</valuemin>
//            <valuemax>3</valuemax>
//            <valuelist>
//                <valueentry>
//                    <name>Left</name>
//                    <value>1</value>
//                </valueentry>
//                <valueentry>
//                    <name>Center</name>
//                    <value>2</value>
//                </valueentry>
//                <valueentry>
//                    <name>Right</name>
//                    <value>3</value>
//                </valueentry>
//            </valuelist>
//            <value>2</value>
//        </parameter>
//        <parameter>
//            <parameterid>fontcolor</parameterid>
//            <name>Font Color</name>
//            <value>
//                <alpha>255</alpha>
//                <red>255</red>
//                <green>255</green>
//                <blue>255</blue>
//            </value>
//        </parameter>
//        <parameter>
//            <parameterid>origin</parameterid>
//            <name>Origin</name>
//            <value>
//                <horiz>0</horiz>
//                <vert>0.34375</vert>
//            </value>
//        </parameter>
//    </effect>
//    <sourcetrack>
//        <mediatype>video</mediatype>
//    </sourcetrack>
//</generatoritem>";

//string xmlTrackStructure3b =
//@"<generatoritem>
//    <start>135</start>
//    <end>219</end>
//    <effect id='subtitle'>
//        <name>Text</name>
//        <effectid>Text</effectid>
//        <effectcategory>Text</effectcategory>
//        <effecttype>generator</effecttype>
//        <mediatype>video</mediatype>
//        <parameter>
//            <parameterid>str</parameterid>
//            <name>Text</name>
//            <value></value>
//        </parameter>
//    </effect>
//</generatoritem>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("sequence/media/video");

            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode track = xml.CreateElement("track");
                track.InnerXml = xmlTrackStructure2;

                double frameRate = Configuration.Settings.General.CurrentFrameRate;
                XmlNode start = track.SelectSingleNode("generatoritem/start");
                start.InnerText = ((int)Math.Round(p.StartTime.TotalSeconds*frameRate)).ToString();

                XmlNode end = track.SelectSingleNode("generatoritem/end");
                end.InnerText = ((int)Math.Round(p.EndTime.TotalSeconds * frameRate)).ToString();

                XmlNode text = track.SelectSingleNode("generatoritem/effect/parameter[parameterid='str']/value");
                text.InnerText = Utilities.RemoveHtmlTags(p.Text);

                XmlNode effect = track.SelectSingleNode("generatoritem/effect");
                if (effect != null && effect.Attributes["id"] != null)
                    effect.Attributes["id"].InnerText = "Subtitle" + number.ToString();

                videoNode.AppendChild(track);
                number++;
            }

            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            string xmlAsText = Encoding.UTF8.GetString(ms.ToArray()).Trim();
            xmlAsText = xmlAsText.Replace("xmeml[]", "xmeml");
            return xmlAsText;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            FrameRate = Configuration.Settings.General.CurrentFrameRate;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(sb.ToString());

                if (xml.DocumentElement.SelectSingleNode("sequence/rate") != null && xml.DocumentElement.SelectSingleNode("sequence/rate/timebase") != null)
                {
                    try
                    {
                        FrameRate = double.Parse(xml.DocumentElement.SelectSingleNode("sequence/rate/timebase").InnerText);
                    }
                    catch
                    {
                        FrameRate = Configuration.Settings.General.CurrentFrameRate;
                    }
                }

                foreach (XmlNode node in xml.SelectNodes("xmeml/sequence/media/video/track"))
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
                                    FrameRate = double.Parse(timebase.InnerText);
                            }

                            double startFrame = 0;
                            double endFrame = 0;
                            XmlNode startNode = generatorItemNode.SelectSingleNode("start");
                            if (startNode != null)
                                startFrame = double.Parse(startNode.InnerText);

                            XmlNode endNode = generatorItemNode.SelectSingleNode("end");
                            if (endNode != null)
                                endFrame = double.Parse(endNode.InnerText);

                            string text = string.Empty;
                            foreach (XmlNode parameterNode in generatorItemNode.SelectNodes("effect/parameter[parameterid='str']"))
                            {
                                XmlNode valueNode = parameterNode.SelectSingleNode("value");
                                if (valueNode != null)
                                    text += valueNode.InnerText;
                            }
                            if (text.Length > 0)
                            {
                                subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble((startFrame / FrameRate) *1000), Convert.ToDouble((endFrame / FrameRate) * 1000)));
                            }
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                subtitle.Renumber(1);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

        }

    }
}


