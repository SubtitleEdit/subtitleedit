using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://backlothelp.netflix.com/hc/en-us/articles/214807928-Lambda-Cap-Creation-Guide-v1-1
    /// </summary>
    public class LambdaCap : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+[A-Z]*\s*\t\s*\d{8}/\d{8}\s*\t\s*", RegexOptions.Compiled);

        public override string Extension => ".cap";

        public override string Name => "Lambda Cap";

        public string Header
        {
            get
            {
                if (Configuration.Settings.General.CurrentFrameRate % 1.0 < 0.001)
                {
                    return "Lambda字幕V4 DF0+1 SCENE\"和文標準\""; // non drop frame
                }
                return "Lambda字幕V4 DF1+1 SCENE\"和文標準\""; // drop frame
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            if (sb.ToString().StartsWith("{\\rtf1", StringComparison.Ordinal))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        private const string AlignVerticalTop = "＠行頭";
        private const string AlignHorizontalLeft = "＠縦左";
        private const string AlignHorizontalRight = "＠縦右";
        private const string JustifiedCenter = "＠中央";
        private const string LongDash1 = "＠幅広［―］＠";
        private const string LongDash2 = "＠幅広［ー］＠";
        private const string AsynchronousContinuation = "＠継続";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // 1<HT>00000000/11111111<HT>Line 1
                // <HT><HT><HT><HT>Line 2
                // 2<HT>11111111/22222222<HT>AAAA Line 1
                // <HT><HT><HT><HT>Line 2
                var text = EncodeStyle(p.Text);
                sb.AppendLine($"{p.Number}\t{EncodeTimeCode(p.StartTime)}/{EncodeTimeCode(p.EndTime)}\t{text}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF().RemoveChar(':'); // HHMMSSFF without seperators, like 00031522
        }

        private static string EncodeStyle(string text)
        {
            text = text.Replace("―", LongDash1);
            text = text.Replace("ー", LongDash2);

            var verticalAlignTop = text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal);
            // var verticalAlignCenter = text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal);
            var horizontalAlignLeft = text.StartsWith("{\\an1}", StringComparison.Ordinal) || text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an7}", StringComparison.Ordinal);
            var horizontalAlignRight = text.StartsWith("{\\an3}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal);
            var s = Utilities.RemoveSsaTags(text);
            bool isWholeLineItalic = s.StartsWith("<i>", StringComparison.Ordinal) && s.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(s, "<i>") == 1;
            if (isWholeLineItalic)
            {
                text = text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
            }
            var lineBuilder = new StringBuilder();
            foreach (var line in text.SplitToLines())
            {
                var l = Utilities.RemoveSsaTags(line);
                bool isLineItalic = l.StartsWith("<i>", StringComparison.Ordinal) && l.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(l, "<i>") == 1;
                if (lineBuilder.Length > 0)
                {
                    lineBuilder.AppendLine();
                    lineBuilder.Append("\t\t\t\t");
                }

                if (l.Contains("<i>") && !(isWholeLineItalic || isLineItalic))
                {
                    l = l.Replace("<i>", "＠斜３［");
                    l = l.Replace("</i>", "］＠");
                }
                l = HtmlUtil.RemoveHtmlTags(l, true);
                lineBuilder.Append(l);
                if (isWholeLineItalic || isLineItalic)
                {
                    lineBuilder.Append(" ＠斜３");
                }

                if (horizontalAlignLeft)
                {
                    lineBuilder.Append(" " + AlignHorizontalLeft);
                }
                else if (horizontalAlignRight)
                {
                    lineBuilder.Append(" " + AlignHorizontalRight);
                }
                if (verticalAlignTop)
                {
                    lineBuilder.Append(" " + AlignVerticalTop);
                }

            }
            return lineBuilder.ToString().TrimEnd();
        }

        private static string DecodeStyle(string line)
        {
            line = line.Trim();
            var max = line.Length;
            var sb = new StringBuilder(max);
            bool readUntilEndCode = false;
            bool italicOn = false;
            bool digitsOn = false;
            int i = 0;
            while (i < max)
            {
                var ch = line[i];
                if (readUntilEndCode)
                {
                    if (ch == '］' && line.Substring(i).StartsWith("］＠", StringComparison.Ordinal))
                    {
                        readUntilEndCode = false;
                        if (italicOn)
                        {
                            sb.Append("</i>");
                            italicOn = false;
                        }
                        else if (digitsOn)
                        {
                            sb.Append("</i>");
                            digitsOn = false;
                        }
                        i++;
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else if (ch == '＠')
                {
                    var part = line.Substring(i);
                    if (part.StartsWith("＠ルビ上［", StringComparison.Ordinal)) // Bouten on first line - horizontal positioning
                    {
                        readUntilEndCode = true;
                    }
                    else if (part.StartsWith("＠ルビ下［", StringComparison.Ordinal)) // Bouten on second line - horizontal positioning
                    {
                        readUntilEndCode = true;
                    }
                    else if (part.StartsWith("＠ルビ右［", StringComparison.Ordinal)) // Bouten on first line - vertical positioning
                    {
                        readUntilEndCode = true;
                    }
                    else if (part.StartsWith("＠ルビ左［", StringComparison.Ordinal)) // Bouten on second line - vertical positioning
                    {
                        readUntilEndCode = true;
                    }
                    else if (part.StartsWith("＠組［", StringComparison.Ordinal)) // Kumi-moji / Tatechuyoko (up to 3 half width digits including numerical punctuation)
                    {
                        readUntilEndCode = true;
                        digitsOn = true;
                        i += 2;
                    }
                    else if (part.StartsWith("＠斜３［", StringComparison.Ordinal)) // italic
                    {
                        readUntilEndCode = true;
                        sb.Append("<i>");
                        italicOn = true;
                        i += 3;
                    }
                    else
                    {
                        sb.Append("＠");
                    }
                }
                else
                {
                    if (ch != '\t')
                    {
                        sb.Append(ch);
                    }
                }
                i++;
            }

            var s = sb.ToString();

            if (s.Contains("＠斜３"))
            {
                s = "<i>" + s + "</i>";
                s = s.Replace("＠斜３", string.Empty);
            }

            s = s.Replace(LongDash1, "―");
            s = s.Replace(LongDash2, "ー");
            s = s.Replace(AsynchronousContinuation, string.Empty);

            if (s.Contains(AlignVerticalTop))
            {
                if (s.Contains(AlignHorizontalLeft))
                {
                    s = "{\\an7}" + s;
                }
                else if (s.Contains(AlignHorizontalRight))
                {
                    s = "{\\an9}" + s;
                }
                else
                {
                    s = "{\\an8}" + s;
                }
            }
            else if (s.Contains(AlignHorizontalLeft))
            {
                s = "{\\an1}" + s;
            }
            else if (s.Contains(AlignHorizontalRight))
            {
                s = "{\\an3}" + s;
            }
            s = s.Replace(AlignVerticalTop, string.Empty);
            s = s.Replace(AlignHorizontalLeft, string.Empty);
            s = s.Replace(AlignHorizontalRight, string.Empty);
            s = s.Replace(JustifiedCenter, string.Empty).TrimEnd();

            if (s.EndsWith(" </i>", StringComparison.Ordinal))
            {
                s = s.Remove(s.Length - 5, 1);
            }

            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    var temp = line.Trim().Split('\t');
                    if (temp.Length > 1)
                    {
                        string[] startParts = { temp[1].Substring(0, 2), temp[1].Substring(2, 2), temp[1].Substring(4, 2), temp[1].Substring(6, 2), };
                        string[] endParts = { temp[1].Substring(9, 2), temp[1].Substring(11, 2), temp[1].Substring(13, 2), temp[1].Substring(15, 2), };
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            string text = line.Remove(0, RegexTimeCodes.Match(line).Length - 1).Trim();
                            p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), DecodeStyle(text));
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = DecodeStyle(line);
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + DecodeStyle(line);
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
