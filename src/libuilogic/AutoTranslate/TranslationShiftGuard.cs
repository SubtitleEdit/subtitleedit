namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Detects a multi-row reply where the model merged two rows into one and padded the end with
    /// a copy of an earlier translation to keep the row count: every row after the merge holds its
    /// neighbour's translation, yet the reply has the expected shape, so it was written out (#16555).
    /// <para>
    /// The tell is the padding: two rows with different source texts that got the same, clearly
    /// non-trivial translation. Short lines ("Yes.", "Thank you.") legitimately repeat, so only
    /// lines that <see cref="TranslationEchoGuard"/> considers in need of translation count.
    /// </para>
    /// </summary>
    public static class TranslationShiftGuard
    {
        /// <summary>
        /// The index of the first row, at or after <paramref name="firstCheckedIndex"/>, whose
        /// translation repeats an earlier row's translation although the source texts differ;
        /// -1 when there is none. Rows before <paramref name="firstCheckedIndex"/> (already
        /// accepted context) are compared against, but never reported.
        /// </summary>
        public static int FindDuplicatedTranslation(IReadOnlyList<string> sources, IReadOnlyList<string> translations, int firstCheckedIndex = 0)
        {
            var count = Math.Min(sources.Count, translations.Count);
            var seen = new Dictionary<string, string>(count);
            for (var i = 0; i < count; i++)
            {
                var translation = TranslationEchoGuard.Normalize(translations[i] ?? string.Empty);
                if (!TranslationEchoGuard.NeedsTranslation(translation))
                {
                    continue;
                }

                var source = TranslationEchoGuard.Normalize(sources[i] ?? string.Empty);
                if (seen.TryGetValue(translation, out var earlierSource))
                {
                    if (i >= firstCheckedIndex && earlierSource != source)
                    {
                        return i;
                    }

                    continue;
                }

                seen.Add(translation, source);
            }

            return -1;
        }

        /// <summary>
        /// The index of the first row whose source has letters but whose translation has neither
        /// letters nor digits ("...", "…", "-"); -1 when there is none. The same merge as in
        /// <see cref="FindDuplicatedTranslation"/>, only padded with a placeholder instead of a
        /// copy (#16585) - TranslateGemma 12B joined a sentence split over two rows into the
        /// first, moved every later row up by one and filled the freed row with "...".
        /// </summary>
        public static int FindPlaceholderTranslation(IReadOnlyList<string> sources, IReadOnlyList<string> translations)
        {
            var count = Math.Min(sources.Count, translations.Count);
            for (var i = 0; i < count; i++)
            {
                if (HasLetterOrDigit(TranslationEchoGuard.Normalize(sources[i] ?? string.Empty)) &&
                    !HasLetterOrDigit(TranslationEchoGuard.Normalize(translations[i] ?? string.Empty)))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool HasLetterOrDigit(string text)
        {
            foreach (var c in text)
            {
                if (char.IsLetterOrDigit(c))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
