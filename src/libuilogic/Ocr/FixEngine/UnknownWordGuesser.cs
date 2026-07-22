namespace Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

public static class UnknownWordGuesser
{
    private static readonly Dictionary<string, string[]> LanguageAffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "eng", new[] { "ing", "ed", "ly", "ment", "ness", "pre", "anti", "tion", "sion" } },
        { "deu", new[] { "ung", "keit", "heit", "schaft", "ent", "ge", "be", "ver" } },
        { "pol", new[] { "ski", "ska", "czy", "nie", "cze", "ość", "ania", "enia" } },
        { "fra", new[] { "ment", "tion", "age", "ance", "ence", "able", "isme" } },
        { "dan", new[] { "else", "ning", "hed", "skab", "ende" } },
        { "swe", new[] { "ning", "het", "skap", "ande", "else" } }
    };

    // Words that frequently get stuck to others in OCR
    private static readonly Dictionary<string, string[]> CommonParticles = new(StringComparer.OrdinalIgnoreCase)
    {
        { "eng", new[] { "of", "the", "to", "in", "it", "is", "and" } },
        { "dan", new[] { "og", "af", "er", "en", "et", "de" } },
        { "deu", new[] { "der", "die", "das", "und", "zu", "in" } }
    };

    // Languages that form closed compounds by concatenating words, so a long unknown word is
    // normally a legitimate compound of two valid words (e.g. Dutch "modderwarboel" = "modder" +
    // "warboel", "zwaardengooier" = "zwaarden" + "gooier") rather than a lost-space OCR error.
    // The phonetic consonant-seam splitting in GenerateSmartSplits must not run for these, or it
    // "fixes" correct compounds by breaking them into two words (#12200). The curated affix and
    // particle splits above stay enabled - they isolate a fragment that is not itself a word, so
    // the spell-checked guess only wins on a genuine merge.
    private static readonly HashSet<string> CompoundingLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "nld", "dut",        // Dutch
        "deu", "ger",        // German
        "afr",               // Afrikaans
        "dan",               // Danish
        "nor", "nob", "nno", // Norwegian
        "swe",               // Swedish
        "isl", "ice",        // Icelandic
        "fin",               // Finnish
        "fao",               // Faroese
        "fry",               // Frisian
        "ltz",               // Luxembourgish
        "hun",               // Hungarian
        "est",               // Estonian
    };

    public static IEnumerable<string> CreateGuessesFromLetters(string word, string threeLetterIsoLanguageName)
    {
        if (string.IsNullOrWhiteSpace(word) || word.Length < 4) // Lowered to 4 to catch "ofit"
        {
            return Enumerable.Empty<string>();
        }

        // Skip names (Upper Case start + mostly lowercase + relatively short)
        if (char.IsUpper(word[0]) && word.Length < 10 && word.Skip(1).All(char.IsLower))
        {
            return Enumerable.Empty<string>();
        }

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var vowels = "aeiouæøåöüäéèêëàâîïôûùyąęół".ToCharArray();
        string lang = threeLetterIsoLanguageName.ToLower();

        // 1. Particle Splitting (Special case for "of", "the", etc.)
        if (CommonParticles.TryGetValue(lang, out var particles))
        {
            foreach (var p in particles)
            {
                if (word.Length >= p.Length + 2)
                {
                    if (word.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(word.Insert(p.Length, " "));
                    }

                    if (word.EndsWith(p, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(word.Insert(word.Length - p.Length, " "));
                    }
                }
            }
        }

        // 2. Morphological Splits
        if (LanguageAffixes.TryGetValue(lang, out var affixes))
        {
            foreach (var affix in affixes)
            {
                if (word.Length > affix.Length + 2)
                {
                    if (word.EndsWith(affix, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(word.Insert(word.Length - affix.Length, " "));
                    }

                    if (word.StartsWith(affix, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(word.Insert(affix.Length, " "));
                    }
                }
            }
        }

        // 3. Smart Phonetic Splits (skipped for closed-compounding languages - see #12200)
        if (word.Length > 7 && !CompoundingLanguages.Contains(lang))
        {
            foreach (var split in GenerateSmartSplits(word, vowels))
            {
                results.Add(split);
            }
        }

        var restrictedLanguages = new[] { "dan", "eng", "swe", "deu", "pol", "fra" };
        if (restrictedLanguages.Contains(lang))
        {
            return results.Where(s => IsValidSplitting(s, vowels)).ToList();
        }

        return results;
    }

    private static IEnumerable<string> GenerateSmartSplits(string word, char[] vowels)
    {
        var guesses = new List<string>();
        for (int i = 3; i < word.Length - 3; i++)
        {
            // Only ever split between two letters. Otherwise a hyphen (or apostrophe) reads as a
            // "consonant" and we split right next to it, e.g. "vaudeville-veteraan" -> "vaudeville-
            // veteraan" - a spurious space after the hyphen. (issue #12156)
            if (!char.IsLetter(word[i]) || !char.IsLetter(word[i - 1]))
            {
                continue;
            }

            bool isVowel = vowels.Contains(char.ToLower(word[i]));
            bool prevIsVowel = vowels.Contains(char.ToLower(word[i - 1]));

            // Split between two consonants (mer-cat)
            if (!isVowel && !prevIsVowel)
            {
                guesses.Add(word.Insert(i, " "));
            }
            // Split after 's' or 'n' before another consonant (compound hint)
            else if ((word[i - 1] == 's' || word[i - 1] == 'n') && !isVowel)
            {
                guesses.Add(word.Insert(i, " "));
            }
        }
        return guesses;
    }

    private static bool IsValidSplitting(string sentence, char[] vowels)
    {
        var parts = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            // Reject single-letter consonants (keep single vowels like 'a' or 'i')
            if (p.Length == 1 && char.IsLetter(p[0]) && !vowels.Contains(char.ToLower(p[0])))
            {
                return false;
            }
        }
        return true;
    }
}