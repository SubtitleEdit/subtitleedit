using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Renders the Japanese profile markup Subtitle Edit uses for "Netflix IMSC 1.1 Japanese"
    /// (&lt;ruby-*&gt;, &lt;bouten-*&gt;, &lt;horizontalDigit&gt;) as ASSA that libass can actually draw.
    /// libass knows nothing about ruby, emphasis marks or vertical writing, so each cue is exploded
    /// into several absolutely positioned render lines: the text itself, plus one line per furigana
    /// or bouten run placed above (horizontal) or beside (vertical) the character it belongs to.
    /// The horizontal offset of those extra lines is not measured - the preceding text is repeated
    /// at {\alpha&amp;FF&amp;} so libass advances the pen itself and the mark lands over the right glyph.
    ///
    /// Without this the tags reach libass verbatim and get drawn as literal text (issue #13861).
    /// Ported from Subtitle Edit 4, with the GDI+ text measuring replaced by a font independent
    /// estimate and the hard coded 720p metrics scaled to the video size.
    /// </summary>
    public static class NetflixImsc11JapaneseToAss
    {
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

        /// <summary>
        /// Subtitle Edit 4's layout constants were eyeballed against 720p with a 40 pixel font;
        /// keeping that ratio reproduces them exactly at 720p and scales everywhere else.
        /// </summary>
        private sealed class Metrics
        {
            internal Metrics(int height)
            {
                FontSize = Math.Max(20, (int)Math.Round(height / 18.0));
                RubyFontSize = Math.Max(10, FontSize / 2);
            }

            internal int FontSize { get; }
            internal int RubyFontSize { get; }

            /// <summary>Scales a value that was tuned against a 40 pixel font.</summary>
            internal int Scale(double valueAtFontSize40) => (int)Math.Round(valueAtFontSize40 * FontSize / 40.0);

            internal int LineHeight => Scale(34);
        }

        /// <summary>
        /// Japanese text is almost entirely full width, so a character class estimate beats measuring
        /// with whatever font happens to be installed - and never returns zero for missing CJK glyphs.
        /// </summary>
        private static double EstimateTextWidth(string text, int fontSize)
        {
            var width = 0.0;
            foreach (var ch in text)
            {
                width += IsFullWidth(ch) ? fontSize : fontSize * 0.5;
            }

            return width;
        }

        private static bool IsFullWidth(char ch)
        {
            return ch >= 0x1100 && (
                ch <= 0x115F || // Hangul Jamo
                ch == 0x2329 || ch == 0x232A ||
                ch >= 0x2E80 && ch <= 0xA4CF && ch != 0x303F || // CJK radicals, kana, CJK ideographs
                ch >= 0xAC00 && ch <= 0xD7A3 || // Hangul syllables
                ch >= 0xF900 && ch <= 0xFAFF || // CJK compatibility ideographs
                ch >= 0xFE10 && ch <= 0xFE19 || // vertical forms
                ch >= 0xFE30 && ch <= 0xFE6F || // CJK compatibility forms
                ch >= 0xFF00 && ch <= 0xFF60 || // full width forms
                ch >= 0xFFE0 && ch <= 0xFFE6);
        }

        /// <summary>
        /// True when the subtitle carries the Japanese profile markup libass cannot draw by itself.
        /// The format alone does not say: "Lambda Cap" decodes furigana, emphasis marks and
        /// tate-chu-yoko to the same markup, and its control codes would otherwise end up on screen
        /// as literal text (issue #14165).
        /// </summary>
        public static bool HasJapaneseMarkup(Subtitle subtitle)
        {
            if (subtitle == null)
            {
                return false;
            }

            foreach (var p in subtitle.Paragraphs)
            {
                var text = p.Text;
                if (!string.IsNullOrEmpty(text) && text.Contains('<') &&
                    (text.Contains("<ruby-", StringComparison.Ordinal) ||
                     text.Contains("<bouten-", StringComparison.Ordinal) ||
                     text.Contains("<horizontalDigit>", StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Paragraph> SplitToAssRenderLines(Paragraph p, int width, int height)
        {
            var metrics = new Metrics(height);
            if (p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal)) // vertical text
            {
                return MakeVerticalParagraphs(p, width, metrics);
            }

            return MakeHorizontalParagraphs(p, width, height, metrics);
        }

        private static List<Paragraph> MakeHorizontalParagraphs(Paragraph p, int width, int height, Metrics metrics)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = metrics.LineHeight;

            // Stack the rows upwards from a fixed bottom margin. Only a cue that actually carries
            // furigana or bouten needs the second row per line - Subtitle Edit 4 reserved it either
            // way and pushed the last line hard against the bottom edge of the video.
            var hasMarkRow = p.Text.Contains("<ruby-", StringComparison.Ordinal) || p.Text.Contains("<bouten-", StringComparison.Ordinal);
            var rowCount = lines.Count * (hasMarkRow ? 2 : 1);
            var startY = height - metrics.Scale(30) - (rowCount - 1) * adjustment;
            if (p.Text.StartsWith("{\\an8", StringComparison.Ordinal))
            {
                startY = metrics.Scale(40);
            }

            var list = new List<Paragraph>();
            var furiganaList = new List<Paragraph>();
            var rubyOn = false;
            var italicOn = false;

            // One left edge for the whole cue: the lines are laid out from it with {\an1}, which is
            // what the region's multiRowAlign="start" asks for, and it keeps the furigana above a
            // line aligned with the line itself. The block as a whole is centered on its widest line.
            var blockWidth = 0.0;
            foreach (var line in lines)
            {
                blockWidth = Math.Max(blockWidth, EstimateTextWidth(GetPlainRenderText(line), metrics.FontSize));
            }

            var startX = (int)Math.Round(width / 2.0 - blockWidth / 2.0);
            if (startX < 0)
            {
                startX = 0;
            }

            if (p.Text.StartsWith("{\\an5", StringComparison.Ordinal))
            {
                startY = (int)Math.Round(height / 2.0 - metrics.FontSize / 2.0);
            }

            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                if (italicOn)
                {
                    line = "<i>" + line;
                }

                var actual = new StringBuilder();
                var i = 0;
                while (i < line.Length)
                {
                    if (line.StartsWithAt(i, "{\\", StringComparison.Ordinal))
                    {
                        var end = line.IndexOf('}', i);
                        if (end < 0)
                        {
                            break;
                        }

                        i = end + 1;
                    }
                    else if (StartsWithAnyTag(line, i, "<i>", "<u>", "<b>"))
                    {
                        actual.Append("{\\i1}");
                        i += 3;
                        italicOn = true;
                    }
                    else if (StartsWithAnyTag(line, i, "</i>", "</u>", "</b>"))
                    {
                        actual.Append("{\\i0}");
                        i += 4;
                        italicOn = false;
                    }
                    else if (line.StartsWithAt(i, "<horizontalDigit>", StringComparison.Ordinal))
                    {
                        // Tate-chu-yoko only means something in a vertical column - horizontal digits
                        // are already horizontal, so just drop the tags (issue #14165).
                        i += "<horizontalDigit>".Length;
                    }
                    else if (line.StartsWithAt(i, "</horizontalDigit>", StringComparison.Ordinal))
                    {
                        i += "</horizontalDigit>".Length;
                    }
                    else if (line.StartsWithAt(i, "<bouten-", StringComparison.Ordinal))
                    {
                        var end = line.IndexOf('>', i);
                        if (end < 0 || end + 1 >= line.Length)
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
                            var furiganaChar = BoutenTagToUnicode(tag);
                            if (!string.IsNullOrWhiteSpace(furiganaChar))
                            {
                                furiganaList.Add(new Paragraph($"{{\\alpha&FF&}}{actual}{{\\alpha&0&}}{furiganaChar}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                            }

                            actual.Append(ch);
                        }

                        var endTagEnd = line.IndexOf('>', endTagStart);
                        if (endTagEnd < 0)
                        {
                            break;
                        }

                        i = endTagEnd + 1;
                    }
                    else if (line.StartsWithAt(i, "<ruby-container>", StringComparison.Ordinal))
                    {
                        if (!TryReadRuby(line, i, out var baseText, out var rubyText, out var rubyTextAfter, out var next))
                        {
                            break;
                        }

                        var preFurigana = string.Empty;
                        if (actual.Length > 0)
                        {
                            preFurigana = $"{{\\alpha&FF&}}{actual.ToString().TrimEnd()}{{\\alpha&0&}}";
                        }

                        if (!string.IsNullOrWhiteSpace(rubyText))
                        {
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs{metrics.RubyFontSize}}}{rubyText}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }

                        if (!string.IsNullOrWhiteSpace(rubyTextAfter))
                        {
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs{metrics.RubyFontSize}}} {rubyTextAfter}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }

                        actual.Append(baseText);
                        i = next;
                        rubyOn = true;
                    }
                    else
                    {
                        actual.Append(line[i]);
                        i++;
                    }
                }

                var actualText = actual.ToString().TrimEnd();
                var displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furiganaList.Count > 0)
                {
                    foreach (var fp in furiganaList)
                    {
                        list.Add(new Paragraph("{\\an1}{\\pos(" + startX + "," + startY + ")}" + fp.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }

                    startY += adjustment;
                    if (rubyOn && index == 0 && lines.Count == 2)
                    {
                        startY += metrics.Scale(3);
                    }
                }

                actualText = "{\\an1}{\\pos(" + startX + "," + startY + ")}" + actualText;
                list.Add(new Paragraph(actualText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                startY += adjustment;

                if (!displayBefore && furiganaList.Count > 0)
                {
                    if (rubyOn && index == 1 && lines.Count == 2)
                    {
                        startY = (int)(startY - adjustment * 0.4);
                    }

                    foreach (var fp in furiganaList)
                    {
                        list.Add(new Paragraph("{\\an1}{\\pos(" + startX + "," + startY + ")}" + fp.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }

                    startY += adjustment;
                }

                furiganaList.Clear();
            }

            return list;
        }

        private static List<Paragraph> MakeVerticalParagraphs(Paragraph p, int width, Metrics metrics)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = metrics.LineHeight;
            var leftAlign = p.Text.StartsWith("{\\an7}", StringComparison.Ordinal);
            var startX = leftAlign
                ? metrics.Scale(9) + lines.Count * 2 * adjustment
                : width - metrics.Scale(50);
            var textY = metrics.Scale(40);
            var furiganaY = metrics.Scale(45);

            var pre = p.Text.Substring(0, 5);
            var list = new List<Paragraph>();
            var furiganaList = new List<Paragraph>();
            var rubyOn = false;
            var italicOn = false;
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var actual = new StringBuilder();
                var i = 0;
                if (italicOn)
                {
                    line = "<i>" + line;
                }

                while (i < line.Length)
                {
                    if (line.StartsWithAt(i, "{\\", StringComparison.Ordinal))
                    {
                        var end = line.IndexOf('}', i);
                        if (end < 0)
                        {
                            break;
                        }

                        i = end + 1;
                    }
                    else if (StartsWithAnyTag(line, i, "<i>", "<u>", "<b>"))
                    {
                        actual.Append("{\\i1}");
                        i += 3;
                        italicOn = true;
                    }
                    else if (StartsWithAnyTag(line, i, "</i>", "</u>", "</b>"))
                    {
                        actual.Append("{\\i0}");
                        i += 4;
                        italicOn = false;
                    }
                    else if (line.StartsWithAt(i, "<horizontalDigit>", StringComparison.Ordinal))
                    {
                        // Tate-chu-yoko: the digits stay side by side inside the vertical column.
                        var end = line.IndexOf('>', i);
                        if (end < 0)
                        {
                            break;
                        }

                        var endTagStart = line.IndexOf("</", end, StringComparison.Ordinal);
                        if (endTagStart < 0)
                        {
                            break;
                        }

                        actual.Append(line.Substring(end + 1, endTagStart - end - 1));
                        actual.AppendLine();
                        i = endTagStart + "</horizontalDigit>".Length;
                    }
                    else if (line.StartsWithAt(i, "</horizontalDigit>", StringComparison.Ordinal))
                    {
                        i += "</horizontalDigit>".Length;
                    }
                    else if (line.StartsWithAt(i, "<bouten-", StringComparison.Ordinal))
                    {
                        var end = line.IndexOf('>', i);
                        if (end < 0 || end + 1 >= line.Length)
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
                            var furiganaChar = BoutenTagToUnicode(tag);
                            if (!string.IsNullOrWhiteSpace(furiganaChar))
                            {
                                var preFurigana = string.Empty;
                                if (actual.Length > 0)
                                {
                                    preFurigana = $"{{\\alpha&FF&}}{actual}{{\\alpha&0&}}";
                                }

                                furiganaList.Add(new Paragraph($"{preFurigana}{furiganaChar}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                            }

                            actual.Append(ch);
                            actual.AppendLine();
                        }

                        var endTagEnd = line.IndexOf('>', endTagStart);
                        if (endTagEnd < 0)
                        {
                            break;
                        }

                        i = endTagEnd + 1;
                    }
                    else if (line.StartsWithAt(i, "<ruby-container>", StringComparison.Ordinal))
                    {
                        if (!TryReadRuby(line, i, out var baseText, out var rubyText, out var rubyTextAfter, out var next))
                        {
                            break;
                        }

                        var preFurigana = string.Empty;
                        if (actual.Length > 0)
                        {
                            preFurigana = $"{{\\alpha&FF&}}{actual.ToString().TrimEnd()}{{\\alpha&0&}}";
                        }

                        if (!string.IsNullOrWhiteSpace(rubyText))
                        {
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs{metrics.RubyFontSize}}}{OneCharPerLine(rubyText)}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }

                        if (!string.IsNullOrWhiteSpace(rubyTextAfter))
                        {
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs{metrics.RubyFontSize}}} {OneCharPerLine(rubyTextAfter)}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }

                        foreach (var ch in baseText)
                        {
                            actual.Append(ch);
                            actual.AppendLine();
                        }

                        i = next;
                        rubyOn = true;
                    }
                    else
                    {
                        actual.AppendLine(line[i].ToString());
                        i++;
                    }
                }

                var displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furiganaList.Count > 0)
                {
                    foreach (var fp in furiganaList)
                    {
                        list.Add(new Paragraph(pre + "\\pos(" + startX + "," + furiganaY + ")}" + fp.Text.TrimEnd(), p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }

                    startX -= adjustment;
                    if (rubyOn && index == 0 && lines.Count == 2)
                    {
                        startX += leftAlign ? -metrics.Scale(8) : metrics.Scale(16);
                    }
                }

                var actualText = pre + "\\pos(" + startX + "," + textY + ")}" + ToVerticalGlyphs(actual.ToString().TrimEnd());
                list.Add(new Paragraph(actualText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                startX -= adjustment;

                if (!displayBefore && furiganaList.Count > 0)
                {
                    if (rubyOn && index == 1 && lines.Count == 2)
                    {
                        startX += leftAlign ? metrics.Scale(14) : -metrics.Scale(8);
                    }

                    foreach (var fp in furiganaList)
                    {
                        list.Add(new Paragraph(pre + "\\pos(" + startX + "," + furiganaY + ")}" + fp.Text.TrimEnd(), p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }

                    startX -= adjustment;
                }

                furiganaList.Clear();
            }

            return list;
        }

        /// <summary>Punctuation that has a dedicated rotated glyph when written top to bottom.</summary>
        private static string ToVerticalGlyphs(string text)
        {
            return text
                .Replace('…', '⋮')
                .Replace('〈', '︿')
                .Replace('〉', '﹀')
                .Replace('—', '︱') // em dash
                .Replace('⸺', '︱') // double em dash (could not find a vertical double em dash)
                .Replace('ー', '⏐') // prolonged sound mark
                .Replace('（', '︵')
                .Replace('）', '︶');
        }

        /// <summary>
        /// The text that actually ends up on the base line - markup gone, and the furigana dropped
        /// (it is drawn on its own render line, so it must not count towards the line width).
        /// </summary>
        private static string GetPlainRenderText(string line)
        {
            var withoutRubyText = RubyTextRegex.Replace(line, string.Empty);
            return NetflixImsc11Japanese.RemoveTags(HtmlUtil.RemoveHtmlTags(withoutRubyText, true));
        }

        private static readonly System.Text.RegularExpressions.Regex RubyTextRegex =
            new System.Text.RegularExpressions.Regex("<ruby-text(-italic|-after)?>.*?</ruby-text(-italic|-after)?>", System.Text.RegularExpressions.RegexOptions.Compiled);

        private static string OneCharPerLine(string text)
        {
            var sb = new StringBuilder();
            foreach (var ch in text)
            {
                sb.Append(ch);
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static bool StartsWithAnyTag(string line, int index, params string[] tags)
        {
            foreach (var tag in tags)
            {
                if (string.CompareOrdinal(line, index, tag, 0, tag.Length) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads one &lt;ruby-container&gt; run starting at <paramref name="index"/>, returning the base
        /// text, the furigana, and where parsing should continue.
        /// </summary>
        private static bool TryReadRuby(string line, int index, out string baseText, out string rubyText, out string rubyTextAfter, out int next)
        {
            baseText = string.Empty;
            rubyText = string.Empty;
            rubyTextAfter = string.Empty;
            next = index;

            var containerEnd = line.IndexOf("</ruby-container>", index, StringComparison.Ordinal);
            if (containerEnd < 0)
            {
                return false;
            }

            if (!TryReadTagContent(line, index, containerEnd, "ruby-base", out baseText) &&
                !TryReadTagContent(line, index, containerEnd, "ruby-base-italic", out baseText))
            {
                return false;
            }

            if (!TryReadTagContent(line, index, containerEnd, "ruby-text", out rubyText))
            {
                TryReadTagContent(line, index, containerEnd, "ruby-text-italic", out rubyText);
            }

            TryReadTagContent(line, index, containerEnd, "ruby-text-after", out rubyTextAfter);

            next = containerEnd + "</ruby-container>".Length;
            return true;
        }

        private static bool TryReadTagContent(string line, int from, int to, string tagName, out string content)
        {
            content = string.Empty;
            var open = "<" + tagName + ">";
            var close = "</" + tagName + ">";
            var start = line.IndexOf(open, from, StringComparison.Ordinal);
            if (start < 0 || start >= to)
            {
                return false;
            }

            start += open.Length;
            var end = line.IndexOf(close, start, StringComparison.Ordinal);
            if (end < 0 || end > to)
            {
                return false;
            }

            content = line.Substring(start, end - start);
            return true;
        }

        public static string Convert(Subtitle subtitle, int width, int height)
        {
            return ConvertToSubtitle(subtitle, width, height).ToText(new AdvancedSubStationAlpha());
        }

        /// <summary>
        /// Same as <see cref="Convert"/>, but stops before serializing so callers can still add to the
        /// result - the video preview grafts the secondary subtitle onto it.
        /// </summary>
        public static Subtitle ConvertToSubtitle(Subtitle subtitle, int width, int height)
        {
            if (width <= 0)
            {
                width = 1280;
            }

            if (height <= 0)
            {
                height = 720;
            }

            var metrics = new Metrics(height);
            var finalSub = new Subtitle();
            foreach (var paragraph in subtitle.Paragraphs)
            {
                finalSub.Paragraphs.AddRange(SplitToAssRenderLines(paragraph, width, height));
            }

            var style = new SsaStyle { FontSize = metrics.FontSize, Bold = false };
            var header = string.Format(AdvancedSubStationAlpha.HeaderNoStyles, string.Empty, style.ToRawAss());
            header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + width.ToString(CultureInfo.InvariantCulture), "[Script Info]", header);
            header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + height.ToString(CultureInfo.InvariantCulture), "[Script Info]", header);
            finalSub.Header = header;

            return finalSub;
        }
    }
}
