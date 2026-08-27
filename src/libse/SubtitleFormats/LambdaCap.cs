using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://backlothelp.netflix.com/hc/en-us/articles/214807928-Lambda-Cap-Creation-Guide-v1-1
    ///
    /// A cue is one tab separated row - number, "start/end", the first text line, and then one field
    /// per layout control code (＠横下, ＠中頭, ＠斜３...). Further text lines follow on their own rows,
    /// indented with tabs. Ruby (furigana), emphasis marks and tate-chu-yoko are decoded to the same
    /// markup the "Netflix IMSC 1.1 Japanese" format uses, so the video preview can draw them instead
    /// of putting the control codes on screen as literal text (issue #14165).
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
                    return "Lambda字幕V4\tDF0+1\tSCENE\"和文標準\""; // non drop frame
                }
                return "Lambda字幕V4\tDF1+1\tSCENE\"和文標準\""; // drop frame
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

        private const string AlignVerticalTop = "＠行頭"; // start at the head of the line - top for horizontal writing
        private const string AlignHorizontalTop = "＠横上"; // horizontal writing, top of the screen
        private const string AlignHorizontalBottom = "＠横下"; // horizontal writing, bottom of the screen (the default)
        private const string AlignHorizontalLeft = "＠縦左"; // vertical writing, left side
        private const string AlignHorizontalRight = "＠縦右"; // vertical writing, right side
        private const string JustifiedCenter = "＠中央"; // every line centered on its own
        private const string JustifiedCenterHead = "＠中頭"; // block centered, lines aligned at their head
        private const string ItalicCode = "＠斜３";
        private const string LongDash1 = "＠幅広［―］＠";
        private const string LongDash2 = "＠幅広［ー］＠";
        private const string AsynchronousContinuation = "＠継続";

        private const string EndCode = "］＠";
        private const string RubyAbove = "＠ルビ上［"; // furigana above (horizontal writing)
        private const string RubyBelow = "＠ルビ下［"; // furigana below (horizontal writing)
        private const string RubyRight = "＠ルビ右［"; // furigana right of the column (vertical writing)
        private const string RubyLeft = "＠ルビ左［"; // furigana left of the column (vertical writing)
        private const string HorizontalDigits = "＠組［"; // tate-chu-yoko - up to three half width digits
        private const string InlineItalic = "＠斜３［";

        /// <summary>A ruby run holding only one of these is an emphasis mark, not furigana.</summary>
        private const string BoutenMarkers = "↓↑・･";

        private const string BoutenTagBefore = "bouten-dot-before";
        private const string BoutenTagAfter = "bouten-dot-after";

        private static readonly char[] RubySeparators = { '｜', '|' };

        /// <summary>Codes that belong to the cue as a whole, not to a spot inside the text.</summary>
        private static readonly string[] LayoutCodes =
        {
            AlignVerticalTop,
            AlignHorizontalTop,
            AlignHorizontalBottom,
            AlignHorizontalLeft,
            AlignHorizontalRight,
            JustifiedCenter,
            JustifiedCenterHead,
            ItalicCode,
            AsynchronousContinuation,
        };

        private static readonly Regex RubyContainerRegex = new Regex(
            @"<ruby-container>\s*<ruby-base(?:-italic)?>(.*?)</ruby-base(?:-italic)?>\s*<(ruby-text|ruby-text-italic|ruby-text-after)>(.*?)</\2>\s*</ruby-container>",
            RegexOptions.Compiled);

        private static readonly Regex BoutenRegex = new Regex(@"<(bouten-[a-z-]+)>(.*?)</\1>", RegexOptions.Compiled);

        private static readonly Regex HorizontalDigitRegex = new Regex(@"<horizontalDigit>(.*?)</horizontalDigit>", RegexOptions.Compiled);

        /// <summary>The text lines and the layout codes collected for one cue.</summary>
        private sealed class LambdaCue
        {
            internal List<string> TextLines { get; } = new List<string>();
            internal HashSet<string> Codes { get; } = new HashSet<string>();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // 1<HT>00000000/11111111<HT>Line 1<HT>＠横下<HT>＠中頭
                // <HT><HT><HT><HT>Line 2
                var text = EncodeStyle(p.Text);
                sb.AppendLine($"{p.Number}\t{EncodeTimeCode(p.StartTime)}/{EncodeTimeCode(p.EndTime)}\t{text}");
            }
            return sb.ToString();
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            // Furigana/bouten/tate-chu-yoko markup means nothing to any other format - a plain text
            // format would otherwise get the tags themselves.
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = NetflixImsc11Japanese.RemoveTags(p.Text);
            }
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF().RemoveChar(':'); // HHMMSSFF without separators, like 00031522
        }

        private static string EncodeStyle(string text)
        {
            var verticalAlignTop = text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal);
            var writtenVerticallyLeft = text.StartsWith("{\\an1}", StringComparison.Ordinal) || text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an7}", StringComparison.Ordinal);
            var writtenVerticallyRight = text.StartsWith("{\\an3}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal);
            var writtenVertically = writtenVerticallyLeft || writtenVerticallyRight;

            var s = Utilities.RemoveSsaTags(text);
            var isWholeLineItalic = s.StartsWith("<i>", StringComparison.Ordinal) && s.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(s, "<i>") == 1;
            if (isWholeLineItalic)
            {
                s = s.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
            }

            var codes = new List<string>();
            if (writtenVertically)
            {
                codes.Add(writtenVerticallyLeft ? AlignHorizontalLeft : AlignHorizontalRight);
                if (verticalAlignTop)
                {
                    codes.Add(AlignVerticalTop);
                }
            }
            else
            {
                codes.Add(verticalAlignTop ? AlignHorizontalTop : AlignHorizontalBottom);

                // Subtitle Edit cannot tell "centered block, head aligned" from "every line centered",
                // so the far more common one is written back.
                codes.Add(JustifiedCenterHead);
            }

            if (isWholeLineItalic)
            {
                codes.Add(ItalicCode);
            }

            var lineBuilder = new StringBuilder();
            var lines = s.SplitToLines();
            for (var index = 0; index < lines.Count; index++)
            {
                if (index > 0)
                {
                    lineBuilder.AppendLine();
                    lineBuilder.Append("\t\t\t\t");
                }

                lineBuilder.Append(EncodeLine(lines[index], writtenVertically));

                if (index == 0)
                {
                    foreach (var code in codes)
                    {
                        lineBuilder.Append('\t').Append(code);
                    }
                }
            }

            return lineBuilder.ToString();
        }

        private static string EncodeLine(string line, bool writtenVertically)
        {
            // Only the horizontal bar is written back as a wide dash - the katakana prolonged sound
            // mark is a normal letter inside words like "スムーズ".
            var s = line.Replace("―", LongDash1);

            var rubyBefore = writtenVertically ? RubyRight : RubyAbove;
            var rubyAfter = writtenVertically ? RubyLeft : RubyBelow;

            s = RubyContainerRegex.Replace(s, match =>
            {
                var code = match.Groups[2].Value == "ruby-text-after" ? rubyAfter : rubyBefore;
                return code + match.Groups[1].Value + RubySeparators[0] + match.Groups[3].Value + EndCode;
            });

            s = BoutenRegex.Replace(s, match =>
            {
                // Lambda marks one character at a time.
                var code = match.Groups[1].Value.EndsWith("-after", StringComparison.Ordinal) ? rubyAfter : rubyBefore;
                var sb = new StringBuilder();
                foreach (var ch in match.Groups[2].Value)
                {
                    sb.Append(code).Append(ch).Append(RubySeparators[0]).Append(BoutenMarkers[0]).Append(EndCode);
                }

                return sb.ToString();
            });

            s = HorizontalDigitRegex.Replace(s, match => HorizontalDigits + match.Groups[1].Value + EndCode);

            if (s.Contains("<i>", StringComparison.Ordinal))
            {
                s = s.Replace("<i>", InlineItalic);
                s = s.Replace("</i>", EndCode);
            }

            return HtmlUtil.RemoveHtmlTags(s, true);
        }

        /// <summary>
        /// Turns the inline control codes of one text field into the markup Subtitle Edit uses for
        /// Japanese - anything unknown is left alone rather than eaten.
        /// </summary>
        private static string DecodeInlineCodes(string input)
        {
            var sb = new StringBuilder(input.Length);
            var i = 0;
            while (i < input.Length)
            {
                if (input[i] == '＠')
                {
                    if (TryDecodeRuby(input, i, sb, out var next) ||
                        TryDecodeBracketCode(input, i, HorizontalDigits, "<horizontalDigit>", "</horizontalDigit>", sb, out next) ||
                        TryDecodeBracketCode(input, i, InlineItalic, "<i>", "</i>", sb, out next))
                    {
                        i = next;
                        continue;
                    }
                }

                sb.Append(input[i]);
                i++;
            }

            return sb.ToString();
        }

        private static bool TryDecodeRuby(string input, int index, StringBuilder sb, out int next)
        {
            next = index;
            var after = false;
            string prefix = null;
            if (StartsWith(input, index, RubyAbove))
            {
                prefix = RubyAbove;
            }
            else if (StartsWith(input, index, RubyRight))
            {
                prefix = RubyRight;
            }
            else if (StartsWith(input, index, RubyBelow))
            {
                prefix = RubyBelow;
                after = true;
            }
            else if (StartsWith(input, index, RubyLeft))
            {
                prefix = RubyLeft;
                after = true;
            }

            if (prefix == null)
            {
                return false;
            }

            var contentStart = index + prefix.Length;
            var end = input.IndexOf(EndCode, contentStart, StringComparison.Ordinal);
            if (end < 0)
            {
                return false;
            }

            var content = input.Substring(contentStart, end - contentStart);
            var separator = content.IndexOfAny(RubySeparators);
            var baseText = separator < 0 ? content : content.Substring(0, separator);
            var rubyText = separator < 0 ? string.Empty : content.Substring(separator + 1);
            AppendRuby(sb, baseText, rubyText, after);
            next = end + EndCode.Length;
            return true;
        }

        private static void AppendRuby(StringBuilder sb, string baseText, string rubyText, bool after)
        {
            if (baseText.Length == 0)
            {
                return;
            }

            if (rubyText.Length == 0)
            {
                sb.Append(baseText);
                return;
            }

            if (IsBoutenMark(rubyText))
            {
                var boutenTag = after ? BoutenTagAfter : BoutenTagBefore;
                sb.Append('<').Append(boutenTag).Append('>').Append(baseText).Append("</").Append(boutenTag).Append('>');
                return;
            }

            var rubyTag = after ? "ruby-text-after" : "ruby-text";
            sb.Append("<ruby-container><ruby-base>").Append(baseText).Append("</ruby-base>")
                .Append('<').Append(rubyTag).Append('>').Append(rubyText).Append("</").Append(rubyTag).Append('>')
                .Append("</ruby-container>");
        }

        private static bool IsBoutenMark(string rubyText)
        {
            foreach (var ch in rubyText)
            {
                if (BoutenMarkers.IndexOf(ch) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryDecodeBracketCode(string input, int index, string code, string openTag, string closeTag, StringBuilder sb, out int next)
        {
            next = index;
            if (!StartsWith(input, index, code))
            {
                return false;
            }

            var contentStart = index + code.Length;
            var end = input.IndexOf(EndCode, contentStart, StringComparison.Ordinal);
            if (end < 0)
            {
                return false;
            }

            sb.Append(openTag).Append(input, contentStart, end - contentStart).Append(closeTag);
            next = end + EndCode.Length;
            return true;
        }

        private static bool StartsWith(string input, int index, string value)
        {
            return string.CompareOrdinal(input, index, value, 0, value.Length) == 0;
        }

        /// <summary>
        /// Pulls the cue level codes out of a text field - Subtitle Edit used to write them space
        /// separated behind the text instead of in their own tab separated fields.
        /// </summary>
        private static string ExtractLayoutCodes(string text, ISet<string> codes)
        {
            foreach (var code in LayoutCodes)
            {
                var index = text.IndexOf(code, StringComparison.Ordinal);
                while (index >= 0)
                {
                    codes.Add(code);
                    text = text.Remove(index, code.Length);
                    index = text.IndexOf(code, StringComparison.Ordinal);
                }
            }

            return text;
        }

        private static void AddCueLine(LambdaCue cue, string line)
        {
            foreach (var field in line.Split('\t'))
            {
                var f = field.Trim();
                if (f.Length == 0)
                {
                    continue;
                }

                if (Array.IndexOf(LayoutCodes, f) >= 0)
                {
                    cue.Codes.Add(f);
                    continue;
                }

                // A short "＠code" field behind the text is a layout code Subtitle Edit does not know -
                // dropping it beats putting it on screen as text.
                if (cue.TextLines.Count > 0 && f[0] == '＠' && f.Length <= 6 && f.IndexOf('［') < 0)
                {
                    continue;
                }

                var text = f.Replace(LongDash1, "―").Replace(LongDash2, "ー");
                text = DecodeInlineCodes(text);
                text = ExtractLayoutCodes(text, cue.Codes).Trim();
                cue.TextLines.Add(text);
            }
        }

        private static void CloseCue(Paragraph p, LambdaCue cue)
        {
            if (p == null)
            {
                return;
            }

            var text = string.Join(Environment.NewLine, cue.TextLines);
            if (text.Length == 0)
            {
                p.Text = string.Empty;
                return;
            }

            if (cue.Codes.Contains(ItalicCode))
            {
                text = "<i>" + text + "</i>";
            }

            var top = cue.Codes.Contains(AlignVerticalTop) || cue.Codes.Contains(AlignHorizontalTop);
            if (cue.Codes.Contains(AlignHorizontalLeft))
            {
                text = (top ? "{\\an7}" : "{\\an1}") + text;
            }
            else if (cue.Codes.Contains(AlignHorizontalRight))
            {
                text = (top ? "{\\an9}" : "{\\an3}") + text;
            }
            else if (top)
            {
                text = "{\\an8}" + text;
            }

            p.Text = text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            var cue = new LambdaCue();
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                var match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    var temp = line.Trim().Split('\t');
                    var timeCodes = temp.Length > 1 ? temp[1].Trim() : string.Empty;
                    if (timeCodes.Length >= 17)
                    {
                        string[] startParts = { timeCodes.Substring(0, 2), timeCodes.Substring(2, 2), timeCodes.Substring(4, 2), timeCodes.Substring(6, 2), };
                        string[] endParts = { timeCodes.Substring(9, 2), timeCodes.Substring(11, 2), timeCodes.Substring(13, 2), timeCodes.Substring(15, 2), };
                        CloseCue(p, cue);
                        cue = new LambdaCue();
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                        AddCueLine(cue, line.Remove(0, match.Length - 1));
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    AddCueLine(cue, line);
                }
            }

            CloseCue(p, cue);
            subtitle.Renumber();
        }

    }
}
