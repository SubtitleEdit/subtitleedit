using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Matroska chapter XML - the interchange format mkvmerge reads via --chapters and mkvextract
    /// writes. Times are "HH:MM:SS.nnnnnnnnn" (nanoseconds).
    /// </summary>
    public class MatroskaChaptersXml : SubtitleFormat
    {
        private static readonly char[] TimeSplitChars = { ':', '.', ',' };

        public override string Extension => ".xml";

        public override string Name => "Matroska chapters";

        public static List<Chapter> ParseChapters(string xmlAsText)
        {
            var chapters = new List<Chapter>();
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlAsText);

            var atoms = xml.SelectNodes("//ChapterAtom");
            if (atoms == null)
            {
                return chapters;
            }

            foreach (XmlNode atom in atoms)
            {
                var start = atom.SelectSingleNode("ChapterTimeStart")?.InnerText;
                if (string.IsNullOrWhiteSpace(start))
                {
                    continue;
                }

                var ms = DecodeTimeCode(start);
                if (ms == null)
                {
                    continue;
                }

                // ChapterString lives under ChapterDisplay, and there can be one display per
                // language; the first one is the title shown by every player that does not care
                // about language selection.
                var title = atom.SelectSingleNode("ChapterDisplay/ChapterString")?.InnerText ?? string.Empty;

                chapters.Add(new Chapter(ms.Value, title.Trim())
                {
                    // A nested atom is a sub-chapter of the atom it sits inside.
                    Nested = atom.ParentNode?.Name == "ChapterAtom",
                });
            }

            return chapters;
        }

        public static string ToXml(IList<Chapter> chapters, string language)
        {
            var lang = string.IsNullOrWhiteSpace(language) ? "und" : language.Trim();
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<Chapters>");
            sb.AppendLine("  <EditionEntry>");
            sb.AppendLine("    <EditionUID>1</EditionUID>");

            for (var i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                sb.AppendLine("    <ChapterAtom>");

                // mkvmerge wants a unique non-zero UID per atom; sequential values keep the output
                // reproducible, which a random UID would not.
                sb.AppendLine($"      <ChapterUID>{i + 1}</ChapterUID>");
                sb.AppendLine($"      <ChapterTimeStart>{EncodeTimeCode(chapter.StartMilliseconds)}</ChapterTimeStart>");
                sb.AppendLine("      <ChapterDisplay>");
                sb.AppendLine($"        <ChapterString>{XmlEscape(chapter.Title)}</ChapterString>");
                sb.AppendLine($"        <ChapterLanguage>{XmlEscape(lang)}</ChapterLanguage>");
                sb.AppendLine("      </ChapterDisplay>");
                sb.AppendLine("    </ChapterAtom>");
            }

            sb.AppendLine("  </EditionEntry>");
            sb.AppendLine("</Chapters>");
            return sb.ToString();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToXml(ChapterHelper.FromSubtitle(subtitle), "und");
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            var xmlAsText = string.Join(Environment.NewLine, lines).Trim();
            if (xmlAsText.IndexOf("<Chapters", StringComparison.OrdinalIgnoreCase) < 0 ||
                xmlAsText.IndexOf("<ChapterAtom", StringComparison.OrdinalIgnoreCase) < 0)
            {
                _errorCount = 1;
                return;
            }

            try
            {
                var chapters = ParseChapters(xmlAsText);
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
            catch
            {
                _errorCount = 1;
                subtitle.Paragraphs.Clear();
            }
        }

        /// <summary>
        /// Accepts the nine-decimal nanosecond form the spec asks for as well as the shorter
        /// millisecond form other tools emit.
        /// </summary>
        public static double? DecodeTimeCode(string time)
        {
            var parts = time.Trim().Split(TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return null;
            }

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes) ||
                !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
            {
                return null;
            }

            double fraction = 0;
            if (parts.Length > 3)
            {
                var digits = parts[3];

                // Pad or trim to milliseconds so "5", "500" and "500000000" all mean half a second.
                if (digits.Length > 3)
                {
                    digits = digits.Substring(0, 3);
                }

                digits = digits.PadRight(3, '0');
                if (!int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
                {
                    return null;
                }

                fraction = ms;
            }

            return (hours * 3600.0 + minutes * 60.0 + seconds) * TimeCode.BaseUnit + fraction;
        }

        public static string EncodeTimeCode(double totalMilliseconds)
        {
            if (totalMilliseconds < 0)
            {
                totalMilliseconds = 0;
            }

            var ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}:{2:00}.{3:000}000000",
                (int)ts.TotalHours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds);
        }

        private static string XmlEscape(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
