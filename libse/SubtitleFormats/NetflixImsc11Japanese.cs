using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class NetflixImsc11Japanese : SubtitleFormat
    {
        public override string Extension => ".xml";
        public override string Name => "Netflix IMSC 1.1 Japanese";

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

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var text = sb.ToString();
            if (!text.Contains("lang=\"ja\"", StringComparison.Ordinal) || !text.Contains("bouten-", StringComparison.Ordinal))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
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
                text = SetFuriganaDots(text);
                var paragraphContent = new XmlDocument();
                paragraphContent.LoadXml($"<root>{text.Replace("&", "&amp;")}</root>");
                TimedText10.ConvertParagraphNodeToTtmlNode(paragraphContent.DocumentElement, xml, paragraph);
            }
            catch // Wrong markup, clear it
            {
                text = Regex.Replace(text, "[<>]", "");
                paragraph.AppendChild(xml.CreateTextNode(text));
            }

            return paragraph;
        }

        private static string SetFuriganaDots(string text)
        {
            return text
                .Replace("<bouten-dot-before>", "<span style=\"bouten-dot-before\">")
                .Replace("</bouten-dot-before>", "</span>")

                .Replace("<bouten-dot-after>", "<span style=\"bouten-dot-after\">")
                .Replace("</bouten-dot-after>", "</span>")

                .Replace("<bouten-dot-outside>", "<span style=\"bouten-dot-outside\">")
                .Replace("</bouten-dot-outside>", "</span>")

                .Replace("<bouten-filled-circle-outside>", "<span style=\"bouten-filled-circle-outside\">")
                .Replace("</bouten-filled-circle-outside>", "</span>")

                .Replace("<bouten-open-circle-outside>", "<span style=\"bouten-open-circle-outside\">")
                .Replace("</bouten-open-circle-outside>", "</span>")

                .Replace("<bouten-open-dot-outside>", "<span style=\"bouten-open-dot-outside\">")
                .Replace("</bouten-open-dot-outside>", "</span>")

                .Replace("<bouten-filled-sesame-outside>", "<span style=\"bouten-filled-sesame-outside\">")
                .Replace("</bouten-filled-sesame-outside>", "</span>")

                .Replace("<bouten-open-sesame-outside>", "<span style=\"bouten-open-sesame-outside\">")
                .Replace("</bouten-open-sesame-outside>", "</span>")

                .Replace("<bouten-auto-outside>", "<span style=\"bouten-auto-outside\">")
                .Replace("</bouten-auto-outside>", "</span>")

                .Replace("<bouten-auto>", "<span style=\"bouten-auto\">")
                .Replace("</bouten-auto>", "</span>");
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

        private string GetAssStyleFromRegion(string region)
        {
            switch (region)
            {
                case "top-center-justified": return @"{\an8}";
                case "force-narrative-example-region": return @"{\an5}";
                case "left": return @"{\an7}"; // @"{\an4\frz-90\fn@}";
                case "right": return @"{\an9}"; // @"{\an6\frz-90\fn@}";
                default: return string.Empty;
            }
        }

        private static List<string> GetStyles()
        {
            return TimedText10.GetStylesFromHeader(GetXmlStructure());
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                xml.LoadXml(sb.ToString().Replace(" & ", " &amp; ").Replace("Q&A", "Q&amp;A").RemoveControlCharactersButWhiteSpace().Trim());
            }

            var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
            if (frameRateAttr != null)
            {
                if (double.TryParse(frameRateAttr.Value, out var fr))
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
                                fr = double.Parse(arr[0]) / double.Parse(arr[1]);
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
            subtitle.Header = sb.ToString();

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);
            foreach (XmlNode node in body.SelectNodes("//ttml:p", namespaceManager))
            {
                TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);
                var assStyle = string.Empty;
                var region = node.Attributes?["region"];
                if (region != null)
                {
                    assStyle = GetAssStyleFromRegion(region.InnerText);
                }

                var text = assStyle + ReadParagraph(node, xml);
                var p = new Paragraph(begin, end, text);
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static string ReadParagraph(XmlNode node, XmlDocument xml)
        {
            var pText = new StringBuilder();
            var styles = GetStyles();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    pText.Append(child.Value);
                }
                else if (child.Name == "br" || child.Name == "tt:br")
                {
                    pText.AppendLine();
                }
                else if (child.Name == "#significant-whitespace" || child.Name == "tt:#significant-whitespace")
                {
                    pText.Append(child.InnerText);
                }
                else if (child.Name == "span" || child.Name == "tt:span")
                {
                    bool isItalic = false;
                    bool isBold = false;
                    bool isUnderlined = false;
                    string fontFamily = null;
                    string color = null;
                    bool boutenDotBefore = false;
                    bool boutenDotAfter = false;
                    bool boutenDotOutside = false;
                    bool boutenFilledCircleOutside = false;
                    bool boutenOpenCircleOutside = false;
                    bool boutenOpenDotOutside = false;
                    bool boutenFilledSesameOutside = false;
                    bool boutenOpenSesameOutside = false;
                    bool boutenAutoOutside = false;
                    bool boutenAuto = false;

                    // Composing styles

                    if (child.Attributes["style"] != null)
                    {
                        string styleName = child.Attributes["style"].Value;
                        if (styleName == "bouten-dot-before")
                        {
                            boutenDotBefore = true;
                        }
                        else if (styleName == "bouten-dot-after")
                        {
                            boutenDotAfter = true;
                        }
                        else if (styleName == "bouten-dot-outside")
                        {
                            boutenDotOutside = true;
                        }
                        else if (styleName == "bouten-filled-circle-outside")
                        {
                            boutenFilledCircleOutside = true;
                        }
                        else if (styleName == "bouten-open-circle-outside")
                        {
                            boutenOpenCircleOutside = true;
                        }
                        else if (styleName == "bouten-open-dot-outside")
                        {
                            boutenOpenDotOutside = true;
                        }
                        else if (styleName == "bouten-filled-sesame-outside")
                        {
                            boutenFilledSesameOutside = true;
                        }
                        else if (styleName == "bouten-open-sesame-outside")
                        {
                            boutenOpenSesameOutside = true;
                        }
                        else if (styleName == "bouten-auto-outside")
                        {
                            boutenAutoOutside = true;
                        }
                        else if (styleName == "bouten-auto")
                        {
                            boutenAuto = true;
                        }
                        else if (styles.Contains(styleName))
                        {
                            try
                            {
                                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                                XmlNode head = xml.DocumentElement.SelectSingleNode("ttml:head", nsmgr);
                                foreach (XmlNode styleNode in head.SelectNodes("//ttml:style", nsmgr))
                                {
                                    string currentStyle = null;
                                    if (styleNode.Attributes["xml:id"] != null)
                                    {
                                        currentStyle = styleNode.Attributes["xml:id"].Value;
                                    }
                                    else if (styleNode.Attributes["id"] != null)
                                    {
                                        currentStyle = styleNode.Attributes["id"].Value;
                                    }

                                    if (currentStyle == styleName)
                                    {
                                        if (styleNode.Attributes["tts:fontStyle"] != null && styleNode.Attributes["tts:fontStyle"].Value == "italic")
                                        {
                                            isItalic = true;
                                        }

                                        if (styleNode.Attributes["tts:fontWeight"] != null && styleNode.Attributes["tts:fontWeight"].Value == "bold")
                                        {
                                            isBold = true;
                                        }

                                        if (styleNode.Attributes["tts:textDecoration"] != null && styleNode.Attributes["tts:textDecoration"].Value == "underline")
                                        {
                                            isUnderlined = true;
                                        }

                                        if (styleNode.Attributes["tts:fontFamily"] != null)
                                        {
                                            fontFamily = styleNode.Attributes["tts:fontFamily"].Value;
                                        }

                                        if (styleNode.Attributes["tts:color"] != null)
                                        {
                                            color = styleNode.Attributes["tts:color"].Value;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e);
                            }
                        }
                    }

                    if (child.Attributes["tts:fontStyle"] != null && child.Attributes["tts:fontStyle"].Value == "italic")
                    {
                        isItalic = true;
                    }

                    if (child.Attributes["tts:fontWeight"] != null && child.Attributes["tts:fontWeight"].Value == "bold")
                    {
                        isBold = true;
                    }

                    if (child.Attributes["tts:textDecoration"] != null && child.Attributes["tts:textDecoration"].Value == "underline")
                    {
                        isUnderlined = true;
                    }

                    if (child.Attributes["tts:fontFamily"] != null)
                    {
                        fontFamily = child.Attributes["tts:fontFamily"].Value;
                    }

                    if (child.Attributes["tts:color"] != null)
                    {
                        color = child.Attributes["tts:color"].Value;
                    }


                    // Applying styles
                    if (isItalic)
                    {
                        pText.Append("<i>");
                    }

                    if (isBold)
                    {
                        pText.Append("<b>");
                    }

                    if (isUnderlined)
                    {
                        pText.Append("<u>");
                    }



                    if (!string.IsNullOrEmpty(fontFamily) || !string.IsNullOrEmpty(color))
                    {
                        pText.Append("<font");

                        if (!string.IsNullOrEmpty(fontFamily))
                        {
                            pText.Append($" face=\"{fontFamily}\"");
                        }

                        if (!string.IsNullOrEmpty(color))
                        {
                            pText.Append($" color=\"{color}\"");
                        }

                        pText.Append(">");
                    }

                    if (boutenDotBefore)
                    {
                        pText.Append("<bouten-dot-before>");
                    }

                    if (boutenDotAfter)
                    {
                        pText.Append("<bouten-dot-after>");
                    }

                    if (boutenDotOutside)
                    {
                        pText.Append("<bouten-dot-outside>");
                    }

                    if (boutenFilledCircleOutside)
                    {
                        pText.Append("<bouten-filled-circle-outside>");
                    }

                    if (boutenOpenCircleOutside)
                    {
                        pText.Append("<bouten-open-circle-outside>");
                    }

                    if (boutenOpenDotOutside)
                    {
                        pText.Append("<bouten-open-dot-outside>");
                    }

                    if (boutenFilledSesameOutside)
                    {
                        pText.Append("<bouten-filled-sesame-outside>");
                    }

                    if (boutenOpenSesameOutside)
                    {
                        pText.Append("<bouten-open-sesame-outside>");
                    }

                    if (boutenAutoOutside)
                    {
                        pText.Append("<bouten-auto-outside>");
                    }

                    if (boutenAuto)
                    {
                        pText.Append("<bouten-auto>");
                    }

                    pText.Append(ReadParagraph(child, xml));

                    if (!string.IsNullOrEmpty(fontFamily) || !string.IsNullOrEmpty(color))
                    {
                        pText.Append("</font>");
                    }

                    if (isUnderlined)
                    {
                        pText.Append("</u>");
                    }

                    if (isBold)
                    {
                        pText.Append("</b>");
                    }

                    if (isItalic)
                    {
                        pText.Append("</i>");
                    }

                    if (boutenDotBefore)
                    {
                        pText.Append("</bouten-dot-before>");
                    }

                    if (boutenDotAfter)
                    {
                        pText.Append("</bouten-dot-after>");
                    }

                    if (boutenDotOutside)
                    {
                        pText.Append("</bouten-dot-outside>");
                    }

                    if (boutenFilledCircleOutside)
                    {
                        pText.Append("</bouten-filled-circle-outside>");
                    }

                    if (boutenOpenCircleOutside)
                    {
                        pText.Append("</bouten-open-circle-outside>");
                    }

                    if (boutenOpenDotOutside)
                    {
                        pText.Append("</bouten-open-dot-outside>");
                    }

                    if (boutenFilledSesameOutside)
                    {
                        pText.Append("</bouten-filled-sesame-outside>");
                    }

                    if (boutenOpenSesameOutside)
                    {
                        pText.Append("</bouten-open-sesame-outside>");
                    }

                    if (boutenAutoOutside)
                    {
                        pText.Append("</bouten-auto-outside>");
                    }

                    if (boutenAuto)
                    {
                        pText.Append("</bouten-auto>");
                    }
                }
            }

            return pText.ToString().Trim();
        }

        private static string BoutenTagToUnicode(string tag)
        {
            switch (tag)
            {
                case "bouten-dot-before":
                case "bouten-dot-after":
                case "bouten-dot-outside":
                    return "•";
                case "bouten-filled-circle-outside":
                    return "●";
                case "bouten-open-circle-outside":
                    return "○";
                case "bouten-open-dot-outside":
                    return "◦";
                case "bouten-filled-sesame-outside":
                    return "﹅";
                case "bouten-open-sesame-outside":
                    return "﹆";
                case "bouten-auto-outside":
                case "bouten-auto":
                    return "﹅";
                default:
                    return " ";
            }
        }

        public static List<Paragraph> SplitToAssRenderLines(Paragraph p, int width, int height)
        {
            if (p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal)) // vertical text
            {
                return MakeVerticalParagraphs(p, width);
            }

            if (p.Text.Contains("<bouten-", StringComparison.Ordinal))
            {
                return MakeHorizontalParagraphs(p, width, height);
            }

            return new List<Paragraph> { p };
        }

        private static List<Paragraph> MakeHorizontalParagraphs(Paragraph p, int width, int height)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = 25;
            var startY = height - (20 + lines.Count * 2 * adjustment);
            var list = new List<Paragraph>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var furigana = new StringBuilder();
                var actual = new StringBuilder();
                int i = 0;
                while (i < line.Length)
                {
                    if (line.Substring(i).StartsWith("{\\"))
                    {
                        var end = line.IndexOf('}', i);
                        if (end < 0)
                        {
                            break;
                        }

                        i = end + 1;
                    }
                    else if (line.Substring(i).StartsWith("<i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("<u>", StringComparison.Ordinal) || line.Substring(i).StartsWith("<b>", StringComparison.Ordinal))
                    {
                        i += 3;
                    }
                    else if (line.Substring(i).StartsWith("</i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</u>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</b>", StringComparison.Ordinal))
                    {
                        i += 4;
                    }
                    else if (line.Substring(i).StartsWith("<bouten-", StringComparison.Ordinal))
                    {
                        var end = line.IndexOf('>', i);
                        if (end < 0)
                        {
                            break;
                        }

                        if (end + 1 >= line.Length)
                        {
                            break;
                        }

                        var endTagStart = line.IndexOf("</", end, StringComparison.Ordinal);
                        if (endTagStart < 0)
                        {
                            break;
                        }

                        var tag = line.Substring(i + 1, end - i - 1);
                        var text = line.Substring(end + 1, endTagStart - end - 1);
                        foreach (var ch in text)
                        {
                            actual.Append(ch);
                            var furiganaChar = BoutenTagToUnicode(tag);
                        }

                        var endTagEnd = line.IndexOf('>', endTagStart);
                        if (endTagEnd < 0)
                        {
                            break;
                        }

                        i = endTagEnd + 1;
                    }
                    else
                    {
                        furigana.AppendLine(" ");
                        actual.Append(line.Substring(i, 1));
                        i++;
                    }
                }

                int startX = 200;
                bool displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furigana.ToString().Trim().Length > 0)
                {
                    var beforeText = furigana.ToString().TrimEnd();
                    beforeText = "{\\pos(" + startX + "," + startY + ")}" + beforeText;
                    list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    startY += adjustment;
                }

                var actualText = actual.ToString().TrimEnd();
                actualText = "{\\pos(" + startX + "," + startY + ")}" + actualText;

                list.Add(new Paragraph(actualText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                startY += adjustment;

                if (!displayBefore && furigana.ToString().Trim().Length > 0)
                {
                    var beforeText = furigana.ToString().TrimEnd();
                    beforeText = "{\\pos(" + startX + "," + startY + ")}" + beforeText;
                    list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    startY += adjustment;
                }
            }

            return list;
        }


        private static List<Paragraph> MakeVerticalParagraphs(Paragraph p, int width)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = 25;
            var startX = 9 + lines.Count * 2 * adjustment;
            var leftAlign = p.Text.StartsWith("{\\an7}", StringComparison.Ordinal);
            if (!leftAlign)
            {
                startX = width - 50;
            }

            string pre = p.Text.Substring(0, 5);
            var list = new List<Paragraph>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var furigana = new StringBuilder();
                var actual = new StringBuilder();
                int i = 0;
                while (i < line.Length)
                {
                    if (line.Substring(i).StartsWith("{\\"))
                    {
                        var end = line.IndexOf('}', i);
                        if (end < 0)
                        {
                            break;
                        }

                        i = end + 1;
                    }
                    else if (line.Substring(i).StartsWith("<i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("<u>", StringComparison.Ordinal) || line.Substring(i).StartsWith("<b>", StringComparison.Ordinal))
                    {
                        i += 3;
                    }
                    else if (line.Substring(i).StartsWith("</i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</u>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</b>", StringComparison.Ordinal))
                    {
                        i += 4;
                    }
                    else if (line.Substring(i).StartsWith("<bouten-", StringComparison.Ordinal))
                    {
                        var end = line.IndexOf('>', i);
                        if (end < 0)
                        {
                            break;
                        }

                        if (end + 1 >= line.Length)
                        {
                            break;
                        }

                        var endTagStart = line.IndexOf("</", end, StringComparison.Ordinal);
                        if (endTagStart < 0)
                        {
                            break;
                        }

                        var tag = line.Substring(i + 1, end - i - 1);
                        var text = line.Substring(end + 1, endTagStart - end - 1);
                        foreach (var ch in text)
                        {
                            actual.Append(ch);
                            actual.AppendLine();
                            furigana.AppendLine(BoutenTagToUnicode(tag));
                        }

                        var endTagEnd = line.IndexOf('>', endTagStart);
                        if (endTagEnd < 0)
                        {
                            break;
                        }

                        i = endTagEnd + 1;
                    }
                    else
                    {
                        furigana.AppendLine(" ");
                        actual.AppendLine(line.Substring(i, 1));
                        i++;
                    }
                }

                bool displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furigana.ToString().Trim().Length > 0)
                {
                    var beforeText = furigana.ToString().TrimEnd();
                    beforeText = pre + "\\pos(" + startX + ",40)}" + beforeText;
                    list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    startX -= adjustment;
                }

                var actualText = actual.ToString().TrimEnd();
                actualText = pre + "\\pos(" + startX + ",40)}" + actualText;

                // change horizontal chars to vertical version
                actualText = actualText
                        .Replace('…', '⋮')
                        .Replace('〈', '︿')
                        .Replace('〉', '﹀')
                        .Replace('—', '︱') // em dash
                        .Replace('⸺', '︱') // em dash
                        .Replace('ー', '⏐') //  // prolonged sound mark
                        .Replace('（', '︵')
                        .Replace('）', '︶')
                    ;

                list.Add(new Paragraph(actualText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                startX -= adjustment;

                if (!displayBefore && furigana.ToString().Trim().Length > 0)
                {
                    var beforeText = furigana.ToString().TrimEnd();
                    beforeText = pre + "\\pos(" + startX + ",40)}" + beforeText;
                    list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    startX -= adjustment;
                }
            }

            return list;
        }

        public static string ToAss(Subtitle subtitle, int width, int height)
        {
            var finalSub = new Subtitle();
            foreach (var paragraph in subtitle.Paragraphs)
            {
                finalSub.Paragraphs.AddRange(SplitToAssRenderLines(paragraph, width, height));
            }

            var oldFontSize = Configuration.Settings.SubtitleSettings.SsaFontSize;
            var oldFontBold = Configuration.Settings.SubtitleSettings.SsaFontBold;
            Configuration.Settings.SubtitleSettings.SsaFontSize = 30; // font size
            Configuration.Settings.SubtitleSettings.SsaFontBold = false;
            finalSub.Header = AdvancedSubStationAlpha.DefaultHeader;
            Configuration.Settings.SubtitleSettings.SsaFontSize = oldFontSize;
            Configuration.Settings.SubtitleSettings.SsaFontBold = oldFontBold;

            finalSub.Header = finalSub.Header.Replace("PlayDepth: 0", @"PlayDepth: 0
PlayResX: 1280
PlayResY: 720").Replace("1280", width.ToString(CultureInfo.InvariantCulture))
               .Replace("720", height.ToString(CultureInfo.InvariantCulture));

            return finalSub.ToText(new AdvancedSubStationAlpha());
        }
    }
}
