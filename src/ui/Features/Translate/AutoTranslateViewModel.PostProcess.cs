using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel
{
    /// <summary>
    /// A row the dictionary matched. A context-dependent rule gets its first suggestion applied and
    /// stays "to check" until the user picks a choice, edits the text or reverts it.
    /// </summary>
    private sealed class DictionaryMatch(string originalText, TranslationPostProcessDictionary.Rule rule)
    {
        public string OriginalText { get; } = originalText;
        public TranslationPostProcessDictionary.Rule Rule { get; } = rule;
        public bool Checked { get; set; } = rule.Replacement != null;
    }

    public sealed record DictionaryChoice(string Text, bool IsCurrent);
    private readonly Dictionary<TranslateRow, DictionaryMatch> _dictionaryMatches = new();

    [ObservableProperty] private bool _usePostProcessDictionary;

    private DictionaryMatch? SelectedDictionaryMatch =>
        SelectedTranslateRow == null ? null : _dictionaryMatches.GetValueOrDefault(SelectedTranslateRow);

    public bool HasSelectedDictionaryMatch => SelectedDictionaryMatch != null;
    public bool HasDictionaryMatches => _dictionaryMatches.Count > 0;
    public bool ShowPostProcessSummary => UsePostProcessDictionary || _dictionaryMatches.Count > 0;
    public string? OriginalDictionaryText => SelectedDictionaryMatch?.OriginalText;
    public string? DictionaryNote => SelectedDictionaryMatch?.Rule.Note;
    public bool SelectedDictionaryMatchNeedsCheck => SelectedDictionaryMatch is { Checked: false };
    public bool SelectedDictionaryMatchHasAlternatives => SelectedDictionaryMatch?.Rule.Suggestions.Count > 1;

    public string? SelectedDictionaryBadge => SelectedDictionaryMatch is not { } match
        ? null
        : match.Rule.Replacement != null ? Se.Language.Translate.PostProcessFixBadge : Se.Language.Translate.PostProcessCheckBadge;

    /// <summary>The rule's texts, with the one the line holds now marked.</summary>
    public IReadOnlyList<DictionaryChoice> DictionaryChoices
    {
        get
        {
            if (SelectedDictionaryMatch is not { } match || SelectedTranslateRow is not { } row)
            {
                return Array.Empty<DictionaryChoice>();
            }

            var texts = match.Rule.Replacement is { } replacement ? new[] { replacement } : match.Rule.Suggestions;
            return texts.Select(text => new DictionaryChoice(text, text == row.TranslatedText)).ToArray();
        }
    }

    public string PostProcessSummary
    {
        get
        {
            var changed = _dictionaryMatches.Count(pair => pair.Key.TranslatedText != pair.Value.OriginalText);
            var review = _dictionaryMatches.Count(pair => !pair.Value.Checked);
            if (UsePostProcessDictionary && _dictionaryMatches.Count == 0 && !HasDictionaryForSelectedLanguages())
            {
                return Se.Language.Translate.PostProcessNoDictionary;
            }

            return string.Format(Se.Language.Translate.PostProcessSummary, changed, review);
        }
    }

    private (TranslationPair? Source, TranslationPair? Target, bool HasDictionary)? _dictionaryAvailability;

    /// <summary>
    /// Whether a dictionary exists for the selected pair. The answer is kept per language pair, so the
    /// summary does not read and parse the dictionary file each time it refreshes.
    /// </summary>
    private bool HasDictionaryForSelectedLanguages()
    {
        var source = SelectedSourceLanguage;
        var target = SelectedTargetLanguage;
        if (_dictionaryAvailability is not { } cached || !ReferenceEquals(cached.Source, source) || !ReferenceEquals(cached.Target, target))
        {
            var hasDictionary = TranslationPostProcessDictionary.Get(source, target, Se.TranslationPostProcessDictionariesFolder) != null;
            _dictionaryAvailability = cached = (source, target, hasDictionary);
        }

        return cached.HasDictionary;
    }

    public string? SelectedDictionaryText
    {
        get => HasSelectedDictionaryMatch ? SelectedTranslateRow?.TranslatedText : null;
        set
        {
            if (value != null && IsTranslateEnabled && SelectedTranslateRow is { } row &&
                HasSelectedDictionaryMatch && row.TranslatedText != value)
            {
                MarkDictionaryMatchChecked(row);
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
        if (text == null || !DictionaryChoices.Any(choice => choice.Text == text) || SelectedTranslateRow is not { } row)
        {
            return;
        }

        // Picking the text the line already holds still confirms it.
        MarkDictionaryMatchChecked(row);
        SelectedDictionaryText = text;
        OnPropertyChanged(nameof(PostProcessSummary));
        RefreshDictionarySelection();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task EditPostProcessDictionary()
    {
        if (Window == null)
        {
            return;
        }

        var target = SelectedTargetLanguage == null ? null : TranslationPostProcessDictionary.GetLanguage(SelectedTargetLanguage);
        var source = SelectedSourceLanguage == null ? null : TranslationPostProcessDictionary.GetLanguage(SelectedSourceLanguage);
        await _windowService.ShowDialogAsync<TranslationDictionaryWindow, TranslationDictionaryViewModel>(
            Window, vm => vm.Initialize(target, source));

        // The rules may have changed: the next run reads them again, and the summary asks again.
        _dictionaryAvailability = null;
        OnPropertyChanged(nameof(PostProcessSummary));
    }

    private void MarkDictionaryMatchChecked(TranslateRow row)
    {
        if (_dictionaryMatches.TryGetValue(row, out var match))
        {
            match.Checked = true;
        }
    }

    /// <summary>Shows the engine's text in the grid under a line the dictionary changed.</summary>
    internal void SyncDictionaryMarker(TranslateRow row)
    {
        row.PostProcessOriginal = _dictionaryMatches.TryGetValue(row, out var match) && match.OriginalText != row.TranslatedText
            ? match.OriginalText
            : null;
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
            if (rule != null)
            {
                // Apply the fix, or the first suggestion for a context-dependent cue.
                _dictionaryMatches[row] = new DictionaryMatch(row.TranslatedText, rule);
                row.TranslatedText = rule.Replacement ?? rule.Suggestions[0];
            }

            SyncDictionaryMarker(row);
        }

        RefreshDictionaryRows();
    }

    private void RefreshDictionaryRows()
    {
        foreach (var row in _dictionaryMatches.Keys)
        {
            SyncDictionaryMarker(row);
        }

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
        OnPropertyChanged(nameof(SelectedDictionaryMatchNeedsCheck));
        OnPropertyChanged(nameof(SelectedDictionaryMatchHasAlternatives));
        OnPropertyChanged(nameof(SelectedDictionaryBadge));
        OnPropertyChanged(nameof(DictionaryChoices));
        OnPropertyChanged(nameof(SelectedDictionaryText));
    }
}
