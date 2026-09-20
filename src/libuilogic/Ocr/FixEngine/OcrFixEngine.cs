using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

public interface IOcrFixEngine
{
    void Initialize(Subtitle subtitle, string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary);
    OcrFixLineResult FixOcrErrors(int index, string text, bool doTryToGuessUnknownWords);
    void Unload();
    bool IsLoaded();
    List<string> GetSpellCheckSuggestions(string word);
    void ChangeAll(string from, string to);
    void SkipAll(string word);
    void AddName(string name);
    List<string> ReloadNames();
}

public partial class OcrFixEngine : IOcrFixEngine, IDoSpell
{
    private bool _isLoaded;
    private OcrFixReplaceList2 _ocrFixReplaceList;
    private string _fiveLetterName;
    private readonly HashSet<string> _wordSpellOkList = new HashSet<string>();
    private string[] _wordSplitList;
    private SpellCheckWordLists _spellCheckWordLists;
    private string _threeLetterIsoLanguageName;
    private string _twoLetterIsoLanguageName = string.Empty;
    private Subtitle _subtitle;
    private HashSet<string> _wordSkipList = new HashSet<string>();
    private Dictionary<string, string> _changeAllDictionary;
    private HashSet<string> _abbreviations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private readonly ISpellChecker _spellCheckManager;

    public OcrFixEngine(ISpellChecker spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
        _wordSkipList = new HashSet<string>();
        _changeAllDictionary = new Dictionary<string, string>();
        _subtitle = new Subtitle();
        _threeLetterIsoLanguageName = string.Empty;
        _isLoaded = false;
        _ocrFixReplaceList = new OcrFixReplaceList2(string.Empty);
        _spellCheckWordLists = new SpellCheckWordLists(string.Empty, this);
        _wordSplitList = Array.Empty<string>();
        _fiveLetterName = string.Empty;
    }

    void IOcrFixEngine.Initialize(Subtitle subtitle, string threeLetterIsoLanguageName, SpellCheckDictionaryDisplay spellCheckDictionary)
    {
        _isLoaded = true;
        var twoLetterIsoLanguageName = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(threeLetterIsoLanguageName);
        _twoLetterIsoLanguageName = twoLetterIsoLanguageName;
        _spellCheckManager.Initialize(spellCheckDictionary.DictionaryFileName, twoLetterIsoLanguageName);

        // Use the same normalized five-letter language name that the main spell checker and the OCR
        // "add to user dictionary" write path use (e.g. es_ANY -> es_ES). Using the raw file base name
        // here made ReloadNames() read a different *_user.xml than words were saved to, so newly added
        // user-dictionary words were never recognized and the prompt kept re-opening the same word (#12824).
        _fiveLetterName = spellCheckDictionary.GetFiveLetterLanguageName()
                          ?? Path.GetFileNameWithoutExtension(spellCheckDictionary.DictionaryFileName);

        _threeLetterIsoLanguageName = threeLetterIsoLanguageName;
        _subtitle = subtitle;

        var names = ReloadNames();
        _wordSplitList = StringWithoutSpaceSplitToWords.LoadWordSplitList(SpellCheckConfig.DictionariesFolder(), _threeLetterIsoLanguageName, names);
        _ocrFixReplaceList = OcrFixReplaceList2.FromLanguageId(_threeLetterIsoLanguageName);
        _abbreviations = AbbreviationList.Load(SpellCheckConfig.DictionariesFolder(), _fiveLetterName);
    }

    public OcrFixLineResult FixOcrErrors(int index, string text, bool doTryToGuessUnknownWords)
    {
        var wordsToIgnore = new List<string>();

        var replacedLine = ReplaceLineFixes(index, text, wordsToIgnore);
        if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
        {
            replacedLine = NormalizeApostrophes(replacedLine);
        }

        replacedLine = FixStartWithUppercaseLetterAfterSentenceEnd(index, replacedLine);
        var splitLine = SplitLine(replacedLine, index);
        if (replacedLine != text)
        {
            splitLine.ReplacementUsed = new ReplacementUsedItem(text, replacedLine, index);
        }

        for (var i = 0; i < splitLine.Words.Count; i++)
        {
            OcrFixLinePartResult? word = splitLine.Words[i];
            if (word.LinePartType != OcrFixLinePartType.Word)
            {
                word.FixedWord = word.Word;
                word.IsSpellCheckedOk = true;
                continue;
            }

            if (wordsToIgnore.Contains(word.Word))
            {
                word.IsSpellCheckedOk = true;
                continue;
            }

            CheckAndFixWord(word, splitLine, i, doTryToGuessUnknownWords);
        }

        return splitLine;
    }

