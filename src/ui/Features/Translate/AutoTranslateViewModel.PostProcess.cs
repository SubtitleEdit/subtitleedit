using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel
{
    private sealed record DictionaryMatch(string OriginalText, TranslationPostProcessDictionary.Rule Rule);
    private readonly Dictionary<TranslateRow, DictionaryMatch> _dictionaryMatches = new();

    [ObservableProperty] private bool _usePostProcessDictionary;

    private DictionaryMatch? SelectedDictionaryMatch =>
        SelectedTranslateRow == null ? null : _dictionaryMatches.GetValueOrDefault(SelectedTranslateRow);

    public bool HasSelectedDictionaryMatch => SelectedDictionaryMatch != null;
    public bool HasDictionaryMatches => _dictionaryMatches.Count > 0;
    public bool ShowPostProcessSummary => UsePostProcessDictionary || _dictionaryMatches.Count > 0;
    public string? OriginalDictionaryText => SelectedDictionaryMatch?.OriginalText;
    public string? DictionaryNote => SelectedDictionaryMatch?.Rule.Note;

    public IReadOnlyList<string> DictionaryChoices
    {
        get
        {
            var rule = SelectedDictionaryMatch?.Rule;
            return rule?.Replacement is { } replacement ? new[] { replacement } : rule?.Suggestions ?? Array.Empty<string>();
        }
    }

    public string PostProcessSummary
    {
        get
        {
            var changed = _dictionaryMatches.Count(pair => pair.Key.TranslatedText != pair.Value.OriginalText);
            var review = _dictionaryMatches.Count(pair =>
                pair.Value.Rule.Suggestions.Count > 0 && pair.Key.TranslatedText == pair.Value.OriginalText);
            if (UsePostProcessDictionary && _dictionaryMatches.Count == 0 &&
                TranslationPostProcessDictionary.Get(SelectedSourceLanguage, SelectedTargetLanguage, Se.TranslationPostProcessDictionariesFolder) == null)
            {
                return Se.Language.Translate.PostProcessNoDictionary;
            }

            return string.Format(Se.Language.Translate.PostProcessSummary, changed, review);
        }
    }

    public string? SelectedDictionaryText
    {
        get => HasSelectedDictionaryMatch ? SelectedTranslateRow?.TranslatedText : null;
        set
        {
            if (value != null && IsTranslateEnabled && SelectedTranslateRow is { } row &&
                HasSelectedDictionaryMatch && row.TranslatedText != value)
            {
                OnTranslationEdited(row, value);
            }
        }
    }

    partial void OnUsePostProcessDictionaryChanged(bool value)
    {
        Se.Settings.AutoTranslate.UsePostProcessDictionary = value;
        OnPropertyChanged(nameof(ShowPostProcessSummary));
        OnPropertyChanged(nameof(PostProcessSummary));
    }

    partial void OnSelectedTranslateRowChanged(TranslateRow? value) => RefreshDictionarySelection();

    partial void OnSelectedSourceLanguageChanged(TranslationPair? value) => OnPropertyChanged(nameof(PostProcessSummary));

    [RelayCommand]
    private void RevertDictionaryText()
    {
        if (OriginalDictionaryText is { } original)
        {
            SelectedDictionaryText = original;
        }
    }

    [RelayCommand]
    private void UseDictionaryText(string? text)
    {
        if (text != null && DictionaryChoices.Contains(text))
        {
            SelectedDictionaryText = text;
        }
    }

    [RelayCommand]
    private void SelectNextDictionaryMatch()
    {
        if (!IsTranslateEnabled || !HasDictionaryMatches)
        {
            return;
        }

        var current = SelectedTranslateRow == null ? -1 : Rows.IndexOf(SelectedTranslateRow);
        for (var offset = 1; offset <= Rows.Count; offset++)
        {
            var index = (current + offset) % Rows.Count;
            if (_dictionaryMatches.ContainsKey(Rows[index]))
            {
                SelectAndScrollToRow(index);
                return;
            }
        }
    }

    internal void ApplyPostProcessDictionary(IEnumerable<TranslateRow> completedRows, TranslationPostProcessDictionary? dictionary)
    {
        foreach (var row in completedRows)
        {
            _dictionaryMatches.Remove(row);
            var rule = dictionary?.Match(row.Text, row.TranslatedText);
            if (rule == null)
            {
                continue;
            }

            _dictionaryMatches[row] = new DictionaryMatch(row.TranslatedText, rule);
            if (rule.Replacement is { } replacement)
            {
                row.TranslatedText = replacement;
            }
        }

        RefreshDictionaryRows();
    }

    private void RefreshDictionaryRows()
    {
        OnPropertyChanged(nameof(ShowPostProcessSummary));
        OnPropertyChanged(nameof(HasDictionaryMatches));
        OnPropertyChanged(nameof(PostProcessSummary));
        RefreshDictionarySelection();
    }

    private void RefreshDictionarySelection()
    {
        OnPropertyChanged(nameof(HasSelectedDictionaryMatch));
        OnPropertyChanged(nameof(OriginalDictionaryText));
        OnPropertyChanged(nameof(DictionaryNote));
        OnPropertyChanged(nameof(DictionaryChoices));
        OnPropertyChanged(nameof(SelectedDictionaryText));
    }
}
