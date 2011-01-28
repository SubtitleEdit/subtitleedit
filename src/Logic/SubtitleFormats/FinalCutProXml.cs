using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class FinalCutProXml : SubtitleFormat
    {
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

        public override string ToText(Subtitle subtitle, string title)
        {
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
                "        <ntsc>TRUE</ntsc>" + Environment.NewLine +
                "        <timebase>30</timebase>" + Environment.NewLine +
                "    </rate>" + Environment.NewLine +
                "    <start></start>" + Environment.NewLine + // start frame?
                "    <end></end>" + Environment.NewLine + // end frame?
                "    <enabled>TRUE</enabled>" + Environment.NewLine +
                "    <anamorphic>FALSE</anamorphic>" + Environment.NewLine +
                "    <alphatype>black</alphatype>" + Environment.NewLine +
                "    <effect id=\"subtitle\">" + Environment.NewLine +
                "        <name>Text</name>" + Environment.NewLine +
                "        <effectid>Text</effectid>" + Environment.NewLine +
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

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode videoNode = xml.DocumentElement.SelectSingleNode("sequence/media/video");

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                XmlNode track = xml.CreateElement("track");
                track.InnerXml = xmlTrackStructure;

                const int frameRate = 30;
                XmlNode start = track.SelectSingleNode("generatoritem/start");
                start.InnerText = (p.StartTime.TotalSeconds*frameRate).ToString();

                XmlNode end = track.SelectSingleNode("generatoritem/end");
                end.InnerText = (p.EndTime.TotalSeconds * frameRate).ToString();

                XmlNode text = track.SelectSingleNode("generatoritem/effect/parameter/value");
                text.InnerText = Utilities.RemoveHtmlTags(p.Text);
                videoNode.AppendChild(track);
            }

            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            const int frameRate = 30;
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(sb.ToString());

                foreach (XmlNode node in xml.SelectNodes("xmeml/sequence/media/video/track"))
                {
                    try
                    {
                        foreach (XmlNode generatorItemNode in node.SelectNodes("generatoritem"))
                        {

                            int startFrame = 0;
                            int endFrame = 0;
                            XmlNode startNode = generatorItemNode.SelectSingleNode("start");
                            if (startNode != null)
                                startFrame = int.Parse(startNode.InnerText);

                            XmlNode endNode = generatorItemNode.SelectSingleNode("end");
                            if (endNode != null)
                                endFrame = int.Parse(endNode.InnerText);

                            string text = string.Empty;
                            foreach (XmlNode parameterNode in generatorItemNode.SelectNodes("effect/parameter[parameterid='str']"))
                            {
                                XmlNode valueNode = parameterNode.SelectSingleNode("value");
                                if (valueNode != null)
                                    text += valueNode.InnerText;
                            }
                            if (text.Length > 0)
                            {
                                subtitle.Paragraphs.Add(new Paragraph(text, Convert.ToDouble((startFrame / frameRate) *1000), Convert.ToDouble((endFrame / frameRate) * 1000)));
                            }
                        }
                    }
                    catch (Exception ex)
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