    /// <summary>
    /// A line that follows a finished sentence starts with an uppercase letter (SE4 parity: OCR
    /// often drops the case of the first glyph, "you're out of luck." after "Yes.", and Tesseract
    /// reads a leading "I" as "l"). Only high-confidence cases are touched:
    /// - the previous line ends directly in . ! or ? - a trailing quote ("...Mother!" you're...)
    ///   can be a quotation inside a running sentence, so it does not count; neither do "..." or
    ///   an abbreviation ("Mr.");
    /// - the previous line is text OCR actually produced, so the first line and a run started
    ///   from the middle are left alone;
    /// - the capitalized first word is one the dictionary knows.
    /// </summary>
    internal string FixStartWithUppercaseLetterAfterSentenceEnd(int index, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var previous = index > 0 ? _subtitle.GetParagraphOrDefault(index - 1) : null;
        if (previous == null || !IsConfidentSentenceEnd(previous.Text))
        {
            return text; // no previous line (or not OCR'd yet) = no evidence a sentence ended
        }

        var st = new StrippableText(text);
        if (st.StrippedText.Length == 0 || !char.IsLower(st.StrippedText[0]) ||
            st.Pre.EndsWith('[') || st.Pre.EndsWith('(') || st.Pre.EndsWith('\'') ||
            st.Pre.Contains("...", StringComparison.Ordinal) || st.Pre.Contains('…') ||
            HtmlUtil.StartsWithUrl(st.StrippedText))
        {
            return text;
        }

        var firstWord = GetFirstWord(st.StrippedText);
        var uppercaseLetter = char.ToUpperInvariant(firstWord[0]);
        if (uppercaseLetter == 'L' && (firstWord == "l" || firstWord.StartsWith("l'", StringComparison.Ordinal)))
        {
            uppercaseLetter = 'I'; // "l said" / "l'm" are a misread "I said" / "I'm"
        }

        var fixedFirstWord = uppercaseLetter + firstWord.Substring(1);
        if (!_spellCheckManager.IsWordCorrect(fixedFirstWord) && !_spellCheckWordLists.HasName(fixedFirstWord))
        {
            return text;
        }

        st.StrippedText = fixedFirstWord + st.StrippedText.Substring(firstWord.Length);
        return st.Pre + st.StrippedText + st.Post;
    }

    private bool IsConfidentSentenceEnd(string previousText)
    {
        if (string.IsNullOrWhiteSpace(previousText))
        {
            return false;
        }

        var lastLine = HtmlUtil.RemoveHtmlTags(previousText, true).Trim('♪', '♫', ' ');
        if (lastLine.Length < 2 || lastLine.EndsWith("...", StringComparison.Ordinal))
        {
            return false;
        }

        var last = lastLine[lastLine.Length - 1];
        if (last != '.' && last != '!' && last != '?')
        {
            return false;
        }

        return last != '.' || !EndsWithAbbreviation(lastLine);
    }

    private static string GetFirstWord(string text)
    {
        var i = 0;
        while (i < text.Length && (char.IsLetter(text[i]) || text[i] == '\'' || text[i] == '’'))
        {
            i++;
        }

        return i == 0 ? text.Substring(0, 1) : text.Substring(0, i);
    }

    private bool EndsWithAbbreviation(string lastLine)
    {
        if (_abbreviations.Count == 0)
        {
            return false;
        }

        var lastSpace = lastLine.LastIndexOf(' ');
        var lastWord = lastSpace < 0 ? lastLine : lastLine.Substring(lastSpace + 1);
        return _abbreviations.Contains(lastWord);
    }

    /// <summary>
    /// Tesseract emits typographic single quotes for apostrophes ("‘cause", "didn’t"); subtitles
    /// use the plain apostrophe. A line holding both an opening and a closing curly quote is
    /// quoting something ("La lettera ‘E’") and is left alone. Runs after the replace list so
    /// entries written with the curly forms still match. Like the per-word straightening in
    /// <see cref="OcrFixReplaceList2.FixCommonWordErrors"/> it is one of the hardcoded rules, so
    /// "use hardcoded rules" turns both off together.
    /// </summary>
    internal static string NormalizeApostrophes(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var hasOpening = text.IndexOf('‘') >= 0;
        var hasClosing = text.IndexOf('’') >= 0;
        if (hasOpening == hasClosing)
        {
            return text; // neither, or a quotation pair
        }

        // Tesseract also emits both forms side by side ("'‘cause", "ma‘'am") - collapse that pair,
        // but only that pair: a straight '' elsewhere on the line is not the OCR's doing.
        return text.Replace("'‘", "'").Replace("‘'", "'").Replace("'’", "'").Replace("’'", "'")
            .Replace('‘', '\'').Replace('’', '\'');
    }

