using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// CANVASs SSTG1 series project file (SSTG1Pro, SSTG1Lite, NetSSTG1) - ".sdb". Import only.
    ///
    /// The file is a Jet 4 (Access 2000) database whose "Standard Jet DB" header text is replaced by
    /// "CANVASs SST FMT" (issue #14704). The cues live in the "Track" table with media frame numbers;
    /// the "Timecodes" table maps media frames to programme timecode frames (one row per continuous
    /// timecode run), "Globals" holds the frame type, and "Format" holds per-character markup (ruby,
    /// italic, kerning...). The column descriptions stored in the database catalog document the codes.
    /// </summary>
    public class CanvassSstg1 : SubtitleFormat
    {
        public const string NameOfFormat = "CANVASs SSTG1";

        private static readonly byte[] Signature = Encoding.ASCII.GetBytes("CANVASs SST FMT");

        private const int SignatureOffset = 4;

        /// <summary>Format table code: ruby (furigana) - strParam holds the ruby text.</summary>
        private const int FormatRuby = 2;

        /// <summary>Format table code: italic - iParam 1 = on.</summary>
        private const int FormatItalic = 8;

        public override string Extension => ".sdb";

        public override string Name => NameOfFormat;

        public override bool IsTextBased => false;

        private class TimecodeSegment
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public long BaseFrames { get; set; }
        }

        private class FormatEntry
        {
            public int From { get; set; }
            public int To { get; set; }
            public int Format { get; set; }
            public int Param { get; set; }
            public string Text { get; set; }
        }

        private class Token
        {
            public string Text { get; set; }
            public bool IsNewLine { get; set; }
            public bool Italic { get; set; }
            public int RawStart { get; set; }
            public int RawEnd { get; set; }
        }

        public static bool HasSignature(byte[] buffer)
        {
            if (buffer == null || buffer.Length < SignatureOffset + Signature.Length)
            {
                return false;
            }

            for (var i = 0; i < Signature.Length; i++)
            {
                if (buffer[SignatureOffset + i] != Signature[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return false;
            }

            try
            {
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Length < 3 * 4096 || fileInfo.Length > 512 * 1024 * 1024)
                {
                    return false;
                }

                var header = FileUtil.ReadBytesShared(fileName, 32);
                if (!HasSignature(header) && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var subtitle = new Subtitle();
                LoadSubtitle(subtitle, lines, fileName);
                return subtitle.Paragraphs.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;

            var buffer = FileUtil.ReadAllBytesShared(fileName);
            if (!JetDatabaseReader.IsJet4(buffer))
            {
                return;
            }

            JetDatabaseReader db;
            try
            {
                db = new JetDatabaseReader(buffer);
            }
            catch
            {
                return;
            }

            if (db.IsEncrypted || !db.HasTable("Track"))
            {
                return;
            }

            var frameRate = GetFrameRate(db, out var frameRateFromFile);
            if (frameRateFromFile)
            {
                Configuration.Settings.General.CurrentFrameRate = frameRate; // like EBU STL: the file knows its own frame rate
            }

            var segments = ReadTimecodes(db);
            var formats = ReadFormats(db);

            var paragraphs = new List<KeyValuePair<int, Paragraph>>();
            foreach (var row in db.ReadTable("Track"))
            {
                var inFrame = GetInt(row, "iInFrame");
                var outFrame = GetInt(row, "iOutFrame");
                if (inFrame == null || outFrame == null)
                {
                    continue;
                }

                var text = GetString(row, "strTranslation");
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = GetString(row, "strOriginal");
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var id = GetInt(row, "iID") ?? 0;
                formats.TryGetValue(id, out var entries);
                text = ApplyFormatting(text, entries);

                var start = FramesToMilliseconds(ToTimecodeFrames(inFrame.Value, segments), frameRate);
                var end = FramesToMilliseconds(ToTimecodeFrames(outFrame.Value, segments), frameRate);
                paragraphs.Add(new KeyValuePair<int, Paragraph>(id, new Paragraph(text, start, end)));
            }

            // Rows are stored in editing order, not in time order; a second track (signs on top of
            // dialogue) is simply merged in by time.
            foreach (var pair in paragraphs.OrderBy(p => p.Value.StartTime.TotalMilliseconds).ThenBy(p => p.Key))
            {
                subtitle.Paragraphs.Add(pair.Value);
            }

            subtitle.Renumber();
        }

        /// <summary>
        /// Globals.iFrameType per the catalog description: 0 = "NTSC DF"/"NTSC NDF" (drop frame is the
        /// separate bDropFrame flag), 1 = "PAL", 2 = "24F", 4 = "FILM".
        /// </summary>
        private static double GetFrameRate(JetDatabaseReader db, out bool fromFile)
        {
            var globals = db.ReadTable("Globals").FirstOrDefault();
            var frameType = globals == null ? null : GetInt(globals, "iFrameType");
            fromFile = true;
            switch (frameType)
            {
                case 0:
                    return 29.97;
                case 1:
                    return 25;
                case 2:
                    return 24;
                case 4:
                    return 23.976;
                default:
                    fromFile = false;
                    var current = Configuration.Settings.General.CurrentFrameRate;
                    return current > 0 ? current : 29.97;
            }
        }

        private static List<TimecodeSegment> ReadTimecodes(JetDatabaseReader db)
        {
            var segments = new List<TimecodeSegment>();
            foreach (var row in db.ReadTable("Timecodes"))
            {
                var start = GetInt(row, "iStartFrame");
                var end = GetInt(row, "iEndFrame");
                var baseFrames = GetInt(row, "iBaseFrames");
                if (start == null || end == null || baseFrames == null)
                {
                    continue;
                }

                segments.Add(new TimecodeSegment { StartFrame = start.Value, EndFrame = end.Value, BaseFrames = baseFrames.Value });
            }

            return segments.OrderBy(s => s.StartFrame).ToList();
        }

        /// <summary>
        /// Media frames are 1-based. Timecodes.iBaseFrames is the cumulative frame count of the
        /// programme timecode at the segment's first media frame, so a cue's timecode frame is
        /// base + (frame - segment start). Without any timecode segment the media frame is used as is.
        /// </summary>
        private static long ToTimecodeFrames(int frame, List<TimecodeSegment> segments)
        {
            if (segments.Count == 0)
            {
                return Math.Max(0, frame - 1);
            }

            TimecodeSegment nearest = null;
            foreach (var segment in segments)
            {
                if (frame >= segment.StartFrame && frame <= segment.EndFrame)
                {
                    return segment.BaseFrames + (frame - segment.StartFrame);
                }

                if (segment.StartFrame <= frame)
                {
                    nearest = segment;
                }
            }

            if (nearest == null)
            {
                nearest = segments[0];
            }

            return Math.Max(0, nearest.BaseFrames + (frame - nearest.StartFrame));
        }

        /// <summary>Per-cue markup entries; the ones with iRubyID set style a ruby run and are skipped.</summary>
        private static Dictionary<int, List<FormatEntry>> ReadFormats(JetDatabaseReader db)
        {
            var result = new Dictionary<int, List<FormatEntry>>();
            foreach (var row in db.ReadTable("Format"))
            {
                var subtitleId = GetInt(row, "iSubtitleID");
                var from = GetInt(row, "iFrom");
                var to = GetInt(row, "iTo");
                var format = GetInt(row, "iFormat");
                if (subtitleId == null || from == null || to == null || format == null)
                {
                    continue;
                }

                if ((GetInt(row, "iRubyID") ?? 0) != 0)
                {
                    continue;
                }

                if (!result.TryGetValue(subtitleId.Value, out var list))
                {
                    list = new List<FormatEntry>();
                    result[subtitleId.Value] = list;
                }

                list.Add(new FormatEntry
                {
                    From = from.Value,
                    To = to.Value,
                    Format = format.Value,
                    Param = GetInt(row, "iParam") ?? 0,
                    Text = GetString(row, "strParam"),
                });
            }

            return result;
        }

        /// <summary>
        /// Character positions in the Format table index the raw text with "\r\n" counting as two
        /// characters. Ruby becomes the same ruby-container markup the Lambda Cap and Netflix IMSC 1.1
        /// Japanese formats use; italic becomes &lt;i&gt; runs. An italic run that crosses a line break is
        /// taken as "the whole cue" - the editor's selection ranges are not reliable across lines.
        /// </summary>
        private static string ApplyFormatting(string rawText, List<FormatEntry> entries)
        {
            var tokens = Tokenize(rawText.TrimEnd('\r', '\n'));

            if (entries != null)
            {
                foreach (var ruby in entries.Where(e => e.Format == FormatRuby && !string.IsNullOrEmpty(e.Text)).OrderBy(e => e.From))
                {
                    MergeRuby(tokens, ruby);
                }

                foreach (var italic in entries.Where(e => e.Format == FormatItalic && e.Param != 0))
                {
                    ApplyItalic(tokens, italic);
                }
            }

            var lines = new List<string>();
            var allItalic = true;
            var lineTokens = new List<Token>();
            foreach (var token in tokens.Concat(new[] { new Token { IsNewLine = true } }))
            {
                if (token.IsNewLine)
                {
                    lines.Add(BuildLine(lineTokens, out var lineItalic));
                    allItalic &= lineItalic;
                    lineTokens.Clear();
                }
                else
                {
                    lineTokens.Add(token);
                }
            }

            if (allItalic && lines.Count > 0)
            {
                return "<i>" + string.Join(Environment.NewLine, lines.Select(l => HtmlUtil.RemoveOpenCloseTags(l, HtmlUtil.TagItalic))) + "</i>";
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();
            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    tokens.Add(new Token { IsNewLine = true, RawStart = i, RawEnd = i + 1 });
                    i += 2;
                }
                else if (c == '\r' || c == '\n')
                {
                    tokens.Add(new Token { IsNewLine = true, RawStart = i, RawEnd = i });
                    i++;
                }
                else
                {
                    tokens.Add(new Token { Text = c.ToString(), RawStart = i, RawEnd = i });
                    i++;
                }
            }

            return tokens;
        }

        private static void MergeRuby(List<Token> tokens, FormatEntry ruby)
        {
            var first = -1;
            var last = -1;
            for (var i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];
                if (t.IsNewLine || t.RawStart < ruby.From || t.RawEnd > ruby.To)
                {
                    continue;
                }

                if (first < 0)
                {
                    first = i;
                }

                last = i;
            }

            if (first < 0)
            {
                return;
            }

            var baseText = string.Concat(tokens.Skip(first).Take(last - first + 1).Select(t => t.Text));
            var merged = new Token
            {
                Text = "<ruby-container><ruby-base>" + baseText + "</ruby-base><ruby-text>" + ruby.Text + "</ruby-text></ruby-container>",
                RawStart = tokens[first].RawStart,
                RawEnd = tokens[last].RawEnd,
                Italic = tokens[first].Italic,
            };
            tokens.RemoveRange(first, last - first + 1);
            tokens.Insert(first, merged);
        }

        private static void ApplyItalic(List<Token> tokens, FormatEntry italic)
        {
            var crossesLineBreak = tokens.Any(t => t.IsNewLine && t.RawStart <= italic.To && t.RawEnd >= italic.From);
            if (crossesLineBreak)
            {
                foreach (var t in tokens)
                {
                    t.Italic = true;
                }

                return;
            }

            foreach (var t in tokens)
            {
                if (!t.IsNewLine && t.RawEnd >= italic.From && t.RawStart <= italic.To)
                {
                    t.Italic = true;
                }
            }
        }

        private static string BuildLine(List<Token> tokens, out bool allItalic)
        {
            var sb = new StringBuilder();
            var inItalic = false;
            allItalic = tokens.Count > 0;
            foreach (var t in tokens)
            {
                if (t.Italic && !inItalic)
                {
                    sb.Append("<i>");
                    inItalic = true;
                }
                else if (!t.Italic && inItalic)
                {
                    sb.Append("</i>");
                    inItalic = false;
                }

                allItalic &= t.Italic;
                sb.Append(t.Text);
            }

            if (inItalic)
            {
                sb.Append("</i>");
            }

            return sb.ToString();
        }

        private static int? GetInt(Dictionary<string, object> row, string column)
        {
            if (row.TryGetValue(column, out var value))
            {
                if (value is int i)
                {
                    return i;
                }

                if (value is double d)
                {
                    return (int)Math.Round(d);
                }
            }

            return null;
        }

        private static string GetString(Dictionary<string, object> row, string column)
        {
            return row.TryGetValue(column, out var value) && value is string s ? s : string.Empty;
        }
    }
}
