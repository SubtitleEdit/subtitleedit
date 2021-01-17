using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://www.fab-online.com/pdf/ESUB-XF.pdf
    /// </summary>
    public class ESubXf : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "ESUB-XF";

        private const string NameSpaceUri = "urn:esub-xf";

        private static readonly Dictionary<string, string> ColorDictionary = new Dictionary<string, string>
        {
            {"white", "FFFFFF"},
            {"green", "72FD59"},
            {"cyan", "91FFFF"},
            {"purple", "F55FF5"},
            {"red", "FF2D34"},
            {"blue", "4545FF"},
            {"yellow", "E8E858"},
            {"violet", "8505FD"}
        };

        public override string ToText(Subtitle subtitle, string title)
        {
            string threeLetterLanguage;
            string languageDisplay;
            try
            {
                var ci = new CultureInfo(LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle));
                threeLetterLanguage = ci.GetThreeLetterIsoLanguageName();
                languageDisplay = ci.EnglishName;
            }
            catch
            {
                threeLetterLanguage = "eng";
                languageDisplay = "English";
            }

            string xmlStructure =
               "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                "<esub-xf xmlns=\"" + NameSpaceUri + "\" framerate=\"" + Configuration.Settings.General.CurrentFrameRate.ToString(CultureInfo.InvariantCulture) + "\" timebase=\"smpte\">" + Environment.NewLine +
                "  <subtitlelist language=\"" + threeLetterLanguage + "\" langname=\"" + languageDisplay + "\" type=\"translation\">" + Environment.NewLine +
                "  </subtitlelist>" + Environment.NewLine +
                "</esub-xf>";

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("esub-xf", NameSpaceUri);
            var subtitleList = xml.DocumentElement.SelectSingleNode("//esub-xf:subtitlelist", ns);
            foreach (var p in subtitle.Paragraphs)
            {
                var text = p.Text;
                var paragraph = xml.CreateElement("subtitle", NameSpaceUri);

                var start = xml.CreateAttribute("display");
                start.InnerText = p.StartTime.ToHHMMSSFF();
                paragraph.Attributes.Append(start);

                var end = xml.CreateAttribute("clear");
                end.InnerText = p.EndTime.ToHHMMSSFF();
                paragraph.Attributes.Append(end);

                var hRegion = xml.CreateElement("hregion", NameSpaceUri);
                if (text.StartsWith("{\\an7}") || p.Text.StartsWith("{\\an8}") || p.Text.StartsWith("{\\an9}"))
                {
                    var vpos = xml.CreateAttribute("vposition");
                    vpos.Value = "top";
                    hRegion.Attributes.Append(vpos);
                }
                paragraph.AppendChild(hRegion);

                text = Utilities.RemoveSsaTags(text);
                if (text.Contains("<i>", StringComparison.OrdinalIgnoreCase) || text.Contains("<b>", StringComparison.OrdinalIgnoreCase) || text.Contains("<font", StringComparison.OrdinalIgnoreCase))
                {
                    GenerateLineWithSpan(text, xml, hRegion);
                }
                else
                {
                    foreach (var line in text.SplitToLines())
                    {
                        var lineNode = xml.CreateElement("line", NameSpaceUri);
                        hRegion.AppendChild(lineNode);
                        lineNode.InnerText = line;
                    }
                }
                subtitleList.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml);
        }

        private static void GenerateLineWithSpan(string text, XmlDocument xml, XmlElement hRegion)
        {
            bool italicOn = false;
            bool boldOn = false;
            bool underlineOn = false;
            var currentColor = string.Empty;
            var currentText = new StringBuilder();
            foreach (var line in text.SplitToLines())
            {
                var lineNode = xml.CreateElement("line", NameSpaceUri);
                hRegion.AppendChild(lineNode);
                int i = 0;
                while (i < line.Length)
                {
                    if (line.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        italicOn = true;
                        i += 3;
                    }
                    else if (line.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        italicOn = false;
                        i += 4;
                    }
                    else if (line.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        boldOn = true;
                        i += 3;
                    }
                    else if (line.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        boldOn = false;
                        i += 4;
                    }
                    else if (line.Substring(i).StartsWith("<u>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        underlineOn = true;
                        i += 3;
                    }
                    else if (line.Substring(i).StartsWith("</u>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        underlineOn = false;
                        i += 4;
                    }
                    else if (line.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        currentColor = string.Empty;
                        var end = line.IndexOf('>', i);
                        if (end > 0)
                        {
                            string tag = line.Substring(i, end - i + 1);
                            if (tag.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                            {
                                int colorStart = tag.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                                int colorEnd = tag.IndexOf('"', colorStart + " color=".Length + 1);
                                if (colorEnd > 0)
                                {
                                    string color = tag.Substring(colorStart, colorEnd - colorStart);
                                    color = color.Remove(0, " color=".Length);
                                    color = color.Trim('"');
                                    color = color.Trim('\'');
                                    currentColor = color;
                                }
                            }
                            i = end + 1;
                        }
                        else
                        {
                            i = int.MaxValue;
                        }
                    }
                    else if (line.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
                        currentColor = string.Empty;
                        i += 7;
                    }
                    else
                    {
                        currentText.Append(line[i]);
                        i++;
                    }
                }
                AppendText(currentText, italicOn, boldOn, underlineOn, currentColor, xml, lineNode);
            }
        }

        private static void AppendText(StringBuilder currentText, bool italicOn, bool boldOn, bool underlineOn, string currentColor, XmlDocument xml, XmlElement lineNode)
        {
            if (currentText.Length == 0)
            {
                return;
            }

            var span = xml.CreateElement("span", NameSpaceUri);
            if (!string.IsNullOrEmpty(currentColor))
            {
                var textColor = xml.CreateAttribute("textcolor");
                textColor.Value = GetAllowedColor(currentColor);
                span.Attributes.Append(textColor);
            }
            if (italicOn)
            {
                var italic = xml.CreateAttribute("italic");
                italic.Value = "on";
                span.Attributes.Append(italic);
            }
            if (boldOn)
            {
                var bold = xml.CreateAttribute("bold");
                bold.Value = "on";
                span.Attributes.Append(bold);
            }
            if (underlineOn)
            {
                var underline = xml.CreateAttribute("underline");
                underline.Value = "on";
                span.Attributes.Append(underline);
            }

            span.InnerText = currentText.ToString();
            lineNode.AppendChild(span);
            currentText.Clear();
        }

        private static string GetAllowedColor(string c)
        {
            if (ColorDictionary.ContainsKey(c))
            {
                return c;
            }

            try
            {
                var color = ColorTranslator.FromHtml(c);
                var minDiff = 1000;
                var minDiffColor = string.Empty;
                int index = 0;
                foreach (var kvp in ColorDictionary)
                {
                    var cd = ColorTranslator.FromHtml("#" + kvp.Value);
                    int difference = Math.Abs(Math.Abs(cd.R - color.R) + Math.Abs(cd.G - color.G) + Math.Abs(cd.B - color.B));
                    if (difference < minDiff)
                    {
                        minDiffColor = kvp.Key;
                        minDiff = difference;
                    }
                    index++;
                }
                return minDiffColor;
            }
            catch
            {
                return string.Empty;
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlString = sb.ToString();
            if (!xmlString.Contains("<subtitlelist"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            subtitle.Header = xmlString;
            char[] timeCodeSeparators = { ':' };
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("esub-xf", NameSpaceUri);
            var isTimebaseSmtp = xml.DocumentElement.Attributes["timebase"]?.Value != "msec"; // "msec" or "smtp"
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//esub-xf:subtitlelist/esub-xf:subtitle", ns))
            {
                try
                {
                    string start = node.Attributes["display"].InnerText;
                    string end = node.Attributes["clear"].InnerText;
                    sb = new StringBuilder();
                    var hregion = node.SelectSingleNode("esub-xf:hregion", ns);
                    var topAlign = hregion.Attributes["vposition"]?.Value == "top";
                    foreach (XmlNode lineNode in node.SelectNodes("esub-xf:hregion/esub-xf:line", ns))
                    {
                        var spanNodes = lineNode.SelectNodes("esub-xf:span", ns);
                        if (spanNodes.Count > 0)
                        {
                            var first = true;
                            foreach (XmlNode spanChild in spanNodes)
                            {
                                if (!first)
                                {
                                    sb.Append(" "); // put space between all spans
                                }
                                sb.Append(GetTextWithStyle(spanChild.InnerText, spanChild.Attributes["italic"]?.Value == "on", spanChild.Attributes["bold"]?.Value == "on", spanChild.Attributes["underline"]?.Value == "on", GetColor(spanChild)));
                                first = false;
                            }
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine(lineNode.InnerText);
                        }
                    }
                    var text = sb.ToString().Trim();
                    text = HtmlUtil.FixInvalidItalicTags(text);
                    if (topAlign)
                    {
                        text = "{\\an8}" + text;
                    }
                    subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCodeFrames(start, timeCodeSeparators, isTimebaseSmtp), DecodeTimeCodeFrames(end, timeCodeSeparators, isTimebaseSmtp), text));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private string GetTextWithStyle(string text, bool italic, bool bold, bool underline, string color)
        {
            if (italic)
            {
                text = "<i>" + text + "</i>";
            }
            if (bold)
            {
                text = "<b>" + text + "</b>";
            }
            if (underline)
            {
                text = "<u>" + text + "</u>";
            }

            if (string.IsNullOrEmpty(color))
            {
                return text;
            }
            return "<font color=\"" + color + "\">" + text + "</font>";
        }

        private string GetColor(XmlNode spanChild)
        {
            if (spanChild.Attributes?["textcolor"] == null)
            {
                return null;
            }

            var colorName = spanChild.Attributes["textcolor"].InnerText;
            if (colorName == "white")
            {
                return null;
            }
            return ColorDictionary.ContainsKey(colorName) ? colorName : null;
        }

        protected static TimeCode DecodeTimeCodeFrames(string timestamp, char[] splitChars, bool isTimebaseSmtp)
        {
            if (isTimebaseSmtp)
            {
                return DecodeTimeCodeFramesFourParts(timestamp.Split(splitChars, StringSplitOptions.RemoveEmptyEntries));
            }
            return new TimeCode(Convert.ToInt32(timestamp));
        }
    }
}
