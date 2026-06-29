using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WeCantSpell.Hunspell;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

/// <summary>
/// Framework-agnostic spell-checking core: Hunspell (pure-managed WeCantSpell) plus the names /
/// user-word / skip / change-all lists and the word-classification rules. Lives in libuilogic so both
/// the UI's interactive <see cref="SpellCheckManager"/> (which adds the SubtitleLineViewModel-bound
/// flow) and seconv can use it. (#11744)
/// </summary>
public class SpellChecker : ISpellChecker, IDoSpell
{
    public int NoOfSkippedWords { get; set; }
    public IWordSpellChecker? WordSpellChecker { get; set; }

    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HashtagRegex = new(@"^#[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    private static readonly Regex NumberRegex = new(@"^-?(?:\d+|\d{1,3}(?:,\d{3})+)?(?:\.\d+)?$", RegexOptions.Compiled);
    private static readonly Regex PercentageRegex = new(@"^-?(?:\d+|\d{1,3}(?:,\d{3})+)?(?:\.\d+)?%$", RegexOptions.Compiled);
    private static readonly Regex CurrencyRegex = new(@"^[$£€]?-?(?:\d+|\d{1,3}(?:,\d{3})+)?(?:\.\d+)?$", RegexOptions.Compiled);

    private static readonly HashSet<string> AllowedTokens = new() { "&", "—", "–", "…" };

    private WordList? _hunspellWeCantSpell;
    protected SpellCheckWordLists? WordLists;
    protected readonly List<string> SkipAllList = new();
    protected readonly Dictionary<string, string> ChangeAllDictionary = new();
    private string _twoLetterLanguageCode = string.Empty;

    public List<SpellCheckDictionaryDisplay> GetDictionaryLanguages(string dictionaryFolder)
    {
        var list = new List<SpellCheckDictionaryDisplay>();
        if (!Directory.Exists(dictionaryFolder))
        {
            return list;
        }

        foreach (var dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
        {
            var name = Path.GetFileNameWithoutExtension(dic);
            if (!name.StartsWith("hyph", StringComparison.Ordinal))
            {
                try
                {
                    var ci = CultureInfo.GetCultureInfo(name.Replace('_', '-'));
                    name = ci.DisplayName + " [" + name + "]";
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    name = "[" + name + "]";
                }

                var item = new SpellCheckDictionaryDisplay
                {
                    DictionaryFileName = dic,
                    Name = name,
                };
                list.Add(item);
            }
        }

        return list;
    }

    public bool Initialize(string dictionaryFile, string twoLetterLanguageCode)
    {
        SkipAllList.Clear();
        _twoLetterLanguageCode = twoLetterLanguageCode ?? string.Empty;

        if (!File.Exists(dictionaryFile))
        {
            return false;
        }

        var affixFile = Path.ChangeExtension(dictionaryFile, ".aff");
        _hunspellWeCantSpell = WordList.CreateFromFiles(dictionaryFile, affixFile);

        if (string.IsNullOrEmpty(twoLetterLanguageCode))
        {
            WordLists = null;
        }
        else
        {
            var name = Path.GetFileNameWithoutExtension(dictionaryFile);
            var fiveLetterCode = SpellCheckDictionaryDisplay.GetFiveLetterLanguageName(name);

            try
            {
                WordLists = new SpellCheckWordLists(fiveLetterCode ?? "en_US", this);
            }
            catch (Exception exception)
            {
                SpellCheckConfig.LogError("Error loading names for SpellCheckManager: " + exception.Message);
                WordLists = new SpellCheckWordLists(string.Empty, this);
            }
        }

        return true;
    }

    public List<string> GetSuggestions(string word)
    {
        if (WordSpellChecker != null)
        {
            return WordSpellChecker.GetSuggestions(word);
        }

        if (_hunspellWeCantSpell == null)
        {
            return new List<string>();
        }

        var suggestions = _hunspellWeCantSpell.Suggest(word);
        return suggestions.ToList();
    }

    public void AddIgnoreWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        NoOfSkippedWords++;

        if (!SkipAllList.Contains(word))
        {
            SkipAllList.Add(word);
        }

        var lowerWord = word.ToLowerInvariant();
        if (!SkipAllList.Contains(lowerWord))
        {
            SkipAllList.Add(lowerWord);
        }

        var upperWord = word.ToUpperInvariant();
        if (!SkipAllList.Contains(upperWord))
        {
            SkipAllList.Add(upperWord);
        }

        if (word.Length > 1)
        {
            var titleWord = char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
            if (!SkipAllList.Contains(titleWord))
            {
                SkipAllList.Add(titleWord);
            }
        }
    }

    public void AddToNames(string word)
    {
        WordLists?.AddName(word);
    }

    public void AdToUserDictionary(string word)
    {
        WordLists?.AddUserWord(word);
    }

    /// <summary>
    /// Reverses <see cref="AddIgnoreWord"/> by removing the word and the case variants it added.
    /// Used by spell-check Undo. Counters are restored separately from a snapshot.
    /// </summary>
    public void RemoveIgnoreWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        SkipAllList.Remove(word);
        SkipAllList.Remove(word.ToLowerInvariant());
        SkipAllList.Remove(word.ToUpperInvariant());
        if (word.Length > 1)
        {
            SkipAllList.Remove(char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());
        }
    }

    public void RemoveChangeAllWord(string fromWord)
    {
        if (string.IsNullOrEmpty(fromWord))
        {
            return;
        }

        ChangeAllDictionary.Remove(fromWord);
        WordLists?.UseAlwaysListRemove(fromWord);
    }

