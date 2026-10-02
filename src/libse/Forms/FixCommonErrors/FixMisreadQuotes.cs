using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    /// <summary>
    /// Fixes double quotes that OCR has read as apostrophes:
    ///   "Hello' -> "Hello"
    ///   'Hello" -> "Hello"
    ///   'Hello' -> "Hello" (whole line wrapped in apostrophes, no double quotes in the text)
    ///   ''Hello' -> "Hello" (two apostrophes are always treated as a double quote)
    /// Contractions (don't), elisions ('cause, goin') and possessives (the boys' toys) are left alone.
    /// English only - other languages use apostrophes differently (Dutch 's avonds, Italian un po').
    /// </summary>
    public class FixMisreadQuotes : IFixCommonError
    {
        public static class Language
        {
            public static string FixMisreadQuotes { get; set; } = "Fix apostrophes misread as double quotes (OCR, English)";
        }

        private static readonly HashSet<string> ElisionStarts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cause", "cos", "coz", "cuz", "em", "til", "till", "bout", "round", "n", "twas", "tis", "kay", "scuse", "nuff", "sup",
        };

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.FixMisreadQuotes;
            var noOfFixes = 0;
            if (!string.Equals(callbacks.Language, "en", StringComparison.OrdinalIgnoreCase))
            {
                callbacks.UpdateFixStatus(noOfFixes, fixAction);
                return;
            }

            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var newText = FixText(p.Text);
                if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    p.Text = newText;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }

            callbacks.UpdateFixStatus(noOfFixes, fixAction);
        }

        public static string FixText(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('\'') < 0 ||
                text.IndexOfAny(new[] { '„', '“', '”', '«', '»' }) >= 0)
            {
                return text;
            }

            // Two apostrophes are a misread double quote
            text = text.Replace("''", "\"");

            // Visible characters (tags removed) mapped back to their index in the original text
            var visible = new StringBuilder(text.Length);
            var map = new List<int>(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                var tagEnd = GetTagEnd(text, i);
                if (tagEnd > i)
                {
                    i = tagEnd;
                    continue;
                }

                visible.Append(text[i]);
                map.Add(i);
            }

            var v = visible.ToString();
            var chars = text.ToCharArray();
            var changed = false;
            foreach (var segment in GetSegments(v))
            {
                foreach (var index in FixSegment(v, segment.Item1, segment.Item2))
                {
                    chars[map[index]] = '"';
                    changed = true;
                }
            }

            return changed ? new string(chars) : text;
        }

        private static int GetTagEnd(string text, int i)
        {
            if (text[i] == '<')
            {
                var end = text.IndexOf('>', i + 1);
                if (end > i + 1 && (text[i + 1] == '/' || char.IsLetter(text[i + 1])) && text.IndexOf('<', i + 1, end - i - 1) < 0)
                {
                    return end;
                }
            }
            else if (text[i] == '{' && i + 1 < text.Length && text[i + 1] == '\\')
            {
                var end = text.IndexOf('}', i + 1);
                if (end > i)
                {
                    return end;
                }
            }

            return -1;
        }

        /// <summary>
        /// Dialogs (all lines start with a dash) are handled line by line, otherwise the whole text is one segment.
        /// </summary>
        private static List<Tuple<int, int>> GetSegments(string v)
        {
            var segments = new List<Tuple<int, int>>();
            var lineStarts = new List<int> { 0 };
            for (var i = 0; i < v.Length; i++)
            {
                if (v[i] == '\n')
                {
                    lineStarts.Add(i + 1);
                }
            }

            var isDialog = lineStarts.Count > 1;
            foreach (var start in lineStarts)
            {
                var s = start;
                while (s < v.Length && (v[s] == ' ' || v[s] == '\t'))
                {
                    s++;
                }

                if (s >= v.Length || v[s] != '-')
                {
                    isDialog = false;
                }
            }

            if (!isDialog)
            {
                segments.Add(new Tuple<int, int>(0, v.Length));
                return segments;
            }

            for (var i = 0; i < lineStarts.Count; i++)
            {
                var end = i + 1 < lineStarts.Count ? lineStarts[i + 1] - 1 : v.Length;
                segments.Add(new Tuple<int, int>(lineStarts[i], end));
            }

            return segments;
        }

        private static List<int> FixSegment(string v, int start, int end)
        {
            var result = new List<int>();
            var doubleQuotes = new List<int>();
            var apostrophes = new List<int>();
            for (var i = start; i < end; i++)
            {
                if (v[i] == '"')
                {
                    doubleQuotes.Add(i);
                }
                else if (v[i] == '\'')
                {
                    apostrophes.Add(i);
                }
            }

            if (apostrophes.Count == 0 || doubleQuotes.Count > 1)
            {
                return result;
            }

            if (doubleQuotes.Count == 1)
            {
                var dq = doubleQuotes[0];
                if (IsOpening(v, dq, start, end))
                {
                    // "Hello' -> "Hello"
                    var closing = FindSingle(apostrophes, a => a > dq && IsClosingApostrophe(v, a, start, end));
                    if (closing >= 0 && !apostrophes.Exists(a => a > dq && a < closing && IsOpeningApostrophe(v, a, start, end)))
                    {
                        result.Add(closing);
                    }
                }
                else if (IsClosing(v, dq, start, end))
                {
                    // 'Hello" -> "Hello"
                    var opening = FindSingle(apostrophes, a => a < dq && IsOpeningApostrophe(v, a, start, end));
                    if (opening >= 0 && !apostrophes.Exists(a => a > opening && a < dq && IsClosingApostrophe(v, a, start, end)))
                    {
                        result.Add(opening);
                    }
                }

                return result;
            }

            // 'Hello' -> "Hello" (the whole segment is wrapped in apostrophes)
            if (apostrophes.Count < 2)
            {
                return result;
            }

            var first = apostrophes[0];
            var last = apostrophes[apostrophes.Count - 1];
            if (first != FirstTextIndex(v, start, end) ||
                last != LastTextIndex(v, start, end) ||
                !IsOpeningApostrophe(v, first, start, end) ||
                !IsClosingApostrophe(v, last, start, end))
            {
                return result;
            }

            for (var i = 1; i < apostrophes.Count - 1; i++)
            {
                if (IsOpeningApostrophe(v, apostrophes[i], start, end) || IsClosingApostrophe(v, apostrophes[i], start, end))
                {
                    return result; // nested or ambiguous quotes
                }
            }

            result.Add(first);
            result.Add(last);
            return result;
        }

        private static int FindSingle(List<int> list, Predicate<int> match)
        {
            var found = -1;
            foreach (var item in list)
            {
                if (match(item))
                {
                    if (found >= 0)
                    {
                        return -1;
                    }

                    found = item;
                }
            }

            return found;
        }

        /// <summary>
        /// First index after leading whitespace and a dialog dash.
        /// </summary>
        private static int FirstTextIndex(string v, int start, int end)
        {
            var i = start;
            while (i < end && char.IsWhiteSpace(v[i]))
            {
                i++;
            }

            if (i < end && v[i] == '-')
            {
                i++;
                while (i < end && char.IsWhiteSpace(v[i]))
                {
                    i++;
                }
            }

            return i;
        }

        /// <summary>
        /// Last index before trailing whitespace and sentence ending punctuation.
        /// </summary>
        private static int LastTextIndex(string v, int start, int end)
        {
            var i = end - 1;
            while (i >= start && (char.IsWhiteSpace(v[i]) || ".,!?;:".IndexOf(v[i]) >= 0))
            {
                i--;
            }

            return i;
        }

        private static bool IsOpenerBoundary(string v, int i, int start)
        {
            return i == start || char.IsWhiteSpace(v[i - 1]) || "-([—".IndexOf(v[i - 1]) >= 0;
        }

        private static bool IsCloserBoundary(string v, int i, int end)
        {
            return i + 1 >= end || char.IsWhiteSpace(v[i + 1]) || ".,!?;:)]-—".IndexOf(v[i + 1]) >= 0;
        }

        private static bool IsOpening(string v, int i, int start, int end)
        {
            return IsOpenerBoundary(v, i, start) && i + 1 < end && !char.IsWhiteSpace(v[i + 1]);
        }

        private static bool IsClosing(string v, int i, int start, int end)
        {
            return i > start && !char.IsWhiteSpace(v[i - 1]) && IsCloserBoundary(v, i, end);
        }

        private static bool IsOpeningApostrophe(string v, int i, int start, int end)
        {
            if (!IsOpenerBoundary(v, i, start) || i + 1 >= end || !char.IsLetter(v[i + 1]))
            {
                return false;
            }

            // elisions like 'cause, 'em, rock 'n' roll
            var wordEnd = i + 1;
            while (wordEnd < end && char.IsLetter(v[wordEnd]))
            {
                wordEnd++;
            }

            return !ElisionStarts.Contains(v.Substring(i + 1, wordEnd - i - 1));
        }

        private static bool IsClosingApostrophe(string v, int i, int start, int end)
        {
            if (i <= start || char.IsWhiteSpace(v[i - 1]) || !IsCloserBoundary(v, i, end))
            {
                return false;
            }

            var prev = v[i - 1];
            if (!char.IsLetterOrDigit(prev) && ".,!?".IndexOf(prev) < 0)
            {
                return false;
            }

            // dropped g like goin', nothin'
            if (i - 2 >= start && (prev == 'n' || prev == 'N') && (v[i - 2] == 'i' || v[i - 2] == 'I'))
            {
                return false;
            }

            // possessive like "the boys' toys"
            if ((prev == 's' || prev == 'S') && i + 2 < end && v[i + 1] == ' ' && char.IsLower(v[i + 2]))
            {
                return false;
            }

            return true;
        }
    }
}
