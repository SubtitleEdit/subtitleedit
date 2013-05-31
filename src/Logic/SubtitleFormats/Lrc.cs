using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// LRC is a format that synchronizes song lyrics with an audio/video file, [mm:ss.xx] where mm is minutes, ss is seconds and xx is hundredths of a second.
    /// </summary>
    public class Lrc : SubtitleFormat
    {
        static Regex _timeCode = new Regex(@"^\[\d+:\d\d\.\d\d\].*$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".lrc"; }
        }

        public override string Name
        {
            get { return "LRC Lyrics"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (new UnknownSubtitle33().IsMine(lines, fileName) || new UnknownSubtitle36().IsMine(lines, fileName) || new TMPlayer().IsMine(lines, fileName))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);

            if (subtitle.Paragraphs.Count > 4)
            {
                bool allStartWithNumber = true;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.Text.Length > 1 && !Utilities.IsInteger(p.Text.Substring(0, 2)))
                    {
                        allStartWithNumber = false;
                        break;
                    }
                }
                if (allStartWithNumber)
                    return false;
            }
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(subtitle.Header) && (subtitle.Header.Contains("[ar:") || subtitle.Header.Contains("[ti:")))
                sb.Append(subtitle.Header);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = Utilities.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "|");
                sb.AppendLine(string.Format("[{0:00}:{1:00}.{2:00}]{3}", p.StartTime.Hours * 60 + p.StartTime.Minutes,
                                                               p.StartTime.Seconds,
                                                               p.StartTime.Milliseconds / 10,
                                                               text.Replace(Environment.NewLine, " ")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        { //[01:05.99]I've been walking in the same way as I do
            _errorCount = 0;
            var header = new StringBuilder();
            foreach (string line in lines)
            {
                if (_timeCode.Match(line).Success)
                {
                    string s = line;
                    s = line.Substring(1, 8);
                    string[] parts = s.Split(":.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        try
                        {
                            int minutes = int.Parse(parts[0]);
                            int seconds = int.Parse(parts[1]);
                            int milliseconds = int.Parse(parts[2]) * 10;
                            string text = line.Remove(0, 9).Trim().TrimStart(']').Trim();
                            var start = new TimeCode(0, minutes, seconds, milliseconds);
                            double duration = Utilities.GetOptimalDisplayMilliseconds(text);
                            var end = new TimeCode(TimeSpan.FromMilliseconds(start.TotalMilliseconds + duration));

                            var p = new Paragraph(start, end, text);
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
                else if (line.StartsWith("[ar:")) // [ar:Lyrics artist]
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                }
                else if (line.StartsWith("[al:")) // [al:Album where the song is from]
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                }
                else if (line.StartsWith("[ti:")) // [ti:Lyrics (song) title]
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                }
                else if (line.StartsWith("[au:")) // [au:Creator of the Songtext]
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                }
                else if (line.StartsWith("[length:")) // [length:How long the song is]
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                }
                else if (line.StartsWith("[by:")) // [by:Creator of the LRC file]
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                }
                else if (line.Trim().Length > 0)
                {
                    if (subtitle.Paragraphs.Count < 1)
                        header.AppendLine(line);
                    _errorCount++;
                }
                else if (subtitle.Paragraphs.Count < 1)
                {
                    header.AppendLine(line);
                }
            }
            subtitle.Header = header.ToString();

            int max = subtitle.Paragraphs.Count;
            for (int i = 0; i < max; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                while (_timeCode.Match(p.Text).Success)
                {
                    string s = p.Text.Substring(1,8);
                    p.Text = p.Text.Remove(0, 10).Trim();
                    string[] parts = s.Split(":.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        int minutes = int.Parse(parts[0]);
                        int seconds = int.Parse(parts[1]);
                        int milliseconds = int.Parse(parts[2]) * 10;
                        string text = GetTextAfterTimeCodes(p.Text);
                        var start = new TimeCode(0, minutes, seconds, milliseconds);
                        double duration = Utilities.GetOptimalDisplayMilliseconds(text);
                        var end = new TimeCode(TimeSpan.FromMilliseconds(start.TotalMilliseconds + duration));

                        var newParagraph = new Paragraph(start, end, text);
                        subtitle.Paragraphs.Add(newParagraph);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
            }

            subtitle.Sort(Enums.SubtitleSortCriteria.StartTime);

            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null && next.StartTime.TotalMilliseconds <= p.EndTime.TotalMilliseconds)
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;

                index++;
                p.Number = index;
            }

        }

        private string GetTextAfterTimeCodes(string s)
        {
            while (_timeCode.IsMatch(s))
                s = s.Remove(0, 10).Trim();
            return s;
        }

    }
}
