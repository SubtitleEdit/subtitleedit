using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// LRC is a format that synchronizes song lyrics with an audio/video file, [mm:ss.xxx] where mm is minutes, ss is seconds and xx is milliseconds.
    ///
    /// https://wiki.nicksoft.info/specifications:lrc-file
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
    public class Lrc3DigitsMs : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d+:\d\d\.\d\d\d\].*$", RegexOptions.Compiled);

        public override string Extension => ".lrc";

        public override string Name => "LRC Lyrics ms";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);

            if (subtitle.Paragraphs.Count > 4)
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
                       !new Lrc().IsMine(lines, fileName) &&
                       !new LrcNoEndTime().IsMine(lines, fileName);
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var header = RemoveSoftwareAndVersion(subtitle.Header);
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(header) && (header.Contains("[ar:") || header.Contains("[ti:") || header.Contains("[by:") || header.Contains("[id:")))
            {
                sb.AppendLine(header);
            }
            else if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine("[ti:" + title.Replace("[", string.Empty).Replace("]", string.Empty) + "]");
            }

            if (!header.Contains("[re:", StringComparison.Ordinal))
            {
                sb.AppendLine("[re: Subtitle Edit]");
            }

            if (!header.Contains("[ve:", StringComparison.Ordinal))
            {
                sb.AppendLine($"[ve: {Utilities.AssemblyVersion}]");
            }

            const string timeCodeFormat = "[{0:00}:{1:00}.{2:000}]{3}";
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var next = subtitle.GetParagraphOrDefault(i + 1);

                var text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, " ");
                var fraction = p.StartTime.Milliseconds;
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
                    sb.AppendLine(string.Format(timeCodeFormat, tc.Hours * 60 + tc.Minutes, tc.Seconds, tc.Milliseconds, string.Empty));
                }
            }

            return sb.ToString().Trim();
        }

        public static string RemoveSoftwareAndVersion(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var line in s.SplitToLines())
            {
                if (line.Trim().StartsWith("[re:") || line.Trim().StartsWith("[ve:"))
                {
                    continue;
                }

                sb.AppendLine(line.Trim());
            }

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        { //[01:05.999]I've been walking in the same way as I do
            _errorCount = 0;
            var offsetInMilliseconds = 0.0d;
            var header = new StringBuilder();
            char[] splitChars = { ':', '.' };
            foreach (var line in lines)
            {
                if (line.StartsWith('[') && RegexTimeCodes.Match(line).Success)
                {
                    var s = line.Substring(1, 8);
                    var parts = s.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        try
                        {
                            var minutes = int.Parse(parts[0]);
                            var seconds = int.Parse(parts[1]);
                            var milliseconds = int.Parse(parts[2]);
                            var text = line.Remove(0, 10).Trim().TrimStart(']').Trim();
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
                    if (double.TryParse(temp, out var d))
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

            header = new StringBuilder(Lrc.RemoveSoftwareAndVersion(header.ToString()));
            header.AppendLine();

            if (!header.ToString().Contains("[re:", StringComparison.Ordinal))
            {
                header.AppendLine("[re: Subtitle Edit]");
            }

            if (!header.ToString().Contains("[ve:", StringComparison.Ordinal))
            {
                header.AppendLine($"[ve: {Utilities.AssemblyVersion}]");
            }

            subtitle.Header = header.ToString();

            var max = subtitle.Paragraphs.Count;
            for (var i = 0; i < max; i++)
            {
                var p = subtitle.Paragraphs[i];
                while (RegexTimeCodes.Match(p.Text).Success)
                {
                    var s = p.Text.Substring(1, 9);
                    p.Text = p.Text.Remove(0, 11).Trim();
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

            var index = 0;
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
                s = s.Remove(0, 11).Trim();
            }

            return s;
        }
    }
}
