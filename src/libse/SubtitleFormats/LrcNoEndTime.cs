using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// LRC is a format that synchronizes song lyrics with an audio/video file, [mm:ss.xx] where mm is minutes, ss is seconds and xx is hundredths of a second.
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
    public class LrcNoEndTime : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d+:\d\d\.\d\d\].*$", RegexOptions.Compiled);
        private const string BySeText = "SE Lrc No End Time";

        public override string Extension => ".lrc";

        public override string Name => "LRC Lyrics, no end time";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(p => sb.Append(p));
            if (!sb.ToString().Contains(BySeText, StringComparison.Ordinal))
            {
                return false;
            }

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
                       !new TMPlayer().IsMine(lines, fileName);
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var header = Lrc.RemoveSoftwareAndVersion(subtitle.Header);
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(header) && (header.Contains("[ar:") || header.Contains("[ti:") || header.Contains("[by:") || header.Contains("[id:")))
            {
                sb.AppendLine(header.Trim());
            }
            else if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine("[ti:" + title.Replace("[", string.Empty).Replace("]", string.Empty) + "]");
            }

            if (!header.Contains("[re:", StringComparison.Ordinal))
            {
                sb.AppendLine($"[re: {BySeText}]");
            }

            if (!header.Contains("[ve:", StringComparison.Ordinal))
            {
                sb.AppendLine($"[ve: {Utilities.AssemblyVersion}]");
            }

            const string timeCodeFormat = "[{0:00}:{1:00}.{2:00}]{3}";
            foreach (var p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, " ");
                sb.AppendLine(string.Format(timeCodeFormat, p.StartTime.Hours * 60 + p.StartTime.Minutes, p.StartTime.Seconds, (int)Math.Round(p.StartTime.Milliseconds / 10.0), text));
            }

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        { //[01:05.99]I've been walking in the same way as I do
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

            if (!header.ToString().Contains("[re:", StringComparison.Ordinal))
            {
                header.AppendLine($"[re: {BySeText}]");
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

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
            if (Math.Abs(offsetInMilliseconds) > 0.01)
            {
                foreach (var paragraph in subtitle.Paragraphs)
                {
                    paragraph.StartTime.TotalMilliseconds += offsetInMilliseconds;
                }
            }

            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = Utilities.AutoBreakLine(p.Text);
                p.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
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
