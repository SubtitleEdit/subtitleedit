using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Common
{
    /// <summary>
    /// Reads and writes the per-line marks that no subtitle format carries - bookmarks and the
    /// forced-narrative flag - to a sidecar file next to the subtitle.
    /// </summary>
    /// <remarks>
    /// The file keeps its original "<c>.SE.bookmarks</c>" name and its bookmark array so files
    /// written by earlier versions keep working; forced marks live in a second array that older
    /// versions simply ignore.
    /// <para>
    /// Marks are keyed by start time (<c>ms</c>) rather than by paragraph index: an index shifts
    /// as soon as a line is inserted above a marked one, which silently moved every mark below it
    /// onto the wrong line at the next load. The index is still written, and used as a last
    /// resort, so files from before the ms key still resolve.
    /// </para>
    /// </remarks>
    public class SubtitleMarksPersistence
    {
        /// <summary>
        /// How far a mark's stored start time may be from a paragraph's and still be considered
        /// the same line. Saving to a frame-based format rounds the times on the way to disk, so
        /// an exact match would fail after re-opening; one frame is ~42 ms at 23.976 fps.
        /// </summary>
        private const double StartTimeToleranceMs = 100.0;

        private readonly Subtitle _subtitle;
        private readonly string? _fileName;

        public SubtitleMarksPersistence(Subtitle subtitle, string? fileName)
        {
            _subtitle = subtitle;
            _fileName = fileName;
        }

        private string GetMarksFileName()
        {
            return _fileName + ".SE.bookmarks";
        }

        public bool Save()
        {
            // IsNullOrEmpty: an untitled subtitle has string.Empty, not null, which made the
            // path a bare relative ".SE.bookmarks" written into the working directory - and
            // clearing bookmarks then deleted whatever file of that name happened to be there.
            if (string.IsNullOrEmpty(_fileName))
            {
                return false;
            }

            var fileName = GetMarksFileName();
            var serializedMarks = SerializeMarks();

            try
            {
                if (serializedMarks != null)
                {
                    File.WriteAllText(fileName, serializedMarks, Encoding.UTF8);
                }
                else if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string? SerializeMarks()
        {
            var bookmarks = new StringBuilder();
            var bookmarkCount = 0;
            var forced = new StringBuilder();
            var forcedCount = 0;

            for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                var key = "\"ms\":" + WriteMs(p.StartTime.TotalMilliseconds) + ",\"idx\":" + i;

                if (p.Bookmark != null)
                {
                    bookmarkCount++;
                    if (bookmarkCount > 1)
                    {
                        bookmarks.Append(',');
                    }

                    bookmarks.Append("{" + key + ",\"txt\":\"" + Json.EncodeJsonText(p.Bookmark) + "\"}");
                }

                if (p.Forced)
                {
                    forcedCount++;
                    if (forcedCount > 1)
                    {
                        forced.Append(',');
                    }

                    forced.Append("{" + key + "}");
                }
            }

            if (bookmarkCount == 0 && forcedCount == 0)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine("{\"bookmarks\":[");
            sb.Append(bookmarks);
            sb.AppendLine("],\"forced\":[");
            sb.Append(forced);
            sb.AppendLine("]}");
            return sb.ToString();
        }

        private static string WriteMs(double totalMilliseconds)
        {
            return Math.Round(totalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        public bool Load()
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                return false;
            }

            var fileName = GetMarksFileName();
            if (!File.Exists(fileName))
            {
                return true;
            }

            try
            {
                var text = File.ReadAllText(fileName, Encoding.UTF8);
                Apply(ReadMarks(text, "bookmarks"), (p, mark) => p.Bookmark = mark.Text);
                Apply(ReadMarks(text, "forced"), (p, _) => p.Forced = true);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resolves each mark to a paragraph and applies it. Exact start-time matches are taken
        /// first so a shifted line cannot steal a mark that belongs to an unmoved one, then near
        /// matches, and only then the stored index - which is all a pre-ms sidecar has.
        /// </summary>
        private void Apply(List<MarkEntry> marks, Action<Paragraph, MarkEntry> apply)
        {
            if (marks.Count == 0)
            {
                return;
            }

            var taken = new HashSet<int>();
            var pending = new List<MarkEntry>();

            foreach (var mark in marks)
            {
                var index = mark.Milliseconds.HasValue
                    ? FindByStartTime(mark.Milliseconds.Value, 0.5, taken)
                    : -1;

                if (index < 0)
                {
                    pending.Add(mark);
                    continue;
                }

                taken.Add(index);
                apply(_subtitle.Paragraphs[index], mark);
            }

            var stillPending = new List<MarkEntry>();
            foreach (var mark in pending)
            {
                var index = mark.Milliseconds.HasValue
                    ? FindByStartTime(mark.Milliseconds.Value, StartTimeToleranceMs, taken)
                    : -1;

                if (index < 0)
                {
                    stillPending.Add(mark);
                    continue;
                }

                taken.Add(index);
                apply(_subtitle.Paragraphs[index], mark);
            }

            foreach (var mark in stillPending)
            {
                var index = mark.Index;
                if (index < 0 || index >= _subtitle.Paragraphs.Count || taken.Contains(index))
                {
                    continue;
                }

                taken.Add(index);
                apply(_subtitle.Paragraphs[index], mark);
            }
        }

        /// <summary>Index of the closest not-yet-taken paragraph within <paramref name="toleranceMs"/>, or -1.</summary>
        private int FindByStartTime(double milliseconds, double toleranceMs, HashSet<int> taken)
        {
            var best = -1;
            var bestDistance = double.MaxValue;

            for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                if (taken.Contains(i))
                {
                    continue;
                }

                var distance = Math.Abs(_subtitle.Paragraphs[i].StartTime.TotalMilliseconds - milliseconds);
                if (distance <= toleranceMs && distance < bestDistance)
                {
                    best = i;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static List<MarkEntry> ReadMarks(string input, string tag)
        {
            var marks = new List<MarkEntry>();

            var array = ExtractArray(input, tag);
            if (array == null)
            {
                return marks;
            }

            foreach (var entry in Json.ReadObjectArray(array))
            {
                var rawText = Json.ReadTag(entry, "txt");
                var mark = new MarkEntry
                {
                    Text = rawText == null ? string.Empty : Json.DecodeJsonText(rawText),
                };

                if (double.TryParse(Json.ReadTag(entry, "ms"), NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
                {
                    mark.Milliseconds = ms;
                }

                if (int.TryParse(Json.ReadTag(entry, "idx"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx))
                {
                    mark.Index = idx;
                }
                else if (!mark.Milliseconds.HasValue)
                {
                    continue;
                }

                marks.Add(mark);
            }

            return marks;
        }

        /// <summary>
        /// The "[...]" text of the named array, or null when the file has no such array - a
        /// sidecar written before forced marks existed has no "forced" array at all. Hand-rolled
        /// rather than a substring search for the first '[': the file now holds two arrays.
        /// </summary>
        private static string? ExtractArray(string input, string tag)
        {
            var needle = "\"" + tag + "\"";
            var start = input.IndexOf(needle, StringComparison.Ordinal);
            if (start < 0)
            {
                return null;
            }

            start = input.IndexOf('[', start + needle.Length);
            if (start < 0)
            {
                return null;
            }

            var depth = 0;
            var inString = false;
            var escaped = false;

            for (var i = start; i < input.Length; i++)
            {
                var c = input[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = !inString;
                }
                else if (inString)
                {
                    // Brackets inside bookmark text are data, not structure.
                }
                else if (c == '[')
                {
                    depth++;
                }
                else if (c == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return input.Substring(start, i - start + 1);
                    }
                }
            }

            return null;
        }

        private sealed class MarkEntry
        {
            /// <summary>Start time of the marked line, or null in a sidecar written before ms keys.</summary>
            public double? Milliseconds { get; set; }

            public int Index { get; set; } = -1;

            public string Text { get; set; } = string.Empty;
        }
    }
}
