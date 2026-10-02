using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    /// <summary>
    /// Collects the screens a <see cref="CcDataC608Parser"/> displays into cues.
    /// </summary>
    public static class Cea608CueBuilder
    {
        /// <summary>
        /// One character pair - plus a space that was trimmed off the end of the previous screen.
        /// </summary>
        private const int MaxGrowthPerCharacterPair = 3;

        /// <summary>
        /// A roll-up carriage return scrolls the screen up and leaves the new base row empty until
        /// the next line's first characters arrive - a few frames of repeated RU/CR/preamble codes.
        /// A screen shown shorter than this that the next line only adds to is folded into it
        /// instead of flashing by as a cue of its own; a longer one (a pause) stays a cue.
        /// </summary>
        private const double MaxRollUpGapMs = 500;

        /// <summary>
        /// Adds a displayed screen as a cue. Roll-up and paint-on captions change the screen with
        /// every character pair, so a line that is still being written grows the cue right before
        /// it instead of adding a cue per character. A new row (roll-up scroll) starts a new cue, and
        /// so does a screen that grew by more than one character pair (back-to-back pop-on captions).
        /// An extended character replaces the standard one sent before it (' then ’), which is
        /// growth too.
        /// </summary>
        public static void Add(List<Paragraph> paragraphs, string text, double startMs, double endMs)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            text = text.Trim();
            if (paragraphs.Count > 0)
            {
                var last = paragraphs[paragraphs.Count - 1];
                var plainText = HtmlUtil.RemoveHtmlTags(text, true);
                var lastPlainText = HtmlUtil.RemoveHtmlTags(last.Text, true);
                var isContiguous = Math.Abs(last.EndTime.TotalMilliseconds - startMs) < 0.5;
                var growth = plainText.Length - lastPlainText.Length;

                // an extended character takes the place of the standard one before it: same
                // length, only the last char differs ("IT'" then "IT’") - not "♪" then "Hey!"
                var isLastCharReplaced = growth == 0 &&
                                         lastPlainText.Length > 1 &&
                                         plainText.StartsWith(lastPlainText.Substring(0, lastPlainText.Length - 1), StringComparison.Ordinal);
                if (isContiguous &&
                    last.NumberOfLines == Utilities.GetNumberOfLines(text) &&
                    growth >= 0 && growth <= MaxGrowthPerCharacterPair &&
                    (plainText.StartsWith(lastPlainText, StringComparison.Ordinal) || isLastCharReplaced))
                {
                    last.Text = text;
                    last.EndTime.TotalMilliseconds = endMs;
                    return;
                }

                // the rows left after a roll-up scroll, then the first characters of the next row
                var lines = plainText.SplitToLines();
                var lastLines = lastPlainText.SplitToLines();
                if (isContiguous &&
                    last.EndTime.TotalMilliseconds - last.StartTime.TotalMilliseconds < MaxRollUpGapMs &&
                    lines.Count == lastLines.Count + 1 &&
                    lines.Take(lastLines.Count).SequenceEqual(lastLines) &&
                    lines[lines.Count - 1].Length <= MaxGrowthPerCharacterPair)
                {
                    last.Text = text;
                    last.EndTime.TotalMilliseconds = endMs;
                    return;
                }
            }

            paragraphs.Add(new Paragraph(text, startMs, endMs));
        }
    }
}
