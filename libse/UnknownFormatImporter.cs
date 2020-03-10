using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Generic subtitle format parser
    /// </summary>
    public class UnknownFormatImporter
    {
        private static readonly char[] ExpectedSplitChars = { '.', ',', ';', ':' };
        public bool UseFrames { get; set; }

        public Subtitle AutoGuessImport(List<string> lines)
        {
            var subtitle = ImportTimeCodesOnSameSeperateLine(lines);
            if (subtitle.Paragraphs.Count < 2)
            {
                subtitle = ImportTimeCodesAndTextOnSameLineOnlySpaceAsSeparator(lines);
            }

            var subTcAndTextOnSameLine = ImportTimeCodesAndTextOnSameLine(lines);
            if (subTcAndTextOnSameLine.Paragraphs.Count > subtitle.Paragraphs.Count)
            {
                subtitle = subTcAndTextOnSameLine;
            }

            var subTcOnAloneLines = ImportTimeCodesOnAloneLines(lines);
            if (subTcOnAloneLines.Paragraphs.Count > subtitle.Paragraphs.Count)
            {
                subtitle = subTcOnAloneLines;
            }

            bool isJson = IsJson(lines);

            if (subtitle.Paragraphs.Count < 2 && !isJson)
            {
                subtitle = ImportTimeCodesInFramesOnSameSeperateLine(lines);
                if (subtitle.Paragraphs.Count < 2)
                {
                    var sameLineSub = ImportTimeCodesInFramesAndTextOnSameLine(lines);
                    if (sameLineSub.Paragraphs.Count < 10 &&
                        (sameLineSub.Paragraphs.Count(p => p.Duration.TotalMilliseconds < 0) > 2 ||
                         sameLineSub.Paragraphs.Count(p => p.Text.Length > 100) > 1))
                    {
                        // probably not a subtitle
                    }
                    else if (sameLineSub.Paragraphs.Count < 20 &&
                        (sameLineSub.Paragraphs.Count(p => p.Duration.TotalMilliseconds < 0) > 8 ||
                         sameLineSub.Paragraphs.Count(p => p.Text.Length > 100) > 5))
                    {
                        // probably not a subtitle
                    }
                    else
                    {
                        subtitle = sameLineSub;
                    }
                }
            }

            if (subtitle.Paragraphs.Count > 1)
            {
                CleanUp(subtitle);
            }

            if (subtitle.Paragraphs.Count < 2 || isJson)
            {
                var jsonSubtitle = new UnknownFormatImporterJson().AutoGuessImport(lines);
                if (jsonSubtitle != null && jsonSubtitle.Paragraphs.Count > 2)
                {
                    subtitle = jsonSubtitle;
                }
            }

            if (subtitle.Paragraphs.Count == 0 && lines.Count == 1 && lines[0].Contains(" --> "))
            {
                subtitle = ImportSubtitleWithNoLineBreaks(lines[0]);
            }

            if (subtitle.Paragraphs.Count < 10 || subtitle.Paragraphs.Average(p => p.Text.Length) > 100)
            {
                var text = string.Join(Environment.NewLine, lines);
                var noLineBreakSub = ImportSubtitleWithNoLineBreaksWithExtraSpaces(text);
                if (noLineBreakSub.Paragraphs.Count > subtitle.Paragraphs.Count * 1.5)
                {
                    subtitle = noLineBreakSub;
                }
            }


            if (subtitle.Paragraphs.Count > 0 && lines.Count > 0 && lines.Count / subtitle.Paragraphs.Count > 25)
            { // no more than 25 raw lines per subtitle lines
                return new Subtitle();
            }

            return subtitle;
        }

        private static bool IsJson(List<string> lines)
        {
            var jp = new JsonParser();
            try
            {
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
                jp.Parse(sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CleanUp(Subtitle subtitle)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = p.Text.Replace("<html>", string.Empty);
                p.Text = p.Text.Replace("</html>", string.Empty);
                p.Text = p.Text.Replace("<div>", string.Empty);
                p.Text = p.Text.Replace("</div>", string.Empty);
                p.Text = p.Text.Replace("<body>", string.Empty);
                p.Text = p.Text.Replace("</body>", string.Empty);
                p.Text = p.Text.Replace("<tt>", string.Empty);
                p.Text = p.Text.Replace("</tt>", string.Empty);
                p.Text = p.Text.Replace("<tr>", string.Empty);
                p.Text = p.Text.Replace("</tr>", string.Empty);
                p.Text = p.Text.Replace("<td>", string.Empty);
                p.Text = p.Text.Replace("</td>", string.Empty);
                p.Text = p.Text.Replace("<table>", string.Empty);
                p.Text = p.Text.Replace("</table>", string.Empty);
                p.Text = p.Text.Replace("<br>", Environment.NewLine);
                p.Text = p.Text.Replace("<br/>", Environment.NewLine);
                p.Text = p.Text.Replace("<br />", Environment.NewLine);
                p.Text = p.Text.Replace("&lt;", "<");
                p.Text = p.Text.Replace("&gt;", ">");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("|", Environment.NewLine).Replace("<p>", Environment.NewLine).Replace("</p>", Environment.NewLine).Trim();
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();
            }
            subtitle.RemoveEmptyLines();
        }

        private Subtitle ImportTimeCodesInFramesAndTextOnSameLine(List<string> lines)
        {
            var regexTimeCodes1 = new Regex(@"\d+", RegexOptions.Compiled);
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count >= 2)
                {
                    string start = matches[0].ToString();
                    string end = matches[1].ToString();

                    if (p != null)
                    {
                        p.Text = sb.ToString().Trim();
                        subtitle.Paragraphs.Add(p);
                    }
                    p = new Paragraph();
                    sb.Clear();
                    try
                    {
                        if (UseFrames)
                        {
                            p.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(start));
                            p.EndTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(end));
                        }
                        else
                        {
                            p.StartTime.TotalMilliseconds = double.Parse(start);
                            p.EndTime.TotalMilliseconds = double.Parse(end);
                        }
                    }
                    catch
                    {
                        p = null;
                    }

                    if (matches[0].Index < 9)
                    {
                        line = line.Remove(0, matches[0].Index);
                    }

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                {
                    sb.AppendLine(line.Trim());
                    if (sb.Length > 200)
                    {
                        return new Subtitle();
                    }
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private Subtitle ImportTimeCodesInFramesOnSameSeperateLine(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 2)
                {
                    string[] arr = line.Replace('-', ' ').Replace('>', ' ').Replace('{', ' ').Replace('}', ' ').Replace('[', ' ').Replace(']', ' ').Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if (start.Length == 1 && end.Length == 1)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            try
                            {
                                if (UseFrames)
                                {
                                    p.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(start[0]));
                                    p.EndTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(end[0]));
                                }
                                else
                                {
                                    p.StartTime.TotalMilliseconds = double.Parse(start[0]);
                                    p.EndTime.TotalMilliseconds = double.Parse(end[0]);
                                }
                            }
                            catch
                            {
                                p = null;
                            }
                        }
                    }
                    else if (arr.Length == 3)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);

                        if (end.Length == 1 && duration.Length == 1)
                        {
                            start = end;
                            end = duration;
                        }

                        if (start.Length == 1 && end.Length == 1)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            try
                            {
                                if (UseFrames)
                                {
                                    p.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(start[0]));
                                    p.EndTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(end[0]));
                                }
                                else
                                {
                                    p.StartTime.TotalMilliseconds = double.Parse(start[0]);
                                    p.EndTime.TotalMilliseconds = double.Parse(end[0]);
                                }
                            }
                            catch
                            {
                                p = null;
                            }
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnAloneLines(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChars = { ' ', '\t', '-', '>', '<', '{', '}', '[', ']' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                        break;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 1)
                    {
                        string[] tc = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if (p == null || Math.Abs(p.EndTime.TotalMilliseconds) > 0.001)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                                sb.Clear();
                            }
                            p = new Paragraph { StartTime = DecodeTime(tc) };
                        }
                        else
                        {
                            p.EndTime = DecodeTime(tc);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesAndTextOnSameLine(List<string> lines)
        {
            var regexTimeCodes1 = new Regex(@"\d+[:.,;]{1}\d\d[:.,;]{1}\d\d[:.,;]{1}\d+", RegexOptions.Compiled);
            var regexTimeCodes2 = new Regex(@"\d+[:.,;]{1}\d\d[:.,;]{1}\d+", RegexOptions.Compiled);
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();

            bool isFirstLineNumber = false;

            int count = -1;
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                {
                    matches = regexTimeCodes2.Matches(line);
                }

                if (matches.Count == 2)
                {
                    var start = matches[0].Value.Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    int i;
                    if (int.TryParse(start[0], out i))
                    {
                        if (count == -1 && i < 2)
                        {
                            count = i;
                        }

                        if (count != i)
                        {
                            isFirstLineNumber = false;
                            break;
                        }
                        count++;
                    }
                }
                if (count > 2)
                {
                    isFirstLineNumber = true;
                }
            }

            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];

                if (isFirstLineNumber)
                {
                    while (line.Length > 0 && char.IsDigit(line[0]))
                    {
                        line = line.Remove(0, 1);
                    }
                }

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                {
                    matches = regexTimeCodes2.Matches(line);
                }

                if (matches.Count == 2)
                {
                    string[] start = matches[0].ToString().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    string[] end = matches[1].ToString().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        p = new Paragraph();
                        sb.Clear();
                        p.StartTime = DecodeTime(start);
                        p.EndTime = DecodeTime(end);
                    }
                    if (matches[0].Index < 9)
                    {
                        line = line.Remove(0, matches[0].Index);
                    }

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (Utilities.IsInteger(line.RemoveChar('[').RemoveChar(']')))
                    {
                        line = string.Empty;
                    }
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            // remove all equal headers
            if (subtitle.Paragraphs.Count > 5)
            {
                string prefix = subtitle.Paragraphs[0].Text;
                foreach (Paragraph paragraph in subtitle.Paragraphs)
                {
                    string text = paragraph.Text.Trim();
                    var newPrefix = new StringBuilder();
                    int i = 0;
                    while (i < prefix.Length && i < text.Length && text[i] == prefix[i])
                    {
                        newPrefix.Append(text[i]);
                        i++;
                    }
                    prefix = newPrefix.ToString();
                }
                if (prefix.Length > 3 && prefix[1] == ':' && prefix[2] == '\\')
                {
                    prefix = string.Empty;
                }

                if (prefix.Length > 0)
                {
                    foreach (Paragraph paragraph in subtitle.Paragraphs)
                    {
                        string text = paragraph.Text.Trim();
                        if (text.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            paragraph.Text = text.Remove(0, prefix.Length);
                        }
                    }
                }
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesAndTextOnSameLineOnlySpaceAsSeparator(List<string> lines)
        {
            var regexTimeCodes1 = new Regex(@"\d+ {1}\d\d {1}\d\d {1}\d+", RegexOptions.Compiled);
            var regexTimeCodes2 = new Regex(@"\d+  {1}\d\d {1}\d+", RegexOptions.Compiled);
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChar = { ' ' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                {
                    matches = regexTimeCodes2.Matches(line);
                }

                if (matches.Count == 2)
                {
                    string[] start = matches[0].ToString().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    string[] end = matches[1].ToString().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        p = new Paragraph();
                        sb.Clear();
                        p.StartTime = DecodeTime(start);
                        p.EndTime = DecodeTime(end);
                    }
                    if (matches[0].Index < 9)
                    {
                        line = line.Remove(0, matches[0].Index);
                    }

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnSameSeperateLine(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChars = { ' ', '\t' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                        break;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    if (line.Contains("->"))
                    {
                        line = line.RemoveChar(' ');
                    }

                    string[] arr = line.Replace('-', ' ').Replace('>', ' ').Replace('{', ' ').Replace('}', ' ').Replace('[', ' ').Replace(']', ' ').Trim().Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                    else if (arr.Length > 3)
                    {
                        string[] start;
                        string[] end;
                        if (arr[0].Length > 9)
                        {
                            start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                            end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            start = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                            end = arr[2].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb = new StringBuilder();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            double averateDuration = 0;
            foreach (Paragraph a in subtitle.Paragraphs)
            {
                double d = a.Duration.TotalSeconds;
                if (d > 10)
                {
                    d = 8;
                }

                averateDuration += d;
            }
            averateDuration = averateDuration / subtitle.Paragraphs.Count;
            if (averateDuration < 0.2 || (averateDuration < 0.5 && subtitle.Paragraphs.Count > 100 && subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].StartTime.TotalSeconds < 140 && subtitle.Paragraphs[subtitle.Paragraphs.Count - 2].StartTime.TotalSeconds < 140))
            {
                subtitle = ImportTimeCodesOnSameSeperateLineNoMilliseconds(lines);
                int i = 0;
                foreach (Paragraph a in subtitle.Paragraphs)
                {
                    i++;
                    var next = subtitle.GetParagraphOrDefault(i);
                    if (next != null && a.EndTime.TotalMilliseconds >= next.StartTime.TotalMilliseconds)
                    {
                        a.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
                return subtitle;
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnSameSeperateLineNoMilliseconds(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChar = { ' ' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = line.Replace('-', ' ').Replace('>', ' ').Replace('{', ' ').Replace('}', ' ').Replace('[', ' ').Replace(']', ' ').Trim().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (start.Length == 3)
                            {
                                start = (arr[0].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (end.Length == 3)
                            {
                                end = (arr[1].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                    else if (arr.Length == 3)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[2].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);

                        if (start.Length == 3)
                        {
                            start = (arr[0].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (end.Length == 3)
                        {
                            end = (arr[1].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (duration.Length == 3)
                        {
                            duration = (arr[2].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (start.Length < 3)
                        {
                            start = end;
                            end = duration;
                        }

                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static string GetLineWithPerhapsOnlyNumbers(string line)
        {
            return line.RemoveChar(' ').RemoveChar('.').RemoveChar(',').RemoveChar('\t').RemoveChar(':').RemoveChar(';').RemoveChar('{').RemoveChar('}').RemoveChar('[').RemoveChar(']').RemoveChar('-').RemoveChar('>').RemoveChar('<');
        }

        private static TimeCode DecodeTime(string[] parts)
        {
            try
            {
                string hour = parts[0];
                string minutes = parts[1];
                string seconds = parts[2];
                string frames;
                if (parts.Length < 4)
                {
                    frames = seconds;
                    seconds = minutes;
                    minutes = hour;
                    hour = "0";
                }
                else
                {
                    frames = parts[3];
                }

                if (frames.Length < 3)
                {
                    return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), SubtitleFormat.FramesToMillisecondsMax999(int.Parse(frames)));
                }

                return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), int.Parse(frames));
            }
            catch
            {
                return new TimeCode();
            }
        }

        private static Subtitle ImportSubtitleWithNoLineBreaks(string text)
        {
            var regex = new Regex(@"^\d+ \d+:\d+:\d+[.,:;]\d+ --> \d+:\d+:\d+[.,:;]\d+\b", RegexOptions.Compiled); // e.g.: 1 00:00:01.502 --> 00:00:03.604
            var subtitle = new Subtitle();
            int i = 0;
            var sb = new StringBuilder();
            Paragraph p = null;
            while (i < text.Length)
            {
                var ch = text[i];
                if (char.IsNumber(ch))
                {
                    var match = regex.Match(text.Substring(i));
                    if (match.Success)
                    {
                        if (p != null)
                        {
                            p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
                        }

                        sb.Clear();
                        var arr = match.Value.Split(' ');
                        if (arr.Length == 4)
                        {
                            i += match.Value.Length;
                            p = new Paragraph
                            {
                                StartTime = DecodeTime(arr[1].Split(ExpectedSplitChars)),
                                EndTime = DecodeTime(arr[3].Split(ExpectedSplitChars))
                            };
                            subtitle.Paragraphs.Add(p);
                            continue;
                        }
                    }
                }
                sb.Append(ch);
                i++;
            }
            if (p != null && string.IsNullOrEmpty(p.Text))
            {
                p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportSubtitleWithNoLineBreaksWithExtraSpaces(string text)
        {
            var regex = new Regex(@"^(\d+: *)?\d+ *: *\d+[.,:;] *\d+ *-{0,3}> *(\d+: *)?\d+ *: *\d+[.,:;] *\d+\b", RegexOptions.Compiled); // e.g.: 1 00:00:01.502 --> 00:00:03.604
            var subtitle = new Subtitle();
            int i = 0;
            var sb = new StringBuilder();
            Paragraph p = null;
            while (i < text.Length)
            {
                var ch = text[i];
                if (char.IsNumber(ch))
                {
                    var match = regex.Match(text.Substring(i));
                    if (match.Success)
                    {
                        if (p != null)
                        {
                            p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
                        }

                        sb.Clear();
                        var arr = match.Value.Split('>');
                        if (arr.Length == 2)
                        {
                            i += match.Value.Length;
                            p = new Paragraph
                            {
                                StartTime = DecodeTime(arr[0].RemoveChar(' ').TrimEnd('-').Split(ExpectedSplitChars)),
                                EndTime = DecodeTime(arr[1].RemoveChar(' ').TrimEnd('-').Split(ExpectedSplitChars))
                            };
                            subtitle.Paragraphs.Add(p);
                            continue;
                        }
                    }
                }
                sb.Append(ch);
                i++;
            }
            if (p != null && string.IsNullOrEmpty(p.Text))
            {
                p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
            }

            subtitle.Renumber();
            return subtitle;
        }
    }
}
