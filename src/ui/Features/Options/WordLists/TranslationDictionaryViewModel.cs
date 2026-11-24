using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public partial class TranslationDictionaryViewModel : ObservableObject
{
    public sealed record LanguageChoice(string Code, string Name)
    {
        public override string ToString() => Name;
    }

    private string _folder = string.Empty;

    public Window? Window { get; set; }
    public IReadOnlyList<LanguageChoice> Languages { get; } = TranslationPostProcessDictionary.Languages
        .Select(code => new LanguageChoice(code, CultureInfo.GetCultureInfo(code).DisplayName))
        .OrderBy(language => language.Name, StringComparer.CurrentCultureIgnoreCase)
        .ToArray();

    [ObservableProperty] private LanguageChoice? _targetLanguage;
    [ObservableProperty] private LanguageChoice? _sourceLanguage;
    [ObservableProperty] private IReadOnlyList<TranslationPostProcessDictionary.Entry> _entries = Array.Empty<TranslationPostProcessDictionary.Entry>();
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRuleCommand))]
    private TranslationPostProcessDictionary.Entry? _selectedEntry;
    [ObservableProperty] private string _sourceCue = string.Empty;
    [ObservableProperty] private string _engineTranslation = string.Empty;
    [ObservableProperty] private string _output = string.Empty;
    [ObservableProperty] private bool _reviewOnly;
    [ObservableProperty] private string _note = string.Empty;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveRuleCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveRuleCommand))]
    private bool _canEdit;

    public void Initialize(LanguageItem? preferredLanguage)
    {
        Initialize(preferredLanguage, Se.TranslationPostProcessDictionariesFolder);
    }

    internal void Initialize(LanguageItem? preferredLanguage, string folder)
    {
        _folder = folder;
        TargetLanguage = FindLanguage(preferredLanguage?.CultureSpecificCode.Replace('_', '-')) ??
            FindLanguage(preferredLanguage?.TwoLetterISOLanguageName) ?? FindLanguage("en");
    }

    /// <summary>Opens on the language pair chosen in Auto Translate, so a new rule starts with the right languages.</summary>
    public void Initialize(string? targetCode, string? sourceCode)
    {
        Initialize(targetCode, sourceCode, Se.TranslationPostProcessDictionariesFolder);
    }

    internal void Initialize(string? targetCode, string? sourceCode, string folder)
    {
        _folder = folder;
        TargetLanguage = FindLanguage(targetCode) ?? FindLanguage("en");
        _preferredSourceLanguage = FindLanguage(sourceCode);
        if (_preferredSourceLanguage != null && _preferredSourceLanguage != TargetLanguage)
        {
            SourceLanguage = _preferredSourceLanguage;
        }
    }

    private LanguageChoice? _preferredSourceLanguage;

    partial void OnTargetLanguageChanged(LanguageChoice? value)
    {
        CanEdit = false;
        Status = string.Empty;
        Entries = Array.Empty<TranslationPostProcessDictionary.Entry>();
        NewRule();
        if (value == null)
        {
            return;
        }

        try
        {
            Entries = TranslationPostProcessDictionary.Read(value.Code, _folder);
            CanEdit = true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            Status = ex.Message;
        }
    }

    partial void OnSelectedEntryChanged(TranslationPostProcessDictionary.Entry? value)
    {
        if (value == null)
        {
            return;
        }

        SourceLanguage = FindLanguage(value.SourceLanguage);
        SourceCue = value.Source;
        EngineTranslation = value.Draft;
        ReviewOnly = value.Rule.Replacement == null;
        Output = value.Rule.Replacement ?? string.Join(Environment.NewLine, value.Rule.Suggestions);
        Note = value.Rule.Note ?? string.Empty;
        Status = string.Empty;
    }

    [RelayCommand]
    private void NewRule()
    {
        SelectedEntry = null;
        SourceLanguage = _preferredSourceLanguage != null && _preferredSourceLanguage != TargetLanguage
            ? _preferredSourceLanguage
            : TargetLanguage?.Code.Equals("en", StringComparison.OrdinalIgnoreCase) == true
                ? Languages.FirstOrDefault(language => language != TargetLanguage)
                : FindLanguage("en");
        SourceCue = string.Empty;
        EngineTranslation = string.Empty;
        Output = string.Empty;
        Note = string.Empty;
        ReviewOnly = false;
        Status = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void SaveRule()
    {
        if (SourceLanguage == null)
        {
            Status = Se.Language.Options.WordLists.TranslationSourceLanguage;
            return;
        }

        var suggestions = ReviewOnly
            ? Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();
        var rule = new TranslationPostProcessDictionary.Rule(ReviewOnly ? null : Output.Trim(), suggestions, Note.Trim());
        var entry = new TranslationPostProcessDictionary.Entry(SourceLanguage.Code, SourceCue.Trim(), EngineTranslation.Trim(), rule);
        var entries = Entries.ToList();
        var index = SelectedEntry == null ? -1 : entries.IndexOf(SelectedEntry);
        if (index >= 0)
        {
            entries[index] = entry;
        }
        else
        {
            entries.Add(entry);
        }

        if (TrySave(entries))
        {
            SelectedEntry = entry;
            Status = Se.Language.General.FileSaved;
        }
    }

    private bool CanRemoveRule() => CanEdit && SelectedEntry != null;

    [RelayCommand(CanExecute = nameof(CanRemoveRule))]
    private void RemoveRule()
    {
        var entries = Entries.Where(entry => entry != SelectedEntry).ToArray();
        if (TrySave(entries))
        {
            NewRule();
            Status = Se.Language.General.FileSaved;
        }
    }

    [RelayCommand]
    private void Close() => Window?.Close();

    /// <summary>Escape closes the window, F1 opens the Auto Translate help where the dictionary is described.</summary>
    internal void OnKeyDown(KeyEventArgs e) => UiUtil.CloseOnEscapeOrShowHelp(e, Window, "features/auto-translate");

    private bool TrySave(IReadOnlyList<TranslationPostProcessDictionary.Entry> entries)
    {
        if (TargetLanguage == null)
        {
            return false;
        }

        try
        {
            TranslationPostProcessDictionary.Save(TargetLanguage.Code, entries, _folder);
            Entries = entries;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Status = ex.Message;
            return false;
        }
    }

    private LanguageChoice? FindLanguage(string? code) =>
        Languages.FirstOrDefault(language => language.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}