    private string ReplaceLineFixes(int index, string text, List<string> wordsToIgnore)
    {
        var spelledOK = IsSpelledCorrect(text);
        var replacedLine = _ocrFixReplaceList.FixOcrErrorViaLineReplaceList(text, _subtitle, index, _spellCheckManager, wordsToIgnore, spelledOK);

        // French typography wants a space before ! ? : ; — Tesseract often returns it glued to the
        // word ("Quoi?"). Apply the same normalization Fix-Common-Errors uses, so OCR output is
        // French-correct without a separate pass (issue #11702).
        if (_twoLetterIsoLanguageName == "fr")
        {
            replacedLine = Utilities.AddSpaceBeforeFrenchPunctuation(replacedLine);
        }

        return replacedLine;
    }

    internal static OcrFixLineResult SplitLine(string line, int index)
    {
        var result = new OcrFixLineResult
        {
            LineIndex = index,
        };
        if (string.IsNullOrEmpty(line))
        {
            return result;
        }
        int i = 0;
        while (i < line.Length)
        {
            // Check for HTML tags starting with "<" and ending with "/>" or ">"
            if (line[i] == '<')
            {
                var tagStart = i;
                var tagEnd = FindTagEnd(line, i);
                if (tagEnd > tagStart)
                {
                    var tag = line.Substring(tagStart, tagEnd - tagStart + 1);
                    result.Words.Add(new OcrFixLinePartResult
                    {
                        LinePartType = OcrFixLinePartType.Tag,
                        WordIndex = tagStart,
                        Word = tag
                    });
                    i = tagEnd + 1;
                    continue;
                }
            }
            // Check for tags starting with "{\" and ending with "}"
            if (i < line.Length - 1 && line[i] == '{' && line[i + 1] == '\\')
            {
                var tagStart = i;
                var tagEnd = line.IndexOf('}', i);
                if (tagEnd > tagStart)
                {
                    var tag = line.Substring(tagStart, tagEnd - tagStart + 1);
                    result.Words.Add(new OcrFixLinePartResult
                    {
                        LinePartType = OcrFixLinePartType.Tag,
                        WordIndex = tagStart,
                        Word = tag
                    });
                    i = tagEnd + 1;
                    continue;
                }
            }
            // Check for whitespace
            if (char.IsWhiteSpace(line[i]))
            {
                var whitespaceStart = i;
                while (i < line.Length && char.IsWhiteSpace(line[i]))
                {
                    i++;
                }
                var whitespace = line.Substring(whitespaceStart, i - whitespaceStart);
                result.Words.Add(new OcrFixLinePartResult
                {
                    LinePartType = OcrFixLinePartType.Whitespace,
                    WordIndex = whitespaceStart,
                    Word = whitespace
                });
                continue;
            }
            // Check for words (letters and digits)
            if (char.IsLetterOrDigit(line[i]) && line[i] != '"')
            {
                var wordStart = i;
                // U+2019 is the apostrophe Tesseract emits for "didn’t"; without it the word split
                // into "didn" + "’" + "t" and "didn" was flagged as unknown.
                while (i < line.Length &&
                       (char.IsLetterOrDigit(line[i]) || line[i] == '\'' || line[i] == '’' || line[i] == '-'))
                {
                    i++;
                }
                var word = line.Substring(wordStart, i - wordStart);
                result.Words.Add(new OcrFixLinePartResult
                {
                    LinePartType = OcrFixLinePartType.Word,
                    WordIndex = wordStart,
                    Word = word
                });
                continue;
            }
            // Everything else is special characters
            var specialCharStart = i;
            i++; // consume at least one char so a stray '<' (no matching '>') cannot cause an infinite loop
            while (i < line.Length &&
                   !char.IsLetterOrDigit(line[i]) &&
                   !char.IsWhiteSpace(line[i]) &&
                   line[i] != '<' &&
                   !(i < line.Length - 1 && line[i] == '{' && line[i + 1] == '\\'))
            {
                i++;
            }
            var specialChars = line.Substring(specialCharStart, i - specialCharStart);
            result.Words.Add(new OcrFixLinePartResult
            {
                LinePartType = OcrFixLinePartType.SpecialCharacters,
                WordIndex = specialCharStart,
                Word = specialChars
            });
        }

        return result;
    }

