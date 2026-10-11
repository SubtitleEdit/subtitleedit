using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Crappy format... should always be saved as UTF-8 without BOM (hacked Main.cs) and <br /> tags should be oldstyle <br/>
    /// </summary>
    public class ItunesTimedText : TimedText10
    {
        public override string Extension => ".itt";

        public new const string NameOfFormat = "iTunes Timed Text";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (new NetflixTimedText().IsMine(lines, fileName))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            XmlNode styleHead = null;
            var convertedFromSubStationAlpha = false;
            if (subtitle.Header != null)
            {
                try
                {
                    var x = new XmlDocument();
                    x.LoadXml(subtitle.Header);
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
                }
                catch
                {
                    styleHead = null;
                }
                if (styleHead == null && (subtitle.Header.Contains("[V4+ Styles]") || subtitle.Header.Contains("[V4 Styles]")))
                {
                    var x = new XmlDocument();
                    x.LoadXml(new ItunesTimedText().ToText(new Subtitle(), "tt")); // load default xml
                    var xnsmgr = new XmlNamespaceManager(x.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    styleHead = x.DocumentElement.SelectSingleNode("ttml:head", xnsmgr);
                    styleHead.SelectSingleNode("ttml:styling", xnsmgr).RemoveAll();
                    foreach (string styleName in AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header))
                    {
                        try
                        {
                            var ssaStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitle.Header);

                            var fontStyle = "normal";
                            if (ssaStyle.Italic)
                            {
                                fontStyle = "italic";
                            }

                            var fontWeight = "normal";
                            if (ssaStyle.Bold)
                            {
                                fontWeight = "bold";
                            }

                            AddStyleToXml(x, styleHead, xnsmgr, ssaStyle.Name, ssaStyle.FontName, fontWeight, fontStyle, Utilities.ColorToHex(ssaStyle.Primary), ssaStyle.FontSize.ToString());
                            convertedFromSubStationAlpha = true;

                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    subtitle.Header = x.OuterXml; // save new xml with styles in header
                }
            }

            var xml = new XmlDocument { XmlResolver = null };
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/ttml#styling");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");

            var frameRate = ((int)Math.Round(Configuration.Settings.General.CurrentFrameRate)).ToString();
            var frameRateMultiplier = "999 1000";
            if (Configuration.Settings.General.CurrentFrameRate % 1.0 < 0.01)
            {
                frameRateMultiplier = "1 1";
            }
            var dropMode = "nonDrop";
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 29.97) < 0.01)
            {
                dropMode = "dropNTSC";
            }

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            if (!string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.TimedTextItunesLanguage))
            {
                language = Configuration.Settings.SubtitleSettings.TimedTextItunesLanguage;
            }

            var xmlStructure = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + Environment.NewLine +
            "<tt xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tt=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xml:lang=\"" + language + "\" ttp:timeBase=\"smpte\" ttp:frameRate=\"" + frameRate + "\" ttp:frameRateMultiplier=\"" + frameRateMultiplier + "\" ttp:dropMode=\"" + dropMode + "\">" + Environment.NewLine +
            "   <head>" + Environment.NewLine +
            "       <styling>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"white\" tts:fontStyle=\"normal\" tts:fontWeight=\"normal\" tts:fontFamily=\"sansSerif\" xml:id=\"normal\"/>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"white\" tts:fontStyle=\"normal\" tts:fontWeight=\"bold\" tts:fontFamily=\"sansSerif\" xml:id=\"bold\"/>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"white\" tts:fontStyle=\"italic\" tts:fontWeight=\"normal\" tts:fontFamily=\"sansSerif\" xml:id=\"italic\"/>" + Environment.NewLine +
            "      </styling>" + Environment.NewLine +
            "      <layout>" + Environment.NewLine +
            "        <region xml:id=\"top\" tts:origin=\"" + Configuration.Settings.SubtitleSettings.TimedTextItunesTopOrigin + "\" tts:extent=\"" + Configuration.Settings.SubtitleSettings.TimedTextItunesTopExtent + "\" tts:textAlign=\"center\" tts:displayAlign=\"before\"/>" + Environment.NewLine +
            "        <region xml:id=\"bottom\" tts:origin=\"" + Configuration.Settings.SubtitleSettings.TimedTextItunesBottomOrigin + "\" tts:extent=\"" + Configuration.Settings.SubtitleSettings.TimedTextItunesBottomExtent + "\" tts:textAlign=\"center\" tts:displayAlign=\"after\"/>" + Environment.NewLine +
            "      </layout>" + Environment.NewLine +
            "   </head>" + Environment.NewLine +
            "   <body>" + Environment.NewLine +
            "       <div />" + Environment.NewLine +
            "   </body>" + Environment.NewLine +
            "</tt>";

            if (styleHead == null)
            {
                xml.LoadXml(xmlStructure);
            }
            else
            {
                try
                {
                    xml.LoadXml(subtitle.Header);
                    XmlNode divNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
                    if (divNode == null)
                    {
                        divNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
                    }

                    if (divNode != null)
                    {
                        var lst = new List<XmlNode>();
                        foreach (XmlNode child in divNode.ChildNodes)
                        {
                            lst.Add(child);
                        }

                        foreach (var child in lst)
                        {
                            divNode.RemoveChild(child);
                        }

                        foreach (XmlNode child in xml.DocumentElement.SelectNodes("//ttml:region", nsmgr))
                        {
                            var id = child.Attributes["xml:id"];
                            if (id?.Value == "top")
                            {
                                var originAttr = child.Attributes["tts:origin"];
                                if (originAttr != null && !string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.TimedTextItunesTopOrigin))
                                {
                                    originAttr.Value = Configuration.Settings.SubtitleSettings.TimedTextItunesTopOrigin;
                                }

                                var extentAttr = child.Attributes["tts:extent"];
                                if (extentAttr != null && !string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.TimedTextItunesTopExtent))
                                {
                                    extentAttr.Value = Configuration.Settings.SubtitleSettings.TimedTextItunesTopExtent;
                                }
                            }
                            else if (id?.Value == "bottom")
                            {
                                var originAttr = child.Attributes["tts:origin"];
                                if (originAttr != null && !string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.TimedTextItunesBottomOrigin))
                                {
                                    originAttr.Value = Configuration.Settings.SubtitleSettings.TimedTextItunesBottomOrigin;
                                }

                                var extentAttr = child.Attributes["tts:extent"];
                                if (extentAttr != null && !string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.TimedTextItunesBottomExtent))
                                {
                                    extentAttr.Value = Configuration.Settings.SubtitleSettings.TimedTextItunesBottomExtent;
                                }
                            }
                        }
                    }
                    else
                    {
                        xml.LoadXml(xmlStructure);
                    }
                }
                catch
                {
                    xml.LoadXml(xmlStructure);
                }
            }

            var body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
            var defaultStyle = Guid.NewGuid().ToString();
            if (body.Attributes["style"] != null)
            {
                defaultStyle = body.Attributes["style"].InnerText;
            }

            var div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).SelectSingleNode("ttml:div", nsmgr);
            if (div == null)
            {
                div = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr).FirstChild;
            }

            var hasBottomRegion = false;
            var hasTopRegion = false;
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//ttml:head/ttml:layout/ttml:region", nsmgr))
            {
                string id = null;
                if (node.Attributes["xml:id"] != null)
                {
                    id = node.Attributes["xml:id"].Value;
                }
                else if (node.Attributes["id"] != null)
                {
                    id = node.Attributes["id"].Value;
                }

                if (id != null && id == "bottom")
                {
                    hasBottomRegion = true;
                }

                if (id != null && id == "top")
                {
                    hasTopRegion = true;
                }
            }

            var hasAlignmentTags = subtitle.Paragraphs.Any(p => p.Text.Contains("{\\an", StringComparison.Ordinal));
            var headerStyles = GetStylesFromHeader(subtitle.Header);
            var isDefaultAlignmentBottom = body != null && body.Attributes != null && body.Attributes["region"] != null && body.Attributes["region"].InnerText == "bottom";
            foreach (var p in subtitle.Paragraphs)
            {
                var paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                var text = p.Text;

                if (hasAlignmentTags || !isDefaultAlignmentBottom) 
                {
                    var regionP = xml.CreateAttribute("region");
                    if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                    {
                        if (hasTopRegion)
                        {
                            regionP.InnerText = "top";
                            paragraph.Attributes.Append(regionP);
                        }
                    }
                    else if (hasBottomRegion)
                    {
                        regionP.InnerText = "bottom";
                        paragraph.Attributes.Append(regionP);
                    }

                    if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                    {
                        text = text.Remove(0, 6);
                    }
                }

                if (convertedFromSubStationAlpha)
                {
                    if (string.IsNullOrEmpty(p.Style))
                    {
                        p.Style = p.Extra;
                    }
                }

                if (subtitle.Header != null && p.Style != null && headerStyles.Contains(p.Style))
                {
                    if (p.Style != defaultStyle)
                    {
                        var styleAttr = xml.CreateAttribute("style");
                        styleAttr.InnerText = p.Style;
                        paragraph.Attributes.Append(styleAttr);
                    }
                }
                else if (!string.IsNullOrEmpty(p.Style))
                {
                    var styleP = xml.CreateAttribute("style");
                    styleP.InnerText = p.Style;
                    paragraph.Attributes.Append(styleP);
                }

                // Formatting tags become nested spans, so <i><font color="x">text</font></i> is written as
                // <span italic><span color>text</span></span> - flat sibling spans lost the italic (#15846).
                // Tags still open at the end of a line are closed before the <br/> and reopened after it.
                var openTags = new List<XmlAttribute>();
                var first = true;
                paragraph.AppendChild(xml.CreateTextNode(string.Empty)); // mixed content - stops the writer from indenting the spans

                foreach (var line in text.SplitToLines())
                {
                    if (!first)
                    {
                        XmlNode br = xml.CreateElement("br", "http://www.w3.org/ns/ttml");
                        paragraph.AppendChild(br);
                    }

                    // parents[n] is the container that was current before openTags[n] was opened
                    var parents = new List<XmlNode>();
                    XmlNode container = paragraph;
                    foreach (var tag in openTags)
                    {
                        parents.Add(container);
                        container = OpenSpan(xml, container, tag);
                    }

                    // Text is collected here and written to the node once per run - "InnerText += c"
                    // per character re-read and re-set the node text for every character of the line.
                    var pending = new StringBuilder();
                    var skipCount = 0;
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (skipCount > 0)
                        {
                            skipCount--;
                            continue;
                        }

                        if (line[i] == '<' && pending.Length > 0)
                        {
                            container.AppendChild(xml.CreateTextNode(pending.ToString()));
                            pending.Clear();
                        }

                        XmlAttribute newTag = null;
                        string closeTagName = null;
                        if (line.StartsWithAt(i, "<i>", StringComparison.OrdinalIgnoreCase))
                        {
                            newTag = CreateParagraphStyleAttribute(xml);
                            newTag.InnerText = "italic";
                            skipCount = 2;
                        }
                        else if (line.StartsWithAt(i, "<b>", StringComparison.OrdinalIgnoreCase))
                        {
                            newTag = xml.CreateAttribute("tts:fontWeight", "http://www.w3.org/ns/ttml#styling");
                            newTag.InnerText = "bold";
                            skipCount = 2;
                        }
                        else if (line.StartsWithAt(i, "<u>", StringComparison.OrdinalIgnoreCase))
                        {
                            newTag = xml.CreateAttribute("tts:textDecoration", "http://www.w3.org/ns/ttml#styling");
                            newTag.InnerText = "underline";
                            skipCount = 2;
                        }
                        else if (line.StartsWithAt(i, "<font ", StringComparison.OrdinalIgnoreCase))
                        {
                            var endIndex = line.IndexOf('>', i + 1);
                            endIndex = endIndex < 0 ? -1 : endIndex - i - 1;
                            if (endIndex > 0)
                            {
                                skipCount = endIndex + 1;
                                var fontContent = line.Substring(i, skipCount);
                                if (fontContent.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                                {
                                    var arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.OrdinalIgnoreCase) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arr.Length > 0)
                                    {
                                        newTag = xml.CreateAttribute("tts:color", "http://www.w3.org/ns/ttml#styling");
                                        newTag.InnerText = arr[0].Trim('\'').Trim('"').Trim('\'');
                                    }
                                }

                                // a <font> without color still gets an entry, so its </font> closes the right tag
                                newTag = newTag ?? xml.CreateAttribute(FontWithoutColor);
                            }
                            else
                            {
                                skipCount = line.Length;
                            }
                        }
                        else if (line.StartsWithAt(i, "</i>", StringComparison.OrdinalIgnoreCase))
                        {
                            closeTagName = "i";
                            skipCount = 3;
                        }
                        else if (line.StartsWithAt(i, "</b>", StringComparison.OrdinalIgnoreCase))
                        {
                            closeTagName = "b";
                            skipCount = 3;
                        }
                        else if (line.StartsWithAt(i, "</u>", StringComparison.OrdinalIgnoreCase))
                        {
                            closeTagName = "u";
                            skipCount = 3;
                        }
                        else if (line.StartsWithAt(i, "</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            closeTagName = "font";
                            skipCount = 6;
                        }
                        else
                        {
                            pending.Append(line[i]);
                        }

                        if (newTag != null)
                        {
                            openTags.Add(newTag);
                            parents.Add(container);
                            container = OpenSpan(xml, container, newTag);
                        }
                        else if (closeTagName != null)
                        {
                            var idx = openTags.FindLastIndex(t => GetTagName(t) == closeTagName);
                            if (idx >= 0)
                            {
                                // close the tag and everything opened inside it, then reopen the inner ones
                                container = parents[idx];
                                openTags.RemoveAt(idx);
                                parents.RemoveRange(idx, parents.Count - idx);
                                for (var j = idx; j < openTags.Count; j++)
                                {
                                    parents.Add(container);
                                    container = OpenSpan(xml, container, openTags[j]);
                                }
                            }
                        }
                    }

                    if (pending.Length > 0)
                    {
                        container.AppendChild(xml.CreateTextNode(pending.ToString()));
                    }

                    first = false;
                }

                RemoveEmptySpans(paragraph);

                var start = xml.CreateAttribute("begin");
                start.InnerText = ConvertToTimeString(p.StartTime, Configuration.Settings.SubtitleSettings.TimedTextItunesTimeCodeFormat);
                paragraph.Attributes.Append(start);

                var end = xml.CreateAttribute("end");
                end.InnerText = ConvertToTimeString(p.EndTime, Configuration.Settings.SubtitleSettings.TimedTextItunesTimeCodeFormat);
                paragraph.Attributes.Append(end);

                div.AppendChild(paragraph);
            }

            var xmlString = ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace("<br />", "<br/>");
            if (subtitle.Header == null)
            {
                subtitle.Header = xmlString;
            }

            return xmlString;
        }

        private const string FontWithoutColor = "font";

        private static XmlNode OpenSpan(XmlDocument xml, XmlNode container, XmlAttribute tag)
        {
            if (tag.Name == FontWithoutColor)
            {
                return container;
            }

            var span = xml.CreateNode(XmlNodeType.Element, "span", null);
            span.Attributes.Append((XmlAttribute)tag.CloneNode(true));
            container.AppendChild(span);
            return span;
        }

        private static string GetTagName(XmlAttribute tag)
        {
            if (tag.Name == "tts:fontWeight")
            {
                return "b";
            }

            if (tag.Name == "tts:textDecoration")
            {
                return "u";
            }

            if (tag.Name == "tts:color" || tag.Name == FontWithoutColor)
            {
                return "font";
            }

            return "i";
        }

        private static void RemoveEmptySpans(XmlNode node)
        {
            foreach (var child in node.ChildNodes.Cast<XmlNode>().ToList())
            {
                if (child.Name == "span")
                {
                    RemoveEmptySpans(child);
                    if (!child.HasChildNodes)
                    {
                        node.RemoveChild(child);
                    }
                }
            }
        }

        private static XmlAttribute CreateParagraphStyleAttribute(XmlDocument xml)
        {
            if (string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.TimedTextItunesStyleAttribute) ||
                Configuration.Settings.SubtitleSettings.TimedTextItunesStyleAttribute == "tts:fontStyle")
            {
                return xml.CreateAttribute("tts:fontStyle", "http://www.w3.org/ns/ttml#styling");
            }

            return xml.CreateAttribute(Configuration.Settings.SubtitleSettings.TimedTextItunesStyleAttribute);
        }
    }
}
