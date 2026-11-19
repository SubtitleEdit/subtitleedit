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

        /// <summary>
        /// The index of the row that swallowed the next row's translation, judged on lengths
        /// alone; -1 when there is none. The case <see cref="FindDuplicatedTranslation"/> and
        /// <see cref="FindPlaceholderTranslation"/> miss: Index-Translate 9B joined a sentence
        /// split over two rows into the first, moved every later row up by one and padded the
        /// freed last row with a copy of a short earlier line ("Hmm."), too short to count as a
        /// duplicate. The tell is that after the merged row the translations fit the length of the
        /// <i>next</i> source far better than their own, and the merged row fits both sources
        /// together. The padding can sit anywhere - the rows after it line up again - so it never
        /// counts, and at least two rows after the merged one must agree, so a single long-winded
        /// translation is not mistaken for a merge.
        /// </summary>
        public static int FindLengthShift(IReadOnlyList<string> sources, IReadOnlyList<string> translations)
        {
            var count = Math.Min(sources.Count, translations.Count);
            if (count < 4)
            {
                return -1;
            }

            var sourceLengths = new int[count];
            var translationLengths = new int[count];
            double sourceTotal = 0;
            double translationTotal = 0;
            for (var i = 0; i < count; i++)
            {
                sourceLengths[i] = CountLettersAndDigits(sources[i] ?? string.Empty);
                translationLengths[i] = CountLettersAndDigits(translations[i] ?? string.Empty);
                sourceTotal += sourceLengths[i];
                translationTotal += translationLengths[i];
            }

            if (sourceTotal == 0 || translationTotal == 0)
            {
                return -1;
            }

            var expansion = translationTotal / sourceTotal;
            for (var k = 0; k < count - 3; k++)
            {
                var mergedCost = LengthCost(translationLengths[k], sourceLengths[k] + sourceLengths[k + 1], expansion);
                var ownCost = LengthCost(translationLengths[k], sourceLengths[k], expansion);
                if (mergedCost >= ownCost || translationLengths[k] < 1.4 * expansion * sourceLengths[k])
                {
                    continue; // row k is not long enough to hold two rows
                }

                // Rows k+1..p-1 hold their next row's translation and row p is the padding; the
                // rows after it may line up again (the model can pad anywhere, not just at the end).
                var aligned = ownCost;
                var shifted = mergedCost;
                var betterShifted = 0;
                var rows = 0;
                for (var p = k + 1; p + 1 < count; p++)
                {
                    var own = LengthCost(translationLengths[p], sourceLengths[p], expansion);
                    var next = LengthCost(translationLengths[p], sourceLengths[p + 1], expansion);
                    aligned += own;
                    shifted += next;
                    rows++;
                    if (next < own)
                    {
                        betterShifted++;
                    }

                    if (rows >= 2 &&
                        betterShifted * 3 >= rows * 2 &&
                        shifted < aligned * 0.4 &&
                        aligned - shifted > 1.5)
                    {
                        return k;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// For a merge at <paramref name="mergedIndex"/> (see <see cref="FindLengthShift"/>): the
        /// row most likely to be the padding, i.e. the split point where the rows before it fit
        /// their next source and the rows after it fit their own best. -1 when there is no room.
        /// </summary>
        public static int FindPaddingIndex(IReadOnlyList<string> sources, IReadOnlyList<string> translations, int mergedIndex)
        {
            var count = Math.Min(sources.Count, translations.Count);
            if (mergedIndex < 0 || mergedIndex + 2 >= count)
            {
                return -1;
            }

            var sourceLengths = new int[count];
            var translationLengths = new int[count];
            double sourceTotal = 0;
            double translationTotal = 0;
            for (var i = 0; i < count; i++)
            {
                sourceLengths[i] = CountLettersAndDigits(sources[i] ?? string.Empty);
                translationLengths[i] = CountLettersAndDigits(translations[i] ?? string.Empty);
                sourceTotal += sourceLengths[i];
                translationTotal += translationLengths[i];
            }

            if (sourceTotal == 0 || translationTotal == 0)
            {
                return -1;
            }

            var expansion = translationTotal / sourceTotal;
            var best = -1;
            var bestCost = double.MaxValue;
            for (var p = mergedIndex + 1; p < count; p++)
            {
                double cost = 0;
                for (var j = mergedIndex + 1; j < p; j++)
                {
                    cost += LengthCost(translationLengths[j], sourceLengths[j + 1], expansion);
                }

                for (var j = p + 1; j < count; j++)
                {
                    cost += LengthCost(translationLengths[j], sourceLengths[j], expansion);
                }

                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = p;
                }
            }

            return best;
        }

        /// <summary>How far a translation's length is from what its source length predicts (log scale, smoothed for short lines).</summary>
        private static double LengthCost(int translationLength, int sourceLength, double expansion)
        {
            const double smoothing = 6;
            return Math.Abs(Math.Log((translationLength + smoothing) / (expansion * sourceLength + smoothing)));
        }

        private static int CountLettersAndDigits(string text)
        {
            var count = 0;
            foreach (var c in TranslationEchoGuard.Normalize(text))
            {
                if (char.IsLetterOrDigit(c))
                {
                    count++;
                }
            }

            return count;
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