    private static int FindTagEnd(string text, int startIndex)
    {
        int i = startIndex + 1; // Start after '<'

        while (i < text.Length)
        {
            if (text[i] == '>')
            {
                // Check if it's a self-closing tag ending with "/>"
                if (i > startIndex + 1 && text[i - 1] == '/')
                {
                    return i;
                }
                // Regular closing tag ">"
                return i;
            }
            i++;
        }

        return startIndex; // No valid tag end found
    }

    private void CheckAndFixWord(OcrFixLinePartResult word, OcrFixLineResult splitLine, int index, bool doTryToGuessUnknownWords)
    {
        var s = word.Word;

        //TODO: check for multi-word names here too (look ahead in splitLine.Words, with e.g. "-")

        var isWordCorrect = false;
        var result = s;
        if (_changeAllDictionary.ContainsKey(s))
        {
            result = _changeAllDictionary[s];
            isWordCorrect = true;
        }
        else
        {
            result = _ocrFixReplaceList.FixCommonWordErrors(word.Word);
            if (result != word.Word)
            {
                word.ReplacementUsed = new ReplacementUsedItem(word.Word, result, splitLine.LineIndex);
            }

            isWordCorrect = _spellCheckManager.IsWordCorrect(result);
            if (result.Contains(' '))  // we trust replacements
            {
                word.FixedWord = result;
                word.IsSpellCheckedOk = IsSpelledCorrect(result);
                return;
            }

            if (!isWordCorrect && _wordSkipList.Contains(s))
            {
                isWordCorrect = true;
            }

            if (!isWordCorrect && _wordSkipList.Contains(result))
            {
                isWordCorrect = true;
            }

            if (!isWordCorrect && _spellCheckWordLists.HasUserWord(result))
            {
                isWordCorrect = true;
            }

            if (!isWordCorrect && _spellCheckWordLists.HasName(result))
            {
                isWordCorrect = true;
            }

            // Only build the line text when it can still be read: every use below is guarded by
            // !isWordCorrect, and isWordCorrect never flips back to false. GetText walks all
            // words of the line, so skipping it for already-correct words (the common case)
            // saves a StringBuilder + string per word.
            var lineText = isWordCorrect ? string.Empty : splitLine.GetText();
            if (!isWordCorrect && _spellCheckWordLists.HasNameExtended(result, lineText))
            {
                isWordCorrect = true;
            }

            var w = result.Trim('-');
            if (!isWordCorrect && w != result &&
                (_wordSkipList.Contains(w) ||
                 _spellCheckManager.IsWordCorrect(w) ||
                 _spellCheckWordLists.HasName(w) ||
                 _spellCheckWordLists.HasNameExtended(w, lineText)))
            {
                isWordCorrect = true;
            }

            w = result.Trim('\'');
            if (!isWordCorrect && w != result &&
                (_wordSkipList.Contains(w) ||
                 _spellCheckManager.IsWordCorrect(w) ||
                 _spellCheckWordLists.HasName(w) ||
                 _spellCheckWordLists.HasNameExtended(w, lineText)))
            {
                isWordCorrect = true;
            }

            w = result.Trim('\'', '"', '-');
            if (!isWordCorrect && w != result &&
                (_wordSkipList.Contains(w) ||
                 _spellCheckManager.IsWordCorrect(w) ||
                 _spellCheckWordLists.HasName(w) ||
                 _spellCheckWordLists.HasNameExtended(w, lineText)))
            {
                isWordCorrect = true;
            }

            // A hyphenated compound whose parts are each correct (e.g. "vaudeville-veteraan",
            // "Bailey-gebied") is correct as a whole - so don't hand it to the unknown-word "guess"
            // path, which would otherwise split it and leave a stray space next to the hyphen. (#12156)
            if (!isWordCorrect && IsHyphenatedWordOfCorrectParts(result, lineText))
            {
                isWordCorrect = true;
            }

            // Binary image compare reads lowercase l as i (italics) or uppercase I (sans-serif
            // fonts), in any language: "friendiy", "hostiie", "InteIIigence". The replace-list
            // rules only cover a fixed pattern per entry, and the PartialWords guesses need
            // "try to guess unknown words" on - so try the letter swap here, unconditionally,
            // and keep it only when the dictionary or names list confirms the result (#13660).
            if (!isWordCorrect && TryFixLMisreadAsI(result, out var lFixed))
            {
                result = lFixed;
                word.GuessUsed = true;
                isWordCorrect = true;
            }

            if (!string.IsNullOrEmpty(result) && !isWordCorrect && doTryToGuessUnknownWords)
            {
                var guesses = new List<string>();

                if (w.Length > 4 && SpellCheckConfig.UseWordSplitList())
                {
                    if (_threeLetterIsoLanguageName == "eng" &&
                        w.EndsWith("in", StringComparison.Ordinal) &&
                        result.Contains(w + "'") &&
                        DoSpell(w + "g"))
                    {
                        // avoid words like "workin'" or "holdin'"
                    }
                    else
                    {
                        var splitWords = StringWithoutSpaceSplitToWords.SplitWord(_wordSplitList, w, _threeLetterIsoLanguageName);
                        if (splitWords != w)
                        {
                            guesses.Add(splitWords);
                        }
                    }
                }

                // Heuristic (affix/particle/phonetic) splitting of merged words is un-compositing too, so
                // it must obey the same "word split list" toggle - otherwise turning that off still left this
                // path splitting words unconditionally, which over-corrects more than it fixes (#12243).
                if (SpellCheckConfig.UseWordSplitList())
                {
                    var autoSplitGuesses = UnknownWordGuesser.CreateGuessesFromLetters(result, _threeLetterIsoLanguageName);
                    if (autoSplitGuesses.Any())
                    {
                        guesses.AddRange(autoSplitGuesses);
                    }
                }

                if (w.Length > 4)
                {
                    // Substitute the replace list's <PartialWords> pairs (e.g. italic OCR often
                    // reads 'l' as 'i': "viei" -> "viel"); a guess only wins if the dictionary
                    // or names list below confirms it.
                    guesses.AddRange(_ocrFixReplaceList.CreateGuessesFromLetters(result, _threeLetterIsoLanguageName));
                }

                foreach (var g in guesses)
                {
                    w = g.Trim('\'', '"', '-');
                    if (IsSpelledCorrect(g) || IsSpelledCorrect(w) || _spellCheckWordLists.HasName(w))
                    {
                        result = g;
                        word.GuessUsed = true;
                        isWordCorrect = true;
                        break;
                    }
                }
            }
        }

        word.FixedWord = result;
        word.IsSpellCheckedOk = isWordCorrect;
    }

