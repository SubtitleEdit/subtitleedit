using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

/// <summary>
/// Interactive spell-check flow for the spell-check window: scans an observable collection of
/// subtitle line view-models and applies word changes back onto them. The framework-agnostic core
/// (Hunspell, word lists, word classification) lives in the <see cref="SpellChecker"/> base in
/// libuilogic so it can be shared with seconv. (#11744)
/// </summary>
public class SpellCheckManager : SpellChecker, ISpellCheckManager
{
    public delegate void SpellCheckWordChangedHandler(object sender, SpellCheckWordChangedEvent e);
    public event SpellCheckWordChangedHandler? OnWordChanged;
    public int NoOfChangedWords { get; set; }
    public int NoOfCorrectWords { get; set; }
    public int NoOfNames { get; set; }
    public int NoOfAddedWords { get; set; }

    private SpellCheckResult? _currentResult;

    public List<SpellCheckResult> CheckSpelling(ObservableCollection<SubtitleLineViewModel> subtitles, SpellCheckResult? startFrom = null, int? stopBeforeLineIndex = null)
    {
        var results = new List<SpellCheckResult>();

        var startLineIndex = startFrom?.LineIndex ?? 0;
        var startWordIndex = startFrom?.WordIndex ?? 0;
        if (startFrom != null)
        {
            startWordIndex++;
        }

        // When wrapping back to the top, scan only the lines above where this run started.
        var endLineIndex = stopBeforeLineIndex.HasValue
            ? Math.Min(stopBeforeLineIndex.Value, subtitles.Count)
            : subtitles.Count;

        for (var lineIndex = startLineIndex; lineIndex < endLineIndex; lineIndex++)
        {
            var p = subtitles[lineIndex];
            var words = SpellCheckWordLists.Split(p.Text);

            for (var wordIndex = startWordIndex; wordIndex < words.Count; wordIndex++)
            {
                var word = words[wordIndex];
                var textBefore = p.Text;
                if (!IsWordCorrect(word, p, words, wordIndex))
                {
                    results.Add(new SpellCheckResult
                    {
                        Paragraph = p,
                        LineIndex = lineIndex,
                        WordIndex = wordIndex,
                        Word = word,
                        Suggestions = GetSuggestions(word.Text),
                        IsCommonMisspelling = IsCommonMisspelling(word.Text),
                    });

                    _currentResult = results.Last();
                    return results;
                }

                // A change-all replacement inside IsWordCorrect may alter the line text, invalidating
                // the indexes of the remaining words, so re-split before checking the next word.
                if (p.Text != textBefore)
                {
                    words = SpellCheckWordLists.Split(p.Text);
                }
            }

            startWordIndex = 0;
        }

        return results;
    }

    public void ChangeWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p)
    {
        if (_currentResult == null)
        {
            return;
        }

        var text = p.Text.Remove(spellCheckWord.Index, spellCheckWord.Length);
        text = text.Insert(spellCheckWord.Index, toWord);
        p.Text = text;

        NoOfChangedWords++;

        OnWordChanged?.Invoke(this, new SpellCheckWordChangedEvent
        {
            Paragraph = p,
            FromWord = fromWord,
            ToWord = toWord,
            Word = spellCheckWord,
        });
    }

    public void ChangeAllWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p)
    {
        if (string.IsNullOrWhiteSpace(fromWord) || string.IsNullOrWhiteSpace(toWord))
        {
            return;
        }

        fromWord = fromWord.Trim();
        toWord = toWord.Trim();
        if (fromWord == toWord)
        {
            return;
        }

        if (!ChangeAllDictionary.ContainsKey(fromWord))
        {
            ChangeAllDictionary.Add(fromWord, toWord);
        }

        ChangeWord(fromWord, toWord, spellCheckWord, p);
    }

    private bool IsWordCorrect(SpellCheckWord spellCheckWord, SubtitleLineViewModel p, List<SpellCheckWord> words, int wordIndex)
    {
        var word = spellCheckWord.Text;
        var text = p.Text;

        if (SkipAllList.Contains(word) ||
            (word.StartsWith('\'') || word.EndsWith('\'')) && SkipAllList.Contains(word.Trim('\'')))
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
            NoOfNames++;
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

        if (WordLists != null && WordLists.IsWordInUserPhrases(wordIndex, words))
        {
            return true;
        }

        var isCorrect = DoSpell(word) || IsEnglishInApostropheActuallyIng(word);

        // A word wrapped in single quotes used as quotation marks (e.g. 'Een ridder ...') keeps the
        // leading/trailing apostrophe attached during splitting, so retry without them. The apostrophe
        // is not a split char because it is part of contractions like 'n / don't. (#11975)
        if (!isCorrect)
        {
            var trimmed = word.Trim('\'');
            if (trimmed.Length > 0 && trimmed != word &&
                (DoSpell(trimmed) ||
                 IsName(trimmed, text) ||
                 (WordLists != null && WordLists.HasUserWord(trimmed))))
            {
                isCorrect = true;
            }
        }

        if (ChangeAllDictionary.ContainsKey(word) && NotSameSpecialEnding(words[wordIndex], ChangeAllDictionary[word], text))
        {
            ChangeWord(word, ChangeAllDictionary[word], words[wordIndex], p);
            return true;
        }

        if (word.EndsWith('\'') && ChangeAllDictionary.ContainsKey(word.TrimEnd('\'')))
        {
            ChangeWord(word, ChangeAllDictionary[word.TrimEnd('\'')] + word.Remove(0, word.TrimEnd('\'').Length), words[wordIndex], p);
            return true;
        }

        if (isCorrect)
        {
            NoOfCorrectWords++;
        }

        return isCorrect;
    }
}
