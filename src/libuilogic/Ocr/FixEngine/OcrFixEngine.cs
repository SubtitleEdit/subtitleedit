using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// The text comes from an OCR engine that writes "*" for a glyph it could not match (nOCR,
    /// binary image compare): fill such gaps inside a word when exactly one letter (or apostrophe)
    /// makes it a dictionary word. Off by default - elsewhere "sh*t" is deliberate censoring.
    /// </summary>
    bool FillUnknownCharacters { get; set; }

    /// <summary>
    /// The text comes from an image matcher (nOCR, binary image compare) whose glyph entries can
    /// fit the wrong character: a slanted italic "l", "i" or "I" read as "/" ("p/ease", "/ can"),
    /// a bold italic apostrophe read as "#" ("He#s"). Off by default - text engines (Tesseract,
    /// Paddle, llama.cpp) never produce those, so a "/" or "#" from them is the real thing
    /// ("PILOTEN / OMGEKOMEN", "C#").
    /// </summary>
    bool FixImageMatcherMisreads { get; set; }
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

    public bool FillUnknownCharacters { get; set; }

    public bool FixImageMatcherMisreads { get; set; }

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

        if (FillUnknownCharacters)
        {
            text = FillUnknownCharactersInWords(text);
        }

        if (FixImageMatcherMisreads)
        {
            text = FixHashMisreadAsApostrophe(text);
            text = FixSlashMisreadAsL(text);
            text = FixSpacedThousandsSeparator(text);
            text = FixSpaceBeforeClosingQuote(text);
        }

        var replacedLine = ReplaceLineFixes(index, text, wordsToIgnore);
        if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
        {
            replacedLine = NormalizeApostrophes(replacedLine);
            replacedLine = NormalizeSpacedEllipses(replacedLine);
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
            // A sans-serif capital I is a plain bar, so nOCR reads it as "l": "lk" / "ls" / "ln"
            // (Dutch), "lt" / "lf" (English). Only for a whole word the dictionary knows with I.
            if (uppercaseLetter != 'L' || firstWord.Length < 2 || !char.IsLower(firstWord[1]) ||
                (firstWord.Length < st.StrippedText.Length && char.IsLetterOrDigit(st.StrippedText[firstWord.Length])))
            {
                return text;
            }

            var iWord = "I" + firstWord.Substring(1);
            if (!_spellCheckManager.IsWordCorrect(iWord) && !_spellCheckWordLists.HasName(iWord))
            {
                return text;
            }

            fixedFirstWord = iWord;
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
    /// Image matchers split the dots of a wide-set ellipsis far enough apart to read each as a
    /// word: ". . ." ("voor. . .", "<i>. . .en</i>"). No subtitle means three spaced periods, so
    /// join them (SE4's hardcoded rules did the same at the line start and end). The two-dot
    /// forms ". .." and ".. ." are Tesseract's and stay with the replace lists, which only join
    /// them at the end of a line after a letter or digit.
    /// </summary>
    internal static string NormalizeSpacedEllipses(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains(". . .", StringComparison.Ordinal))
        {
            return text;
        }

        return SpacedEllipsis().Replace(text, "...");
    }

    [GeneratedRegex(@"(?<![.…])\. \. \.(?![.…])")]
    private static partial Regex SpacedEllipsis();

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

        var hasOpening = text.Contains('‘');
        var hasClosing = text.Contains('’');
        if (hasOpening == hasClosing)
        {
            return text; // neither, or a quotation pair
        }

        // Tesseract also emits both forms side by side ("'‘cause", "ma‘'am") - collapse that pair,
        // but only that pair: a straight '' elsewhere on the line is not the OCR's doing.
        return text.Replace("'‘", "'").Replace("‘'", "'").Replace("'’", "'").Replace("’'", "'")
            .Replace('‘', '\'').Replace('’', '\'');
    }

    // A word with a "#" between two letters, or next to a slash ("He#s", "isn#t", "/#//").
    [GeneratedRegex(@"(?<![\p{L}\d#/'’])[\p{L}'’/]*[\p{L}/]#[\p{L}/][\p{L}'’/#]*")]
    private static partial Regex WordWithHash();

    /// <summary>
    /// A bold italic apostrophe fits a "#" entry ("He#s", "isn#t", "You#//"). No word has a hash
    /// between two letters, so read it as an apostrophe when the dictionary or names list knows
    /// the result; a word that also has slashes is left to the slash rule to judge.
    /// </summary>
    private string FixHashMisreadAsApostrophe(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains('#'))
        {
            return text;
        }

        return WordWithHash().Replace(text, m =>
        {
            var candidate = m.Value.Replace('#', '\'');
            if (candidate.Contains('/'))
            {
                return candidate;
            }

            var bare = candidate.Trim('\'', '’');
            return bare.Length > 1 && (IsSpelledCorrect(bare) || _spellCheckWordLists.HasName(bare)) ? candidate : m.Value;
        });
    }

    // A run of letters with a slash touching a letter ("p/ease", "/itt/e", "wi/I", "/'m"). A slash
    // right after "<" is a closing tag ("</i>"), not a misread letter - "li" is a word in some
    // dictionaries (Dutch), which turned every "</i>" into "<li>".
    [GeneratedRegex(@"(?<![\p{L}\d/'’<])[\p{L}'’/]*(?:\p{L}/|/['’]?\p{L})[\p{L}'’/]*")]
    private static partial Regex WordWithSlash();

    // A slash standing alone between spaces (or at a line or tag edge): a slanted italic "I".
    [GeneratedRegex(@"(?<![^\s>])/(?![^\s<])")]
    private static partial Regex LoneSlash();

    /// <summary>
    /// Image matchers read a slanted italic "l" as "/" ("p/ease", "re/igions", "/itt/e"), and a
    /// slanted "i" ("w/th", "do/ng") or "I" ("/t's", "/'m", "/ can") too. The line splitter cuts
    /// words at the slash, so the per-word fixes never see the whole word - repair it on the
    /// line: every slash to "l", and an "I" after a lowercase letter to "l" ("wi/I", "teI/ing");
    /// failing that, the other readings of the slashes ("i" anywhere, "I" at the start of the
    /// word). Each is kept only when the dictionary or names list accepts the result. A real
    /// slash between two words ("and/or", "AC/DC") is left alone, and so is a lone slash
    /// separating numbers or two all-caps words ("1 / 2", "PILOTEN / OMGEKOMEN" on a title card).
    /// </summary>
    private string FixSlashMisreadAsL(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains('/'))
        {
            return text;
        }

        if (LoneSlash().IsMatch(text) && IsSpelledCorrect("I"))
        {
            text = LoneSlash().Replace(text, m => IsSeparatorSlash(text, m.Index) ? "/" : "I");
        }

        return WordWithSlash().Replace(text, m =>
        {
            var token = m.Value;
            // "//example.com" after "http:" is a URL; "a//", "wi//" and "ki//ing" are words.
            if (token.StartsWith("//", StringComparison.Ordinal) || m.Index > 0 && text[m.Index - 1] == ':')
            {
                return token;
            }

            var parts = token.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1 && parts.All(p => p.Length > 1 && IsSpelledCorrect(p)))
            {
                return token; // "and/or", "he/she"
            }

            var chars = token.Replace('/', 'l').ToCharArray();
            for (var i = 1; i < chars.Length; i++)
            {
                if (chars[i] == 'I' && char.IsLower(chars[i - 1]))
                {
                    chars[i] = 'l';
                }
            }

            var candidate = new string(chars);
            if (IsWordOrName(candidate))
            {
                return candidate;
            }

            // The slanted stroke may be an "i" instead ("w/th", "aff/n/ty"), or a capital "I" at
            // the start of the word ("/t's", "/'m"): try every reading of the slashes, "l"
            // first, and keep the first one the dictionary knows.
            var slashes = new List<int>();
            for (var i = 0; i < token.Length && slashes.Count <= MaxSlashesToRead; i++)
            {
                if (token[i] == '/')
                {
                    slashes.Add(i);
                }
            }

            if (slashes.Count > MaxSlashesToRead)
            {
                return token;
            }

            // "/t's" is "it's" mid-sentence and "It's" at the start of one.
            var readings = new string[slashes.Count];
            for (var k = 0; k < readings.Length; k++)
            {
                readings[k] = slashes[k] != 0 ? "li" : IsSentenceStart(text, m.Index) ? "lIi" : "liI";
            }

            foreach (var reading in ReadingsOf(readings))
            {
                for (var k = 0; k < slashes.Count; k++)
                {
                    chars[slashes[k]] = reading[k];
                }

                candidate = new string(chars);
                if (IsWordOrName(candidate))
                {
                    return candidate;
                }
            }

            return token;
        });
    }

    private const int MaxSlashesToRead = 4;

    // "20. 000": a one-to-three digit group, a period, a space and a three-digit group. A four-digit
    // number before the period ("1940. 300") is a year ending a sentence, not a thousands group.
    [GeneratedRegex(@"(?<![\d.,])(\d{1,3})\. (\d{3})(?!\d)")]
    private static partial Regex SpacedThousandsSeparator();

    /// <summary>
    /// Italic digits lean into the thousands separator, so the space detector puts a word space
    /// after the period: "20. 000" -> "20.000", "klim tot 19. 000 voet" -> "19.000".
    /// </summary>
    internal static string FixSpacedThousandsSeparator(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains(". ", StringComparison.Ordinal))
        {
            return text;
        }

        return SpacedThousandsSeparator().Replace(text, "$1.$2");
    }

    // Sentence end, a space, then a closing quote followed by whitespace, a tag or the end.
    [GeneratedRegex(@"([.!?…]) ([""'”’»])(?=\s|<|$)")]
    private static partial Regex SpaceBeforeClosingQuote();

    /// <summary>
    /// A closing quote after an italic period gets a word space in front of it ("voorbij. '",
    /// "beginnen. \""): drop it. A quote that opens the next word ("Kom.' 'Nee.'", "'s avonds")
    /// is not followed by whitespace and stays.
    /// </summary>
    internal static string FixSpaceBeforeClosingQuote(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains(' '))
        {
            return text;
        }

        return SpaceBeforeClosingQuote().Replace(text, "$1$2");
    }

    /// <summary>
    /// A lone slash next to a number ("1 / 2") or between two all-caps words ("PILOTEN /
    /// OMGEKOMEN") is a separator, not a slanted italic "I" - an italic "I" sits between
    /// ordinary words ("/ can", "and / will").
    /// </summary>
    private static bool IsSeparatorSlash(string text, int index)
    {
        var before = NeighbourWord(text, index, -1);
        var after = NeighbourWord(text, index, 1);
        if (before.Any(char.IsDigit) || after.Any(char.IsDigit))
        {
            return true;
        }

        return IsAllCaps(before) && IsAllCaps(after);
    }

    // Image matchers read the sans-serif capital "I" as "l" ("PlLOTEN"); the per-word rules only
    // repair that after this runs, so an "l" counts as uppercase here.
    private static bool IsAllCaps(string word)
    {
        return word.Any(char.IsUpper) && word.All(c => !char.IsLower(c) || c == 'l');
    }

    /// <summary>
    /// The run of non-whitespace characters before (<paramref name="direction"/> -1) or after
    /// (+1) <paramref name="index"/>, skipping whitespace and tags; empty at a line edge.
    /// </summary>
    private static string NeighbourWord(string text, int index, int direction)
    {
        var i = index + direction;
        while (i >= 0 && i < text.Length)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                i += direction;
            }
            else if (direction < 0 && text[i] == '>')
            {
                var open = text.LastIndexOf('<', i);
                if (open < 0)
                {
                    break;
                }

                i = open - 1;
            }
            else if (direction > 0 && text[i] == '<')
            {
                var close = text.IndexOf('>', i);
                if (close < 0)
                {
                    break;
                }

                i = close + 1;
            }
            else
            {
                break;
            }
        }

        var end = i;
        while (end >= 0 && end < text.Length && !char.IsWhiteSpace(text[end]) && text[end] != '<' && text[end] != '>')
        {
            end += direction;
        }

        return direction < 0 ? text.Substring(end + 1, i - end) : text.Substring(i, end - i);
    }

    /// <summary>
    /// Nothing but whitespace, dashes, quotes and tags between the start of the text or the end
    /// of a sentence and <paramref name="index"/>.
    /// </summary>
    private static bool IsSentenceStart(string text, int index)
    {
        var i = index - 1;
        while (i >= 0)
        {
            var c = text[i];
            if (c == '>')
            {
                var open = text.LastIndexOf('<', i);
                if (open < 0)
                {
                    return false;
                }

                i = open - 1;
                continue;
            }

            if (char.IsWhiteSpace(c) || c is '-' or '–' or '"' or '\'' or '“' or '‘' or '«' or '(' or '[')
            {
                i--;
                continue;
            }

            return c is '.' or '!' or '?' or '…' or ':';
        }

        return true;
    }

    /// <summary>Every combination of one letter per position, first letters first.</summary>
    private static IEnumerable<string> ReadingsOf(string[] lettersPerPosition)
    {
        var picks = new int[lettersPerPosition.Length];
        while (true)
        {
            yield return string.Concat(picks.Select((pick, k) => lettersPerPosition[k][pick]));

            var position = picks.Length - 1;
            while (position >= 0 && ++picks[position] == lettersPerPosition[position].Length)
            {
                picks[position] = 0;
                position--;
            }

            if (position < 0)
            {
                yield break;
            }
        }
    }

    private bool IsWordOrName(string candidate)
    {
        var bare = candidate.Trim('\'', '’');
        return bare.Length > 1 && (IsSpelledCorrect(bare) || _spellCheckWordLists.HasName(bare));
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

            // Dutch capitalizes "één" as "Eén" (the capital drops its accent), which the spell checker
            // lowercases to the non-word "eén" - so the accent fix below turned it into "Een" (#16031).
            if (!isWordCorrect && IsDutchCapitalizedAccentedWord(result))
            {
                isWordCorrect = true;
            }

            // nOCR and binary image compare read a plain letter as an accented lookalike when the
            // accent sits where the letter's own dot or serif is ("ìs", "pà", "vøn") and confuse
            // accented letters of similar bodies ("č" read as "ë"). Pixels can't settle these;
            // the dictionary can, so try the lookalikes and keep a result only when it is confirmed.
            if (!isWordCorrect && TryFixAccentMisread(result, out var accentFixed))
            {
                result = accentFixed;
                word.GuessUsed = true;
                isWordCorrect = true;
            }

            // Splitting a word via the word split list ("soproudofyou" -> "so proud of you") runs even
            // with "try to guess unknown words" off, like SE4's cautious guess level: every part must
            // be a dictionary word, so it rarely breaks a correct word. The heuristic splitter and the
            // letter-substitution guesses below are the over-correcting ones (#12441), so only they
            // follow the setting.
            if (!string.IsNullOrEmpty(result) && !isWordCorrect)
            {
                var guesses = new List<string>();

                // A word glued to a character that is not punctuation (nOCR's "*" for an unknown
                // glyph, "exper*ment") is only a fragment of a word, so splitting it ("ex per*ment")
                // always breaks it.
                var isStandaloneWord = IsStandaloneWord(splitLine, index);

                if (w.Length > 4 && isStandaloneWord && SpellCheckConfig.UseWordSplitList())
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
                        if (splitWords != w && !HasMisplacedUppercasePart(splitWords))
                        {
                            guesses.Add(splitWords);
                        }
                    }
                }

                // Heuristic (affix/particle/phonetic) splitting of merged words is un-compositing too, so
                // it must obey the same "word split list" toggle - otherwise turning that off still left this
                // path splitting words unconditionally, which over-corrects more than it fixes (#12243).
                if (doTryToGuessUnknownWords && isStandaloneWord && SpellCheckConfig.UseWordSplitList())
                {
                    var autoSplitGuesses = UnknownWordGuesser.CreateGuessesFromLetters(result, _threeLetterIsoLanguageName);
                    if (autoSplitGuesses.Any())
                    {
                        guesses.AddRange(autoSplitGuesses);
                    }
                }

                if (doTryToGuessUnknownWords && w.Length > 4)
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

        // An image matcher fits the sans-serif "I" and "l" to the same bar, so for a word nothing
        // knows, the letters around the bar decide: a capital I after a lowercase letter, or
        // between a capital and a lowercase letter at the start, is an "l" ("CowsIip", "controI",
        // "FIight"); an "l" among capitals is an "I" ("JUNl", "lFF", "AUSTRALlERS"). Names that
        // no dictionary has ("Cowslip", "Heinkels") only get this shape rule - the word was
        // unknown before it too, and a real word never has that case pattern (Mc/Mac aside).
        if (!isWordCorrect && FixImageMatcherMisreads && TryFixBarMisreadByCase(result, out var barFixed))
        {
            result = barFixed;
            word.GuessUsed = true;
            isWordCorrect = IsSpelledCorrect(result) || _spellCheckWordLists.HasName(result.Trim('\'', '-'));
        }

        // An overhanging f, r or t leans over the j, v or w next to it, so the space detector
        // swallows the word space: "Hoorje", "kentje", "Vijfvan", "ofwe". Split there when the
        // dictionary knows both halves (#16031).
        if (!isWordCorrect && FixImageMatcherMisreads && TrySplitAtOverhang(result, out var splitAtOverhang))
        {
            result = splitAtOverhang;
            word.GuessUsed = true;
            isWordCorrect = true;
        }

        word.FixedWord = result;
        word.IsSpellCheckedOk = isWordCorrect;
    }

    /// <summary>
    /// Splits a word of letters at the first "f", "r" or "t" followed by "j", "v" or "w" where both
    /// halves are dictionary words or names and the right half is a short function word of at
    /// most three letters ("je", "we", "van"): "Hoorje" -> "Hoor je", "Vijfvan" -> "Vijf van". The
    /// length cap keeps an unlisted compound or interjection whole ("Getverdikkeme" is not "Get
    /// verdikkeme"). A word with digits or punctuation is left alone.
    /// </summary>
    private bool TrySplitAtOverhang(string word, out string split)
    {
        split = word;
        if (word.Length < 4 || !word.All(char.IsLetter))
        {
            return false;
        }

        for (var i = Math.Max(2, word.Length - MaxOverhangRightHalf); i <= word.Length - 2; i++)
        {
            var before = char.ToLowerInvariant(word[i - 1]);
            var after = char.ToLowerInvariant(word[i]);
            if (before is not ('f' or 'r' or 't') || after is not ('j' or 'v' or 'w'))
            {
                continue;
            }

            var left = word.Substring(0, i);
            var right = word.Substring(i);
            if (IsWordOrName(left) && IsWordOrName(right))
            {
                split = left + " " + right;
                return true;
            }
        }

        return false;
    }

    private const int MaxOverhangRightHalf = 3;

    /// <summary>
    /// Per run of letters in <paramref name="word"/>: in a run of capitals (two or more besides the
    /// bars, or one with a bar in front) every "l" becomes "I"; otherwise an "I" right after a lowercase letter, or as the
    /// second letter between a capital and a lowercase letter, becomes "l". "Il", "lk" and
    /// "McIntyre" are left alone.
    /// </summary>
    internal static bool TryFixBarMisreadByCase(string word, out string fixedWord)
    {
        fixedWord = word;
        if (string.IsNullOrEmpty(word) || word.IndexOf('I') < 0 && word.IndexOf('l') < 0)
        {
            return false;
        }

        var chars = word.ToCharArray();
        var start = 0;
        while (start < chars.Length)
        {
            if (!char.IsLetter(chars[start]))
            {
                start++;
                continue;
            }

            var end = start;
            while (end < chars.Length && char.IsLetter(chars[end]))
            {
                end++;
            }

            FixBarsInLetterRun(chars, start, end);
            start = end;
        }

        fixedWord = new string(chars);
        return fixedWord != word;
    }

    private static void FixBarsInLetterRun(char[] chars, int start, int end)
    {
        var uppers = 0;
        var lowersBesidesL = 0;
        var bars = 0;
        for (var i = start; i < end; i++)
        {
            var c = chars[i];
            if (c == 'l')
            {
                bars++;
            }
            else if (char.IsUpper(c))
            {
                uppers++;
            }
            else if (char.IsLower(c))
            {
                lowersBesidesL++;
            }
        }

        // Two capitals besides the bars make a run all-caps ("JUNl", "lFF"); with one, only a bar
        // in front of the capital does ("lN", "lS") - "Il" is Italian and a roman numeral.
        if (bars > 0 && lowersBesidesL == 0 && (uppers >= 2 || uppers == 1 && chars[start] == 'l'))
        {
            for (var i = start; i < end; i++)
            {
                if (chars[i] == 'l')
                {
                    chars[i] = 'I';
                }
            }

            return;
        }

        if (lowersBesidesL == 0 && bars == 0)
        {
            return; // all caps without a bar, or "I" alone
        }

        for (var i = start; i < end; i++)
        {
            if (chars[i] != 'I')
            {
                continue;
            }

            if (i > start && char.IsLower(chars[i - 1]))
            {
                // "McIntyre", "MacIsaac": the capital belongs to the name.
                var isMc = i - start >= 2 && chars[i - 2] == 'M' && chars[i - 1] == 'c';
                var isMac = i - start >= 3 && chars[i - 3] == 'M' && chars[i - 2] == 'a' && chars[i - 1] == 'c';
                if (!isMc && !isMac)
                {
                    chars[i] = 'l';
                }
            }
            else if (i == start + 1 && char.IsUpper(chars[start]))
            {
                // "FIight", "AIvar", "AIIenwood": the bars between the capital and the first
                // lowercase letter.
                var j = i;
                while (j < end && chars[j] == 'I')
                {
                    j++;
                }

                if (j < end && char.IsLower(chars[j]))
                {
                    for (var k = i; k < j; k++)
                    {
                        chars[k] = 'l';
                    }
                }
            }
        }
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

    private static readonly string[] AccentLookalikeGroups =
    {
        "aàáâãäåā", "AÀÁÂÃÄÅĀ", "eèéêëēěęė", "EÈÉÊËĒĚĘĖ", "iìíîïī", "IÌÍÎÏĪ", "oòóôõöøō", "OÒÓÔÕÖØŌ",
        "uùúûüūů", "UÙÚÛÜŪŮ", "cçčć", "CÇČĆ", "sšśş", "SŠŚŞ", "zžźż", "ZŽŹŻ", "nñńň", "NÑŃŇ",
        "yýÿ", "YÝŸ", "rř", "RŘ", "dď", "DĎ", "tť", "TŤ", "lł", "LŁ", "gğ", "GĞ", "Bß",
    };

    // Lookalikes across different base letters (the hook of "č" vs the dots of "ë").
    private static readonly string[] AccentCrossGroups = { "čë", "ČË", "šë", "žä", "ŽÄ" };

    // Languages written without accented letters: an accented letter there is a loanword or name
    // ("café", "René", "résumé") that the dictionary usually lacks, so it must not be stripped.
    private static readonly HashSet<string> PlainLatinLanguages = new(StringComparer.Ordinal) { "en", "id", "ms" };

    // Letters whose accent sits where the dot is - nOCR's typical misread of a plain "i" ("ìs", "movíes").
    private const string DotPositionAccents = "ìíîÌÍÎ";

    private string GetAccentLookalikesForLanguage(char c)
    {
        var lookalikes = GetAccentLookalikes(c);
        if (!PlainLatinLanguages.Contains(_twoLetterIsoLanguageName) || DotPositionAccents.Contains(c))
        {
            return lookalikes;
        }

        return new string(lookalikes.Where(ch => ch > 127).ToArray());
    }

    private static string GetAccentLookalikes(char c)
    {
        var sb = new StringBuilder();
        foreach (var group in AccentLookalikeGroups)
        {
            if (group.Contains(c))
            {
                sb.Append(group);
            }
        }

        foreach (var group in AccentCrossGroups)
        {
            if (group.Contains(c))
            {
                sb.Append(group);
            }
        }

        // The OCR did see a mark, so other accented letters come first and the plain letter last
        // (Danish "Màske" is "Måske", not "Maske"); "ìs" still reaches "is" when "ís" is no word.
        var lookalikes = sb.ToString().Where(ch => ch != c).Distinct().ToList();
        return new string(lookalikes.Where(ch => ch > 127).Concat(lookalikes.Where(ch => ch <= 127)).ToArray());
    }

    /// <summary>
    /// "Eén", "Eénmaal": a plain capital followed by the same vowel with an accent is correct Dutch
    /// when the fully accented lowercase form ("één", "éénmaal") is a dictionary word.
    /// </summary>
    private bool IsDutchCapitalizedAccentedWord(string word)
    {
        if (_twoLetterIsoLanguageName != "nl" || word.Length < 3 ||
            "AEIOU".IndexOf(word[0]) < 0 || word[1] <= 127 || !char.IsLower(word[1]))
        {
            return false;
        }

        var baseLetter = word[1].ToString().Normalize(NormalizationForm.FormD)[0];
        if (baseLetter != char.ToLowerInvariant(word[0]))
        {
            return false;
        }

        return IsSpelledCorrect(word[1] + word.Substring(1).ToLowerInvariant());
    }

    private bool IsAccentCandidate(char c)
    {
        return c > 127 && char.IsLetter(c) && GetAccentLookalikesForLanguage(c).Length > 0;
    }

    /// <summary>
    /// Tries the lookalikes of every accented letter in <paramref name="word"/> (fewest changes
    /// first, other accented letters before the plain letter) and returns the first variant the
    /// dictionary or names list accepts.
    /// </summary>
    private bool TryFixAccentMisread(string word, out string fixedWord)
    {
        fixedWord = word;
        if (word.Length < 2)
        {
            return false;
        }

        var positions = new List<int>();
        for (var i = 0; i < word.Length; i++)
        {
            var ch = word[i];
            if (IsAccentCandidate(ch))
            {
                positions.Add(i);
            }
            else if (!char.IsLetter(ch) && ch != '\'' && ch != '-')
            {
                return false;
            }
        }

        const int maxAccentPositions = 3;
        if (positions.Count == 0 || positions.Count > maxAccentPositions)
        {
            return false;
        }

        var alternatives = positions.Select(p => GetAccentLookalikesForLanguage(word[p])).ToList();
        var chars = word.ToCharArray();
        string? found = null;

        bool Accept()
        {
            var candidate = new string(chars);
            if (IsSpelledCorrect(candidate) || _spellCheckWordLists.HasName(candidate.Trim('\'', '-')))
            {
                found = candidate;
                return true;
            }

            return false;
        }

        // Change exactly `changes` of the positions (from index `from` on), recursively.
        bool Search(int from, int changes)
        {
            if (changes == 0)
            {
                return Accept();
            }

            for (var k = from; k < positions.Count; k++)
            {
                var pos = positions[k];
                foreach (var alt in alternatives[k])
                {
                    chars[pos] = alt;
                    if (Search(k + 1, changes - 1))
                    {
                        return true;
                    }
                }

                chars[pos] = word[pos];
            }

            return false;
        }

        for (var changes = 1; changes <= positions.Count; changes++)
        {
            if (Search(0, changes))
            {
                fixedWord = found!;
                return true;
            }
        }

        return false;
    }

    private const char UnknownCharacter = '*';
    private const string UnknownCharacterCandidates = "abcdefghijklmnopqrstuvwxyz'";
    private const string UnknownCharacterCandidatesFirst = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly Regex UnknownCharacterWordRegex = new(@"[\p{L}\d'*]*\*[\p{L}\d'*]*", RegexOptions.Compiled);

    /// <summary>
    /// "exper*ment" -> "experiment", "assho*e" -> "asshole": each "*" inside a word is tried as
    /// every letter (and an apostrophe, "they*ve"), and the word is only changed when exactly one
    /// combination is a dictionary word. A trailing "*" is left alone - it is as often a missed
    /// full stop or comma - and so are words with more than two gaps or fewer than three letters.
    /// </summary>
    internal string FillUnknownCharactersInWords(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf(UnknownCharacter) < 0 || !_isLoaded)
        {
            return text;
        }

        return UnknownCharacterWordRegex.Replace(text, match =>
        {
            var word = match.Value.TrimEnd(UnknownCharacter);
            var trailing = match.Value.Substring(word.Length);
            var gaps = 0;
            var letters = 0;
            foreach (var ch in word)
            {
                if (ch == UnknownCharacter)
                {
                    gaps++;
                }
                else if (char.IsLetter(ch))
                {
                    letters++;
                }
                else if (char.IsDigit(ch))
                {
                    return match.Value; // "c0m*ng" - the 0 is misread too
                }
            }

            if (gaps == 0 || gaps > 2 || letters < 3)
            {
                return match.Value;
            }

            string? found = null;
            var chars = word.ToCharArray();
            if (!TryFillGap(chars, 0, ref found))
            {
                return match.Value; // ambiguous
            }

            return found == null ? match.Value : found + trailing;
        });
    }

    // Fills the gaps from position 'start' on; returns false as soon as a second dictionary word
    // turns up, so an ambiguous gap ("b*t" -> bat/bet/bit/but) is never filled.
    private bool TryFillGap(char[] chars, int start, ref string? found)
    {
        var gap = Array.IndexOf(chars, UnknownCharacter, start);
        if (gap < 0)
        {
            var candidate = new string(chars);
            if (candidate[0] == '\'' || candidate[^1] == '\'' || !DoSpell(candidate))
            {
                return true;
            }

            if (found != null)
            {
                // Hunspell also accepts "Landscape" for "landscape"; the lowercase one came first.
                return string.Equals(found, candidate, StringComparison.OrdinalIgnoreCase);
            }

            found = candidate;
            return true;
        }

        foreach (var ch in gap == 0 ? UnknownCharacterCandidatesFirst : UnknownCharacterCandidates)
        {
            chars[gap] = ch;
            if (!TryFillGap(chars, gap + 1, ref found))
            {
                chars[gap] = UnknownCharacter;
                return false;
            }
        }

        chars[gap] = UnknownCharacter;
        return true;
    }

    private static readonly SearchValues<char> StandaloneWordNeighbours = SearchValues.Create(".,!?:;\"'()[]-…♪♫¿¡“”„‘’«»—–");

    // True when the word part is surrounded only by white space, tags, the line ends or
    // ordinary punctuation.
    private static bool IsStandaloneWord(OcrFixLineResult splitLine, int index)
    {
        return IsStandaloneNeighbour(splitLine, index - 1) && IsStandaloneNeighbour(splitLine, index + 1);
    }

    private static bool IsStandaloneNeighbour(OcrFixLineResult splitLine, int index)
    {
        if (index < 0 || index >= splitLine.Words.Count)
        {
            return true;
        }

        var part = splitLine.Words[index];
        return part.LinePartType != OcrFixLinePartType.SpecialCharacters ||
               !part.Word.AsSpan().ContainsAnyExcept(StandaloneWordNeighbours);
    }

    // A capitalized part after the first one must be a name or "I": "forAndy" -> "for Andy" is
    // fine, but "ofhaIfa" (OCR read the l in "half" as I) -> "of ha If a" is not.
    private bool HasMisplacedUppercasePart(string splitWords)
    {
        var parts = splitWords.Split(' ');
        for (var i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length > 0 && char.IsUpper(part[0]) &&
                part != "I" && !part.StartsWith("I'", StringComparison.Ordinal) &&
                !_spellCheckWordLists.HasName(part))
            {
                return true;
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