    // Tries every combination of the word's 'i'/'I' letters replaced by 'l' (fewest swaps first)
    // and returns the first one the dictionary or names list accepts. Only words of at least five
    // letters are considered, like the other unknown-word guesses, and a word with more than six
    // candidate letters only gets the single-letter and all-letters variants.
    private bool TryFixLMisreadAsI(string word, out string fixedWord)
    {
        fixedWord = word;
        if (word.Length < 5)
        {
            return false;
        }

        var positions = new List<int>();
        for (var i = 0; i < word.Length; i++)
        {
            var ch = word[i];
            if (ch == 'i' || ch == 'I')
            {
                positions.Add(i);
            }
            else if (!char.IsLetter(ch) && ch != '\'' && ch != '-')
            {
                return false;
            }
        }

        if (positions.Count == 0)
        {
            return false;
        }

        var chars = word.ToCharArray();
        var found = string.Empty;
        bool Accept(IReadOnlyList<int> swap)
        {
            foreach (var idx in swap)
            {
                chars[idx] = 'l';
            }

            var candidate = new string(chars);
            foreach (var idx in swap)
            {
                chars[idx] = word[idx];
            }

            if (IsSpelledCorrect(candidate) || _spellCheckWordLists.HasName(candidate.Trim('\'', '-')))
            {
                found = candidate;
                return true;
            }

            return false;
        }

        const int maxCombinationLetters = 6;
        if (positions.Count > maxCombinationLetters)
        {
            foreach (var idx in positions)
            {
                if (Accept(new[] { idx }))
                {
                    fixedWord = found;
                    return true;
                }
            }

            if (Accept(positions))
            {
                fixedWord = found;
                return true;
            }

            return false;
        }

        // All non-empty subsets of the positions, smallest subsets first.
        var total = 1 << positions.Count;
        for (var size = 1; size <= positions.Count; size++)
        {
            for (var mask = 1; mask < total; mask++)
            {
                if (System.Numerics.BitOperations.PopCount((uint)mask) != size)
                {
                    continue;
                }

                var swap = new List<int>(size);
                for (var bit = 0; bit < positions.Count; bit++)
                {
                    if ((mask & (1 << bit)) != 0)
                    {
                        swap.Add(positions[bit]);
                    }
                }

                if (Accept(swap))
                {
                    fixedWord = found;
                    return true;
                }
            }
        }

        return false;
    }

