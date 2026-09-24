using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// LRC is a format that synchronizes song lyrics with an audio/video file, [mm:ss.xx] where mm is minutes, ss is seconds and xx is hundredths of a second.
    ///
    /// https://en.wikipedia.org/wiki/LRC_(file_format)
    ///
    /// Tags:
    ///     [al:''Album where the song is from'']
    ///     [ar:''Lyrics artist'']
    ///     [by:''Creator of the LRC file'']
    ///     [offset:''+/- Overall timestamp adjustment in milliseconds, + shifts time up, - shifts down'']
    ///     [re:''The player or editor that creates LRC file'']
    ///     [ti:''Lyrics(song) title'']
    ///     [ve:''version of program'']
    /// </summary>
    public class Lrc : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d+:\d\d\.\d\d\].*$", RegexOptions.Compiled);
        private static readonly Regex RegexHeaderTag = new Regex(@"^\[[A-Za-z]+:.*\]$", RegexOptions.Compiled);

        public override string Extension => ".lrc";

        public override string Name => "LRC Lyrics";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);

            if (subtitle.Paragraphs.Count >= 1)
            {
                var allStartWithNumber = true;
                foreach (var p in subtitle.Paragraphs)
                {
                    if (p.Text.Length > 1 && !Utilities.IsInteger(p.Text.Substring(0, 2)))
                    {
                        allStartWithNumber = false;
                        break;
                    }
                }
                if (allStartWithNumber)
                {
                    return false;
                }
            }

            if (subtitle.Paragraphs.Count > _errorCount)
            {
                return !new UnknownSubtitle33().IsMine(lines, fileName) &&
                       !new UnknownSubtitle36().IsMine(lines, fileName) &&
                       !new TMPlayer().IsMine(lines, fileName) &&
                       !new LrcNoEndTime().IsMine(lines, fileName);
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append(GetHeaderOrDefault(subtitle.Header, title, "Subtitle Edit"));
            sb.AppendLine();

            const string timeCodeFormat = "[{0:00}:{1:00}.{2:00}]{3}";
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var next = subtitle.GetParagraphOrDefault(i + 1);

                var text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, " ");
                var fraction = (int)Math.Round(p.StartTime.Milliseconds / 10.0);
                if (fraction >= 100)
                {
                    var ms = new TimeCode(p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, 0).TotalMilliseconds;
                    ms += 1000;
                    p = new Paragraph(p.Text, ms, p.EndTime.TotalMilliseconds);
                    fraction = 0;
                }

                sb.AppendLine(string.Format(timeCodeFormat, p.StartTime.Hours * 60 + p.StartTime.Minutes, p.StartTime.Seconds, fraction, text));

                if (next == null || next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 100)
                {
                    var tc = new TimeCode(p.EndTime.TotalMilliseconds);

                    // Same carry the start time above does: rounding 999 ms gives 100, and the
                    // ":00" format printed all three digits ("[01:04.100]") - a line RegexTimeCodes
                    // rejects, so the end marker was lost on reload and malformed for LRC players.
                    var endFraction = (int)Math.Round(tc.Milliseconds / 10.0);
                    if (endFraction >= 100)
                    {
                        tc = new TimeCode(new TimeCode(tc.Hours, tc.Minutes, tc.Seconds, 0).TotalMilliseconds + 1000);
                        endFraction = 0;
                    }

                    sb.AppendLine(string.Format(timeCodeFormat, tc.Hours * 60 + tc.Minutes, tc.Seconds, endFraction, string.Empty));
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// The header to write: the tag lines the subtitle already has ([ti:], [ar:], [re:], ...),
        /// exactly as they are, or - when it has none - a default header with the title and
        /// the software/version stamp. A header read from a file is written back unchanged
        /// (#15212). [offset:] is left out: the reader has already applied it to the time codes.
        /// </summary>
        public static string GetHeaderOrDefault(string header, string title, string software)
        {
            var sb = new StringBuilder();
            foreach (var line in (header ?? string.Empty).SplitToLines())
            {
                var s = line.Trim();
                if (RegexHeaderTag.IsMatch(s) && !s.StartsWith("[offset:", StringComparison.Ordinal))
                {
                    sb.AppendLine(s);
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }

            if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine("[ti:" + title.Replace("[", string.Empty).Replace("]", string.Empty) + "]");
            }

            sb.AppendLine($"[re: {software}]");
            sb.AppendLine($"[ve: {Utilities.AssemblyVersion}]");
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        { //[01:05.99]I've been walking in the same way as I do
            _errorCount = 0;
            var offsetInMilliseconds = 0.0d;
            var header = new StringBuilder();
            char[] splitChars = { ':', '.' };
            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimStart('\uFEFF');
                if (line.StartsWith('[') && RegexTimeCodes.IsMatch(line))
                {
                    var s = line.Substring(1, 8);
                    var parts = s.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        try
                        {
                            var minutes = int.Parse(parts[0]);
                            var seconds = int.Parse(parts[1]);
                            var milliseconds = int.Parse(parts[2]) * 10;
                            var text = line.Remove(0, 9).Trim().TrimStart(']').Trim();
                            var start = new TimeCode(0, minutes, seconds, milliseconds);
                            var p = new Paragraph(start, new TimeCode(), text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (line.StartsWith("[ar:", StringComparison.Ordinal)) // [ar:Lyrics artist]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[id:", StringComparison.Ordinal)) // [ar:Lyrics artist]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[al:", StringComparison.Ordinal)) // [al:Album where the song is from]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[ti:", StringComparison.Ordinal)) // [ti:Lyrics (song) title]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[au:", StringComparison.Ordinal)) // [au:Creator of the song text]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[length:", StringComparison.Ordinal)) // [length:How long the song is]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[offset:", StringComparison.Ordinal)) // [length:How long the song is]
                {
                    var temp = line.Replace("[offset:", string.Empty).Replace("]", string.Empty).Replace("'", string.Empty).RemoveChar(' ').TrimEnd();
                    if (double.TryParse(temp, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d))
                    {
                        offsetInMilliseconds = d;
                    }
                }
                else if (line.StartsWith("[by:", StringComparison.Ordinal)) // [by:Creator of the LRC file]
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (line.StartsWith("[ve:", StringComparison.Ordinal)) // editor version
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }

                    _errorCount++;
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (subtitle.Paragraphs.Count < 1)
                    {
                        header.AppendLine(line);
                    }

                    _errorCount++;
                }
                else if (subtitle.Paragraphs.Count < 1)
                {
                    header.AppendLine(line);
                }
            }

            subtitle.Header = header.ToString();

            var max = subtitle.Paragraphs.Count;
            for (var i = 0; i < max; i++)
            {
                var p = subtitle.Paragraphs[i];
                while (RegexTimeCodes.IsMatch(p.Text))
                {
                    var s = p.Text.Substring(1, 8);
                    p.Text = p.Text.Remove(0, 10).Trim();
                    var parts = s.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        var minutes = int.Parse(parts[0]);
                        var seconds = int.Parse(parts[1]);
                        var milliseconds = int.Parse(parts[2]) * 10;
                        var text = GetTextAfterTimeCodes(p.Text);
                        var start = new TimeCode(0, minutes, seconds, milliseconds);
                        var newParagraph = new Paragraph(start, new TimeCode(), text);
                        subtitle.Paragraphs.Add(newParagraph);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
            }

            subtitle.Sort(SubtitleSortCriteria.StartTime);

            int index = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = Utilities.AutoBreakLine(p.Text);
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null)
                {
                    if (string.IsNullOrEmpty(next.Text))
                    {
                        p.EndTime = new TimeCode(next.StartTime.TotalMilliseconds);
                    }
                    else
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                    if (p.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    {
                        double duration = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                        p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + duration);
                    }
                }
                else
                {
                    var duration = Utilities.GetOptimalDisplayMilliseconds(p.Text, 16) + 1500;
                    p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + duration);
                }
                index++;
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
            if (Math.Abs(offsetInMilliseconds) > 0.01)
            {
                foreach (var paragraph in subtitle.Paragraphs)
                {
                    paragraph.StartTime.TotalMilliseconds += offsetInMilliseconds;
                    paragraph.EndTime.TotalMilliseconds += offsetInMilliseconds;
                }
            }
        }

        private static string GetTextAfterTimeCodes(string s)
        {
            while (RegexTimeCodes.IsMatch(s))
            {
                s = s.Remove(0, 10).Trim();
            }

            return s;
        }
    }
}
