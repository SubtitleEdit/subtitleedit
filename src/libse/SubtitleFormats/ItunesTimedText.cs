using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
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
            var settings = Configuration.Settings.SubtitleSettings;

            // A template header (File > Properties > "Header source: template file") replaces whatever
            // header the source file carries, so every saved .itt gets the same styling/layout (#16257).
            var templateXml = LoadHeaderTemplate(settings.TimedTextItunesHeaderTemplate);
            var useTemplate = templateXml != null;
            var sourceIsSubStationAlpha = subtitle.Header != null && (subtitle.Header.Contains("[V4+ Styles]") || subtitle.Header.Contains("[V4 Styles]"));

            XmlNode styleHead = null;
            var convertedFromSubStationAlpha = false;
            if (!useTemplate && subtitle.Header != null)
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
            var nsmgr = MakeNamespaceManager(xml);

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
            if (!string.IsNullOrEmpty(settings.TimedTextItunesLanguage))
            {
                language = settings.TimedTextItunesLanguage;
            }

            var defaultColor = XmlAttributeValueOrDefault(settings.TimedTextItunesDefaultColor, "white");
            var bodyAttributes = string.Empty;
            if (BuiltInStyles.Contains(settings.TimedTextItunesDefaultStyle ?? string.Empty))
            {
                bodyAttributes += " style=\"" + settings.TimedTextItunesDefaultStyle + "\"";
            }

            if (BuiltInRegions.Contains(settings.TimedTextItunesDefaultRegion ?? string.Empty))
            {
                bodyAttributes += " region=\"" + settings.TimedTextItunesDefaultRegion + "\"";
            }

            var xmlStructure = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + Environment.NewLine +
            "<tt xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tt=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xml:lang=\"" + language + "\" ttp:timeBase=\"smpte\" ttp:frameRate=\"" + frameRate + "\" ttp:frameRateMultiplier=\"" + frameRateMultiplier + "\" ttp:dropMode=\"" + dropMode + "\">" + Environment.NewLine +
            "   <head>" + Environment.NewLine +
            "       <styling>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"" + defaultColor + "\" tts:fontStyle=\"normal\" tts:fontWeight=\"normal\" tts:fontFamily=\"sansSerif\" xml:id=\"normal\"/>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"" + defaultColor + "\" tts:fontStyle=\"normal\" tts:fontWeight=\"bold\" tts:fontFamily=\"sansSerif\" xml:id=\"bold\"/>" + Environment.NewLine +
            "         <style tts:fontSize=\"100%\" tts:color=\"" + defaultColor + "\" tts:fontStyle=\"italic\" tts:fontWeight=\"normal\" tts:fontFamily=\"sansSerif\" xml:id=\"italic\"/>" + Environment.NewLine +
            "      </styling>" + Environment.NewLine +
            "      <layout>" + Environment.NewLine +
            "        <region xml:id=\"top\" tts:origin=\"" + settings.TimedTextItunesTopOrigin + "\" tts:extent=\"" + settings.TimedTextItunesTopExtent + "\" tts:textAlign=\"center\" tts:displayAlign=\"before\"/>" + Environment.NewLine +
            "        <region xml:id=\"bottom\" tts:origin=\"" + settings.TimedTextItunesBottomOrigin + "\" tts:extent=\"" + settings.TimedTextItunesBottomExtent + "\" tts:textAlign=\"center\" tts:displayAlign=\"after\"/>" + Environment.NewLine +
            "      </layout>" + Environment.NewLine +
            "   </head>" + Environment.NewLine +
            "   <body" + bodyAttributes + ">" + Environment.NewLine +
            "       <div />" + Environment.NewLine +
            "   </body>" + Environment.NewLine +
            "</tt>";

            if (useTemplate)
            {
                // The template defines look, layout and metadata; the timing parameters follow the
                // current frame rate like the built-in header, and the language setting wins when set.
                xml = templateXml;
                nsmgr = MakeNamespaceManager(xml);
                ClearBodyDiv(xml, nsmgr);
                SetRootParameter(xml, "frameRate", frameRate);
                SetRootParameter(xml, "frameRateMultiplier", frameRateMultiplier);
                SetRootParameter(xml, "dropMode", dropMode);
                if (!string.IsNullOrEmpty(settings.TimedTextItunesLanguage))
                {
                    SetRootLanguage(xml, settings.TimedTextItunesLanguage);
                }

                ApplyRegionSettings(xml, nsmgr, settings);
                ApplyBodyDefaults(xml, nsmgr, settings);
            }
            else if (styleHead == null)
            {
                xml.LoadXml(xmlStructure);
            }
            else
            {
                try
                {
                    xml.LoadXml(subtitle.Header);
                    if (ClearBodyDiv(xml, nsmgr))
                    {
                        // The time codes are written at the current frame rate, so the source's
                        // timing parameters must follow it (e.g. after a frame rate change, #16257).
                        SetRootParameter(xml, "frameRate", frameRate);
                        SetRootParameter(xml, "frameRateMultiplier", frameRateMultiplier);
                        SetRootParameter(xml, "dropMode", dropMode);
                        ApplyRegionSettings(xml, nsmgr, settings);
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

            var headerStyles = useTemplate ? GetStylesFromHeader(xml) : GetStylesFromHeader(subtitle.Header);
            var bodyRegion = body?.Attributes?["region"]?.InnerText;
            foreach (var p in subtitle.Paragraphs)
            {
                var paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
                var text = p.Text;

                // A line only names its region when it differs from the body's - with
                // <body region="bottom"> just the top-aligned lines get region="top" (#16257).
                string wantedRegion = null;
                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    if (hasTopRegion)
                    {
                        wantedRegion = "top";
                    }
                }
                else if (hasBottomRegion)
                {
                    wantedRegion = "bottom";
                }

                if (wantedRegion != null && wantedRegion != bodyRegion)
                {
                    var regionP = xml.CreateAttribute("region");
                    regionP.InnerText = wantedRegion;
                    paragraph.Attributes.Append(regionP);
                }

                if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6 && text[5] == '}')
                {
                    text = text.Remove(0, 6);
                }

                if (convertedFromSubStationAlpha)
                {
                    if (string.IsNullOrEmpty(p.Style))
                    {
                        p.Style = p.Extra;
                    }
                }

                if (useTemplate)
                {
                    // Only styles the template knows are referenced; the rest fall back to the body default.
                    var styleName = p.Style;
                    if (string.IsNullOrEmpty(styleName) && sourceIsSubStationAlpha)
                    {
                        styleName = p.Extra;
                    }

                    if (!string.IsNullOrEmpty(styleName) && styleName != defaultStyle && headerStyles.Contains(styleName))
                    {
                        var styleAttr = xml.CreateAttribute("style");
                        styleAttr.InnerText = styleName;
                        paragraph.Attributes.Append(styleAttr);
                    }
                }
                else if (subtitle.Header != null && p.Style != null && headerStyles.Contains(p.Style))
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
        private const string TtmlNamespace = "http://www.w3.org/ns/ttml";

        private static XmlNamespaceManager MakeNamespaceManager(XmlDocument xml)
        {
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", TtmlNamespace);
            nsmgr.AddNamespace("ttp", "http://www.w3.org/ns/10/ttml#parameter");
            nsmgr.AddNamespace("tts", "http://www.w3.org/ns/ttml#styling");
            nsmgr.AddNamespace("ttm", "http://www.w3.org/ns/10/ttml#metadata");
            return nsmgr;
        }

        private static readonly string[] BuiltInStyles = { "normal", "bold", "italic" };
        private static readonly string[] BuiltInRegions = { "top", "bottom" };

        /// <summary>
        /// Parses a template header (see <see cref="SubtitleSettings.TimedTextItunesHeaderTemplate"/>):
        /// a TTML document with a head and a body. Null when there is none or it does not parse.
        /// </summary>
        public static XmlDocument LoadHeaderTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return null;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(template.Trim());
                if (xml.DocumentElement == null || xml.DocumentElement.NamespaceURI != TtmlNamespace)
                {
                    return null;
                }

                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", TtmlNamespace);
                if (xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr) == null)
                {
                    return null;
                }

                var body = xml.DocumentElement.SelectSingleNode("ttml:body", nsmgr);
                if (body == null)
                {
                    body = xml.CreateElement("body", TtmlNamespace);
                    xml.DocumentElement.AppendChild(body);
                }

                if (body.SelectSingleNode("ttml:div", nsmgr) == null)
                {
                    body.AppendChild(xml.CreateElement("div", TtmlNamespace));
                }

                return xml;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Empties the body's div (or the body's first child), where the lines go. False when there is no such node.
        /// </summary>
        private static bool ClearBodyDiv(XmlDocument xml, XmlNamespaceManager nsmgr)
        {
            var bodyNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr);
            var divNode = bodyNode?.SelectSingleNode("ttml:div", nsmgr) ?? bodyNode?.FirstChild;
            if (divNode == null)
            {
                return false;
            }

            var lst = new List<XmlNode>();
            foreach (XmlNode child in divNode.ChildNodes)
            {
                lst.Add(child);
            }

            foreach (var child in lst)
            {
                divNode.RemoveChild(child);
            }

            return true;
        }

        /// <summary>
        /// The top/bottom region origin and extent from the settings win over a header's own.
        /// </summary>
        private static void ApplyRegionSettings(XmlDocument xml, XmlNamespaceManager nsmgr, SubtitleSettings settings)
        {
            foreach (XmlNode child in xml.DocumentElement.SelectNodes("//ttml:region", nsmgr))
            {
                var id = child.Attributes["xml:id"];
                if (id?.Value == "top")
                {
                    SetAttributeIfPresent(child, "tts:origin", settings.TimedTextItunesTopOrigin);
                    SetAttributeIfPresent(child, "tts:extent", settings.TimedTextItunesTopExtent);
                }
                else if (id?.Value == "bottom")
                {
                    SetAttributeIfPresent(child, "tts:origin", settings.TimedTextItunesBottomOrigin);
                    SetAttributeIfPresent(child, "tts:extent", settings.TimedTextItunesBottomExtent);
                }
            }
        }

        private static void SetAttributeIfPresent(XmlNode node, string attributeName, string value)
        {
            var attr = node.Attributes[attributeName];
            if (attr != null && !string.IsNullOrEmpty(value))
            {
                attr.Value = value;
            }
        }

        /// <summary>
        /// The default style/region settings override the template body's own style/region
        /// attributes - only when the template actually defines them.
        /// </summary>
        private static void ApplyBodyDefaults(XmlDocument xml, XmlNamespaceManager nsmgr, SubtitleSettings settings)
        {
            var bodyNode = xml.DocumentElement.SelectSingleNode("//ttml:body", nsmgr) as XmlElement;
            if (bodyNode == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(settings.TimedTextItunesDefaultStyle) && GetStylesFromHeader(xml).Contains(settings.TimedTextItunesDefaultStyle))
            {
                bodyNode.SetAttribute("style", settings.TimedTextItunesDefaultStyle);
            }

            if (!string.IsNullOrEmpty(settings.TimedTextItunesDefaultRegion) && GetRegionsFromHeader(xml).Contains(settings.TimedTextItunesDefaultRegion))
            {
                bodyNode.SetAttribute("region", settings.TimedTextItunesDefaultRegion);
            }
        }

        /// <summary>
        /// Sets a ttp:* attribute on the root, matched by local name so a template's own prefix/namespace is kept.
        /// </summary>
        private static void SetRootParameter(XmlDocument xml, string localName, string value)
        {
            var root = xml.DocumentElement;
            foreach (XmlAttribute attr in root.Attributes)
            {
                if (attr.LocalName == localName && attr.NamespaceURI.Contains("ttml#parameter"))
                {
                    attr.Value = value;
                    return;
                }
            }

            var ns = root.GetNamespaceOfPrefix("ttp");
            if (string.IsNullOrEmpty(ns))
            {
                ns = "http://www.w3.org/ns/ttml#parameter";
            }

            root.SetAttribute(localName, ns, value);
        }

        private static void SetRootLanguage(XmlDocument xml, string language)
        {
            var root = xml.DocumentElement;
            var attr = root.Attributes["xml:lang"];
            if (attr != null)
            {
                attr.Value = language;
                return;
            }

            root.SetAttribute("lang", "http://www.w3.org/XML/1998/namespace", language);
        }

        /// <summary>
        /// A settings value that goes straight into an XML attribute; anything that could break the markup falls back.
        /// </summary>
        private static string XmlAttributeValueOrDefault(string value, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value) || value.IndexOfAny(new[] { '"', '<', '>', '&' }) >= 0)
            {
                return defaultValue;
            }

            return value.Trim();
        }

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