    // True when the word is a hyphenated compound (at least two parts) and every part is a
    // correct word, a name, or a skipped word. Used to keep valid hyphenated words out of the
    // unknown-word guess/split path. (#12156)
    private bool IsHyphenatedWordOfCorrectParts(string word, string lineText)
    {
        if (word.IndexOf('-') <= 0)
        {
            return false;
        }

        var parts = word.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (!_wordSkipList.Contains(part) &&
                !_spellCheckManager.IsWordCorrect(part) &&
                !_spellCheckWordLists.HasName(part) &&
                !_spellCheckWordLists.HasNameExtended(part, lineText))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsSpelledCorrect(string s)
    {
        if (_spellCheckManager.IsWordCorrect(s))
        {
            return true;
        }

        // Split on line breaks too - the whole-line check gets multi-line text, and splitting
        // only on space made "kamers\r\nwaar" one unknown "word", so every two-line subtitle
        // counted as misspelled and the spell-check-gated OCR regexes ran on correct lines.
        if (s.Contains(' ') || s.Contains('\n'))
        {
            var parts = s.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (!_spellCheckManager.IsWordCorrect(part.Trim('¡', '¿', ',', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟')))
                {
                    return false;
                }
            }

            return true;
        }

        if (s.Contains('-'))
        {
            var parts = s.Split('-', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (!_spellCheckManager.IsWordCorrect(part))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public void Unload()
    {
        _wordSkipList.Clear();
        _wordSpellOkList.Clear();
        _changeAllDictionary = new Dictionary<string, string>();
        _isLoaded = false;
        _subtitle = new Subtitle();
        _threeLetterIsoLanguageName = string.Empty;
    }

    public bool IsLoaded()
    {
        return _isLoaded;
    }

    public bool DoSpell(string word)
    {
        if (_isLoaded)
        {
            return _spellCheckManager.IsWordCorrect(word);
        }

        return false;
    }

    public List<string> GetSpellCheckSuggestions(string word)
    {
        if (_isLoaded)
        {
            var suggestions = _spellCheckManager.GetSuggestions(word);

            if (suggestions.Count > 0 && HasMostlyUppercaseLetters(word))
            {
                var uc = suggestions[0].ToUpperInvariant();
                if (!suggestions.Contains(uc))
                {
                    suggestions.Insert(0, uc);
                }
            }

            return suggestions;
        }

        return [];
    }

    private static bool HasMostlyUppercaseLetters(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return false;
        }

        var letterCount = 0;
        var uppercaseCount = 0;
        for (int i = 0; i < word.Length; i++)
        {
            if (char.IsLetter(word[i]))
            {
                letterCount++;
                if (char.IsUpper(word[i]))
                {
                    uppercaseCount++;
                }
            }
        }

        return uppercaseCount > letterCount / 2;
    }

    public void ChangeAll(string from, string to)
    {
        _changeAllDictionary.TryAdd(from, to);
        _ocrFixReplaceList.AddWordOrPartial(from, to);
    }

    public void SkipAll(string word)
    {
        if (!_wordSkipList.Contains(word) && !string.IsNullOrWhiteSpace(word))
        {
            _wordSkipList.Add(word);
        }
    }

    public void AddName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !_isLoaded || string.IsNullOrEmpty(name))
        {
            return;
        }

        _spellCheckWordLists.AddName(name.Trim());
    }

    public List<string> ReloadNames()
    {
        try
        {
            _spellCheckWordLists = new SpellCheckWordLists(_fiveLetterName, this);
        }
        catch (Exception exception)
        {
            SpellCheckConfig.LogError("Error loading names for OCR fix engine: " + exception.Message);
            _spellCheckWordLists = new SpellCheckWordLists(string.Empty, this);
        }

        // Read the names *after* rebuilding for _fiveLetterName. Reading first returned the
        // previous language's names (or none at all on the first run), and Initialize feeds
        // this list into the word-split list - so run-together words containing a proper
        // name were split against the wrong language's name set.
        return _spellCheckWordLists.GetAllNames();
    }
}
