using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public partial class AiReviewViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _engines;
    [ObservableProperty] private string _selectedEngine;
    [ObservableProperty] private bool _isOllamaVisible;
    [ObservableProperty] private bool _isLlamaCppVisible;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private ObservableCollection<LlamaCppModelDisplay> _llamaCppModels;
    [ObservableProperty] private LlamaCppModelDisplay? _selectedLlamaCppModel;
    [ObservableProperty] private string _languageDisplay;
    [ObservableProperty] private ObservableCollection<ReviewFilterChip> _filterChips;
    [ObservableProperty] private ObservableCollection<ReviewSuggestionItem> _suggestions;
    [ObservableProperty] private ReviewSuggestionItem? _selectedSuggestion;
    [ObservableProperty] private bool _isReviewing;
    [ObservableProperty] private bool _isNotReviewing = true;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _reasonText;
    [ObservableProperty] private string _summaryText;
    [ObservableProperty] private string _applyButtonText;
    [ObservableProperty] private string _warningNoteText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; } = new();
    public int SelectedCount => _allSuggestions.Count(s => s.IsSelected);

    private readonly IWindowService _windowService;
    private readonly List<ReviewSuggestionItem> _allSuggestions = new();
    private Subtitle _subtitle = new();
    private string _languageCode = "en";
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _syncingSelection;

    public AiReviewViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Engines = new ObservableCollection<string> { SeAiReview.EngineLlamaCpp, SeAiReview.EngineOllama };
        SelectedEngine = Se.Settings.Tools.AiReview.Engine == SeAiReview.EngineOllama
            ? SeAiReview.EngineOllama
            : SeAiReview.EngineLlamaCpp;
        OllamaModel = Se.Settings.Tools.AiReview.OllamaModel;
        LlamaCppModels = new ObservableCollection<LlamaCppModelDisplay>();
        SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(
            LlamaCppModels,
            LlamaCppServerManager.GetAllReviewModels(),
            Se.Settings.Tools.AiReview.LlamaCppModelFileName);

        LanguageDisplay = string.Empty;
        StatusText = string.Empty;
        ReasonText = string.Empty;
        SummaryText = string.Empty;
        WarningNoteText = string.Empty;
        Suggestions = new ObservableCollection<ReviewSuggestionItem>();

        var l = Se.Language.Tools.AiReview;
        FilterChips = new ObservableCollection<ReviewFilterChip>
        {
            new() { Category = null, Label = l.CategoryAll, IsActive = true },
            new() { Category = ReviewCategory.Spelling, Label = l.CategorySpelling },
            new() { Category = ReviewCategory.Grammar, Label = l.CategoryGrammar },
            new() { Category = ReviewCategory.Punctuation, Label = l.CategoryPunctuation },
            new() { Category = ReviewCategory.Casing, Label = l.CategoryCasing },
            new() { Category = ReviewCategory.Other, Label = l.CategoryOther },
        };

        ApplyButtonText = string.Format(l.ApplyXFixes, 0);
        UpdateSummary();
        UpdateEngineVisibility();
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat? subtitleFormat)
    {
        _subtitle = subtitle;
        _languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        LanguageDisplay = GetLanguageDisplayName(_languageCode);
    }

    private static string GetLanguageDisplayName(string code)
    {
        try
        {
            var name = CultureInfo.GetCultureInfo(code).EnglishName;
            var idx = name.IndexOf(" (", StringComparison.Ordinal);
            return idx > 0 ? name.Substring(0, idx) : name;
        }
        catch (CultureNotFoundException)
        {
            return code;
        }
    }

    partial void OnSelectedEngineChanged(string value)
    {
        UpdateEngineVisibility();
    }

    partial void OnIsReviewingChanged(bool value)
    {
        IsNotReviewing = !value;
    }

    private void UpdateEngineVisibility()
    {
        IsOllamaVisible = SelectedEngine == SeAiReview.EngineOllama;
        IsLlamaCppVisible = !IsOllamaVisible;
    }

    partial void OnSelectedSuggestionChanged(ReviewSuggestionItem? value)
    {
        if (value == null)
        {
            ReasonText = string.Empty;
            return;
        }

        var l = Se.Language.Tools.AiReview;
        var unitLines = _allSuggestions.Where(s => s.UnitId == value.UnitId).Select(s => s.Number).OrderBy(n => n).ToList();
        var who = unitLines.Count > 1
            ? string.Format(l.LinesXToY, unitLines.First(), unitLines.Last())
            : string.Format(l.LineX, value.Number);
        ReasonText = string.IsNullOrEmpty(value.Reason) ? who : $"{who}: {value.Reason}";
    }

    private void RefreshLlamaCppModels()
    {
        var selectedFileName = SelectedLlamaCppModel?.Model.FileName;
        SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(
            LlamaCppModels,
            LlamaCppServerManager.GetAllReviewModels(),
            selectedFileName);
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.AiReview;
        settings.Engine = SelectedEngine;
        settings.OllamaModel = OllamaModel;
        settings.LlamaCppModelFileName = SelectedLlamaCppModel?.Model.FileName ?? string.Empty;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Review()
    {
        if (IsReviewing || Window == null)
        {
            return;
        }

        SaveSettings();
        var l = Se.Language.Tools.AiReview;

        string url;
        var model = string.Empty;
        if (SelectedEngine == SeAiReview.EngineLlamaCpp)
        {
            var display = SelectedLlamaCppModel;
            if (display == null ||
                !await LlamaCppDownloadHelper.EnsureReadyAsync(Window, _windowService, display.Model.FileName,
                    LlamaCppServerManager.GetAllReviewModels(), persistAsTranslateModel: false))
            {
                RefreshLlamaCppModels();
                return;
            }

            RefreshLlamaCppModels(); // pick up the fresh install state (green dot)
            display = SelectedLlamaCppModel;
            if (display == null)
            {
                return;
            }

            IsReviewing = true;
            StatusText = "llama.cpp...";
            try
            {
                await LlamaCppServerManager.EnsureServerRunningAsync(display.Model, CancellationToken.None);
            }
            catch (Exception e)
            {
                IsReviewing = false;
                StatusText = string.Empty;
                await MessageBox.Show(Window, Se.Language.General.Error,
                    string.Format(l.EngineError, e.Message), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            url = LlamaCppServerManager.ApiUrl;
        }
        else
        {
            url = Se.Settings.Tools.AiReview.OllamaUrl;
            model = OllamaModel.Trim();
            IsReviewing = true;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var ct = _cancellationTokenSource.Token;

        ClearSuggestions();
        ProgressValue = 0;

        var lines = new List<ReviewLine>();
        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            var text = _subtitle.Paragraphs[i].Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                lines.Add(new ReviewLine(i + 1, text));
            }
        }

        var unitIds = AiReviewChunker.BuildUnitIds(lines);
        var unitIdByNumber = new Dictionary<int, int>();
        for (var i = 0; i < lines.Count; i++)
        {
            unitIdByNumber[lines[i].Number] = unitIds[i];
        }

        var chunks = AiReviewChunker.BuildChunks(lines, Se.Settings.Tools.AiReview.MaxLinesPerBatch);
        var systemPrompt = AiReviewProtocol.BuildSystemPrompt(Se.Settings.Tools.AiReview.Prompt, GetLanguageDisplayName(_languageCode));

        using var client = new AiReviewClient();
        var processedLines = 0;
        var consecutiveErrors = 0;

        try
        {
            foreach (var chunk in chunks)
            {
                ct.ThrowIfCancellationRequested();
                StatusText = string.Format(l.ReviewingLineXOfY, chunk.Lines[0].Number, _subtitle.Paragraphs.Count);

                var userContent = AiReviewProtocol.BuildUserContent(chunk);
                var editableNumbers = new HashSet<int>(chunk.Lines.Select(x => x.Number));

                List<AiReviewChange>? changes = null;
                try
                {
                    var reply = await client.ChatAsync(url, model, systemPrompt, userContent, ct);
                    changes = AiReviewProtocol.ParseChanges(reply, editableNumbers);
                    if (changes.Count == 0 && AiReviewProtocol.ExtractJsonObject(reply) == null)
                    {
                        // invalid reply - one retry for this chunk
                        reply = await client.ChatAsync(url, model, systemPrompt, userContent, ct);
                        changes = AiReviewProtocol.ParseChanges(reply, editableNumbers);
                    }

                    consecutiveErrors = 0;
                }
                catch (HttpRequestException e)
                {
                    consecutiveErrors++;
                    if (consecutiveErrors >= 3)
                    {
                        await MessageBox.Show(Window, Se.Language.General.Error,
                            string.Format(l.EngineError, e.Message), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                }

                if (changes != null)
                {
                    foreach (var change in changes)
                    {
                        AddSuggestion(change, unitIdByNumber);
                    }
                }

                processedLines += chunk.Lines.Count;
                ProgressValue = Math.Min(100.0, processedLines * 100.0 / Math.Max(1, lines.Count));
            }

            StatusText = _allSuggestions.Count == 0 && processedLines >= lines.Count
                ? l.NoIssuesFound
                : string.Format(l.ReviewDone, _allSuggestions.Count, processedLines);
        }
        catch (OperationCanceledException)
        {
            StatusText = string.Format(l.ReviewDone, _allSuggestions.Count, processedLines);
        }
        finally
        {
            ProgressValue = 100;
            IsReviewing = false;
        }
    }

    private void ClearSuggestions()
    {
        _allSuggestions.Clear();
        Suggestions.Clear();
        foreach (var chip in FilterChips)
        {
            chip.Count = 0;
        }

        WarningNoteText = string.Empty;
        ReasonText = string.Empty;
        UpdateSummary();
    }

    private void AddSuggestion(AiReviewChange change, Dictionary<int, int> unitIdByNumber)
    {
        var paragraphIndex = change.Number - 1;
        if (paragraphIndex < 0 || paragraphIndex >= _subtitle.Paragraphs.Count)
        {
            return;
        }

        var before = _subtitle.Paragraphs[paragraphIndex].Text;
        var after = change.NewText;
        if (before.Trim() == after.Trim())
        {
            return;
        }

        if (!AiReviewProtocol.TagsMatch(before, after))
        {
            return; // the model touched formatting tags - not trustworthy, skip
        }

        var l = Se.Language.Tools.AiReview;
        var ratio = after.Length / (double)Math.Max(1, before.Length);
        var isWarning = ratio > 1.4 || ratio < 0.6;
        var reason = change.Reason;
        if (isWarning)
        {
            reason = string.IsNullOrEmpty(reason) ? l.LargeChangeWarning : $"{l.LargeChangeWarning} - {reason}";
        }

        var item = new ReviewSuggestionItem
        {
            Number = change.Number,
            ParagraphIndex = paragraphIndex,
            UnitId = unitIdByNumber.TryGetValue(change.Number, out var unitId) ? unitId : -change.Number,
            Category = change.Category,
            Before = before,
            After = after,
            Reason = reason,
            IsWarning = isWarning,
            IsSelected = !isWarning,
        };
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ReviewSuggestionItem.IsSelected))
            {
                OnSuggestionSelectedChanged(item);
            }
        };

        // Review() runs on the UI thread (its awaits resume on the captured context), so add
        // synchronously - posting via the dispatcher made the end-of-review status read a stale
        // (possibly empty) suggestion count while the last chunk's items were still queued.
        _allSuggestions.Add(item);
        if (PassesFilter(item))
        {
            Suggestions.Add(item);
        }

        UpdateChipCounts();
        UpdateSummary();
    }

    private void OnSuggestionSelectedChanged(ReviewSuggestionItem item)
    {
        if (_syncingSelection)
        {
            return;
        }

        _syncingSelection = true;
        try
        {
            foreach (var other in _allSuggestions)
            {
                if (other != item && other.UnitId == item.UnitId)
                {
                    other.IsSelected = item.IsSelected;
                }
            }
        }
        finally
        {
            _syncingSelection = false;
        }

        UpdateSummary();
    }

    private bool PassesFilter(ReviewSuggestionItem item)
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

        var warnings = _allSuggestions.Count(s => s.IsWarning);
        WarningNoteText = warnings > 0
            ? string.Format(Se.Language.Tools.AiReview.XNeedACloserLook, warnings)
            : string.Empty;
    }

    private void UpdateSummary()
    {
        var l = Se.Language.Tools.AiReview;
        var selected = SelectedCount;
        SummaryText = string.Format(l.XSuggestionsYSelected, _allSuggestions.Count, selected);
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
        foreach (var item in _allSuggestions)
        {
            if (PassesFilter(item))
            {
                Suggestions.Add(item);
            }
        }
    }

    [RelayCommand]
    private void StopReview()
    {
        _cancellationTokenSource.Cancel();
    }

    [RelayCommand]
    private void SelectAll()
    {
        SetAllSelected(true);
    }

    [RelayCommand]
    private void InvertSelection()
    {
        _syncingSelection = true;
        try
        {
            foreach (var item in _allSuggestions)
            {
                item.IsSelected = !item.IsSelected;
            }
        }
        finally
        {
            _syncingSelection = false;
        }

        UpdateSummary();
    }

    private void SetAllSelected(bool selected)
    {
        _syncingSelection = true;
        try
        {
            foreach (var item in _allSuggestions)
            {
                item.IsSelected = selected;
            }
        }
        finally
        {
            _syncingSelection = false;
        }

        UpdateSummary();
    }

    [RelayCommand]
    private async Task PickOllamaModel()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<Ocr.PickOllamaModelWindow, Ocr.PickOllamaModelViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.PickOllamaModel, OllamaModel, Se.Settings.Tools.AiReview.OllamaUrl);
        });

        if (result is { OkPressed: true, SelectedModel: not null })
        {
            OllamaModel = result.SelectedModel;
        }
    }

    [RelayCommand]
    private async Task EditPrompt()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<AiReviewPromptWindow, AiReviewPromptViewModel>(Window, vm => vm.Initialize());
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();

        FixedSubtitle = new Subtitle(_subtitle, false);
        foreach (var item in _allSuggestions.Where(s => s.IsSelected))
        {
            if (item.ParagraphIndex >= 0 && item.ParagraphIndex < FixedSubtitle.Paragraphs.Count)
            {
                FixedSubtitle.Paragraphs[item.ParagraphIndex].Text = item.After;
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _cancellationTokenSource.Cancel();
            Window?.Close();
        }
    }

    internal void OnClosing()
    {
        _cancellationTokenSource.Cancel();
        UiUtil.SaveWindowPosition(Window);
    }
}
