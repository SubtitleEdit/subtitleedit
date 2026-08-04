using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// Helpers for TTML/IMSC1 ("stpp") subtitle samples in MP4 (ISO/IEC 14496-30).
    /// </summary>
    public static class Mp4TtmlHelper
    {
        /// <summary>
        /// Splits concatenated TTML documents. An stpp mdat (or a single large read of one)
        /// often holds several complete TTML sample documents back to back — one per sample —
        /// and feeding the concatenation to an XML parser fails on the second document.
        /// </summary>
        public static List<string> SplitTtmlDocuments(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                return result;
            }

            var pos = 0;
            while (pos < text.Length)
            {
                var start = FindRootTag(text, pos);
                if (start < 0)
                {
                    break;
                }

                var close = FindCloseTag(text, start);
                if (close < 0)
                {
                    result.Add(text.Substring(start));
                    break;
                }

                result.Add(text.Substring(start, close - start));
                pos = close;
            }

            if (result.Count == 0)
            {
                result.Add(text);
            }

            return result;
        }

        /// <summary>
        /// Index of the next TTML root tag ("&lt;tt&gt;", "&lt;tt ...&gt;" or a namespaced
        /// "&lt;ns:tt ...&gt;" like "&lt;tt:tt&gt;") at or after <paramref name="startIndex"/>,
        /// or -1. Plain "&lt;tt" prefix matching is not enough: it would also hit
        /// "&lt;ttm:title&gt;" inside the document head.
        /// </summary>
        private static int FindRootTag(string text, int startIndex)
        {
            var pos = startIndex;
            while (pos < text.Length)
            {
                var idx = text.IndexOf("<tt", pos, StringComparison.Ordinal);
                if (idx < 0)
                {
                    return -1;
                }

                var after = idx + 3;
                if (after >= text.Length)
                {
                    return -1;
                }

                var c = text[after];
                if (c == '>' || c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    return idx;
                }

                if (c == ':' && after + 2 < text.Length && text[after + 1] == 't' && text[after + 2] == 't')
                {
                    return idx;
                }

                pos = idx + 1;
            }

            return -1;
        }

        /// <summary>
        /// Index just past the root close tag ("&lt;/tt&gt;" or "&lt;/tt:tt&gt;") that ends the
        /// document whose root starts at <paramref name="rootIndex"/>, or -1.
        /// </summary>
        private static int FindCloseTag(string text, int rootIndex)
        {
            var pos = rootIndex;
            while (pos < text.Length)
            {
                var idx = text.IndexOf("</tt", pos, StringComparison.Ordinal);
                if (idx < 0)
                {
                    return -1;
                }

                var after = idx + 4;
                if (after < text.Length && (text[after] == '>' || text[after] == ':'))
                {
                    var end = text.IndexOf('>', after);
                    return end < 0 ? -1 : end + 1;
                }

                pos = idx + 1;
            }

            return -1;
        }

        /// <summary>
        /// Parses a single TTML document into paragraphs. Returns an empty list if the
        /// document is not valid TTML (e.g. an empty gap-filler document).
        /// </summary>
        public static List<Paragraph> ParseTtmlDocument(string xml)
        {
            try
            {
                var lines = xml.SplitToLines(100_000);
                var format = new TimedText10();
                if (!format.IsMine(lines, null))
                {
                    return new List<Paragraph>();
                }

                var subtitle = new Subtitle();
                format.LoadSubtitle(subtitle, lines, null);
                return subtitle.Paragraphs;
            }
            catch
            {
                return new List<Paragraph>();
            }
        }

        /// <summary>
        /// Whether the cue times in a TTML sample document are relative to the sample start
        /// (Smooth Streaming style zero-based documents) rather than absolute media times.
        /// If all cues end within the sample's duration while the sample itself starts later,
        /// the times are segment-relative and must be shifted by the sample start.
        /// </summary>
        public static bool AreTimesSampleRelative(List<Paragraph> docParagraphs, double sampleStartMs, double sampleDurationMs)
        {
            if (docParagraphs.Count == 0 || sampleStartMs <= 0 || sampleDurationMs <= 0)
            {
                return false;
            }

            foreach (var p in docParagraphs)
            {
                if (p.EndTime.TotalMilliseconds > sampleDurationMs)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
