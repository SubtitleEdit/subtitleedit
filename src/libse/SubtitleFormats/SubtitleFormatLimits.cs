using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Hard per-subtitle limits a format enforces on save - see <see cref="SubtitleFormat.FormatLimits"/>.
    /// </summary>
    public class SubtitleFormatLimits
    {
        /// <summary>Max visible characters per line (HTML tags excluded), or null for no limit.</summary>
        public int? MaxCharactersPerLine { get; set; }

        /// <summary>Max number of lines per subtitle, or null for no limit.</summary>
        public int? MaxLines { get; set; }

        /// <summary>
        /// Largest time code the format can write, or null for no limit - see
        /// <see cref="SubtitleFormat.MaxTimeCodeTotalMilliseconds"/>.
        /// </summary>
        public double? MaxTimeTotalMilliseconds { get; set; }

        /// <summary>
        /// The format's <see cref="SubtitleFormat.FormatLimits"/> plus its time code limit, or null
        /// when the format has neither.
        /// </summary>
        public static SubtitleFormatLimits ForFormat(SubtitleFormat format)
        {
            var limits = format.FormatLimits;
            var maxTime = format.MaxTimeCodeTotalMilliseconds;
            if (maxTime >= TimeCode.MaxSupportedTotalMilliseconds)
            {
                return limits;
            }

            limits = limits ?? new SubtitleFormatLimits();
            limits.MaxTimeTotalMilliseconds = maxTime;
            return limits;
        }

        /// <summary>
        /// True if the paragraph has a time code past <see cref="MaxTimeTotalMilliseconds"/>.
        /// </summary>
        public bool IsTimeCodeTooLarge(Paragraph paragraph)
        {
            if (!MaxTimeTotalMilliseconds.HasValue)
            {
                return false;
            }

            var max = MaxTimeTotalMilliseconds.Value + 0.01;
            return paragraph.StartTime.TotalMilliseconds > max || paragraph.EndTime.TotalMilliseconds > max;
        }

        /// <summary>
        /// True if any paragraph has a time code past <see cref="MaxTimeTotalMilliseconds"/>.
        /// </summary>
        public bool HasTimeCodeTooLarge(Subtitle subtitle)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                if (IsTimeCodeTooLarge(p))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the 1-based numbers of the paragraphs that break these limits - the ones the
        /// writer will re-wrap or truncate, or whose time codes it cannot write.
        /// </summary>
        public List<int> GetViolatingParagraphNumbers(Subtitle subtitle)
        {
            var result = new List<int>();
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var lines = subtitle.Paragraphs[i].Text.SplitToLines();
                var tooManyLines = MaxLines.HasValue && lines.Count > MaxLines.Value;
                var tooLong = false;
                if (MaxCharactersPerLine.HasValue)
                {
                    foreach (var line in lines)
                    {
                        if (HtmlUtil.RemoveHtmlTags(line, true).Length > MaxCharactersPerLine.Value)
                        {
                            tooLong = true;
                            break;
                        }
                    }
                }

                if (tooManyLines || tooLong || IsTimeCodeTooLarge(subtitle.Paragraphs[i]))
                {
                    result.Add(i + 1);
                }
            }

            return result;
        }
    }
}
