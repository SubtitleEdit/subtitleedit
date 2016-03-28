using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    // https://github.com/SubtitleEdit/subtitleedit/issues/detail?id=18
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

    public class DCSubtitle : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "D-Cinema interop"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<DCSubtitle"))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(xmlAsString);

                    var subtitles = xml.DocumentElement.SelectNodes("//Subtitle");
                    return subtitles != null && subtitles.Count > 0;
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

        private static string RemoveSubStationAlphaFormatting(string s)
        {
            return Utilities.RemoveSsaTags(s);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string languageEnglishName;
            try
            {
                string languageShortName = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                var ci = CultureInfo.CreateSpecificCulture(languageShortName);
                languageEnglishName = ci.EnglishName;
                int indexOfStartP = languageEnglishName.IndexOf('(');
                if (indexOfStartP > 1)
                    languageEnglishName = languageEnglishName.Remove(indexOfStartP).Trim();
            }
            catch
            {
                languageEnglishName = "English";
            }

            string hex = Guid.NewGuid().ToString().Replace("-", string.Empty);
            hex = hex.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");

            string xmlStructure = "<DCSubtitle Version=\"1.0\">" + Environment.NewLine +
                                    "    <SubtitleID>" + hex.ToLower() + "</SubtitleID>" + Environment.NewLine +
                                    "    <MovieTitle></MovieTitle>" + Environment.NewLine +
                                    "    <ReelNumber>1</ReelNumber>" + Environment.NewLine +
                                    "    <Language>" + languageEnglishName + "</Language>" + Environment.NewLine +
                                    "    <LoadFont URI=\"" + Configuration.Settings.SubtitleSettings.DCinemaFontFile + "\" Id=\"Font1\"/>" + Environment.NewLine +
                                    "    <Font Id=\"Font1\" Color=\"FFFFFFFF\" Effect=\"border\" EffectColor=\"FF000000\" Italic=\"no\" Underlined=\"no\" Script=\"normal\" Size=\"42\">" + Environment.NewLine +
                                    "    </Font>" + Environment.NewLine +
                                    "</DCSubtitle>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.PreserveWhitespace = true;

            var ss = Configuration.Settings.SubtitleSettings;
            string loadedFontId = "Font1";
            if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontId))
                loadedFontId = ss.CurrentDCinemaFontId;

            if (string.IsNullOrEmpty(ss.CurrentDCinemaMovieTitle))
                ss.CurrentDCinemaMovieTitle = title;

            if (ss.CurrentDCinemaFontSize == 0 || string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                Configuration.Settings.SubtitleSettings.InitializeDCinameSettings(true);

            xml.DocumentElement.SelectSingleNode("MovieTitle").InnerText = ss.CurrentDCinemaMovieTitle;
            xml.DocumentElement.SelectSingleNode("SubtitleID").InnerText = ss.CurrentDCinemaSubtitleId.Replace("urn:uuid:", string.Empty);
            xml.DocumentElement.SelectSingleNode("ReelNumber").InnerText = ss.CurrentDCinemaReelNumber;
            xml.DocumentElement.SelectSingleNode("Language").InnerText = ss.CurrentDCinemaLanguage;
            xml.DocumentElement.SelectSingleNode("LoadFont").Attributes["URI"].InnerText = ss.CurrentDCinemaFontUri;
            xml.DocumentElement.SelectSingleNode("LoadFont").Attributes["Id"].InnerText = loadedFontId;
            int fontSize = ss.CurrentDCinemaFontSize;
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Id"].InnerText = loadedFontId;
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Color"].InnerText = "FF" + Utilities.ColorToHex(ss.CurrentDCinemaFontColor).TrimStart('#').ToUpper();
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Effect"].InnerText = ss.CurrentDCinemaFontEffect;
            xml.DocumentElement.SelectSingleNode("Font").Attributes["EffectColor"].InnerText = "FF" + Utilities.ColorToHex(ss.CurrentDCinemaFontEffectColor).TrimStart('#').ToUpper();
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Size"].InnerText = ss.CurrentDCinemaFontSize.ToString();

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
                    fadeUpTime.InnerText = Configuration.Settings.SubtitleSettings.DCinemaFadeUpTime.ToString();
                    subNode.Attributes.Append(fadeUpTime);

                    XmlAttribute fadeDownTime = xml.CreateAttribute("FadeDownTime");
                    fadeDownTime.InnerText = Configuration.Settings.SubtitleSettings.DCinemaFadeDownTime.ToString();
                    subNode.Attributes.Append(fadeDownTime);

                    XmlAttribute start = xml.CreateAttribute("TimeIn");
                    start.InnerText = ConvertToTimeString(p.StartTime);
                    subNode.Attributes.Append(start);

                    XmlAttribute end = xml.CreateAttribute("TimeOut");
                    end.InnerText = ConvertToTimeString(p.EndTime);
                    subNode.Attributes.Append(end);

                    bool alignLeft = p.Text.StartsWith("{\\a1}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a9}", StringComparison.Ordinal) || // sub station alpha
                                    p.Text.StartsWith("{\\an1}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an7}", StringComparison.Ordinal); // advanced sub station alpha

                    bool alignRight = p.Text.StartsWith("{\\a3}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a11}", StringComparison.Ordinal) || // sub station alpha
                                      p.Text.StartsWith("{\\an3}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an6}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal); // advanced sub station alpha

                    bool alignVTop = p.Text.StartsWith("{\\a5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a6}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a7}", StringComparison.Ordinal) || // sub station alpha
                                    p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an8}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal); // advanced sub station alpha

                    bool alignVCenter = p.Text.StartsWith("{\\a9}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a10}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a11}", StringComparison.Ordinal) || // sub station alpha
                                      p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an6}", StringComparison.Ordinal); // advanced sub station alpha

                    // remove styles for display text (except italic)
                    string text = RemoveSubStationAlphaFormatting(p.Text);

                    var lines = text.SplitToLines();
                    int vPos = 1 + lines.Length * 7;
                    int vPosFactor = (int)Math.Round(fontSize / 7.4);
                    if (alignVTop)
                    {
                        vPos = Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8
                    }
                    else if (alignVCenter)
                    {
                        vPos = (int)Math.Round((lines.Length * vPosFactor * -1) / 2.0);
                    }
                    else
                    {
                        vPos = (lines.Length * vPosFactor) - vPosFactor + Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8
                    }

                    bool isItalic = false;
                    int fontNo = 0;
                    Stack<string> fontColors = new Stack<string>();
                    foreach (string line in lines)
                    {
                        XmlNode textNode = xml.CreateElement("Text");

                        XmlAttribute vPosition = xml.CreateAttribute("VPosition");
                        vPosition.InnerText = vPos.ToString();
                        textNode.Attributes.Append(vPosition);

                        if (Configuration.Settings.SubtitleSettings.DCinemaZPosition != 0)
                        {
                            XmlAttribute zPosition = xml.CreateAttribute("ZPosition");
                            zPosition.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Configuration.Settings.SubtitleSettings.DCinemaZPosition);
                            textNode.Attributes.Append(zPosition);
                        }

                        XmlAttribute vAlign = xml.CreateAttribute("VAlign");
                        if (alignVTop)
                            vAlign.InnerText = "top";
                        else if (alignVCenter)
                            vAlign.InnerText = "center";
                        else
                            vAlign.InnerText = "bottom";
                        textNode.Attributes.Append(vAlign);

                        XmlAttribute hAlign = xml.CreateAttribute("HAlign");
                        if (alignLeft)
                            hAlign.InnerText = "left";
                        else if (alignRight)
                            hAlign.InnerText = "right";
                        else
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

                                    if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                    {
                                        XmlAttribute fontEffect = xml.CreateAttribute("Effect");
                                        fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                        fontNode.Attributes.Append(fontEffect);
                                    }

                                    if (line.Length > i + 5 && line.Substring(i + 4).StartsWith("</font>"))
                                    {
                                        XmlAttribute fontColor = xml.CreateAttribute("Color");
                                        fontColor.InnerText = fontColors.Pop();
                                        fontNode.Attributes.Append(fontColor);
                                        fontNo--;
                                        i += 7;
                                    }

                                    fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt = new StringBuilder();
                                }
                                isItalic = false;
                                i += 3;
                            }
                            else if (line.Substring(i).StartsWith("<font color=") && line.Substring(i + 3).Contains('>'))
                            {
                                int endOfFont = line.IndexOf('>', i);
                                if (txt.Length > 0)
                                {
                                    nodeTemp.InnerText = txt.ToString();
                                    html.Append(nodeTemp.InnerXml);
                                    txt = new StringBuilder();
                                }
                                string c = line.Substring(i + 12, endOfFont - (i + 12));
                                c = c.Trim('"').Trim('\'').Trim();
                                if (c.StartsWith('#'))
                                    c = c.TrimStart('#').ToUpper().PadLeft(8, 'F');
                                fontColors.Push(c);
                                fontNo++;
                                i += endOfFont - i;
                            }
                            else if (fontNo > 0 && line.Substring(i).StartsWith("</font>"))
                            {
                                if (txt.Length > 0)
                                {
                                    XmlNode fontNode = xml.CreateElement("Font");

                                    XmlAttribute fontColor = xml.CreateAttribute("Color");
                                    fontColor.InnerText = fontColors.Pop();
                                    fontNode.Attributes.Append(fontColor);

                                    if (line.Length > i + 9 && line.Substring(i + 7).StartsWith("</i>"))
                                    {
                                        XmlAttribute italic = xml.CreateAttribute("Italic");
                                        italic.InnerText = "yes";
                                        fontNode.Attributes.Append(italic);
                                        isItalic = false;
                                        i += 4;
                                    }

                                    if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                    {
                                        XmlAttribute fontEffect = xml.CreateAttribute("Effect");
                                        fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                        fontNode.Attributes.Append(fontEffect);
                                    }

                                    fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt = new StringBuilder();
                                }
                                fontNo--;
                                i += 6;
                            }
                            else
                            {
                                txt.Append(line[i]);
                            }
                            i++;
                        }
                        if (fontNo > 0)
                        {
                            if (txt.Length > 0)
                            {
                                XmlNode fontNode = xml.CreateElement("Font");

                                XmlAttribute fontColor = xml.CreateAttribute("Color");
                                fontColor.InnerText = fontColors.Peek();
                                fontNode.Attributes.Append(fontColor);

                                if (isItalic)
                                {
                                    XmlAttribute italic = xml.CreateAttribute("Italic");
                                    italic.InnerText = "yes";
                                    fontNode.Attributes.Append(italic);
                                }

                                if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                {
                                    XmlAttribute fontEffect = xml.CreateAttribute("Effect");
                                    fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                    fontNode.Attributes.Append(fontEffect);
                                }

                                fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                html.Append(fontNode.OuterXml);
                            }
                            else if (html.Length > 0 && html.ToString().StartsWith("<Font "))
                            {
                                XmlDocument temp = new XmlDocument();
                                temp.LoadXml("<root>" + html + "</root>");
                                XmlNode fontNode = xml.CreateElement("Font");
                                fontNode.InnerXml = temp.DocumentElement.SelectSingleNode("Font").InnerXml;
                                foreach (XmlAttribute a in temp.DocumentElement.SelectSingleNode("Font").Attributes)
                                {
                                    XmlAttribute newA = xml.CreateAttribute(a.Name);
                                    newA.InnerText = a.InnerText;
                                    fontNode.Attributes.Append(newA);
                                }

                                XmlAttribute fontColor = xml.CreateAttribute("Color");
                                fontColor.InnerText = fontColors.Peek();
                                fontNode.Attributes.Append(fontColor);

                                if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                {
                                    XmlAttribute fontEffect = xml.CreateAttribute("Effect");
                                    fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                    fontNode.Attributes.Append(fontEffect);
                                }

                                html = new StringBuilder();
                                html.Append(fontNode.OuterXml);
                            }
                        }
                        else if (isItalic)
                        {
                            if (txt.Length > 0)
                            {
                                XmlNode fontNode = xml.CreateElement("Font");

                                XmlAttribute italic = xml.CreateAttribute("Italic");
                                italic.InnerText = "yes";
                                fontNode.Attributes.Append(italic);

                                if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                {
                                    XmlAttribute fontEffect = xml.CreateAttribute("Effect");
                                    fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                    fontNode.Attributes.Append(fontEffect);
                                }

                                fontNode.InnerText = HtmlUtil.RemoveHtmlTags(line);
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
                        if (alignVTop)
                            vPos += vPosFactor;
                        else
                            vPos -= vPosFactor;
                    }

                    mainListFont.AppendChild(subNode);
                    no++;
                }
            }
            string s = ToUtf8XmlString(xml).Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"");
            while (s.Contains("</Font>  ") || s.Contains("  <Font ") || s.Contains(Environment.NewLine + "<Font ") || s.Contains("</Font>" + Environment.NewLine))
            {
                while (s.Contains("  Font"))
                    s = s.Replace("  <Font ", " <Font ");
                while (s.Contains("\tFont"))
                    s = s.Replace("\t<Font ", " <Font ");

                s = s.Replace("</Font>  ", "</Font> ");
                s = s.Replace("  <Font ", " <Font ");
                s = s.Replace(Environment.NewLine + "<Font ", "<Font ");
                s = s.Replace(Environment.NewLine + " <Font ", "<Font ");
                s = s.Replace("</Font>" + Environment.NewLine, "</Font>");
                s = s.Replace("><", "> <");
            }
            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());

            var ss = Configuration.Settings.SubtitleSettings;
            try
            {
                ss.InitializeDCinameSettings(false);
                XmlNode node = xml.DocumentElement.SelectSingleNode("SubtitleID");
                if (node != null)
                    ss.CurrentDCinemaSubtitleId = node.InnerText;

                node = xml.DocumentElement.SelectSingleNode("ReelNumber");
                if (node != null)
                    ss.CurrentDCinemaReelNumber = node.InnerText;

                node = xml.DocumentElement.SelectSingleNode("Language");
                if (node != null)
                    ss.CurrentDCinemaLanguage = node.InnerText;

                node = xml.DocumentElement.SelectSingleNode("MovieTitle");
                if (node != null)
                    ss.CurrentDCinemaMovieTitle = node.InnerText;

                node = xml.DocumentElement.SelectSingleNode("LoadFont");
                if (node != null)
                {
                    if (node.Attributes["URI"] != null)
                        ss.CurrentDCinemaFontUri = node.Attributes["URI"].InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("Font");
                if (node != null)
                {
                    if (node.Attributes["ID"] != null)
                        ss.CurrentDCinemaFontId = node.Attributes["ID"].InnerText;
                    if (node.Attributes["Size"] != null)
                        ss.CurrentDCinemaFontSize = Convert.ToInt32(node.Attributes["Size"].InnerText);
                    if (node.Attributes["Color"] != null)
                        ss.CurrentDCinemaFontColor = System.Drawing.ColorTranslator.FromHtml("#" + node.Attributes["Color"].InnerText);
                    if (node.Attributes["Effect"] != null)
                        ss.CurrentDCinemaFontEffect = node.Attributes["Effect"].InnerText;
                    if (node.Attributes["EffectColor"] != null)
                        ss.CurrentDCinemaFontEffectColor = System.Drawing.ColorTranslator.FromHtml("#" + node.Attributes["EffectColor"].InnerText);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//Subtitle"))
            {
                try
                {
                    var pText = new StringBuilder();
                    string lastVPosition = string.Empty;
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name)
                        {
                            case "Text":
                                if (innerNode.Attributes["VPosition"] != null)
                                {
                                    string vPosition = innerNode.Attributes["VPosition"].InnerText;
                                    if (vPosition != lastVPosition)
                                    {
                                        if (pText.Length > 0 && lastVPosition.Length > 0)
                                            pText.AppendLine();
                                        lastVPosition = vPosition;
                                    }
                                }
                                bool alignLeft = false;
                                bool alignRight = false;
                                bool alignVTop = false;
                                bool alignVCenter = false;
                                if (innerNode.Attributes["HAlign"] != null)
                                {
                                    string hAlign = innerNode.Attributes["HAlign"].InnerText;
                                    if (hAlign == "left")
                                        alignLeft = true;
                                    else if (hAlign == "right")
                                        alignRight = true;
                                }
                                if (innerNode.Attributes["VAlign"] != null)
                                {
                                    string hAlign = innerNode.Attributes["VAlign"].InnerText;
                                    if (hAlign == "top")
                                        alignVTop = true;
                                    else if (hAlign == "center")
                                        alignVCenter = true;
                                }
                                if (alignLeft || alignRight || alignVCenter || alignVTop)
                                {
                                    if (!pText.ToString().StartsWith("{\\an"))
                                    {
                                        string pre = string.Empty;
                                        if (alignVTop)
                                        {
                                            if (alignLeft)
                                                pre = "{\\an7}";
                                            else if (alignRight)
                                                pre = "{\\an9}";
                                            else
                                                pre = "{\\an8}";
                                        }
                                        else if (alignVCenter)
                                        {
                                            if (alignLeft)
                                                pre = "{\\an4}";
                                            else if (alignRight)
                                                pre = "{\\an6}";
                                            else
                                                pre = "{\\an5}";
                                        }
                                        else
                                        {
                                            if (alignLeft)
                                                pre = "{\\an1}";
                                            else if (alignRight)
                                                pre = "{\\an3}";
                                        }
                                        string temp = pre + pText;
                                        pText = new StringBuilder();
                                        pText.Append(temp);
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
                                           innerInnerNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (innerInnerNode.Attributes["Color"] != null)
                                                pText.Append("<i><font color=\"" + GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font><i>");
                                            else
                                                pText.Append("<i>" + innerInnerNode.InnerText + "</i>");
                                        }
                                        else if (innerInnerNode.Name == "Font" && innerInnerNode.Attributes["Color"] != null)
                                        {
                                            if (innerInnerNode.Attributes["Italic"] != null && innerInnerNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                                pText.Append("<i><font color=\"" + GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font><i>");
                                            else
                                                pText.Append("<font color=\"" + GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font>");
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

                    if (node.ParentNode.Name == "Font" && node.ParentNode.Attributes["Italic"] != null && node.ParentNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase) &&
                        !pText.ToString().Contains("<i>"))
                    {
                        string text = pText.ToString();
                        if (text.StartsWith("{\\an") && text.Length > 6)
                            text = text.Insert(6, "<i>") + "</i>";
                        else
                            text = "<i>" + text + "</i>";
                        pText = new StringBuilder(text);
                    }

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

            subtitle.Renumber();
        }

        private static string GetColorStringForDCinema(string p)
        {
            string s = p.ToUpper().Trim();
            if (s.Replace("#", string.Empty).
                Replace("0", string.Empty).
                Replace("1", string.Empty).
                Replace("2", string.Empty).
                Replace("3", string.Empty).
                Replace("4", string.Empty).
                Replace("5", string.Empty).
                Replace("6", string.Empty).
                Replace("7", string.Empty).
                Replace("8", string.Empty).
                Replace("9", string.Empty).
                Replace("A", string.Empty).
                Replace("B", string.Empty).
                Replace("C", string.Empty).
                Replace("D", string.Empty).
                Replace("E", string.Empty).
                Replace("F", string.Empty).Length == 0)
            {
                return s.TrimStart('#');
            }
            else
            {
                return p;
            }
        }

        private static string GetColorStringFromDCinema(string p)
        {
            string s = p.ToLower().Trim();
            if (s.Replace("#", string.Empty).
                Replace("0", string.Empty).
                Replace("1", string.Empty).
                Replace("2", string.Empty).
                Replace("3", string.Empty).
                Replace("4", string.Empty).
                Replace("5", string.Empty).
                Replace("6", string.Empty).
                Replace("7", string.Empty).
                Replace("8", string.Empty).
                Replace("9", string.Empty).
                Replace("a", string.Empty).
                Replace("b", string.Empty).
                Replace("c", string.Empty).
                Replace("d", string.Empty).
                Replace("e", string.Empty).
                Replace("f", string.Empty).Length == 0)
            {
                if (s.StartsWith('#'))
                    return s;
                else
                    return "#" + s;
            }
            else
            {
                return p;
            }
        }

        private static TimeCode GetTimeCode(string s)
        {
            string[] parts = s.Split(new char[] { ':', '.', ',' });

            int milliseconds = (int)(int.Parse(parts[3]) * 4); // 000 to 249
            if (s.Contains('.'))
                milliseconds = int.Parse(parts[3].PadRight(3, '0'));
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), milliseconds);
        }

        public static string ConvertToTimeString(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:000}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 4);
        }

    }
}
