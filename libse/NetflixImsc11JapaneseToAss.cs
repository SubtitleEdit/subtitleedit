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

            if (p.Text.Contains("<bouten-", StringComparison.Ordinal))
            {
                return MakeHorizontalParagraphs(p, width, height);
            }

            return new List<Paragraph> { p };
        }

        private static List<Paragraph> MakeHorizontalParagraphs(Paragraph p, int width, int height)
        {
            var lines = p.Text.SplitToLines();
            var adjustment = 34;
            var startY = height - (20 + lines.Count * 2 * adjustment);
            var list = new List<Paragraph>();
            var furiganaList = new List<Paragraph>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
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
                    else
                    {
                        actual.Append(line.Substring(i, 1));
                        i++;
                    }
                }

                var actualText = actual.ToString().TrimEnd();
                int actualTextWidth;
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    actualTextWidth = (int)g.MeasureString(actualText, new Font(SystemFonts.DefaultFont.FontFamily, 20)).Width;
                }

                int startX = width / 2 - (actualTextWidth / 2);

                bool displayBefore = lines.Count == 2 && index == 0 || lines.Count == 1;
                if (displayBefore && furiganaList.Count > 0)
                {
                    foreach (var fp in furiganaList)
                    {
                        var beforeText = "{\\an1}{\\pos(" + startX + "," + startY + ")}" + fp.Text;
                        list.Add(new Paragraph(beforeText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                    }
                    startY += adjustment;
                }

                actualText = "{\\an1}{\\pos(" + startX + "," + startY + ")}" + actualText;

                list.Add(new Paragraph(actualText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds));
                startY += adjustment;

                if (!displayBefore && furiganaList.Count > 0)
                {
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
                        furigana.AppendLine(" ");
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
