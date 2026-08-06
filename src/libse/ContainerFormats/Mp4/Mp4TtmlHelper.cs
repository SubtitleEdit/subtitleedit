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
                var start = FindRootTag(text, pos, out var rootTagName);
                if (start < 0)
                {
                    break;
                }

                var close = FindCloseTag(text, start, rootTagName);
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
        /// or -1. <paramref name="rootTagName"/> receives the qualified name as written, so the
        /// matching close tag can be found by exact name. Plain "&lt;tt" prefix matching is not
        /// enough: it would also hit "&lt;ttm:title&gt;" inside the document head.
        /// </summary>
        private static int FindRootTag(string text, int startIndex, out string rootTagName)
        {
            rootTagName = null;
            var pos = startIndex;
            while (pos < text.Length)
            {
                var i = text.IndexOf('<', pos);
                if (i < 0)
                {
                    return -1;
                }

                pos = i + 1;
                var nameStart = i + 1;
                var nameEnd = nameStart;
                while (nameEnd < text.Length && IsNameChar(text[nameEnd]))
                {
                    nameEnd++;
                }

                if (nameEnd == nameStart || nameEnd >= text.Length)
                {
                    continue; // "</...", "<?xml ...", "<!--" or a truncated tag
                }

                var afterName = text[nameEnd];
                if (afterName != '>' && afterName != '/' && afterName != ' ' && afterName != '\t' && afterName != '\r' && afterName != '\n')
                {
                    continue;
                }

                var qualifiedName = text.Substring(nameStart, nameEnd - nameStart);
                var colon = qualifiedName.LastIndexOf(':');
                var localName = colon < 0 ? qualifiedName : qualifiedName.Substring(colon + 1);
                if (localName == "tt")
                {
                    rootTagName = qualifiedName;
                    return i;
                }
            }

            return -1;
        }

        private static bool IsNameChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == ':' || c == '_' || c == '-' || c == '.';
        }

        /// <summary>
        /// Index just past the close tag that ends the document whose root starts at
        /// <paramref name="rootIndex"/>, or -1. The close tag must carry the root's own
        /// qualified name: matching "&lt;/tt" loosely also hits "&lt;/tt:p&gt;", which cut
        /// namespaced documents off after their first cue.
        /// </summary>
        private static int FindCloseTag(string text, int rootIndex, string rootTagName)
        {
            var closeTag = "</" + rootTagName;
            var pos = rootIndex;
            while (pos < text.Length)
            {
                var idx = text.IndexOf(closeTag, pos, StringComparison.Ordinal);
                if (idx < 0)
                {
                    return -1;
                }

                var after = idx + closeTag.Length;
                while (after < text.Length && (text[after] == ' ' || text[after] == '\t' || text[after] == '\r' || text[after] == '\n'))
                {
                    after++;
                }

                if (after < text.Length && text[after] == '>')
                {
                    return after + 1;
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
        /// If all cues start before the sample does and end within the sample's duration, the
        /// times are segment-relative and must be shifted by the sample start.
        /// </summary>
        public static bool AreTimesSampleRelative(List<Paragraph> docParagraphs, double sampleStartMs, double sampleDurationMs)
        {
            if (docParagraphs.Count == 0 || sampleStartMs <= 0 || sampleDurationMs <= 0)
            {
                return false;
            }

            foreach (var p in docParagraphs)
            {
                // A cue with absolute media times cannot start before the sample carrying it.
                // Without this the check misfired on any absolute cue that happened to end
                // inside the sample duration (long segments, or a track with a start offset),
                // shifting it a whole sample start too late.
                if (p.StartTime.TotalMilliseconds >= sampleStartMs)
                {
                    return false;
                }

                if (p.EndTime.TotalMilliseconds > sampleDurationMs)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
