using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// A piece of a YouTube timed text line with one formatting - what becomes one s element.
    /// </summary>
    public class YttRun
    {
        public string Text { get; set; } = string.Empty;

        /// <summary>Pen of an s tag - null means the line pen.</summary>
        public int? PenId { get; set; }

        /// <summary>Karaoke start (s t attribute), milliseconds from the start of the line.</summary>
        public int? StartOffset { get; set; }

        /// <summary>Other s attributes, kept as read.</summary>
        public List<KeyValuePair<string, string>> OtherAttributes { get; set; } = new List<KeyValuePair<string, string>>();

        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }

        /// <summary>Color of a font tag around the run, "#RRGGBB".</summary>
        public string Color { get; set; }

        public bool HasHtmlFormatting => Bold || Italic || Underline || Color != null;

        internal bool HasSameSpan(YttRun other)
        {
            return PenId == other.PenId &&
                   StartOffset == other.StartOffset &&
                   OtherAttributes.SequenceEqual(other.OtherAttributes) &&
                   Bold == other.Bold &&
                   Italic == other.Italic &&
                   Underline == other.Underline &&
                   Color == other.Color;
        }
    }

    /// <summary>
    /// The text of a YouTube timed text line as Subtitle Edit keeps it: plain text in the line pen,
    /// s tags (<c>&lt;s p="3" t="450"&gt;...&lt;/s&gt;</c>) for parts in another pen or with karaoke timing,
    /// plus the usual i/b/u/font tags, which become pens on save.
    /// </summary>
    public static class YttText
    {
        public const string ZeroWidthSpace = "\u200B";

        /// <summary>The space YTSubConverter pads lines with, wrapped in zero-width spaces so it can be found again.</summary>
        private const string PaddingSpace = "\u200B \u200B";

        private static readonly Regex AttributeRegex = new Regex("([A-Za-z_:][-A-Za-z0-9_:.]*)\\s*=\\s*(?:\"([^\"]*)\"|'([^']*)'|([^\\s>]+))", RegexOptions.Compiled);
        private static readonly Regex AssaAlignmentRegex = new Regex(@"\\an([1-9])", RegexOptions.Compiled);

        /// <summary>
        /// Text of a p element. s elements become s tags when they carry their own pen or karaoke timing;
        /// the rest become plain text. The per-word s elements of YouTube's auto captions have timing too,
        /// but they are recognized by their confidence (ac) attribute and also become plain text.
        /// </summary>
        public static string FromXml(XmlElement pElement, int? linePen)
        {
            var sb = new StringBuilder();
            foreach (XmlNode node in pElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.SignificantWhitespace || node.NodeType == XmlNodeType.Whitespace || node.NodeType == XmlNodeType.CDATA)
                {
                    sb.Append(CleanText(node.Value));
                }
                else if (node is XmlElement element)
                {
                    var text = CleanText(element.InnerText);
                    var pen = YttHead.GetInt(element, "p");
                    var hasOwnPen = pen != null && pen != linePen;
                    var isKaraoke = element.Attributes["t"] != null && element.Attributes["ac"] == null;
                    if (element.LocalName != "s" || (!hasOwnPen && !isKaraoke))
                    {
                        sb.Append(text);
                        continue;
                    }

                    if (text.Length == 0 && element.Attributes["t"] == null)
                    {
                        continue; // YTSubConverter padding
                    }

                    sb.Append("<s");
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        if (attribute.Name != "p" || hasOwnPen)
                        {
                            YttHead.AppendAttribute(sb, attribute.Name, attribute.Value); // the line pen is implied
                        }
                    }

                    sb.Append('>').Append(text).Append("</s>");
                }
            }

            // pretty-printed files have line breaks around the s elements
            return sb.ToString().Replace("\r\n", "\n").Trim().Replace("\n", Environment.NewLine);
        }

        private static string CleanText(string s)
        {
            return (s ?? string.Empty).Replace(PaddingSpace, string.Empty).Replace(ZeroWidthSpace, string.Empty);
        }

        /// <summary>
        /// Splits a line's text into runs. ASSA override blocks are dropped; an \an tag in them is returned
        /// as <paramref name="assaAlignment"/>. Line breaks stay in the run text as "\n".
        /// </summary>
        public static List<YttRun> Parse(string text, out int? assaAlignment)
        {
            assaAlignment = null;
            var runs = new List<YttRun>();
            if (string.IsNullOrEmpty(text))
            {
                return runs;
            }

            text = text.Replace("\r\n", "\n").Replace("\\N", "\n");
            var bold = 0;
            var italic = 0;
            var underline = 0;
            var colors = new Stack<string>();
            YttRun span = null; // attributes of the open s tag
            var current = new StringBuilder();

            void Flush()
            {
                if (current.Length == 0)
                {
                    return;
                }

                var run = new YttRun
                {
                    Text = current.ToString(),
                    Bold = bold > 0,
                    Italic = italic > 0,
                    Underline = underline > 0,
                    Color = colors.Count > 0 ? colors.Peek() : null,
                };
                if (span != null)
                {
                    run.PenId = span.PenId;
                    run.StartOffset = span.StartOffset;
                    run.OtherAttributes = span.OtherAttributes;
                }

                var last = runs.LastOrDefault();
                if (last != null && last.HasSameSpan(run))
                {
                    last.Text += run.Text;
                }
                else
                {
                    runs.Add(run);
                }

                current.Clear();
            }

            var i = 0;
            while (i < text.Length)
            {
                var ch = text[i];
                if (ch == '{' && i + 1 < text.Length && text[i + 1] == '\\')
                {
                    var end = text.IndexOf('}', i);
                    if (end > 0)
                    {
                        var match = AssaAlignmentRegex.Match(text.Substring(i, end - i));
                        if (match.Success)
                        {
                            assaAlignment = match.Groups[1].Value[0] - '0';
                        }

                        i = end + 1;
                        continue;
                    }
                }

                if (ch == '<')
                {
                    var end = text.IndexOf('>', i);
                    if (end > 0)
                    {
                        var tag = text.Substring(i + 1, end - i - 1).Trim();
                        var lower = tag.ToLowerInvariant();
                        var handled = true;
                        if (lower == "i" || lower == "b" || lower == "u" || lower == "/i" || lower == "/b" || lower == "/u" || lower == "/font" || lower == "/s")
                        {
                            Flush();
                            switch (lower)
                            {
                                case "i": italic++; break;
                                case "/i": italic = Math.Max(0, italic - 1); break;
                                case "b": bold++; break;
                                case "/b": bold = Math.Max(0, bold - 1); break;
                                case "u": underline++; break;
                                case "/u": underline = Math.Max(0, underline - 1); break;
                                case "/font":
                                    if (colors.Count > 0)
                                    {
                                        colors.Pop();
                                    }

                                    break;
                                case "/s": span = null; break;
                            }
                        }
                        else if (lower.StartsWith("font ", StringComparison.Ordinal))
                        {
                            Flush();
                            var color = colors.Count > 0 ? colors.Peek() : null;
                            foreach (var attribute in ParseAttributes(tag.Substring(5)))
                            {
                                if (attribute.Key.Equals("color", StringComparison.OrdinalIgnoreCase))
                                {
                                    color = NormalizeColor(attribute.Value) ?? color;
                                }
                            }

                            colors.Push(color);
                        }
                        else if (lower == "s" || lower.StartsWith("s ", StringComparison.Ordinal))
                        {
                            Flush();
                            span = new YttRun();
                            foreach (var attribute in ParseAttributes(tag.Substring(1)))
                            {
                                if (attribute.Key == "p")
                                {
                                    span.PenId = YttHead.ParseInt(attribute.Value);
                                }
                                else if (attribute.Key == "t")
                                {
                                    span.StartOffset = YttHead.ParseInt(attribute.Value);
                                }
                                else
                                {
                                    span.OtherAttributes.Add(attribute);
                                }
                            }
                        }
                        else
                        {
                            handled = false;
                        }

                        if (handled)
                        {
                            i = end + 1;
                            continue;
                        }
                    }
                }

                current.Append(ch);
                i++;
            }

            Flush();
            return runs;
        }

        public static List<KeyValuePair<string, string>> ParseAttributes(string s)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (Match match in AttributeRegex.Matches(s))
            {
                var value = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Success ? match.Groups[3].Value : match.Groups[4].Value;
                list.Add(new KeyValuePair<string, string>(match.Groups[1].Value, value));
            }

            return list;
        }

        /// <summary>"#RRGGBB" for an HTML color (name, #rgb, #rrggbb, #aarrggbb...), null if not a color.</summary>
        public static string NormalizeColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return null;
            }

            try
            {
                var c = HtmlUtil.GetColorFromString(color.Trim());
                if (c == SKColors.White && !IsWhite(color))
                {
                    return null; // GetColorFromString falls back to white
                }

                return ToHtml(c);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsWhite(string color)
        {
            var s = color.Trim().TrimStart('#').ToLowerInvariant();
            return s == "white" || s == "fff" || s == "ffffff" || s == "ffffffff";
        }

        public static string ToHtml(SKColor c)
        {
            return "#" + c.Red.ToString("X2", CultureInfo.InvariantCulture) + c.Green.ToString("X2", CultureInfo.InvariantCulture) + c.Blue.ToString("X2", CultureInfo.InvariantCulture);
        }

        public static SKColor FromHtml(string color, SKColor fallback)
        {
            if (string.IsNullOrEmpty(color))
            {
                return fallback;
            }

            try
            {
                return ColorTranslator.FromHtml(color.StartsWith("#", StringComparison.Ordinal) ? color : "#" + color);
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// The pen a run is drawn with: its own pen or the line pen, with the run's i/b/u/font
        /// formatting on top. Never null.
        /// </summary>
        public static YttPen GetEffectivePen(YttHead head, YttRun run, int? linePen)
        {
            var basePen = head.GetPen(run.PenId ?? linePen);
            var pen = basePen?.Clone() ?? new YttPen();
            if (run.Bold)
            {
                pen.Bold = true;
            }

            if (run.Italic)
            {
                pen.Italic = true;
            }

            if (run.Underline)
            {
                pen.Underline = true;
            }

            if (run.Color != null)
            {
                pen.ForeColor = run.Color;
                if (pen.ForeOpacity == null)
                {
                    pen.ForeOpacity = YttPen.DefaultForeOpacity;
                }
            }

            return pen;
        }

        /// <summary>
        /// Text with pens turned into i/b/u/font tags and s tags removed - for formats that have no pens.
        /// </summary>
        public static string ToHtml(string text, YttHead head, int? linePen)
        {
            var runs = Parse(text, out _);
            var sb = new StringBuilder();
            foreach (var run in runs)
            {
                var pen = GetEffectivePen(head, run, linePen);
                var runText = run.Text;
                var color = pen.ForeColor;
                if (color != null && (string.Equals(color, YttPen.DefaultForeColor, StringComparison.OrdinalIgnoreCase) || string.Equals(color, "#FEFEFE", StringComparison.OrdinalIgnoreCase)))
                {
                    color = null;
                }

                if (color != null)
                {
                    runText = "<font color=\"" + color + "\">" + runText + "</font>";
                }

                if (pen.Underline == true)
                {
                    runText = "<u>" + runText + "</u>";
                }

                if (pen.Bold == true)
                {
                    runText = "<b>" + runText + "</b>";
                }

                if (pen.Italic == true)
                {
                    runText = "<i>" + runText + "</i>";
                }

                sb.Append(runText);
            }

            return sb.ToString()
                .Replace("</i><i>", string.Empty)
                .Replace("</b><b>", string.Empty)
                .Replace("</u><u>", string.Empty)
                .Replace("\n", Environment.NewLine);
        }

        /// <summary>Text without any tags - what is shown.</summary>
        public static string GetPlainText(string text)
        {
            return string.Concat(Parse(text, out _).Select(r => r.Text)).Replace("\n", Environment.NewLine);
        }
    }
}
