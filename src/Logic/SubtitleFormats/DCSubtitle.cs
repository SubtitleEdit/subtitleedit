using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

// http://code.google.com/p/subtitleedit/issues/detail?id=18
//<?xml version="1.0" encoding="UTF-8"?>
//<DCSubtitle Version="1.0">
//  <SubtitleID>4EB245B8-4D3A-4158-9516-95DD20E8322E</SubtitleID>
//  <MovieTitle>Unknown</MovieTitle>
//  <ReelNumber>1</ReelNumber>
//  <Language>Swedish</Language>
//  <Font Italic="no">
//    <Subtitle SpotNumber="1" TimeIn="00:00:06:040" TimeOut="00:00:08:040" FadeUpTime="20" FadeDownTime="20">
//      <Text Direction="horizontal" HAlign="center" HPosition="0.0" VAlign="bottom" VPosition="6.0">DETTA HAR HÄNT...</Text>
//    </Subtitle>
//  </Font>
//</DCSubtitle>

    class DCSubtitle : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "D-Cinema"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<DCSubtitle"))
            {
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.LoadXml(xmlAsString);

                    var subtitles = xml.DocumentElement.SelectNodes("//Subtitle");
                    if (subtitles != null)
                        return subtitles != null && subtitles.Count > 0;
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string languageEnglishName;
            try
            {
                string languageShortName = Utilities.AutoDetectGoogleLanguage(subtitle);
                CultureInfo ci = CultureInfo.CreateSpecificCulture(languageShortName);
                languageEnglishName = ci.EnglishName;
                int indexOfStartP = languageEnglishName.IndexOf("(");
                if (indexOfStartP > 1)
                    languageEnglishName = languageEnglishName.Remove(indexOfStartP).Trim();
            }
            catch
            {
                languageEnglishName = "English";
            }

            string xmlStructure = "<DCSubtitle Version=\"1.0\">" + Environment.NewLine +
                                    "    <SubtitleID>4EB245B8-4D3A-4158-9516-95DD20E8322E</SubtitleID>" + Environment.NewLine +
                                    "    <MovieTitle></MovieTitle>" + Environment.NewLine +
                                    "    <ReelNumber>1</ReelNumber>" + Environment.NewLine +
                                    "    <Language>" + languageEnglishName + "</Language>" + Environment.NewLine +
                                    "    <LoadFont URI=\"" + Configuration.Settings.SubtitleSettings.DCinemaFontFile + "\" Id=\"Font1\"/>" + Environment.NewLine +
                                    "    <Font Color=\"FFFFFFFF\" Effect=\"shadow\" EffectColor=\"FF000000\" Italic=\"no\" Underlined=\"no\" Script=\"normal\" Size=\"42\">" + Environment.NewLine +
                                    "    </Font>" + Environment.NewLine +
                                    "</DCSubtitle>";

            string loadedFontId = "Font1";
            int fontSize = Configuration.Settings.SubtitleSettings.DCinemaFontSize;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.DocumentElement.SelectSingleNode("MovieTitle").InnerText = title;

            // use settings from exsiting header if available
            XmlDocument xmlHeader = null;
            if (!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.Contains("<DCSubtitle"))
            {
                try
                {
                    xmlHeader = new XmlDocument();
                    xmlHeader.LoadXml(subtitle.Header);

                    var node = xmlHeader.DocumentElement.SelectSingleNode("SubtitleID");
                    if (node != null)
                        xml.DocumentElement.SelectSingleNode("SubtitleID").InnerText = node.InnerText;

                    node = xmlHeader.DocumentElement.SelectSingleNode("ReelNumber");
                    if (node != null)
                        xml.DocumentElement.SelectSingleNode("ReelNumber").InnerText = node.InnerText;

                    node = xmlHeader.DocumentElement.SelectSingleNode("Language");
                    if (node != null)
                        xml.DocumentElement.SelectSingleNode("Language").InnerText = node.InnerText;

                    node = xmlHeader.DocumentElement.SelectSingleNode("LoadFont");
                    if (node != null)
                    {
                        if (node.Attributes["Id"] != null)
                            loadedFontId = node.Attributes["Id"].InnerText;
                        if (node.Attributes["URI"] != null)
                            xml.DocumentElement.SelectSingleNode("LoadFont").Attributes["URI"].Value = node.Attributes["URI"].InnerText;
                    }
                    else
                    {
                        loadedFontId = null;
                        xml.DocumentElement.RemoveChild(xml.DocumentElement.SelectSingleNode("LoadFont"));
                    }

                    node = xmlHeader.DocumentElement.SelectSingleNode("Font");
                    if (node != null && node.Attributes["Size"] != null)
                    {
                        int temp;
                        if (int.TryParse(node.Attributes["Size"].Value, out temp))
                        {
                            if (temp > 4 && temp < 100)
                            {
                                fontSize = temp;
                                xml.DocumentElement.SelectSingleNode("Font").Attributes["Size"].Value = fontSize.ToString();
                            }
                        }
                    }
                }
                catch
                {
                    xmlHeader = null;
                }
            }

            if (loadedFontId != null)
            {
                var fontNode = xml.DocumentElement.SelectSingleNode("Font");
                XmlAttribute a = xml.CreateAttribute("Id");
                a.InnerText = loadedFontId;
                fontNode.Attributes.Prepend(a);
            };

            XmlNode mainListFont = xml.DocumentElement.SelectSingleNode("Font");
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (!string.IsNullOrEmpty(p.Text))
                {
                    XmlNode subNode = xml.CreateElement("Subtitle");

                    XmlAttribute id = xml.CreateAttribute("SpotNumber");
                    id.InnerText = (no + 1).ToString();
                    subNode.Attributes.Append(id);

                    XmlAttribute fadeUpTime = xml.CreateAttribute("FadeUpTime");
                    fadeUpTime.InnerText = Configuration.Settings.SubtitleSettings.DCinemaFadeUpDownTime.ToString();
                    subNode.Attributes.Append(fadeUpTime);

                    XmlAttribute fadeDownTime = xml.CreateAttribute("FadeDownTime");
                    fadeDownTime.InnerText = Configuration.Settings.SubtitleSettings.DCinemaFadeUpDownTime.ToString();
                    subNode.Attributes.Append(fadeDownTime);

                    XmlAttribute start = xml.CreateAttribute("TimeIn");
                    start.InnerText = ConvertToTimeString(p.StartTime);
                    subNode.Attributes.Append(start);

                    XmlAttribute end = xml.CreateAttribute("TimeOut");
                    end.InnerText = ConvertToTimeString(p.EndTime);
                    subNode.Attributes.Append(end);

                    string[] lines = p.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    int vPos = 1 + lines.Length * 7;
                    int vPosFactor = (int)Math.Round(fontSize / 7.4);
                    vPos = (lines.Length * vPosFactor) - vPosFactor + Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8

                    bool isItalic = false;
                    foreach (string line in lines)
                    {
                        XmlNode textNode = xml.CreateElement("Text");

                        XmlAttribute vPosition = xml.CreateAttribute("VPosition");
                        vPosition.InnerText = vPos.ToString();
                        textNode.Attributes.Append(vPosition);

                        XmlAttribute vAlign = xml.CreateAttribute("VAlign");
                        vAlign.InnerText = "bottom";
                        textNode.Attributes.Append(vAlign);

                        XmlAttribute hAlign = xml.CreateAttribute("HAlign");
                        hAlign.InnerText = "center";
                        textNode.Attributes.Append(hAlign);

                        XmlAttribute direction = xml.CreateAttribute("Direction");
                        direction.InnerText = "horizontal";
                        textNode.Attributes.Append(direction);

                        int i = 0;
                        var txt = new StringBuilder();
                        var html = new StringBuilder();
                        XmlNode nodeTemp = xml.CreateElement("temp");
                        while (i < line.Length)
                        {
                            if (!isItalic && line.Substring(i).StartsWith("<i>"))
                            {
                                if (txt.Length > 0)
                                {
                                    nodeTemp.InnerText = txt.ToString();
                                    html.Append(nodeTemp.InnerXml);
                                    txt = new StringBuilder();
                                }
                                isItalic = true;
                                i += 2;
                            }
                            else if (isItalic && line.Substring(i).StartsWith("</i>"))
                            {
                                if (txt.Length > 0)
                                {
                                    XmlNode fontNode = xml.CreateElement("Font");

                                    XmlAttribute italic = xml.CreateAttribute("Italic");
                                    italic.InnerText = "yes";
                                    fontNode.Attributes.Append(italic);

                                    fontNode.InnerText = Utilities.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt = new StringBuilder();
                                }
                                isItalic = false;
                                i += 3;
                            }
                            else
                            {
                                txt.Append(line.Substring(i, 1));
                            }
                            i++;
                        }
                        if (isItalic)
                        {
                            if (txt.Length > 0)
                            {
                                XmlNode fontNode = xml.CreateElement("Font");

                                XmlAttribute italic = xml.CreateAttribute("Italic");
                                italic.InnerText = "yes";
                                fontNode.Attributes.Append(italic);

                                fontNode.InnerText = Utilities.RemoveHtmlTags(line);
                                html.Append(fontNode.OuterXml);
                            }
                        }
                        else
                        {
                            if (txt.Length > 0)
                            {
                                nodeTemp.InnerText = txt.ToString();
                                html.Append(nodeTemp.InnerXml);
                            }
                        }
                        textNode.InnerXml = html.ToString();

                        subNode.AppendChild(textNode);
                        vPos -= vPosFactor;
                    }

                    mainListFont.AppendChild(subNode);
                    no++;
                }
            }

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim().Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"");
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument();
            xml.LoadXml(sb.ToString());

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//Subtitle"))
            {
                try
                {
                    StringBuilder pText = new StringBuilder();
                    string lastVPosition = string.Empty;
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name.ToString())
                        {
                            case "Text":
                                if (innerNode.Attributes["VPosition"] != null)
                                {
                                    string vPosition = innerNode.Attributes["VPosition"].InnerText;
                                    if (vPosition != lastVPosition)
                                    {
                                        if (pText.Length > 0 && lastVPosition != string.Empty)
                                            pText.AppendLine();
                                        lastVPosition = vPosition;
                                    }
                                }
                                if (innerNode.ChildNodes.Count == 0)
                                {
                                    pText.Append(innerNode.InnerText);
                                }
                                else
                                {
                                    foreach (XmlNode innerInnerNode in innerNode)
                                    {
                                        if (innerInnerNode.Name == "Font" && innerInnerNode.Attributes["Italic"] != null &&
                                            innerInnerNode.Attributes["Italic"].InnerText.ToLower() == "yes")
                                        {
                                            pText.Append("<i>" + innerInnerNode.InnerText + "</i>");
                                        }
                                        else
                                        {
                                            pText.Append(innerInnerNode.InnerText);
                                        }
                                    }
                                }
                                break;
                            default:
                                pText.Append(innerNode.InnerText);
                                break;
                        }
                    }
                    string start = node.Attributes["TimeIn"].InnerText;
                    string end = node.Attributes["TimeOut"].InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(start), GetTimeCode(end), pText.ToString()));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }

            if (subtitle.Paragraphs.Count > 0)
                subtitle.Header = xml.OuterXml; // save id/language/font for later use

            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCode(string s)
        {
            string[] parts = s.Split(new char[] { ':', '.', ',' });

            int milliseconds = (int)(int.Parse(parts[3]) / 249.0 * 1000); // 000 to 249
            if (milliseconds > 999)
                milliseconds = 999;

            var ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), milliseconds);
            return new TimeCode(ts);
        }

        private static string ConvertToTimeString(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 4);
        }

    }
}


