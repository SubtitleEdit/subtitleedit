using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// Sentence-cases a piece of text - the "Normal casing" fix from the "Change casing" dialog,
    /// but applied to the current text box selection instead of whole lines (#13093).
    /// </summary>
    public static class SentenceCaser
    {
        private static string? _cachedLanguage;
        private static FixCasing? _cachedFixCasing;

        /// <summary>
        /// Sentence-cases <paramref name="selectedText"/>, keeping any leading/trailing white space.
        /// </summary>
        /// <param name="textBefore">
        /// The text preceding the selection. It decides whether the selection starts a new sentence,
        /// so selecting the middle of a sentence does not capitalize the first word.
        /// </param>
        /// <param name="selectedText">The text to sentence-case.</param>
        /// <param name="language">Two letter language code, used for the name list and culture.</param>
        public static string SentenceCase(string textBefore, string selectedText, string language)
        {
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                return selectedText;
            }

            // "Normal casing" trims the text it fixes, which would eat the white space around the
            // selection - keep it and only run the fixer on the core.
            var start = 0;
            while (start < selectedText.Length && char.IsWhiteSpace(selectedText[start]))
            {
                start++;
            }

            var end = selectedText.Length;
            while (end > start && char.IsWhiteSpace(selectedText[end - 1]))
            {
                end--;
            }

            var prefix = selectedText.Substring(0, start);
            var core = selectedText.Substring(start, end - start);
            var suffix = selectedText.Substring(end);

            var subtitle = new Subtitle();
            var hasTextBefore = !string.IsNullOrWhiteSpace(textBefore);
            if (hasTextBefore)
            {
                // Same start/end times means no gap, so the fixer treats the selection as a
                // continuation of the text before it.
                subtitle.Paragraphs.Add(new Paragraph(textBefore, 0, 1000));
            }

            subtitle.Paragraphs.Add(new Paragraph(core, 1000, 2000));

            GetFixCasing(language).Fix(subtitle);

            return prefix + subtitle.Paragraphs[hasTextBefore ? 1 : 0].Text + suffix;
        }

        // Loading the name list hits the disk, so keep the last one around - this runs on a keystroke.
        private static FixCasing GetFixCasing(string language)
        {
            if (_cachedFixCasing == null || _cachedLanguage != language)
            {
                _cachedFixCasing = new FixCasing(language) { FixNormal = true };
                _cachedLanguage = language;
            }

            return _cachedFixCasing;
        }
    }
}
