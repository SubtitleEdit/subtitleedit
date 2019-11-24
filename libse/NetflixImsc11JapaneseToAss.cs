using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core
{
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

        public static List<Paragraph> SplitToAssRenderLines(Paragraph p, int width, int height)
        {
            if (p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal)) // vertical text
            {
                return MakeVerticalParagraphs(p, width);
            }

            return MakeHorizontalParagraphs(p, width, height);
        }

        private static List<Paragraph> MakeHorizontalParagraphs(Paragraph p, int width, int height)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = 34;
            var startY = height - lines.Count * 2 * adjustment + 30;
            if (p.Text.StartsWith("{\\an8", StringComparison.Ordinal))
            {
                startY = 40;
            }

            var list = new List<Paragraph>();
            var furiganaList = new List<Paragraph>();
            var rubyOn = false;
            var italinOn = false;
            int startX;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var actualText = NetflixImsc11Japanese.RemoveTags(HtmlUtil.RemoveHtmlTags(p.Text, true));
                var actualTextSize = g.MeasureString(actualText, new Font(SystemFonts.DefaultFont.FontFamily, 20));
                startX = (int)(width / 2.0 - actualTextSize.Width / 2.0);
                if (p.Text.StartsWith("{\\an5", StringComparison.Ordinal))
                {
                    startY = (int)(height / 2.0 - actualTextSize.Height / 2.0);
                }
            }

            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                if (italinOn)
                {
                    line = "<i>" + line;
                }
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
                        actual.Append("{\\i1}");
                        i += 3;
                        italinOn = true;
                    }
                    else if (line.Substring(i).StartsWith("</i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</u>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</b>", StringComparison.Ordinal))
                    {
                        actual.Append("{\\i0}");
                        i += 4;
                        italinOn = false;
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
                    else if (line.Substring(i).StartsWith("<ruby-container>", StringComparison.Ordinal))
                    {
                        var baseTextStart = line.IndexOf("<ruby-base>", i, StringComparison.Ordinal);
                        var baseTextEnd = line.IndexOf("</ruby-base>", i, StringComparison.Ordinal);
                        if (baseTextStart < 0 || baseTextEnd < 0)
                        {
                            baseTextStart = line.IndexOf("<ruby-base-italic>", i, StringComparison.Ordinal);
                            baseTextEnd = line.IndexOf("</ruby-base-italic>", i, StringComparison.Ordinal);
                            if (baseTextStart < 0 || baseTextEnd < 0)
                            {
                                break;
                            }
                        }
                        baseTextStart += "<ruby-base>".Length;
                        var baseText = line.Substring(baseTextStart, baseTextEnd - baseTextStart);

                        var extraText = string.Empty;
                        var extraTextStart = line.IndexOf("<ruby-text>", i, StringComparison.Ordinal);
                        var extraTextEnd = line.IndexOf("</ruby-text>", i, StringComparison.Ordinal);
                        if (extraTextStart >= 0 || extraTextEnd >= 0 && extraTextStart < extraTextEnd)
                        {
                            extraTextStart += "<ruby-text>".Length;
                            extraText = line.Substring(extraTextStart, extraTextEnd - extraTextStart);
                        }

                        if (string.IsNullOrEmpty(extraText))
                        {
                            extraTextStart = line.IndexOf("<ruby-text-italic>", i, StringComparison.Ordinal);
                            extraTextEnd = line.IndexOf("</ruby-text-italic>", i, StringComparison.Ordinal);
                            if (extraTextStart >= 0 || extraTextEnd >= 0 && extraTextStart < extraTextEnd)
                            {
                                extraTextStart += "<ruby-text-italic>".Length;
                                extraText = line.Substring(extraTextStart, extraTextEnd - extraTextStart);
                            }
                        }

                        var extraTextAfter = string.Empty;
                        var extraTextStartAfter = line.IndexOf("<ruby-text-after>", i, StringComparison.Ordinal);
                        var extraTextEndAfter = line.IndexOf("</ruby-text-after>", i, StringComparison.Ordinal);
                        if (extraTextStartAfter >= 0 || extraTextEndAfter >= 0 && extraTextStartAfter < extraTextEndAfter)
                        {
                            extraTextStartAfter += "<ruby-text-after>".Length;
                            extraText = line.Substring(extraTextStartAfter, extraTextEndAfter - extraTextStartAfter);
                        }

                        var preFurigana = string.Empty;
                        if (actual.Length > 0)
                        {
                            preFurigana = $"{{\\alpha&FF&}}{actual.ToString().TrimEnd()}{{\\alpha&0&}}";
                        }
                        if (!string.IsNullOrWhiteSpace(extraText))
                        {
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs20}}{extraText}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }
                        if (!string.IsNullOrWhiteSpace(extraTextAfter))
                        {
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs20}} {extraTextAfter}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }
                        actual.Append(baseText);

                        var endTagEnd = line.IndexOf("</ruby-container>", i, StringComparison.Ordinal);
                        if (endTagEnd < 0)
                        {
                            break;
                        }
                        i = endTagEnd + "</ruby-container>".Length;
                        rubyOn = true;
                    }
                    else
                    {
                        actual.Append(line.Substring(i, 1));
                        i++;
                    }
                }

                var actualText = actual.ToString().TrimEnd();
                bool displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furiganaList.Count > 0)
                {
                    foreach (var fp in furiganaList)
                    {
                        var beforeText = "{\\an1}{\\pos(" + startX + "," + startY + ")}" + fp.Text;
                        list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }
                    startY += adjustment;
                    if (rubyOn && index == 0 && lines.Count == 2)
                    {
                        startY += 3;
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
                        var beforeText = "{\\an1}{\\pos(" + startX + "," + startY + ")}" + fp.Text;
                        list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }
                    startY += adjustment;
                }
                furiganaList.Clear();
            }

            return list;
        }


        private static List<Paragraph> MakeVerticalParagraphs(Paragraph p, int width)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = 34;
            var startX = 9 + lines.Count * 2 * adjustment;
            var leftAlign = p.Text.StartsWith("{\\an7}", StringComparison.Ordinal);
            if (!leftAlign)
            {
                startX = width - 50;
            }

            string pre = p.Text.Substring(0, 5);
            var list = new List<Paragraph>();
            var furiganaList = new List<Paragraph>();
            var rubyOn = false;
            var italinOn = false;
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var actual = new StringBuilder();
                int i = 0;
                if (italinOn)
                {
                    line = "<i>" + line;
                }
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
                        actual.Append("{\\i1}");
                        i += 3;
                        italinOn = true;
                    }
                    else if (line.Substring(i).StartsWith("</i>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</u>", StringComparison.Ordinal) || line.Substring(i).StartsWith("</b>", StringComparison.Ordinal))
                    {
                        actual.Append("{\\i0}");
                        i += 4;
                        italinOn = false;
                    }
                    else if (line.Substring(i).StartsWith("<horizontalDigit>", StringComparison.Ordinal))
                    {
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

                        var text = line.Substring(end + 1, endTagStart - end - 1);
                        actual.Append(text);
                        actual.AppendLine();
                        i = endTagStart + "</horizontalDigit>".Length;
                    }
                    else if (line.Substring(i).StartsWith("</horizontalDigit>", StringComparison.Ordinal))
                    {
                        i += "</horizontalDigit>".Length;
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
                    else if (line.Substring(i).StartsWith("<ruby-container>", StringComparison.Ordinal))
                    {
                        var baseTextStart = line.IndexOf("<ruby-base>", i, StringComparison.Ordinal);
                        var baseTextEnd = line.IndexOf("</ruby-base>", i, StringComparison.Ordinal);
                        if (baseTextStart < 0 || baseTextEnd < 0)
                        {
                            baseTextStart = line.IndexOf("<ruby-base-italic>", i, StringComparison.Ordinal);
                            baseTextEnd = line.IndexOf("</ruby-base-italic>", i, StringComparison.Ordinal);
                            if (baseTextStart < 0 || baseTextEnd < 0)
                            {
                                break;
                            }
                        }
                        baseTextStart += "<ruby-base>".Length;
                        var baseText = line.Substring(baseTextStart, baseTextEnd - baseTextStart);

                        var extraText = string.Empty;
                        var extraTextStart = line.IndexOf("<ruby-text>", i, StringComparison.Ordinal);
                        var extraTextEnd = line.IndexOf("</ruby-text>", i, StringComparison.Ordinal);
                        if (extraTextStart >= 0 || extraTextEnd >= 0 && extraTextStart < extraTextEnd)
                        {
                            extraTextStart += "<ruby-text>".Length;
                            extraText = line.Substring(extraTextStart, extraTextEnd - extraTextStart);
                        }

                        if (string.IsNullOrEmpty(extraText))
                        {
                            extraTextStart = line.IndexOf("<ruby-text-italic>", i, StringComparison.Ordinal);
                            extraTextEnd = line.IndexOf("</ruby-text-italic>", i, StringComparison.Ordinal);
                            if (extraTextStart >= 0 || extraTextEnd >= 0 && extraTextStart < extraTextEnd)
                            {
                                extraTextStart += "<ruby-text-italic>".Length;
                                extraText = line.Substring(extraTextStart, extraTextEnd - extraTextStart);
                            }
                        }

                        var extraTextAfter = string.Empty;
                        var extraTextStartAfter = line.IndexOf("<ruby-text-after>", i, StringComparison.Ordinal);
                        var extraTextEndAfter = line.IndexOf("</ruby-text-after>", i, StringComparison.Ordinal);
                        if (extraTextStartAfter >= 0 || extraTextEndAfter >= 0 && extraTextStartAfter < extraTextEndAfter)
                        {
                            extraTextStartAfter += "<ruby-text-after>".Length;
                            extraText = line.Substring(extraTextStartAfter, extraTextEndAfter - extraTextStartAfter);
                        }

                        var preFurigana = string.Empty;
                        if (actual.Length > 0)
                        {
                            preFurigana = $"{{\\alpha&FF&}}{actual.ToString().TrimEnd()}{{\\alpha&0&}}";
                        }
                        if (!string.IsNullOrWhiteSpace(extraText))
                        {
                            var sb = new StringBuilder();
                            foreach (var ch in extraText)
                            {
                                sb.Append(ch);
                                sb.AppendLine();
                            }
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs20}}{sb.ToString().TrimEnd()}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }
                        if (!string.IsNullOrWhiteSpace(extraTextAfter))
                        {
                            var sb = new StringBuilder();
                            foreach (var ch in extraTextAfter)
                            {
                                sb.Append(ch);
                                sb.AppendLine();
                            }
                            furiganaList.Add(new Paragraph($"{preFurigana}{{\\fs20}} {sb.ToString().TrimEnd()}", p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                        }

                        foreach (var ch in baseText)
                        {
                            actual.Append(ch);
                            actual.AppendLine();
                        }

                        var endTagEnd = line.IndexOf("</ruby-container>", i, StringComparison.Ordinal);
                        if (endTagEnd < 0)
                        {
                            break;
                        }
                        i = endTagEnd + "</ruby-container>".Length;
                        rubyOn = true;
                    }
                    else
                    {
                        actual.AppendLine(line.Substring(i, 1));
                        i++;
                    }
                }

                bool displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furiganaList.Count > 0)
                {
                    foreach (var fp in furiganaList)
                    {
                        var text = pre + "\\pos(" + startX + ",45)}" + fp.Text.TrimEnd();
                        list.Add(new Paragraph(text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }
                    startX -= adjustment;
                    if (rubyOn && index == 0 && lines.Count == 2)
                    {
                        if (leftAlign)
                        {
                            startX -= 8;
                        }
                        else
                        {
                            startX += 16;
                        }
                    }
                }

                var actualText = actual.ToString().TrimEnd();
                actualText = pre + "\\pos(" + startX + ",40)}" + actualText;

                // change horizontal chars to vertical version
                actualText = actualText
                        .Replace('…', '⋮')
                        .Replace('〈', '︿')
                        .Replace('〉', '﹀')
                        .Replace('—', '︱') // em dash
                        .Replace('⸺', '︱') // double em dash (could not find double em dash vertical)
                        .Replace('ー', '⏐') // prolonged sound mark
                        .Replace('（', '︵')
                        .Replace('）', '︶');

                list.Add(new Paragraph(actualText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                startX -= adjustment;

                if (!displayBefore && furiganaList.Count > 0)
                {
                    if (rubyOn && index == 1 && lines.Count == 2)
                    {
                        if (leftAlign)
                        {
                            startX += 14;
                        }
                        else
                        {
                            startX -= 8;
                        }
                    }
                    foreach (var fp in furiganaList)
                    {
                        var text = pre + "\\pos(" + startX + ",45)}" + fp.Text.TrimEnd();
                        list.Add(new Paragraph(text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }
                    startX -= adjustment;
                }
            }

            return list;
        }

        public static string Convert(Subtitle subtitle, int width, int height)
        {
            var finalSub = new Subtitle();
            foreach (var paragraph in subtitle.Paragraphs)
            {
                finalSub.Paragraphs.AddRange(SplitToAssRenderLines(paragraph, width, height));
            }

            var oldFontSize = Configuration.Settings.SubtitleSettings.SsaFontSize;
            var oldFontBold = Configuration.Settings.SubtitleSettings.SsaFontBold;
            Configuration.Settings.SubtitleSettings.SsaFontSize = 40; // font size
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
