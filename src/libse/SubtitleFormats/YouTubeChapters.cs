using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// The chapter list pasted into a YouTube video description: "0:00 Intro" lines, first one at
    /// zero.
    /// </summary>
    public class YouTubeChapters : SubtitleFormat
    {
        /// <summary>
        /// Timestamp then title. The trailing lookahead rejects a fourth time part, so a
        /// "00:00:00:00 text" line from a frame-based format is not mistaken for a chapter.
        /// </summary>
        private static readonly Regex RegexChapterLine = new Regex(
            @"^(?:(\d{1,3}):)?(\d{1,2}):(\d{2})(?![:.,\d])\s*[-–—:]?\s*(.*)$",
            RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "YouTube chapters";

        public static List<Chapter> ParseChapters(List<string> lines, out int errorCount)
        {
            var chapters = new List<Chapter>();
            errorCount = 0;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var match = RegexChapterLine.Match(line);
                if (!match.Success)
                {
                    errorCount++;
                    continue;
                }

                var hours = match.Groups[1].Success
                    ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
                    : 0;
                var minutes = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                var seconds = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                if (seconds > 59 || (match.Groups[1].Success && minutes > 59))
                {
                    errorCount++;
                    continue;
                }

                var ms = (hours * 3600.0 + minutes * 60.0 + seconds) * TimeCode.BaseUnit;
                chapters.Add(new Chapter(ms, match.Groups[4].Value.Trim()));
            }

            return chapters;
        }

        public static string ToDescriptionText(IList<Chapter> chapters)
        {
            var sb = new StringBuilder();
            foreach (var chapter in chapters)
            {
                sb.AppendLine($"{EncodeTimeCode(chapter.StartMilliseconds)} {chapter.Title}".TrimEnd());
            }

            return sb.ToString();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToDescriptionText(ChapterHelper.FromSubtitle(subtitle));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            var chapters = ParseChapters(lines, out _errorCount);

            // YouTube only accepts a chapter list that starts at zero, and requiring that here is
            // also what stops this very loose format from claiming other "time then text" files.
            if (chapters.Count < 2 || chapters[0].StartMilliseconds > 0)
            {
                _errorCount = Math.Max(_errorCount, 1) + chapters.Count;
                return;
            }

            foreach (var p in ChapterHelper.ToSubtitle(chapters).Paragraphs)
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        internal static string EncodeTimeCode(double totalMilliseconds)
        {
            if (totalMilliseconds < 0)
            {
                totalMilliseconds = 0;
            }

            // Seconds resolution: YouTube ignores anything finer.
            var ts = TimeSpan.FromSeconds(Math.Floor(totalMilliseconds / TimeCode.BaseUnit));
            return ts.TotalHours >= 1
                ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds)
                : string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}", ts.Minutes, ts.Seconds);
        }
    }
}
