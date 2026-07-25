using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Grammar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.GrammarCheck;

public partial class GrammarCheckViewModel : ObservableObject
{
    [ObservableProperty] private string _serverUrl;
    [ObservableProperty] private ObservableCollection<LanguageToolLanguage> _languages;
    [ObservableProperty] private LanguageToolLanguage? _selectedLanguage;
    [ObservableProperty] private bool _isPicky;
    [ObservableProperty] private ObservableCollection<ReviewFilterChip> _filterChips;
    [ObservableProperty] private ObservableCollection<GrammarCheckSuggestionItem> _suggestions;
    [ObservableProperty] private GrammarCheckSuggestionItem? _selectedSuggestion;
    [ObservableProperty] private ObservableCollection<string> _replacementOptions;
    [ObservableProperty] private string? _selectedReplacementOption;
    [ObservableProperty] private bool _hasReplacementOptions;
    [ObservableProperty] private bool _isChecking;
    [ObservableProperty] private bool _isNotChecking = true;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private bool _hasMessage;
    [ObservableProperty] private string _summaryText;
    [ObservableProperty] private string _applyButtonText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; } = new();

    /// <summary>Number of lines actually changed by <see cref="Ok"/>.</summary>
    public int FixedCount { get; private set; }

    public int SelectedCount => _allSuggestions.Count(s => s.IsSelected && s.CanApply);

    private readonly IWindowService _windowService;
    private readonly List<GrammarCheckSuggestionItem> _allSuggestions = new();
    private Subtitle _subtitle = new();
    private string _autoDetectedLanguageCode = "en";
    private CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>Cancels the language lookup when the window closes, so a hung server does not keep it alive.</summary>
    private readonly CancellationTokenSource _windowCancellationTokenSource = new();

    private bool _languagesLoaded;
    private bool _updatingReplacementOptions;

    public GrammarCheckViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        var settings = Se.Settings.Tools.GrammarCheck;
        ServerUrl = settings.ServerUrl;
        IsPicky = settings.Picky;
        Languages = new ObservableCollection<LanguageToolLanguage> { MakeAutoLanguage() };
        SelectedLanguage = Languages[0];
        Suggestions = new ObservableCollection<GrammarCheckSuggestionItem>();
        ReplacementOptions = new ObservableCollection<string>();
        StatusText = string.Empty;
        MessageText = string.Empty;
        SummaryText = string.Empty;

        var l = Se.Language.Tools.GrammarCheck;
        FilterChips = new ObservableCollection<ReviewFilterChip>
        {
            new() { Category = null, Label = l.CategoryAll, IsActive = true },
            new() { Category = ReviewCategory.Spelling, Label = l.CategorySpelling },
            new() { Category = ReviewCategory.Grammar, Label = l.CategoryGrammar },
            new() { Category = ReviewCategory.Punctuation, Label = l.CategoryPunctuation },
            new() { Category = ReviewCategory.Casing, Label = l.CategoryCasing },
            new() { Category = ReviewCategory.Other, Label = l.CategoryStyle },
        };

        ApplyButtonText = string.Format(l.ApplyXFixes, 0);
        UpdateSummary();
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat? subtitleFormat)
    {
        _subtitle = subtitle;
        _autoDetectedLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
    }

    /// <summary>Asks the server for its languages - also the connection test, so errors are shown as status.</summary>
    [RelayCommand]
    private async Task RefreshLanguages()
    {
        var l = Se.Language.Tools.GrammarCheck;
        var url = ServerUrl.Trim();
        StatusText = string.Format(l.Connecting, url);

        using var client = new LanguageToolClient();
        try
        {
            var languages = await client.GetLanguagesAsync(url, _windowCancellationTokenSource.Token);
            PopulateLanguages(languages);
            StatusText = string.Format(l.ConnectedXLanguages, languages.Count);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            StatusText = string.Format(l.ServerError, e.Message);
        }
    }

    /// <summary>Fills the language drop-down with what the server offers and picks the best entry.</summary>
    public void PopulateLanguages(IReadOnlyList<LanguageToolLanguage> languages)
    {
        // Only keep the current pick when the list is being reloaded (the user pressed refresh after
        // changing the server) - on the first load it is still the "Auto" placeholder, and the
        // language detected from the subtitle beats leaving it to the server, which sees only a few
        // short lines at a time.
        var previous = _languagesLoaded ? SelectedLanguage?.LongCode : null;
        var saved = Se.Settings.Tools.GrammarCheck.Language;
        Languages.Clear();
        Languages.Add(MakeAutoLanguage());
        foreach (var language in languages.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            if (Languages.All(x => x.LongCode != language.LongCode))
            {
                Languages.Add(language);
            }
        }

        SelectedLanguage = FindLanguage(previous)
                           ?? FindLanguage(saved == LanguageToolLanguage.AutoCode ? null : saved)
                           ?? FindLanguage(_autoDetectedLanguageCode)
                           ?? Languages[0];

        _languagesLoaded = languages.Count > 0;
    }

    /// <summary>
    /// Finds a language by long code ("en-US"), falling back to the first variant of a plain code
    /// ("en"), which is what the subtitle auto-detect gives us.
    /// </summary>
    private LanguageToolLanguage? FindLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var wanted = code.Trim();
        return Languages.FirstOrDefault(x => x.LongCode.Equals(wanted, StringComparison.OrdinalIgnoreCase))
               ?? Languages.FirstOrDefault(x => !x.IsAuto && x.Code.Equals(wanted, StringComparison.OrdinalIgnoreCase));
    }

    private static LanguageToolLanguage MakeAutoLanguage()
    {
        return new LanguageToolLanguage
        {
            Name = Se.Language.General.Auto,
            Code = LanguageToolLanguage.AutoCode,
            LongCode = LanguageToolLanguage.AutoCode,
        };
    }

    partial void OnIsCheckingChanged(bool value)
    {
        IsNotChecking = !value;
    }

    partial void OnMessageTextChanged(string value)
    {
        HasMessage = !string.IsNullOrEmpty(value);
    }

    partial void OnSelectedSuggestionChanged(GrammarCheckSuggestionItem? value)
    {
        _updatingReplacementOptions = true;
        try
        {
            ReplacementOptions.Clear();
            if (value == null)
            {
                SelectedReplacementOption = null;
                HasReplacementOptions = false;
                MessageText = string.Empty;
                return;
            }

            foreach (var replacement in value.Replacements)
            {
                ReplacementOptions.Add(replacement);
            }

            SelectedReplacementOption = value.CanApply ? value.Replacement : null;
            HasReplacementOptions = ReplacementOptions.Count > 1;

            var l = Se.Language.Tools.GrammarCheck;
            var message = value.Message.Length > 0 ? value.Message : value.RuleId;
            MessageText = value.CanApply
                ? $"{string.Format(l.LineX, value.Number)}: {message}"
                : $"{string.Format(l.LineX, value.Number)}: {message} - {l.NoAutomaticFix}";
        }
        finally
        {
            _updatingReplacementOptions = false;
        }
    }

    partial void OnSelectedReplacementOptionChanged(string? value)
    {
        if (_updatingReplacementOptions || value == null || SelectedSuggestion == null)
        {
            return;
        }

        SelectedSuggestion.Replacement = value;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.GrammarCheck;
        settings.ServerUrl = ServerUrl.Trim();
        settings.Language = SelectedLanguage?.LongCode ?? LanguageToolLanguage.AutoCode;
        settings.Picky = IsPicky;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Check()
    {
        if (IsChecking || Window == null)
        {
            return;
        }

        SaveSettings();
        var l = Se.Language.Tools.GrammarCheck;

        _cancellationTokenSource = new CancellationTokenSource();
        var ct = _cancellationTokenSource.Token;

        ClearSuggestions();
        ProgressValue = 0;
        IsChecking = true;

        var lineIndexes = new List<int>();
        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(_subtitle.Paragraphs[i].Text))
            {
                lineIndexes.Add(i);
            }
        }

        var settings = Se.Settings.Tools.GrammarCheck;
        var batchSize = Math.Max(1, settings.MaxLinesPerBatch);
        var options = new LanguageToolOptions
        {
            Language = SelectedLanguage?.LongCode ?? LanguageToolLanguage.AutoCode,
            Picky = IsPicky,
            DisabledRules = settings.DisabledRules,
            Username = settings.Username,
            ApiKey = settings.ApiKey,
        };

        using var client = new LanguageToolClient();
        var processedLines = 0;
        var consecutiveErrors = 0;

        try
        {
            for (var start = 0; start < lineIndexes.Count; start += batchSize)
            {
                ct.ThrowIfCancellationRequested();

                var batch = lineIndexes.GetRange(start, Math.Min(batchSize, lineIndexes.Count - start));
                StatusText = string.Format(l.CheckingLineXOfY, batch[0] + 1, _subtitle.Paragraphs.Count);

                var annotated = LanguageToolAnnotatedText.Build(batch.Select(i => _subtitle.Paragraphs[i].Text).ToList());
                if (!annotated.IsEmpty)
                {
                    try
                    {
                        var matches = await client.CheckAsync(ServerUrl, annotated.Json, options, ct);
                        AddMatches(matches, annotated, batch);
                        consecutiveErrors = 0;
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        consecutiveErrors++;
                        if (consecutiveErrors >= 3)
                        {
                            await MessageBox.Show(Window, Se.Language.General.Error,
                                string.Format(l.ServerError, e.Message), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                    }
                }

                processedLines += batch.Count;
                ProgressValue = Math.Min(100.0, processedLines * 100.0 / Math.Max(1, lineIndexes.Count));
            }

            StatusText = _allSuggestions.Count == 0 && processedLines >= lineIndexes.Count
                ? l.NoIssuesFound
                : string.Format(l.CheckDone, _allSuggestions.Count, processedLines);
        }
        catch (OperationCanceledException)
        {
            StatusText = string.Format(l.CheckDone, _allSuggestions.Count, processedLines);
        }
        finally
        {
            ProgressValue = 100;
            IsChecking = false;
        }
    }

    /// <summary>
    /// Turns the matches of one batch into rows. <paramref name="paragraphIndexes"/> holds the
    /// paragraph each line of <paramref name="annotated"/> came from.
    /// </summary>
    public void AddMatches(IReadOnlyList<LanguageToolMatch> matches, LanguageToolAnnotatedText annotated, IReadOnlyList<int> paragraphIndexes)
    {
        foreach (var match in matches)
        {
            AddSuggestion(match, annotated, paragraphIndexes);
        }
    }

    /// <summary>
    /// Turns one match into a row. Matches that cannot be mapped back to a single line - they crossed a
    /// line break or a formatting tag - are dropped: applying those would break the tag or the break.
    /// </summary>
    private void AddSuggestion(LanguageToolMatch match, LanguageToolAnnotatedText annotated, IReadOnlyList<int> batch)
    {
        if (!annotated.TryMapToLine(match.Offset, match.Length, out var lineIndex, out var lineOffset) ||
            lineIndex < 0 || lineIndex >= batch.Count)
        {
            return;
        }

        var paragraphIndex = batch[lineIndex];
        if (paragraphIndex < 0 || paragraphIndex >= _subtitle.Paragraphs.Count)
        {
            return;
        }

        var before = _subtitle.Paragraphs[paragraphIndex].Text;
        if (lineOffset < 0 || lineOffset + match.Length > before.Length)
        {
            return;
        }

        var fragment = before.Substring(lineOffset, match.Length);
        var replacements = match.Replacements.Where(r => r != fragment).ToList();
        var category = GrammarCheckSuggestionItem.MapCategory(match.CategoryId, match.IssueType);
        var item = new GrammarCheckSuggestionItem
        {
            Number = paragraphIndex + 1,
            ParagraphIndex = paragraphIndex,
            Offset = lineOffset,
            Length = match.Length,
            Category = category,
            Before = before,
            Fragment = fragment,
            Message = match.Message,
            ShortMessage = match.ShortMessage.Length > 0 ? match.ShortMessage : match.RuleDescription,
            RuleId = match.RuleId,
            Replacements = replacements,
            Replacement = replacements.Count > 0 ? replacements[0] : string.Empty,
            // Spelling/grammar/punctuation fixes are safe to tick by default; style is a matter of
            // taste, so those start unticked even though they can be applied.
            IsSelected = replacements.Count > 0 && category != ReviewCategory.Other,
        };

        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(GrammarCheckSuggestionItem.IsSelected))
            {
                UpdateSummary();
            }
        };

        _allSuggestions.Add(item);
        if (PassesFilter(item))
        {
            Suggestions.Add(item);
        }

        UpdateChipCounts();
        UpdateSummary();
    }

    private void ClearSuggestions()
    {
        _allSuggestions.Clear();
        Suggestions.Clear();
        foreach (var chip in FilterChips)
        {
            chip.Count = 0;
        }

        MessageText = string.Empty;
        UpdateSummary();
    }

    private bool PassesFilter(GrammarCheckSuggestionItem item)
    {
        var active = FilterChips.FirstOrDefault(c => c.IsActive);
        return active?.Category == null || active.Category == item.Category;
    }

    private void UpdateChipCounts()
    {
        foreach (var chip in FilterChips)
        {
            chip.Count = chip.Category == null
                ? _allSuggestions.Count
                : _allSuggestions.Count(s => s.Category == chip.Category);
        }
    }

    private void UpdateSummary()
    {
        var l = Se.Language.Tools.GrammarCheck;
        var selected = SelectedCount;
        SummaryText = string.Format(l.XIssuesYSelected, _allSuggestions.Count, selected);
        ApplyButtonText = string.Format(l.ApplyXFixes, selected);
    }

    [RelayCommand]
    private void SetFilter(ReviewFilterChip chip)
    {
        foreach (var c in FilterChips)
        {
            c.IsActive = c == chip;
        }

        Suggestions.Clear();
        foreach (var item in _allSuggestions.Where(PassesFilter))
        {
            Suggestions.Add(item);
        }
    }

    [RelayCommand]
    private void StopCheck()
    {
        _cancellationTokenSource.Cancel();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in _allSuggestions.Where(s => s.CanApply))
        {
            item.IsSelected = true;
        }

        UpdateSummary();
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var item in _allSuggestions.Where(s => s.CanApply))
        {
            item.IsSelected = !item.IsSelected;
        }

        UpdateSummary();
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<GrammarCheckSettingsWindow, GrammarCheckSettingsViewModel>(
            Window, vm => vm.Initialize());
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();

        FixedSubtitle = new Subtitle(_subtitle, false);
        FixedCount = 0;
        foreach (var group in _allSuggestions.Where(s => s.IsSelected && s.CanApply).GroupBy(s => s.ParagraphIndex))
        {
            if (group.Key < 0 || group.Key >= FixedSubtitle.Paragraphs.Count)
            {
                continue;
            }

            var paragraph = FixedSubtitle.Paragraphs[group.Key];
            var text = LanguageToolFix.Apply(paragraph.Text,
                group.Select(s => new LanguageToolFixItem(s.Offset, s.Length, s.Replacement)));
            if (text != paragraph.Text)
            {
                paragraph.Text = text;
                FixedCount++;
            }
        }

        OkPressed = true;
        _cancellationTokenSource.Cancel();
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        Window?.Close();
    }

    internal void OnLoaded()
    {
        if (!_languagesLoaded)
        {
            RefreshLanguagesCommand.Execute(null);
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _cancellationTokenSource.Cancel();
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/grammar-check");
        }
    }

    internal void OnClosing()
    {
        _cancellationTokenSource.Cancel();
        _windowCancellationTokenSource.Cancel();
        UiUtil.SaveWindowPosition(Window);
    }
}
