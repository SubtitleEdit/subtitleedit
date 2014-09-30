using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    public class UknownFormatImporter
    {

        public bool UseFrames { get; set; }

        public Subtitle AutoGuessImport(string[] lines)
        {
            Subtitle subTcOnSameSeparateLine = ImportTimeCodesOnSameSeperateLine(lines);
            Subtitle subTcAndTextOnSameLine = ImportTimeCodesAndTextOnSameLine(lines);
            Subtitle subTcOnAloneLines = ImportTimeCodesOnAloneLines(lines);

            Subtitle subtitle = subTcOnSameSeparateLine;
            if (subtitle.Paragraphs.Count < 2)
                subtitle = ImportTimeCodesAndTextOnSameLineOnlySpaceAsSeparator(lines);

            if (subTcAndTextOnSameLine.Paragraphs.Count > subtitle.Paragraphs.Count)
                subtitle = subTcAndTextOnSameLine;
            if (subTcOnAloneLines.Paragraphs.Count > subtitle.Paragraphs.Count)
                subtitle = subTcOnAloneLines;
            if (subtitle.Paragraphs.Count < 2)
            {
                subtitle = ImportTimeCodesInFramesOnSameSeperateLine(lines);
                if (subtitle.Paragraphs.Count < 2)
                {
                    subtitle = ImportTimeCodesInFramesAndTextOnSameLine(lines);
                }
            }
            if (subtitle.Paragraphs.Count > 1)
                CleanUp(subtitle);
            return subtitle;
        }

        private static void CleanUp(Subtitle subtitle)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = p.Text.Replace("<div>", string.Empty);
                p.Text = p.Text.Replace("</div>", string.Empty);
                p.Text = p.Text.Replace("<body>", string.Empty);
                p.Text = p.Text.Replace("</body>", string.Empty);
                p.Text = p.Text.Replace("</tt>", string.Empty);
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("|", Environment.NewLine).Replace("<p>", Environment.NewLine).Replace("</p>", Environment.NewLine).Trim();
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();
            }
            subtitle.RemoveEmptyLines();
        }

        private Subtitle ImportTimeCodesInFramesAndTextOnSameLine(string[] lines)
        {
            Regex regexTimeCodes1 = new Regex(@"\d+", RegexOptions.Compiled);
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Length; idx++)
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
                    sb = new StringBuilder();
                    try
                    {
                        if (UseFrames)
                        {
                            p.StartFrame = int.Parse(start);
                            p.EndFrame = int.Parse(end);
                            p.CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);
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
                        line = line.Remove(0, matches[0].Index);
                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                {
                    sb.AppendLine(line.Trim());
                    if (sb.Length > 200)
                        return new Subtitle();
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
            return subtitle;
        }

        private Subtitle ImportTimeCodesInFramesOnSameSeperateLine(string[] lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = line.Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty).Replace("\t", string.Empty).Replace(":", string.Empty).Replace(";", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("-", string.Empty).Replace(">", string.Empty).Replace("<", string.Empty);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                        allNumbers = false;
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 2)
                {
                    string[] arr = line.Replace("-", " ").Replace(">", " ").Replace("{", " ").Replace("}", " ").Replace("[", " ").Replace("]", " ").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (start.Length == 1 && end.Length == 1)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb = new StringBuilder();
                            try
                            {
                                if (UseFrames)
                                {
                                    p.StartFrame = int.Parse(start[0]);
                                    p.EndFrame = int.Parse(end[0]);
                                    p.CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);
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
                        string[] start = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

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
                            sb = new StringBuilder();
                            try
                            {
                                if (UseFrames)
                                {
                                    p.StartFrame = int.Parse(start[0]);
                                    p.EndFrame = int.Parse(end[0]);
                                    p.CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);
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
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);
            subtitle.Renumber(1);
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnAloneLines(string[] lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = line.Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty).Replace("\t", string.Empty).Replace(":", string.Empty).Replace(";", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("-", string.Empty).Replace(">", string.Empty).Replace("<", string.Empty);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                        allNumbers = false;
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = line.Replace("-", " ").Replace(">", " ").Replace("{", " ").Replace("}", " ").Replace("[", " ").Replace("]", " ").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 1)
                    {
                        string[] tc = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (p == null || (p.EndTime.TotalMilliseconds != 0))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                                sb = new StringBuilder();
                            }
                            p = new Paragraph();
                            p.StartTime = DecodeTime(tc);
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
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
            return subtitle;
        }

        private static Subtitle ImportTimeCodesAndTextOnSameLine(string[] lines)
        {
            Regex regexTimeCodes1 = new Regex(@"\d+[:.,;]{1}\d\d[:.,;]{1}\d\d[:.,;]{1}\d+", RegexOptions.Compiled);
            Regex regexTimeCodes2 = new Regex(@"\d+[:.,;]{1}\d\d[:.,;]{1}\d+", RegexOptions.Compiled);
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();

            bool isFirstLineNumber = false;

            int count = -1;
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];
                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                    matches = regexTimeCodes2.Matches(line);
                if (matches.Count == 2)
                {
                    var start = matches[0].Value.Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    int i;
                    if (int.TryParse(start[0], out i))
                    {
                        if (count == -1 && i < 2)
                            count = i;
                        if (count != i)
                        {
                            isFirstLineNumber = false;
                            break;
                        }
                        count++;
                    }
                }
                if (count > 2)
                    isFirstLineNumber = true;
            }

            for (int idx = 0; idx < lines.Length; idx++)
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
                    matches = regexTimeCodes2.Matches(line);
                if (matches.Count == 2)
                {
                    string[] start = matches[0].ToString().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] end = matches[1].ToString().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
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
                    if (matches[0].Index < 9)
                        line = line.Remove(0, matches[0].Index);

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                    sb.AppendLine(line.Trim());
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
                    prefix = string.Empty;

                if (prefix.Length > 0)
                {
                    foreach (Paragraph paragraph in subtitle.Paragraphs)
                    {
                        string text = paragraph.Text.Trim();
                        if (text.StartsWith(prefix))
                            paragraph.Text = text.Remove(0, prefix.Length);
                    }
                }
            }

            subtitle.Renumber(1);
            return subtitle;
        }

        private static Subtitle ImportTimeCodesAndTextOnSameLineOnlySpaceAsSeparator(string[] lines)
        {
            Regex regexTimeCodes1 = new Regex(@"\d+ {1}\d\d {1}\d\d {1}\d+", RegexOptions.Compiled);
            Regex regexTimeCodes2 = new Regex(@"\d+  {1}\d\d {1}\d+", RegexOptions.Compiled);
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                    matches = regexTimeCodes2.Matches(line);
                if (matches.Count == 2)
                {
                    string[] start = matches[0].ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] end = matches[1].ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                    if (matches[0].Index < 9)
                        line = line.Remove(0, matches[0].Index);
                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                    sb.AppendLine(line.Trim());
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnSameSeperateLine(string[] lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = line.Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty).Replace("\t", string.Empty).Replace(":", string.Empty).Replace(";", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("-", string.Empty).Replace(">", string.Empty).Replace("<", string.Empty);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                        allNumbers = false;
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = line.Replace("-", " ").Replace(">", " ").Replace("{", " ").Replace("}", " ").Replace("[", " ").Replace("]", " ").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
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
                    else if (arr.Length == 3)
                    {
                        string[] start = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[2].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

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
                            sb = new StringBuilder();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
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
                    d = 8;
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
                        a.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
                    }
                }
                return subtitle;
            }

            subtitle.Renumber(1);
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnSameSeperateLineNoMilliseconds(string[] lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = line.Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty).Replace("\t", string.Empty).Replace(":", string.Empty).Replace(";", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("-", string.Empty).Replace(">", string.Empty).Replace("<", string.Empty);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                        allNumbers = false;
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = line.Replace("-", " ").Replace(">", " ").Replace("{", " ").Replace("}", " ").Replace("[", " ").Replace("]", " ").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (start.Length == 3)
                                start = (arr[0].Trim() + ".000").Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (end.Length == 3)
                                end = (arr[1].Trim() + ".000").Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

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
                    else if (arr.Length == 3)
                    {
                        string[] start = arr[0].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[2].Trim().Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

                        if (start.Length == 3)
                            start = (arr[0].Trim() + ".000").Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (end.Length == 3)
                            end = (arr[1].Trim() + ".000").Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (duration.Length == 3)
                            duration = (arr[2].Trim() + ".000").Split(new[] { '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

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
                            sb = new StringBuilder();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}") || line.StartsWith("][]"))
                        line = line.Remove(0, 3);
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber(1);
            return subtitle;
        }

        private static TimeCode DecodeTime(string[] parts)
        {
            try
            {
                string hour = parts[0];
                string minutes = parts[1];
                string seconds = parts[2];
                string frames = string.Empty;
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
                    return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), SubtitleFormat.FramesToMillisecondsMax999(int.Parse(frames)));
                else
                    return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), int.Parse(frames));
            }
            catch
            {
                return new TimeCode(0, 0, 0, 0);
            }
        }

    }
}
