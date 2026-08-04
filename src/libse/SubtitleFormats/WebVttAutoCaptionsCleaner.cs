using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Cleans up YouTube auto-generated ("ASR") WebVTT captions, which use a roll-up layout that is
    /// unusable in an editor: every spoken line arrives twice - first with per-word timestamps
    /// (Let&lt;00:00:00.320&gt;&lt;c&gt; us&lt;/c&gt;...), then again as plain text carried over as the top line
    /// of the following cue - glued together by 10 ms "bridge" cues holding nothing new. Every cue is
    /// also tagged "align:start position:0%", which the parser turns into a {\an1} tag on each cue.
    ///
    /// Cleaning keeps one cue per spoken line with the original cue timings, dropping the word
    /// timestamps, the carried-over duplicate lines, the bridge cues and the position tags.
    /// </summary>
    public static partial class WebVttAutoCaptionsCleaner
    {
#if NET7_0_OR_GREATER
        [GeneratedRegex(@"\<\d+:\d+:\d+\.\d+\>")] // <00:00:10.049>
        private static partial Regex RegexWordTimeCodeGen();
        private static readonly Regex RegexWordTimeCode = RegexWordTimeCodeGen();

        [GeneratedRegex(@"\</?c[^>]*\>")] // <c>, <c.colorE5E5E5>, </c>
        private static partial Regex RegexCTagGen();
        private static readonly Regex RegexCTag = RegexCTagGen();

        [GeneratedRegex(@"^\{\\an\d\}")]
        private static partial Regex RegexPositionTagGen();
        private static readonly Regex RegexPositionTag = RegexPositionTagGen();
#else
        private static readonly Regex RegexWordTimeCode = new Regex(@"\<\d+:\d+:\d+\.\d+\>", RegexOptions.Compiled); // <00:00:10.049>
        private static readonly Regex RegexCTag = new Regex(@"\</?c[^>]*\>", RegexOptions.Compiled); // <c>, <c.colorE5E5E5>, </c>
        private static readonly Regex RegexPositionTag = new Regex(@"^\{\\an\d\}", RegexOptions.Compiled);
#endif

        /// <summary>
        /// Cues that only exist to bridge two roll-up states are 10 ms long in every file seen so far.
        /// </summary>
        private const double MaxBridgeDurationMilliseconds = 100;

        /// <summary>
        /// True if <paramref name="subtitle"/> looks like YouTube auto-generated captions: word-level
        /// time codes inside the text plus the very short "bridge" cues that hold no new text. Both
        /// signals are required - a hand-made karaoke-style WebVTT has the former but not the latter,
        /// and must not be touched.
        /// </summary>
        public static bool IsAutoCaptions(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs.Count < 4)
            {
                return false;
            }

            var withWordTimeCodes = 0;
            var bridges = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (RegexWordTimeCode.IsMatch(p.Text))
                {
                    withWordTimeCodes++;
                }
                else if (p.DurationTotalMilliseconds <= MaxBridgeDurationMilliseconds)
                {
                    bridges++;
                }
            }

            return withWordTimeCodes >= 3 &&
                   bridges >= 2 &&
                   withWordTimeCodes * 4 >= subtitle.Paragraphs.Count;
        }

        /// <summary>
        /// Rewrites <paramref name="subtitle"/> in place: strips word timestamps, &lt;c&gt; tags and
        /// position tags, then drops every line already shown by the previous cue - which leaves the
        /// bridge cues empty, so they go away too.
        /// </summary>
        public static void Clean(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            var cleaned = new List<Paragraph>(subtitle.Paragraphs.Count);
            var previousLines = new List<string>();
            foreach (var p in subtitle.Paragraphs)
            {
                var lines = GetPlainLines(p.Text);
                var newLines = new List<string>(lines.Count);
                foreach (var line in lines)
                {
                    if (!previousLines.Contains(line))
                    {
                        newLines.Add(line);
                    }
                }

                // Remember what this cue displayed (not just what was new) - the next cue carries
                // exactly these lines over as its top line(s).
                previousLines = lines;

                if (newLines.Count == 0)
                {
                    continue;
                }

                p.Text = string.Join(Environment.NewLine, newLines);
                p.Style = string.Empty; // the raw "align:start position:0%" the position tag came from
                cleaned.Add(p);
            }

            subtitle.Paragraphs.Clear();
            subtitle.Paragraphs.AddRange(cleaned);
            subtitle.Renumber();
        }

        private static List<string> GetPlainLines(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                return result;
            }

            var stripped = RegexWordTimeCode.Replace(text, string.Empty);
            stripped = RegexCTag.Replace(stripped, string.Empty);
            foreach (var line in stripped.SplitToLines())
            {
                var s = RegexPositionTag.Replace(line, string.Empty).Trim();
                if (s.Length > 0)
                {
                    result.Add(s);
                }
            }

            return result;
        }
    }
}
