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
        /// Returns the 1-based numbers of the paragraphs that break these limits - the ones the
        /// writer will re-wrap or truncate.
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

                if (tooManyLines || tooLong)
                {
                    result.Add(i + 1);
                }
            }

            return result;
        }
    }
}
