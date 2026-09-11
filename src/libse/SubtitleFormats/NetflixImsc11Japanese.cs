using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// IMSC 1.1 Viewer: https://www.sandflow.com/imsc1_1/
    /// More about bouten/furigana: https://www.japanesewithanime.com/2018/03/furigana-dots-bouten.html
    /// Netflix blog entry: https://medium.com/netflix-techblog/implementing-japanese-subtitles-on-netflix-c165fbe61989
    /// </summary>
    public class NetflixImsc11Japanese : SubtitleFormat
    {
        public override string Extension => ".xml";
        public override string Name => "Netflix IMSC 1.1 Japanese";

        // Carries the region of every paragraph, and the regions themselves in the header.
        public override bool HasPositionSupport => true;

        private static string GetXmlStructure()
        {
            return @"<?xml version='1.0' encoding='UTF-8' standalone='no'?>
<tt xml:lang='ja' xmlns='http://www.w3.org/ns/ttml' ttp:contentProfiles='http://www.w3.org/ns/ttml/profile/imsc1.1/text' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' ttp:timeBase='media' ttp:frameRate='24' ttp:frameRateMultiplier='1000 1001' ttp:tickRate='10000000' xmlns:ebutts='urn:ebu:tt:style' xmlns:itts='http://www.w3.org/ns/ttml/profile/imsc1#styling' xmlns:ittp='http://www.w3.org/ns/ttml/profile/imsc1#parameter' xmlns:ittm='http://www.w3.org/ns/ttml/profile/imsc1#metadata' ittp:aspectRatio='16 9'>
  <head>
    <styling>
      <initial tts:color='white' tts:fontFamily='proportionalSansSerif' tts:fontSize='100%' tts:rubyReserve='outside' xml:id='initialStyles'/>
      <style tts:shear='16.6667%' xml:id='italic'/>

      <!-- Variants of bouten styles -->
      <style tts:textEmphasis='dot before' xml:id='bouten-dot-before'/>
      <style tts:textEmphasis='dot after' xml:id='bouten-dot-after'/>
      <style tts:textEmphasis='dot outside' xml:id='bouten-dot-outside'/>
      <style tts:textEmphasis='filled circle outside' xml:id='bouten-filled-circle-outside'/>
      <style tts:textEmphasis='open circle outside' xml:id='bouten-open-circle-outside'/>
      <style tts:textEmphasis='open dot outside' xml:id='bouten-open-dot-outside'/>
      <style tts:textEmphasis='filled sesame outside' xml:id='bouten-filled-sesame-outside'/>
      <style tts:textEmphasis='open sesame outside' xml:id='bouten-open-sesame-outside'/>
      <style tts:textEmphasis='auto outside' xml:id='bouten-auto-outside'/>
      <style tts:textEmphasis='auto' xml:id='bouten-auto'/>

      <style tts:textCombine='all' xml:id='horizontalDigit'/>
      <style tts:ruby='base' xml:id='ruby-base'/>
      <style tts:ruby='text' xml:id='ruby-text'/>
      <style tts:ruby='text' xml:id='ruby-text-after'/>
      <style tts:ruby='base' tts:shear='16.6667%' xml:id='ruby-base-italic'/>
      <style tts:ruby='text' tts:shear='16.6667%' xml:id='ruby-text-italic'/>
      <style tts:ruby='container' xml:id='ruby-container'/>
    </styling>
    <layout>
      <region ebutts:multiRowAlign='start' tts:displayAlign='after' tts:extent='80.000% 80.000%' tts:origin='10.000% 10.000%' tts:textAlign='center' xml:id='bottom-left-justified'/>

      <!-- This region is used to display English text events which divide this document into the various sections outlined in Netflix's Japanese Timed-Text Style Guide. Top-center-justified positioning shall not be used for Japanese authoring -->
      <region tts:displayAlign='before' tts:extent='80.000% 80.000%' tts:origin='10.000% 10.000%' tts:textAlign='center' xml:id='top-center-justified'/>

      <!-- In exceptional cases, some forced narrative events may be positioned creatively to mimic on-screen text as referenced in Section I.3 'Alignment' -->
      <region ebutts:multiRowAlign='end' tts:displayAlign='before' tts:extent='50.000% 50.000%' tts:origin='30.000% 50.000%' tts:textAlign='center' tts:shear='16.6667%' xml:id='force-narrative-example-region'/>

      <region ebutts:multiRowAlign='start' tts:displayAlign='after' tts:extent='80.000% 80.000%' tts:origin='10.000% 10.000%' tts:textAlign='start' tts:writingMode='tbrl' xml:id='left'/>
      <region ebutts:multiRowAlign='start' tts:displayAlign='before' tts:extent='80.000% 80.000%' tts:origin='10.000% 10.000%' tts:textAlign='start' tts:writingMode='tbrl' xml:id='right'/>
    </layout>
  </head>
  <body>
    <div>
    </div>
  </body>
</tt>
".Replace('\'', '"');
        }

        /// <summary>
        /// Bouten (emphasis) styles are optional, so also accept the ruby/furigana styling
        /// attributes from the IMSC 1.1 Japanese profile - these never occur in EBU-TT-D.
        /// </summary>
        internal static bool ContainsJapaneseProfileStyling(string text)
        {
            return text.Contains("bouten-", StringComparison.Ordinal) ||
                   text.Contains("tts:ruby=", StringComparison.Ordinal) ||
                   text.Contains("tts:rubyReserve=", StringComparison.Ordinal);
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var text = JoinLines(lines);
            if (!text.Contains("lang=\"ja\"", StringComparison.Ordinal) || !ContainsJapaneseProfileStyling(text))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            // The fixed template header declares ttp:frameRate 24 with multiplier 1000/1001,
            // and readers (including SE's own) parse the HH:MM:SS:FF frames part at that
            // declared 23.976 - so the frames must also be WRITTEN at 23.976, not at whatever
            // video happens to be open (a 29.97 video shifted every written time code).
            var savedFrameRate = Configuration.Settings.General.CurrentFrameRate;
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(GetXmlStructure());
                var namespaceManager = new XmlNamespaceManager(xml.NameTable);
                namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                var div = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager).SelectSingleNode("ttml:div", namespaceManager);
                foreach (var p in subtitle.Paragraphs)
                {
                    var paragraphNode = MakeParagraph(xml, p);
                    div.AppendChild(paragraphNode);
                }

                var xmlString = ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
                subtitle.Header = xmlString;
                return xmlString;
            }
            finally
            {
                Configuration.Settings.General.CurrentFrameRate = savedFrameRate;
            }
        }

        private static XmlNode MakeParagraph(XmlDocument xml, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("p", "http://www.w3.org/ns/ttml");
            string text = p.Text.RemoveControlCharactersButWhiteSpace();

            XmlAttribute start = xml.CreateAttribute("begin");
            start.InnerText = TimedText10.ConvertToTimeString(p.StartTime);
            paragraph.Attributes.Append(start);

            XmlAttribute dur = xml.CreateAttribute("dur");
            dur.InnerText = TimedText10.ConvertToTimeString(p.Duration);
            paragraph.Attributes.Append(dur);

            XmlAttribute region = xml.CreateAttribute("region");
            region.InnerText = GetRegionFromText(p.Text);
            paragraph.Attributes.Append(region);

            // Trying to parse and convert paragraph content
            try
            {
                text = Utilities.RemoveSsaTags(text);
                text = string.Join("<br/>", text.SplitToLines());
                var paragraphContent = new XmlDocument();
                paragraphContent.LoadXml($"<root>{text.Replace("&", "&amp;")}</root>");
                ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);
            }
            catch // Wrong markup (e.g. a literal "5 < 6"): keep the words and line breaks, drop the tags
            {
                // Stripping every < and > turned the tags into text and lost the line break (see TimedText10).
                var fallbackLines = text.Split(new[] { "<br/>" }, StringSplitOptions.None);
                for (var i = 0; i < fallbackLines.Length; i++)
                {
                    if (i > 0)
                    {
                        paragraph.AppendChild(xml.CreateElement("br"));
                    }

                    paragraph.AppendChild(xml.CreateTextNode(HtmlUtil.RemoveHtmlTags(fallbackLines[i], true)));
                }
            }

            return paragraph;
        }

        internal static void ConvertParagraphNodeToTtmlNode(XmlNode node, XmlDocument ttmlXml, XmlNode ttmlNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlText)
                {
                    ttmlNode.AppendChild(ttmlXml.CreateTextNode(child.Value));
                }
                else if (child.Name == "br")
                {
                    XmlNode br = ttmlXml.CreateElement("br");
                    ttmlNode.AppendChild(br);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, br);
                }
                else if (child.Name == "i")
                {
                    XmlNode span = ttmlXml.CreateElement("span");
                    XmlAttribute attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = "italic";
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);

                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else if (child.Name.StartsWith("bouten-", StringComparison.Ordinal) || child.Name == "horizontalDigit" || child.Name.StartsWith("ruby-", StringComparison.Ordinal))
                {
                    var span = ttmlXml.CreateElement("span");
                    var attr = ttmlXml.CreateAttribute("style");
                    attr.InnerText = child.Name;
                    span.Attributes.Append(attr);
                    ttmlNode.AppendChild(span);
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, span);
                }
                else // Default - skip node
                {
                    ConvertParagraphNodeToTtmlNode(child, ttmlXml, ttmlNode);
                }
            }
        }

        private static string GetRegionFromText(string text)
        {
            if (text.StartsWith(@"{\an8", StringComparison.Ordinal))
            {
                return "top-center-justified";
            }

            if (text.StartsWith(@"{\an5", StringComparison.Ordinal))
            {
                return "force-narrative-example-region";
            }

            if (text.StartsWith(@"{\an4", StringComparison.Ordinal) ||
                text.StartsWith(@"{\an7", StringComparison.Ordinal))
            {
                return "left";
            }

            if (text.StartsWith(@"{\an6", StringComparison.Ordinal) ||
                text.StartsWith(@"{\an9", StringComparison.Ordinal))
            {
                return "right";
            }

            return "bottom-left-justified";
        }

        private static string GetAssStyleFromRegionName(string region)
        {
            switch (region)
            {
                case "top-center-justified": return @"{\an8}";
                case "force-narrative-example-region": return @"{\an5}";
                case "left": return @"{\an7}";
                case "right": return @"{\an9}";
                default: return null;
            }
        }

        /// <summary>
        /// Real Netflix documents name their regions "region0", "region1"... - only files authored by
        /// Subtitle Edit itself use the names above. Fall back to the region attributes so vertical
        /// (tbrl/tblr) and top/middle placement survive documents written by anything else (issue #13861).
        /// </summary>
        private static string GetAssStyleFromRegion(string regionId, TtmlNodeIndex index)
        {
            var byName = GetAssStyleFromRegionName(regionId);
            if (byName != null)
            {
                return byName;
            }

            var regionNode = index.Region(regionId);
            if (regionNode == null)
            {
                return string.Empty;
            }

            var displayAlign = GetTtmlStyleValue(index, regionNode, "tts:displayAlign", includeInitial: true) ?? "after";
            var writingMode = GetTtmlStyleValue(index, regionNode, "tts:writingMode", includeInitial: true) ?? "lrtb";

            if (writingMode.StartsWith("tb", StringComparison.Ordinal))
            {
                // Vertical writing: the block direction runs sideways, so "displayAlign" picks a
                // side, not a height. tbrl stacks columns right-to-left, so "before" is the right
                // edge; tblr is the mirror image. Subtitle Edit's vertical layout only knows the
                // two corners (see NetflixImsc11JapaneseToAss), so "center" snaps to the start side.
                var rightToLeft = !writingMode.StartsWith("tblr", StringComparison.Ordinal);
                var atStart = displayAlign != "after";
                return rightToLeft == atStart ? @"{\an9}" : @"{\an7}";
            }

            var originY = ParseRegionPercentY(GetTtmlStyleValue(index, regionNode, "tts:origin", includeInitial: true)) ?? 10.0;
            var extentY = ParseRegionPercentY(GetTtmlStyleValue(index, regionNode, "tts:extent", includeInitial: true)) ?? 80.0;
            double anchorY;
            switch (displayAlign)
            {
                case "before":
                    anchorY = originY;
                    break;
                case "center":
                    anchorY = originY + extentY / 2.0;
                    break;
                default: // after
                    anchorY = originY + extentY;
                    break;
            }

            if (anchorY <= 33)
            {
                return @"{\an8}";
            }

            if (anchorY < 66)
            {
                return @"{\an5}";
            }

            return string.Empty; // bottom is the default region
        }

        private static double? ParseRegionPercentY(string originOrExtent)
        {
            if (string.IsNullOrEmpty(originOrExtent))
            {
                return null;
            }

            var parts = originOrExtent.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !parts[1].EndsWith("%", StringComparison.Ordinal))
            {
                return null;
            }

            return double.TryParse(parts[1].TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : (double?)null;
        }

        /// <summary>
        /// The document's style / region elements by id and its "initial" elements, built once per
        /// load. Every attribute lookup used to create an XmlNamespaceManager and run a
        /// document-wide XPath - about ten per span, more per paragraph for the region.
        /// </summary>
        private sealed class TtmlNodeIndex
        {
            private readonly Dictionary<string, XmlNode> _styles = new Dictionary<string, XmlNode>();
            private readonly Dictionary<string, XmlNode> _regions = new Dictionary<string, XmlNode>();

            public List<XmlNode> Initials { get; } = new List<XmlNode>();

            public static TtmlNodeIndex Build(XmlDocument xml)
            {
                var index = new TtmlNodeIndex();
                if (xml.DocumentElement == null)
                {
                    return index;
                }

                try
                {
                    var nsmgr = new XmlNamespaceManager(xml.NameTable);
                    nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    AddFirstById(index._styles, xml.DocumentElement.SelectNodes("//ttml:style", nsmgr));
                    AddFirstById(index._regions, xml.DocumentElement.SelectNodes("//ttml:region", nsmgr));
                    foreach (XmlNode initial in xml.DocumentElement.SelectNodes("//ttml:initial", nsmgr))
                    {
                        index.Initials.Add(initial);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }

                return index;
            }

            private static void AddFirstById(Dictionary<string, XmlNode> target, XmlNodeList nodes)
            {
                foreach (XmlNode node in nodes)
                {
                    var id = node.Attributes?["xml:id"]?.Value ?? node.Attributes?["id"]?.Value;
                    // The replaced scan returned the first match in document order.
                    if (id != null && !target.ContainsKey(id))
                    {
                        target[id] = node;
                    }
                }
            }

            public XmlNode Style(string id) => id != null && _styles.TryGetValue(id, out var node) ? node : null;

            public XmlNode Region(string id) => id != null && _regions.TryGetValue(id, out var node) ? node : null;
        }

        /// <summary>
        /// Reads a tts: attribute off an element, following TTML referential styling: the element's own
        /// attribute wins, then any style it references (a space separated list, later ids win), and -
        /// only when <paramref name="includeInitial"/> is set - the document's "initial" values.
        /// Layout attributes want the initial fallback; presentation attributes must not have it, or
        /// the document-wide default color and font would end up on a &lt;font&gt; tag around every span.
        /// </summary>
        private static string GetTtmlStyleValue(TtmlNodeIndex index, XmlNode node, string attributeName, bool includeInitial = false, int depth = 0)
        {
            if (node == null || depth > 5)
            {
                return null;
            }

            var own = node.Attributes?[attributeName]?.Value;
            if (!string.IsNullOrEmpty(own))
            {
                return own;
            }

            var styleRef = node.Attributes?["style"]?.Value;
            if (!string.IsNullOrEmpty(styleRef))
            {
                var ids = styleRef.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = ids.Length - 1; i >= 0; i--)
                {
                    var value = GetTtmlStyleValue(index, index.Style(ids[i]), attributeName, includeInitial, depth + 1);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            if (!includeInitial || depth > 0)
            {
                return null;
            }

            foreach (var initial in index.Initials)
            {
                var value = initial.Attributes?[attributeName]?.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(JoinLines(lines).RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                try
                {
                    xml.LoadXml(JoinLines(lines).Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A").RemoveControlCharactersButWhiteSpace().Trim());
                }
                catch (Exception exception)
                {
                    // The retry is the last chance to make sense of the file; a truncated or
                    // damaged one must read as "not mine", not throw out of the reader (and out
                    // of IsMine, which runs for every format when a file is opened).
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    _errorCount = 1;
                    return;
                }
            }

            var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
            if (frameRateAttr != null)
            {
                if (double.TryParse(frameRateAttr.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var fr))
                {
                    if (fr > 20 && fr < 100)
                    {
                        Configuration.Settings.General.CurrentFrameRate = fr;
                    }

                    var frameRateMultiplier = xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
                    if (frameRateMultiplier != null)
                    {
                        if (frameRateMultiplier.InnerText == "999 1000" && Math.Abs(fr - 30) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 29.97;
                        }
                        else if (frameRateMultiplier.InnerText == "999 1000" && Math.Abs(fr - 24) < 0.01)
                        {
                            Configuration.Settings.General.CurrentFrameRate = 23.976;
                        }
                        else
                        {
                            var arr = frameRateMultiplier.InnerText.Split();
                            if (arr.Length == 2 && Utilities.IsInteger(arr[0]) && Utilities.IsInteger(arr[1]) && int.Parse(arr[1]) > 0)
                            {
                                fr = double.Parse(arr[0], CultureInfo.InvariantCulture) / double.Parse(arr[1], CultureInfo.InvariantCulture);
                                if (fr > 20 && fr < 100)
                                {
                                    Configuration.Settings.General.CurrentFrameRate = fr;
                                }
                            }
                        }
                    }
                }
            }

            if (BatchSourceFrameRate.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = BatchSourceFrameRate.Value;
            }

            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormatSource = null;
            subtitle.Header = JoinLines(lines);

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);
            var index = TtmlNodeIndex.Build(xml);
            foreach (XmlNode node in body.SelectNodes("//ttml:p", namespaceManager))
            {
                TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);
                var assStyle = string.Empty;
                var region = node.Attributes?["region"];
                if (region != null)
                {
                    assStyle = GetAssStyleFromRegion(region.InnerText, index);
                }

                // Netflix puts the shear (their italic) for a whole cue on the <p> itself, not on a
                // span, so a paragraph-level style has to be read too - see issue #13861. It is
                // inherited by the content instead of wrapping it, so it survives the line breaks.
                var paragraphStyle = ReadSpanStyle(node, index);
                var text = assStyle + ReadParagraph(node, index, paragraphStyle.IsItalic);
                var p = new Paragraph(begin, end, text);
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private sealed class JapaneseSpanStyle
        {
            public bool IsItalic { get; set; }
            public bool IsBold { get; set; }
            public bool IsUnderlined { get; set; }
            public string FontFamily { get; set; }
            public string Color { get; set; }
            public string Bouten { get; set; }
            public bool HorizontalDigit { get; set; }
            public bool RubyContainer { get; set; }
            public bool RubyBase { get; set; }
            public bool RubyText { get; set; }
            public bool RubyTextAfter { get; set; }
        }

        private static readonly HashSet<string> BoutenStyleNames = new HashSet<string>
        {
            "bouten-dot-before",
            "bouten-dot-after",
            "bouten-dot-outside",
            "bouten-filled-circle-outside",
            "bouten-open-circle-outside",
            "bouten-open-dot-outside",
            "bouten-filled-sesame-outside",
            "bouten-open-sesame-outside",
            "bouten-auto-outside",
            "bouten-auto",
        };

        /// <summary>
        /// "dot before" -> "bouten-dot-before". Anything outside the profile's vocabulary still gets
        /// an emphasis mark rather than being dropped silently.
        /// </summary>
        private static string BoutenStyleNameFromTextEmphasis(string textEmphasis)
        {
            if (string.IsNullOrWhiteSpace(textEmphasis) || textEmphasis == "none")
            {
                return null;
            }

            var name = "bouten-" + string.Join("-", textEmphasis.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return BoutenStyleNames.Contains(name) ? name : "bouten-auto";
        }

        private static bool IsShearSet(string shear)
        {
            if (string.IsNullOrWhiteSpace(shear))
            {
                return false;
            }

            var value = shear.Trim().TrimEnd('%');
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
                ? Math.Abs(percent) > 0.001
                : true;
        }

        /// <summary>
        /// Style ids are only meaningful in documents Subtitle Edit wrote itself - Netflix generates
        /// "style0", "style1"... - so the real styling has to come off the attributes of the referenced
        /// style nodes. The named ids are still honored first so our own files keep round-tripping
        /// even when their style definitions are missing (issue #13861).
        /// </summary>
        private static JapaneseSpanStyle ReadSpanStyle(XmlNode node, TtmlNodeIndex index)
        {
            var style = new JapaneseSpanStyle();

            var styleRef = node.Attributes?["style"]?.Value;
            if (!string.IsNullOrEmpty(styleRef))
            {
                foreach (var styleName in styleRef.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ApplyKnownStyleName(styleName, style);
                }
            }

            var ruby = GetTtmlStyleValue(index, node, "tts:ruby");
            if (ruby == "container")
            {
                style.RubyContainer = true;
            }
            else if (ruby == "base")
            {
                style.RubyBase = true;
            }
            else if (ruby == "text")
            {
                style.RubyText = true;
                if (GetTtmlStyleValue(index, node, "tts:rubyPosition") == "after")
                {
                    style.RubyTextAfter = true;
                }
            }
            else if (ruby == "textContainer" || ruby == "baseContainer")
            {
                // Grouping wrappers with no text of their own - nothing to mark up.
            }

            var textEmphasis = BoutenStyleNameFromTextEmphasis(GetTtmlStyleValue(index, node, "tts:textEmphasis"));
            if (textEmphasis != null)
            {
                style.Bouten = textEmphasis;
            }

            if (GetTtmlStyleValue(index, node, "tts:textCombine") == "all")
            {
                style.HorizontalDigit = true;
            }

            // Netflix's Japanese profile has no tts:fontStyle - a slanted cue is expressed as a shear.
            if (GetTtmlStyleValue(index, node, "tts:fontStyle") == "italic" || IsShearSet(GetTtmlStyleValue(index, node, "tts:shear")))
            {
                style.IsItalic = true;
            }

            if (GetTtmlStyleValue(index, node, "tts:fontWeight") == "bold")
            {
                style.IsBold = true;
            }

            if (GetTtmlStyleValue(index, node, "tts:textDecoration") == "underline")
            {
                style.IsUnderlined = true;
            }

            var fontFamily = GetTtmlStyleValue(index, node, "tts:fontFamily");
            if (!string.IsNullOrEmpty(fontFamily))
            {
                style.FontFamily = fontFamily;
            }

            var color = GetTtmlStyleValue(index, node, "tts:color");
            if (!string.IsNullOrEmpty(color))
            {
                style.Color = color;
            }

            return style;
        }

        private static void ApplyKnownStyleName(string styleName, JapaneseSpanStyle style)
        {
            if (BoutenStyleNames.Contains(styleName))
            {
                style.Bouten = styleName;
                return;
            }

            switch (styleName)
            {
                case "italic":
                    style.IsItalic = true;
                    break;
                case "horizontalDigit":
                    style.HorizontalDigit = true;
                    break;
                case "ruby-container":
                    style.RubyContainer = true;
                    break;
                case "ruby-base":
                    style.RubyBase = true;
                    break;
                case "ruby-base-italic":
                    style.RubyBase = true;
                    style.IsItalic = true;
                    break;
                case "ruby-text":
                    style.RubyText = true;
                    break;
                case "ruby-text-italic":
                    style.RubyText = true;
                    style.IsItalic = true;
                    break;
                case "ruby-text-after":
                    style.RubyText = true;
                    style.RubyTextAfter = true;
                    break;
            }
        }

        private static string ReadParagraph(XmlNode node, TtmlNodeIndex index, bool inheritedItalic)
        {
            var pText = new StringBuilder();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    AppendMaybeItalic(pText, child.Value, inheritedItalic);
                }
                else if (child.Name == "br" || child.Name == "tt:br")
                {
                    pText.AppendLine();
                }
                else if (child.Name == "#significant-whitespace" || child.Name == "tt:#significant-whitespace")
                {
                    AppendMaybeItalic(pText, child.InnerText, inheritedItalic);
                }
                else if (child.Name == "span" || child.Name == "tt:span")
                {
                    var style = ReadSpanStyle(child, index);
                    if (inheritedItalic)
                    {
                        style.IsItalic = true;
                    }

                    AppendSpan(pText, child, index, style);
                }
            }

            return pText.ToString().TrimEnd();
        }

        private static void AppendMaybeItalic(StringBuilder pText, string text, bool italic)
        {
            if (italic && !string.IsNullOrWhiteSpace(text))
            {
                pText.Append("<i>").Append(text).Append("</i>");
                return;
            }

            pText.Append(text);
        }

        private static void AppendSpan(StringBuilder pText, XmlNode child, TtmlNodeIndex index, JapaneseSpanStyle style)
        {
            // A sheared ruby base/text has its own style name, so it must not also get an <i> around it.
            var italicViaRubyTag = style.IsItalic && (style.RubyBase || style.RubyText);
            var italicTag = style.IsItalic && !italicViaRubyTag;
            var hasFont = !string.IsNullOrEmpty(style.FontFamily) || !string.IsNullOrEmpty(style.Color);

            var openTags = new List<string>();
            if (italicTag)
            {
                openTags.Add("i");
            }

            if (style.IsBold)
            {
                openTags.Add("b");
            }

            if (style.IsUnderlined)
            {
                openTags.Add("u");
            }

            if (!string.IsNullOrEmpty(style.Bouten))
            {
                openTags.Add(style.Bouten);
            }

            if (style.HorizontalDigit)
            {
                openTags.Add("horizontalDigit");
            }

            if (style.RubyContainer)
            {
                openTags.Add("ruby-container");
            }

            if (style.RubyBase)
            {
                openTags.Add(italicViaRubyTag ? "ruby-base-italic" : "ruby-base");
            }

            if (style.RubyText)
            {
                openTags.Add(style.RubyTextAfter ? "ruby-text-after" : italicViaRubyTag ? "ruby-text-italic" : "ruby-text");
            }

            foreach (var tag in openTags)
            {
                pText.Append('<').Append(tag).Append('>');
            }

            if (hasFont)
            {
                pText.Append("<font");
                if (!string.IsNullOrEmpty(style.FontFamily))
                {
                    pText.Append($" face=\"{style.FontFamily}\"");
                }

                if (!string.IsNullOrEmpty(style.Color))
                {
                    pText.Append($" color=\"{style.Color}\"");
                }

                pText.Append('>');
            }

            // Italic already emitted here must not be repeated by the children; a sheared ruby
            // *container* has no tag of its own, so that one does keep inheriting.
            pText.Append(ReadParagraph(child, index, style.IsItalic && !italicTag && !italicViaRubyTag));

            if (hasFont)
            {
                pText.Append("</font>");
            }

            for (var i = openTags.Count - 1; i >= 0; i--)
            {
                pText.Append("</").Append(openTags[i]).Append('>');
            }
        }

        public static string RemoveTags(string text)
        {
            return text
                .Replace("<bouten-dot-before>", string.Empty)
                .Replace("</bouten-dot-before>", string.Empty)

                .Replace("<bouten-dot-after>", string.Empty)
                .Replace("</bouten-dot-after>", string.Empty)

                .Replace("<bouten-dot-outside>", string.Empty)
                .Replace("</bouten-dot-outside>", string.Empty)

                .Replace("<bouten-filled-circle-outside>", string.Empty)
                .Replace("</bouten-filled-circle-outside>", string.Empty)

                .Replace("<bouten-open-circle-outside>", string.Empty)
                .Replace("</bouten-open-circle-outside>", string.Empty)

                .Replace("<bouten-open-dot-outside>", string.Empty)
                .Replace("</bouten-open-dot-outside>", string.Empty)

                .Replace("<bouten-filled-sesame-outside>", string.Empty)
                .Replace("</bouten-filled-sesame-outside>", string.Empty)

                .Replace("<bouten-open-sesame-outside>", string.Empty)
                .Replace("</bouten-open-sesame-outside>", string.Empty)

                .Replace("<bouten-auto-outside>", string.Empty)
                .Replace("</bouten-auto-outside>", string.Empty)

                .Replace("<bouten-auto>", string.Empty)
                .Replace("</bouten-auto>", string.Empty)

                .Replace("<horizontalDigit>", string.Empty)
                .Replace("</horizontalDigit>", string.Empty)

                .Replace("<ruby-container>", string.Empty)
                .Replace("</ruby-container>", string.Empty)

                .Replace("<ruby-base>", string.Empty)
                .Replace("</ruby-base>", string.Empty)

                .Replace("<ruby-base-italic>", string.Empty)
                .Replace("</ruby-base-italic>", string.Empty)

                .Replace("<ruby-text>", string.Empty)
                .Replace("</ruby-text>", string.Empty)

                .Replace("<ruby-text-after>", string.Empty)
                .Replace("</ruby-text-after>", string.Empty)

                .Replace("<ruby-text-italic>", string.Empty)
                .Replace("</ruby-text-italic>", string.Empty);
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = RemoveTags(p.Text);
            }
        }
    }
}
