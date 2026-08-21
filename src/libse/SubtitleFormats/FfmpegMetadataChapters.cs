using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// FFmpeg metadata chapters (";FFMETADATA1" + "[CHAPTER]" blocks) - what ffmpeg reads and writes
    /// with -map_metadata, and so the way chapters get muxed into MP4 and Matroska.
    /// </summary>
    public class FfmpegMetadataChapters : SubtitleFormat
    {
        public const string Header = ";FFMETADATA1";

        /// <summary>
        /// Milliseconds, so START/END can be written as plain integers.
        /// </summary>
        private const string TimeBase = "1/1000";

        public override string Extension => ".ffmeta";

        public override string Name => "FFmpeg metadata chapters";

        public override List<string> AlternateExtensions => new List<string> { ".txt", ".ini" };

        public static List<Chapter> ParseChapters(List<string> lines)
        {
            var chapters = new List<Chapter>();
            var inChapter = false;

            // Kept as a fraction rather than a single multiplier: folding "1/1000000000" into one
            // double first and multiplying the tick count by it afterwards loses precision, so a
            // whole number of milliseconds comes back as 12500.000000000002.
            double timeBaseNumerator = 1;
            double timeBaseDenominator = 1;
            double? start = null;
            var title = string.Empty;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Equals("[CHAPTER]", StringComparison.OrdinalIgnoreCase))
                {
                    AddIfComplete(chapters, start, title);
                    inChapter = true;
                    timeBaseNumerator = 1;
                    timeBaseDenominator = 1;
                    start = null;
                    title = string.Empty;
                    continue;
                }

                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    // Any other section ([STREAM], [FORMAT], ...) ends the chapter being read.
                    AddIfComplete(chapters, start, title);
                    inChapter = false;
                    start = null;
                    title = string.Empty;
                    continue;
                }

                if (!inChapter || line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal) ||
                    line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var idx = line.IndexOf('=');
                if (idx < 1)
                {
                    continue;
                }

                var key = line.Substring(0, idx).Trim();
                var value = line.Substring(idx + 1).Trim();

                if (key.Equals("TIMEBASE", StringComparison.OrdinalIgnoreCase))
                {
                    ParseTimeBase(value, ref timeBaseNumerator, ref timeBaseDenominator);
                }
                else if (key.Equals("START", StringComparison.OrdinalIgnoreCase))
                {
                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ticks))
                    {
                        start = ticks * timeBaseNumerator * TimeCode.BaseUnit / timeBaseDenominator;
                    }
                }
                else if (key.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    title = Unescape(value);
                }
            }

            AddIfComplete(chapters, start, title);
            return chapters;
        }

        public static string ToFfmpegMetadata(IList<Chapter> chapters)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);

            for (var i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                var end = ChapterHelper.GetEndMilliseconds(chapters, i);

                sb.AppendLine();
                sb.AppendLine("[CHAPTER]");
                sb.AppendLine($"TIMEBASE={TimeBase}");
                sb.AppendLine($"START={(long)Math.Round(Math.Max(0, chapter.StartMilliseconds))}");
                sb.AppendLine($"END={(long)Math.Round(Math.Max(0, end))}");
                sb.AppendLine($"title={Escape(chapter.Title)}");
            }

            return sb.ToString();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToFfmpegMetadata(ChapterHelper.FromSubtitle(subtitle));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            // The magic header is mandatory in a real ffmetadata file, and demanding it is what
            // keeps this format from claiming every ini-shaped text file that comes past.
            if (lines.Count == 0 || !lines[0].TrimStart('﻿').Trim().StartsWith(Header, StringComparison.OrdinalIgnoreCase))
            {
                _errorCount = 1;
                return;
            }

            var chapters = ParseChapters(lines);
            if (chapters.Count == 0)
            {
                _errorCount = 1;
                return;
            }

            foreach (var p in ChapterHelper.ToSubtitle(chapters).Paragraphs)
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static void AddIfComplete(List<Chapter> chapters, double? start, string title)
        {
            if (start.HasValue)
            {
                chapters.Add(new Chapter(start.Value, title));
            }
        }

        /// <summary>
        /// Reads a "num/den" time base, leaving the fraction unevaluated.
        /// </summary>
        private static void ParseTimeBase(string value, ref double numerator, ref double denominator)
        {
            var parts = value.Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var num) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var den) &&
                num > 0 && den > 0)
            {
                numerator = num;
                denominator = den;
                return;
            }

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var single) && single > 0)
            {
                numerator = single;
                denominator = 1;
            }
        }

        /// <summary>
        /// ffmetadata treats =, ;, # and \ as special, and a trailing backslash continues the line.
        /// </summary>
        internal static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                if (ch == '=' || ch == ';' || ch == '#' || ch == '\\')
                {
                    sb.Append('\\');
                    sb.Append(ch);
                }
                else if (ch == '\r')
                {
                    // Skipped: the \n of a CRLF carries the escape on its own.
                }
                else if (ch == '\n')
                {
                    sb.Append(' ');
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        internal static string Unescape(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('\\') < 0)
            {
                return text ?? string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    i++;
                }

                sb.Append(text[i]);
            }

            return sb.ToString();
        }
    }
}