    public void RemoveFromNames(string word)
    {
        WordLists?.RemoveName(word);
    }

    public void RemoveFromUserDictionary(string word)
    {
        WordLists?.RemoveUserWord(word);
    }

    public bool IsWordCorrect(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return true;
        }

        if (AllowedTokens.Contains(word))
        {
            return true;
        }

        if (WordSpellChecker != null)
        {
            if (WordSpellChecker.DoSpell(word))
            {
                return true;
            }
        }
        else if (_hunspellWeCantSpell != null && _hunspellWeCantSpell.Check(word))
        {
            return true;
        }

        return IsEnglishInApostropheActuallyIng(word);
    }

    /// <summary>
    /// English-only convenience rule: words spelled with a dropped "g" such as
    /// "runnin'" or "doin'" are accepted as if they ended in "ing" ("running", "doing").
    /// Controlled by the Tools setting "Treat words ending in 'in'' as 'ing'".
    /// </summary>
    protected bool IsEnglishInApostropheActuallyIng(string word)
    {
        if (!SpellCheckConfig.TreatInApostropheAsIng())
        {
            return false;
        }

        if (!_twoLetterLanguageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (word.Length > 3 &&
            word.EndsWith("in'", StringComparison.OrdinalIgnoreCase) &&
            char.IsLetter(word[word.Length - 4]))
        {
            // drop the trailing apostrophe and append "g": "runnin'" -> "running"
            return DoSpell(word.Substring(0, word.Length - 1) + "g");
        }

        return false;
    }

    public bool IsWordCorrect(SpellCheckWord spellCheckWord, string text)
    {
        var word = spellCheckWord.Text;

        if (SkipAllList.Contains(word.ToUpperInvariant()) ||
            (word.StartsWith('\'') || word.EndsWith('\'')) && SkipAllList.Contains(word.Trim('\'').ToUpperInvariant()))
        {
            NoOfSkippedWords++;
            return true;
        }

        if (IsEmailUrlOrHashTag(word))
        {
            return true;
        }

        if (IsNumber(word))
        {
            return true;
        }

        if (IsName(word, text))
        {
            return true;
        }

        if (IsPartOfHtmlOrAssaTag(spellCheckWord, text))
        {
            return true;
        }

        if (WordLists != null && WordLists.HasUserWord(word))
        {
            return true;
        }

        var isCorrect = IsWordCorrect(word);

        // A word wrapped in single quotes used as quotation marks (e.g. 'Een ridder ...') keeps the
        // leading/trailing apostrophe attached during splitting, so retry without them. The apostrophe
        // is not a split char because it is part of contractions like 'n / don't. (#11975)
        if (!isCorrect)
        {
            var trimmed = word.Trim('\'');
            if (trimmed.Length > 0 && trimmed != word &&
                (IsWordCorrect(trimmed) ||
                 IsName(trimmed, text) ||
                 (WordLists != null && WordLists.HasUserWord(trimmed))))
            {
                isCorrect = true;
            }
        }

        return isCorrect;
    }

    public bool DoSpell(string word)
    {
        if (WordSpellChecker != null)
        {
            return WordSpellChecker.DoSpell(word);
        }

        return _hunspellWeCantSpell != null && _hunspellWeCantSpell.Check(word);
    }

    protected static bool IsPartOfHtmlOrAssaTag(SpellCheckWord spellCheckWord, string text)
    {
        if (text.Contains('<') || text.Contains('{'))
        {
            var index = spellCheckWord.Index;
            while (index >= 0)
            {
                var c = text[index];
                if (c == '<')
                {
                    var nextIdx = text.IndexOf('>', index);
                    if (nextIdx > index)
                    {
                        return true;
                    }
                    break;
                }
                else if (c == '{')
                {
                    var nextIdx = text.IndexOf('}', index);
                    if (nextIdx > index)
                    {
                        return true;
                    }
                    break;
                }

                if (c == '>' || c == '}')
                {
                    break;
                }

                index--;
            }
        }

        return false;
    }

    protected static bool IsEmailUrlOrHashTag(string word)
    {
        return EmailRegex.IsMatch(word) || UrlRegex.IsMatch(word) || HashtagRegex.IsMatch(word);
    }

    protected static bool IsNumber(string word)
    {
        return NumberRegex.IsMatch(word) || PercentageRegex.IsMatch(word) || CurrencyRegex.IsMatch(word);
    }

    protected bool IsName(string word, string text)
    {
        if (WordLists == null)
        {
            return false;
        }

        if (WordLists.HasName(word))
        {
            return true;
        }

        return WordLists.HasNameExtended(word, text);
    }

    protected bool IsCommonMisspelling(string word) //TODO: some auto corrections?
    {
        return false;
    }

    /// <summary>
    /// Do not allow changing "Who is lookin' at X" with "lokin" word to "lokin'" via repalce word.
    /// </summary>
    protected static bool NotSameSpecialEnding(SpellCheckWord spellCheckWord, string replaceWord, string text)
    {
        if (spellCheckWord.Index + spellCheckWord.Length + 1 >= text.Length)
        {
            return true;
        }

        var wordPlusOne = text.Substring(spellCheckWord.Index, spellCheckWord.Length + 1).TrimStart();
        if (replaceWord.EndsWith('\'') && !replaceWord.EndsWith("''", StringComparison.Ordinal) && wordPlusOne == replaceWord)
        {
            return false;
        }

        if (replaceWord.EndsWith('"') && !replaceWord.EndsWith("\"\"", StringComparison.Ordinal) && wordPlusOne == replaceWord)
        {
            return false;
        }

        return true;
    }
}
