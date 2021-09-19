using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

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

    public class DCinemaInterop : SubtitleFormat
    {
        internal class SubtitleLine
        {
            internal string Text { get; set; }
            internal string VerticalPosition { get; set; }
            internal string VerticalAlignment { get; set; }

            internal SubtitleLine(string text, string verticalPosition, string verticalAlignment)
            {
                Text = text;
                VerticalPosition = verticalPosition;
                VerticalAlignment = verticalAlignment;
            }

            internal double GetVerticalPositionAsNumber()
            {
                return double.TryParse(VerticalPosition, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d) ? d : 0;
            }
        }

        public override string Extension => ".xml";

        public override string Name => "D-Cinema interop";

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
                    if (xml.DocumentElement != null)
                    {
                        var subtitles = xml.DocumentElement.SelectNodes("//Subtitle");
                        return subtitles != null && subtitles.Count > 0;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string languageEnglishName;
            try
            {
                var languageShortName = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                var ci = CultureInfo.CreateSpecificCulture(languageShortName);
                languageEnglishName = ci.EnglishName;
                var indexOfStartP = languageEnglishName.IndexOf('(');
                if (indexOfStartP > 1)
                {
                    languageEnglishName = languageEnglishName.Remove(indexOfStartP).Trim();
                }
            }
            catch
            {
                languageEnglishName = "English";
            }

            var xmlStructure = "<DCSubtitle Version=\"1.0\">" + Environment.NewLine +
                               "    <SubtitleID>" + GenerateId() + "</SubtitleID>" + Environment.NewLine +
                               "    <MovieTitle></MovieTitle>" + Environment.NewLine +
                               "    <ReelNumber>1</ReelNumber>" + Environment.NewLine +
                               "    <Language>" + languageEnglishName + "</Language>" + Environment.NewLine +
                               "    <LoadFont URI=\"" + Configuration.Settings.SubtitleSettings.DCinemaFontFile + "\" Id=\"Font1\"/>" + Environment.NewLine +
                               "    <Font Id=\"Font1\" Color=\"FFFFFFFF\" Effect=\"border\" EffectColor=\"FF000000\" Italic=\"no\" Underlined=\"no\" Script=\"normal\" Size=\"42\">" + Environment.NewLine +
                               "    </Font>" + Environment.NewLine +
                               "</DCSubtitle>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.PreserveWhitespace = true;

            var ss = Configuration.Settings.SubtitleSettings;
            var loadedFontId = "Font1";
            if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontId))
            {
                loadedFontId = ss.CurrentDCinemaFontId;
            }

            if (string.IsNullOrEmpty(ss.CurrentDCinemaMovieTitle))
            {
                ss.CurrentDCinemaMovieTitle = title;
            }

            if (ss.CurrentDCinemaFontSize == 0 || string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
            {
                Configuration.Settings.SubtitleSettings.InitializeDCinameSettings(true);
            }

            xml.DocumentElement.SelectSingleNode("MovieTitle").InnerText = ss.CurrentDCinemaMovieTitle;

            if (!ss.DCinemaAutoGenerateSubtitleId)
            {
                xml.DocumentElement.SelectSingleNode("SubtitleID").InnerText = ss.CurrentDCinemaSubtitleId.Replace("urn:uuid:", string.Empty);
            }

            xml.DocumentElement.SelectSingleNode("ReelNumber").InnerText = ss.CurrentDCinemaReelNumber;
            xml.DocumentElement.SelectSingleNode("Language").InnerText = ss.CurrentDCinemaLanguage;
            xml.DocumentElement.SelectSingleNode("LoadFont").Attributes["URI"].InnerText = ss.CurrentDCinemaFontUri;
            xml.DocumentElement.SelectSingleNode("LoadFont").Attributes["Id"].InnerText = loadedFontId;
            var fontSize = ss.CurrentDCinemaFontSize;
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Id"].InnerText = loadedFontId;
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Color"].InnerText = "FF" + Utilities.ColorToHex(ss.CurrentDCinemaFontColor).TrimStart('#').ToUpperInvariant();
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Effect"].InnerText = ss.CurrentDCinemaFontEffect;
            xml.DocumentElement.SelectSingleNode("Font").Attributes["EffectColor"].InnerText = "FF" + Utilities.ColorToHex(ss.CurrentDCinemaFontEffectColor).TrimStart('#').ToUpperInvariant();
            xml.DocumentElement.SelectSingleNode("Font").Attributes["Size"].InnerText = ss.CurrentDCinemaFontSize.ToString();

            var mainListFont = xml.DocumentElement.SelectSingleNode("Font");
            var no = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (!string.IsNullOrEmpty(p.Text))
                {
                    var subNode = xml.CreateElement("Subtitle");

                    var id = xml.CreateAttribute("SpotNumber");
                    id.InnerText = (no + 1).ToString();
                    subNode.Attributes.Append(id);

                    var fadeUpTime = xml.CreateAttribute("FadeUpTime");
                    fadeUpTime.InnerText = Configuration.Settings.SubtitleSettings.DCinemaFadeUpTime.ToString();
                    subNode.Attributes.Append(fadeUpTime);

                    var fadeDownTime = xml.CreateAttribute("FadeDownTime");
                    fadeDownTime.InnerText = Configuration.Settings.SubtitleSettings.DCinemaFadeDownTime.ToString();
                    subNode.Attributes.Append(fadeDownTime);

                    var start = xml.CreateAttribute("TimeIn");
                    start.InnerText = ConvertToTimeString(p.StartTime);
                    subNode.Attributes.Append(start);

                    var end = xml.CreateAttribute("TimeOut");
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

                    var text = Utilities.RemoveSsaTags(p.Text);

                    var lines = text.SplitToLines();
                    int vPos;
                    var vPosFactor = (int)Math.Round(fontSize / 7.4);
                    if (alignVTop)
                    {
                        vPos = Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8
                    }
                    else if (alignVCenter)
                    {
                        vPos = (int)Math.Round((lines.Count * vPosFactor * -1) / 2.0);
                    }
                    else
                    {
                        vPos = (lines.Count * vPosFactor) - vPosFactor + Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8
                    }

                    var isItalic = false;
                    var fontNo = 0;
                    var fontColors = new Stack<string>();
                    foreach (var line in lines)
                    {
                        var textNode = xml.CreateElement("Text");

                        var vPosition = xml.CreateAttribute("VPosition");
                        vPosition.InnerText = vPos.ToString();
                        textNode.Attributes.Append(vPosition);

                        if (Math.Abs(Configuration.Settings.SubtitleSettings.DCinemaZPosition) > 0.01)
                        {
                            var zPosition = xml.CreateAttribute("ZPosition");
                            zPosition.InnerText = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Configuration.Settings.SubtitleSettings.DCinemaZPosition);
                            textNode.Attributes.Append(zPosition);
                        }

                        var vAlign = xml.CreateAttribute("VAlign");
                        if (alignVTop)
                        {
                            vAlign.InnerText = "top";
                        }
                        else if (alignVCenter)
                        {
                            vAlign.InnerText = "center";
                        }
                        else
                        {
                            vAlign.InnerText = "bottom";
                        }

                        textNode.Attributes.Append(vAlign);

                        var hAlign = xml.CreateAttribute("HAlign");
                        if (alignLeft)
                        {
                            hAlign.InnerText = "left";
                        }
                        else if (alignRight)
                        {
                            hAlign.InnerText = "right";
                        }
                        else
                        {
                            hAlign.InnerText = "center";
                        }

                        textNode.Attributes.Append(hAlign);

                        var direction = xml.CreateAttribute("Direction");
                        direction.InnerText = "horizontal";
                        textNode.Attributes.Append(direction);

                        var i = 0;
                        var txt = new StringBuilder();
                        var html = new StringBuilder();
                        XmlNode nodeTemp = xml.CreateElement("temp");
                        while (i < line.Length)
                        {
                            if (!isItalic && line.Substring(i).StartsWith("<i>", StringComparison.Ordinal))
                            {
                                if (txt.Length > 0)
                                {
                                    nodeTemp.InnerText = txt.ToString();
                                    html.Append(nodeTemp.InnerXml);
                                    txt.Clear();
                                }
                                isItalic = true;
                                i += 2;
                            }
                            else if (isItalic && line.Substring(i).StartsWith("</i>", StringComparison.Ordinal))
                            {
                                if (txt.Length > 0)
                                {
                                    var fontNode = xml.CreateElement("Font");

                                    var italic = xml.CreateAttribute("Italic");
                                    italic.InnerText = "yes";
                                    fontNode.Attributes.Append(italic);

                                    if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                    {
                                        var fontEffect = xml.CreateAttribute("Effect");
                                        fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                        fontNode.Attributes.Append(fontEffect);
                                    }

                                    if (line.Length > i + 5 && line.Substring(i + 4).StartsWith("</font>", StringComparison.Ordinal))
                                    {
                                        var fontColor = xml.CreateAttribute("Color");
                                        fontColor.InnerText = fontColors.Pop();
                                        fontNode.Attributes.Append(fontColor);
                                        fontNo--;
                                        i += 7;
                                    }

                                    fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt.Clear();
                                }
                                isItalic = false;
                                i += 3;
                            }
                            else if (line.Substring(i).StartsWith("<font color=", StringComparison.Ordinal) && line.Substring(i + 3).Contains('>'))
                            {
                                var endOfFont = line.IndexOf('>', i);
                                if (txt.Length > 0)
                                {
                                    nodeTemp.InnerText = txt.ToString();
                                    html.Append(nodeTemp.InnerXml);
                                    txt.Clear();
                                }
                                var c = GetDCinemaColorString(line.Substring(i + 12, endOfFont - (i + 12)));
                                fontColors.Push(c);
                                fontNo++;
                                i = endOfFont;
                            }
                            else if (fontNo > 0 && line.Substring(i).StartsWith("</font>", StringComparison.Ordinal))
                            {
                                if (txt.Length > 0)
                                {
                                    var fontNode = xml.CreateElement("Font");

                                    var fontColor = xml.CreateAttribute("Color");
                                    fontColor.InnerText = fontColors.Pop();
                                    fontNode.Attributes.Append(fontColor);

                                    if (line.Length > i + 9 && line.Substring(i + 7).StartsWith("</i>", StringComparison.Ordinal))
                                    {
                                        var italic = xml.CreateAttribute("Italic");
                                        italic.InnerText = "yes";
                                        fontNode.Attributes.Append(italic);
                                        isItalic = false;
                                        i += 4;
                                    }

                                    if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                    {
                                        var fontEffect = xml.CreateAttribute("Effect");
                                        fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                        fontNode.Attributes.Append(fontEffect);
                                    }

                                    fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt.Clear();
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
                                var fontNode = xml.CreateElement("Font");

                                var fontColor = xml.CreateAttribute("Color");
                                fontColor.InnerText = fontColors.Peek();
                                fontNode.Attributes.Append(fontColor);

                                if (isItalic)
                                {
                                    var italic = xml.CreateAttribute("Italic");
                                    italic.InnerText = "yes";
                                    fontNode.Attributes.Append(italic);
                                }

                                if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                {
                                    var fontEffect = xml.CreateAttribute("Effect");
                                    fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                    fontNode.Attributes.Append(fontEffect);
                                }

                                fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                html.Append(fontNode.OuterXml);
                            }
                            else if (html.Length > 0 && html.ToString().StartsWith("<Font ", StringComparison.Ordinal))
                            {
                                var temp = new XmlDocument();
                                temp.LoadXml("<root>" + html + "</root>");
                                var fontNode = xml.CreateElement("Font");
                                fontNode.InnerXml = temp.DocumentElement.SelectSingleNode("Font").InnerXml;
                                foreach (XmlAttribute a in temp.DocumentElement.SelectSingleNode("Font").Attributes)
                                {
                                    var newA = xml.CreateAttribute(a.Name);
                                    newA.InnerText = a.InnerText;
                                    fontNode.Attributes.Append(newA);
                                }

                                var fontColor = xml.CreateAttribute("Color");
                                fontColor.InnerText = fontColors.Peek();
                                fontNode.Attributes.Append(fontColor);

                                if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                {
                                    var fontEffect = xml.CreateAttribute("Effect");
                                    fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                    fontNode.Attributes.Append(fontEffect);
                                }

                                html.Clear();
                                html.Append(fontNode.OuterXml);
                            }
                        }
                        else if (isItalic)
                        {
                            if (txt.Length > 0)
                            {
                                var fontNode = xml.CreateElement("Font");

                                var italic = xml.CreateAttribute("Italic");
                                italic.InnerText = "yes";
                                fontNode.Attributes.Append(italic);

                                if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
                                {
                                    var fontEffect = xml.CreateAttribute("Effect");
                                    fontEffect.InnerText = ss.CurrentDCinemaFontEffect;
                                    fontNode.Attributes.Append(fontEffect);
                                }

                                fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
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
                        if (html.Length == 0)
                        {
                            textNode.InnerText = " "; // We need to have at least a single space character on exporting empty subtitles, because otherwise I will get errors on import.ou need to have at least a single space character on exporting empty subtitles, otherwise we will get errors on import.
                        }

                        subNode.AppendChild(textNode);
                        if (alignVTop)
                        {
                            vPos += vPosFactor;
                        }
                        else
                        {
                            vPos -= vPosFactor;
                        }
                    }

                    mainListFont.AppendChild(subNode);
                    no++;
                }
            }

            var s = ToUtf8XmlString(xml).Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"");
            while (s.Contains("</Font>  ") || s.Contains("  <Font ") || s.Contains(Environment.NewLine + "<Font ") || s.Contains("</Font>" + Environment.NewLine))
            {
                while (s.Contains("  Font"))
                {
                    s = s.Replace("  <Font ", " <Font ");
                }

                while (s.Contains("\tFont"))
                {
                    s = s.Replace("\t<Font ", " <Font ");
                }

                s = s.Replace("</Font>  ", "</Font> ");
                s = s.Replace("  <Font ", " <Font ");
                s = s.Replace("\r\n<Font ", "<Font ");
                s = s.Replace("\r\n <Font ", "<Font ");
                s = s.Replace(Environment.NewLine + "<Font ", "<Font ");
                s = s.Replace(Environment.NewLine + " <Font ", "<Font ");
                s = s.Replace("</Font>\r\n", "</Font>");
                s = s.Replace("</Font>" + Environment.NewLine, "</Font>");
                s = s.Replace("><", "> <");
                s = s.Replace("</Font> </Text>", "</Font></Text>");
                s = s.Replace("horizontal\"> <Font", "horizontal\"><Font");
            }
            return s;
        }

        public static string GenerateId()
        {
            return Guid.NewGuid().ToString().RemoveChar('-').Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-").ToLowerInvariant();
        }

        internal static string GetDCinemaColorString(string c)
        {
            c = c.Trim('"').Trim('\'').Trim();
            if (c.StartsWith('#'))
            {
                c = c.TrimStart('#').ToUpperInvariant().PadLeft(8, 'F');
            }
            else
            {
                try
                {
                    var color = ColorTranslator.FromHtml(c);
                    c = "FF" + Utilities.ColorToHex(color).TrimStart('#').ToUpperInvariant();
                }
                catch
                {
                    // ignore error
                }
            }

            return c;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());
            if (xml.DocumentElement == null)
            {
                return;
            }

            var ss = Configuration.Settings.SubtitleSettings;
            try
            {
                ss.InitializeDCinameSettings(false);
                XmlNode node = xml.DocumentElement.SelectSingleNode("SubtitleID");
                if (node != null)
                {
                    ss.CurrentDCinemaSubtitleId = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("ReelNumber");
                if (node != null)
                {
                    ss.CurrentDCinemaReelNumber = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("Language");
                if (node != null)
                {
                    ss.CurrentDCinemaLanguage = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("MovieTitle");
                if (node != null)
                {
                    ss.CurrentDCinemaMovieTitle = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("LoadFont");
                if (node?.Attributes?["URI"] != null)
                {
                    ss.CurrentDCinemaFontUri = node.Attributes["URI"].InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("Font");
                if (node != null)
                {
                    if (node.Attributes?["ID"] != null)
                    {
                        ss.CurrentDCinemaFontId = node.Attributes["ID"].InnerText;
                    }

                    if (node.Attributes?["Size"] != null)
                    {
                        ss.CurrentDCinemaFontSize = Convert.ToInt32(node.Attributes["Size"].InnerText);
                    }

                    if (node.Attributes?["Color"] != null)
                    {
                        ss.CurrentDCinemaFontColor = ColorTranslator.FromHtml("#" + node.Attributes["Color"].InnerText);
                    }

                    if (node.Attributes?["Effect"] != null)
                    {
                        ss.CurrentDCinemaFontEffect = node.Attributes["Effect"].InnerText;
                    }

                    if (node.Attributes?["EffectColor"] != null)
                    {
                        ss.CurrentDCinemaFontEffectColor = System.Drawing.ColorTranslator.FromHtml("#" + node.Attributes["EffectColor"].InnerText);
                    }
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
                    var textLines = new List<SubtitleLine>();
                    var pText = new StringBuilder();
                    var vAlignment = string.Empty;
                    var vPosition = string.Empty;
                    var lastVPosition = string.Empty;
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        if (innerNode.Name == "Text")
                        {
                            if (innerNode.Attributes["VPosition"] != null)
                            {
                                vPosition = innerNode.Attributes["VPosition"].InnerText;
                                var vAlignmentNode = innerNode.Attributes["VAlign"];
                                if (vAlignmentNode != null)
                                {
                                    vAlignment = vAlignmentNode.InnerText;
                                }

                                if (vPosition != lastVPosition)
                                {
                                    if (pText.Length > 0 && lastVPosition.Length > 0)
                                    {
                                        textLines.Add(new SubtitleLine(pText.ToString(), lastVPosition, vAlignment));
                                        pText.Clear();
                                    }

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
                                {
                                    alignLeft = true;
                                }
                                else if (hAlign == "right")
                                {
                                    alignRight = true;
                                }
                            }

                            if (innerNode.Attributes["VAlign"] != null)
                            {
                                string hAlign = innerNode.Attributes["VAlign"].InnerText;
                                if (hAlign == "top")
                                {
                                    alignVTop = true;
                                }
                                else if (hAlign == "center")
                                {
                                    alignVCenter = true;
                                }
                            }

                            if (alignLeft || alignRight || alignVCenter || alignVTop)
                            {
                                if (!pText.ToString().StartsWith("{\\an", StringComparison.Ordinal))
                                {
                                    string pre = string.Empty;
                                    if (alignVTop)
                                    {
                                        if (alignLeft)
                                        {
                                            pre = "{\\an7}";
                                        }
                                        else if (alignRight)
                                        {
                                            pre = "{\\an9}";
                                        }
                                        else
                                        {
                                            pre = "{\\an8}";
                                        }
                                    }
                                    else if (alignVCenter)
                                    {
                                        if (alignLeft)
                                        {
                                            pre = "{\\an4}";
                                        }
                                        else if (alignRight)
                                        {
                                            pre = "{\\an6}";
                                        }
                                        else
                                        {
                                            pre = "{\\an5}";
                                        }
                                    }
                                    else
                                    {
                                        if (alignLeft)
                                        {
                                            pre = "{\\an1}";
                                        }
                                        else if (alignRight)
                                        {
                                            pre = "{\\an3}";
                                        }
                                    }

                                    string temp = pre + pText;
                                    pText.Clear();
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
                                        {
                                            pText.Append("<i><font color=\"" + GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font><i>");
                                        }
                                        else
                                        {
                                            pText.Append("<i>" + innerInnerNode.InnerText + "</i>");
                                        }
                                    }
                                    else if (innerInnerNode.Name == "Font" && innerInnerNode.Attributes["Color"] != null)
                                    {
                                        if (innerInnerNode.Attributes["Italic"] != null && innerInnerNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                        {
                                            pText.Append("<i><font color=\"" + GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font><i>");
                                        }
                                        else
                                        {
                                            pText.Append("<font color=\"" + GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font>");
                                        }
                                    }
                                    else
                                    {
                                        pText.Append(innerInnerNode.InnerText);
                                    }
                                }
                            }
                        }
                        else if (innerNode.Name == "Font")
                        {
                            var italic = innerNode.Name == "Font" &&
                                         innerNode.Attributes["Italic"] != null &&
                                         innerNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase);

                            var pre = string.Empty;
                            if (italic)
                            {
                                pre = "<i>";
                            }

                            foreach (XmlNode innerInnerNode in innerNode)
                            {
                                if (innerInnerNode.Attributes["VPosition"] != null)
                                {
                                    vPosition = innerInnerNode.Attributes["VPosition"].InnerText;
                                    if (vPosition != lastVPosition)
                                    {
                                        if (pText.Length > 0 && lastVPosition.Length > 0)
                                        {
                                            pText.AppendLine();
                                            pText.Append(pre);
                                            pre = string.Empty;
                                        }

                                        lastVPosition = vPosition;
                                    }
                                }

                                pText.Append(pre);
                                pre = string.Empty;
                                pText.Append(innerInnerNode.InnerText);
                            }

                            if (italic)
                            {
                                pText.Append("</i>");
                            }
                        }
                        else
                        {
                            pText.Append(innerNode.InnerText);
                        }
                    }

                    if (pText.Length > 0)
                    {
                        textLines.Add(new SubtitleLine(pText.ToString(), lastVPosition, vAlignment));
                    }

                    string text;
                    if (textLines.All(p => string.Equals(p.VerticalAlignment, "bottom", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        text = string.Join(Environment.NewLine, textLines.OrderByDescending(p => p.GetVerticalPositionAsNumber()).Select(p => p.Text));
                    }
                    else
                    {
                        text = string.Join(Environment.NewLine, textLines.OrderBy(p => p.GetVerticalPositionAsNumber()).Select(p => p.Text));
                    }

                    text = text.Replace(Environment.NewLine + "{\\an1}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an2}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an3}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an4}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an5}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an6}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an7}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an8}", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + "{\\an9}", Environment.NewLine);

                    string start = node.Attributes["TimeIn"].InnerText;
                    string end = node.Attributes["TimeOut"].InnerText;

                    if (node.ParentNode.Name == "Font" && node.ParentNode.Attributes["Italic"] != null && node.ParentNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase) &&
                        !text.Contains("<i>"))
                    {
                        if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6)
                        {
                            text = text.Insert(6, "<i>") + "</i>";
                        }
                        else
                        {
                            text = "<i>" + text + "</i>";
                        }
                    }

                    subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(start), GetTimeCode(end), HtmlUtil.FixInvalidItalicTags(text)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }

            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Header = xml.OuterXml; // save id/language/font for later use
            }

            subtitle.Renumber();
        }

        internal static string GetColorStringFromDCinema(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            var hex = s.TrimStart('#');
            for (var i = s.Length - 1; i >= 0; i--)
            {
                if (!CharUtils.IsHexadecimal(s[i]))
                {
                    return s;
                }
            }

            return "#" + hex;
        }

        private static TimeCode GetTimeCode(string s)
        {
            var parts = s.Split(':', '.', ',');

            var milliseconds = int.Parse(parts[3]) * 4; // 000 to 249
            if (s.Contains('.'))
            {
                milliseconds = int.Parse(parts[3].PadRight(3, '0'));
            }

            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), milliseconds);
        }

        public static string ConvertToTimeString(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds / 4:000}";
        }
    }
}