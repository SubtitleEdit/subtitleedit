using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

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
        /// Adds a displayed screen as a cue. Roll-up and paint-on captions change the screen with
        /// every character pair, so a line that is still being written grows the cue right before
        /// it instead of adding a cue per character. A new row (roll-up scroll) starts a new cue, and
        /// so does a screen that grew by more than one character pair (back-to-back pop-on captions).
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
                if (Math.Abs(last.EndTime.TotalMilliseconds - startMs) < 0.5 &&
                    last.NumberOfLines == Utilities.GetNumberOfLines(text) &&
                    plainText.Length - lastPlainText.Length <= MaxGrowthPerCharacterPair &&
                    plainText.StartsWith(lastPlainText, StringComparison.Ordinal))
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
