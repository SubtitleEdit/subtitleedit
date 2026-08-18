using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppEngineSettings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public partial class AiReviewViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _engines;
    [ObservableProperty] private string _selectedEngine;
    [ObservableProperty] private bool _isOllamaVisible;
    [ObservableProperty] private bool _isLlamaCppVisible;
    [ObservableProperty] private bool _isOpenAiCompatibleVisible;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private string _openAiCompatibleUrl;
    [ObservableProperty] private string _openAiCompatibleModel;
    [ObservableProperty] private string _openAiCompatibleApiKey;
    [ObservableProperty] private int _requestDelaySeconds;
    [ObservableProperty] private ObservableCollection<LlamaCppModelDisplay> _llamaCppModels;
    [ObservableProperty] private LlamaCppModelDisplay? _selectedLlamaCppModel;
    [ObservableProperty] private string _languageDisplay;
    [ObservableProperty] private ObservableCollection<ReviewFilterChip> _filterChips;
    [ObservableProperty] private ObservableCollection<ReviewSuggestionItem> _suggestions;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCurrentLineCommand))]
    private ReviewSuggestionItem? _selectedSuggestion;
    [ObservableProperty] private bool _isReviewing;
    [ObservableProperty] private bool _isNotReviewing = true;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _reasonText;
    [ObservableProperty] private bool _hasReason;
    [ObservableProperty] private string _summaryText;
    [ObservableProperty] private string _applyButtonText;
    [ObservableProperty] private string _warningNoteText;
    [ObservableProperty] private bool _hasWarningNote;
    [ObservableProperty] private bool _isPlayVisible;

    /// <summary>
    /// True for callers with a live target (both main-window entry points): an "Apply" button is
    /// shown next to Ok, so the checked fixes can be handed over without closing and a long review
    /// can be worked through in passes (issue #13807). Callers without a target have nowhere to
    /// push a pass, so they get the plain Ok/Cancel pair.
    /// </summary>
    [ObservableProperty] private bool _isApplyVisible;

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
    private int _appliedCount;

    // Set by callers with a live target (both main-window entry points): "Apply" pushes the checked
    // fixes to them and the window stays open, so a long review can be worked through in passes
    // instead of ending at the first Apply (issue #13807).
    private Action<Subtitle>? _applyCallback;

    // Video preview hooks handed in by the caller - they drive the main window's video player.
    // Null when no video is loaded; the play button is then hidden.
    private Action<int>? _playLine;
    private Action? _stopPlayback;
    private bool _hasPlayed;

    public AiReviewViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Engines = new ObservableCollection<string>();
        SelectedEngine = AiEngineCombo.Populate(Engines, Se.Settings.Tools.AiReview.Engine);
        OllamaModel = Se.Settings.Tools.AiReview.OllamaModel;
        OpenAiCompatibleUrl = Se.Settings.Tools.AiReview.OpenAiCompatibleUrl;
        OpenAiCompatibleModel = Se.Settings.Tools.AiReview.OpenAiCompatibleModel;
        OpenAiCompatibleApiKey = Se.Settings.Tools.AiReview.OpenAiCompatibleApiKey;
        RequestDelaySeconds = Se.Settings.Tools.AiReview.RequestDelaySeconds;
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
            new() { Category = null, Label = Se.Language.General.All, IsActive = true },
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

    /// <summary>
    /// Sets up the review. <paramref name="playLine"/> plays the line at a paragraph index of
    /// <paramref name="subtitle"/> in the main video player and pauses at its end, so a suggested
    /// fix can be checked against the audio before it is applied; pass null (no video loaded) to
    /// hide the play button. <paramref name="stopPlayback"/> stops such a preview when the window
    /// closes - only ever called when this window actually started playback.
    /// </summary>
    /// <param name="applyCallback">
    /// When set, the Apply button hands the fixed subtitle to the caller and leaves the window open
    /// - the applied suggestions drop out of the list and the rest stay reviewable, so a review that
    /// took minutes to produce does not have to be run again to apply a second batch (issue #13807).
    /// Callers without a live target pass null and get the old apply-and-close behavior.
    /// </param>
    public void Initialize(
        Subtitle subtitle,
        SubtitleFormat? subtitleFormat,
        Action<int>? playLine = null,
        Action? stopPlayback = null,
        Action<Subtitle>? applyCallback = null)
    {
        _subtitle = subtitle;
        _playLine = playLine;
        _stopPlayback = stopPlayback;
        _applyCallback = applyCallback;
        IsApplyVisible = applyCallback != null;
        IsPlayVisible = playLine != null;
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

    partial void OnReasonTextChanged(string value)
    {
        HasReason = !string.IsNullOrEmpty(value);
    }

    partial void OnWarningNoteTextChanged(string value)
    {
        HasWarningNote = !string.IsNullOrEmpty(value);
    }

    private void UpdateEngineVisibility()
    {
        IsOllamaVisible = SelectedEngine == SeAiReview.EngineOllama;
        IsOpenAiCompatibleVisible = SelectedEngine == SeAiReview.EngineOpenAiCompatible;
        IsLlamaCppVisible = !IsOllamaVisible && !IsOpenAiCompatibleVisible;
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

    /// <summary>
    /// Re-fills the engine combo so its llama.cpp install-status dot is recomputed - the rows keep
    /// the status they were built with, so an engine download/update leaves a stale dot otherwise.
    /// </summary>
    private void RefreshEngines()
    {
        var selected = SelectedEngine;
        SelectedEngine = string.Empty; // drop the selection first, so the combo also rebuilds the closed-state row
        SelectedEngine = AiEngineCombo.Populate(Engines, selected);
    }

    /// <summary>
    /// Opens the shared llama.cpp engine settings dialog (installed backend, pinned release, install
    /// status). Its download button stops the server first - it holds llama-server open, so a running
    /// server would block replacing the binary - then re-downloads and refreshes the model dots.
    /// </summary>
    [RelayCommand]
    private async Task ShowLlamaCppEngineSettings()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<LlamaCppEngineSettingsWindow, LlamaCppEngineSettingsViewModel>(
            Window,
            vm => vm.Initialize(RedownloadLlamaCppEngineAsync));

        RefreshLlamaCppModels();
        RefreshEngines();
    }

    private async Task RedownloadLlamaCppEngineAsync()
    {
        if (Window == null)
        {
            return;
        }

        LlamaCppServerManager.StopServer();

        // Reuse the installed backend so the user is not re-asked CPU/Vulkan/CUDA on a re-download;
        // null on a fresh install (or off Windows), which lets DownloadAsync prompt.
        var variant = OperatingSystem.IsWindows()
            ? DownloadHashManager.DetectLlamaCppWindowsVariant(LlamaCppServerManager.GetAndCreateFolder())
            : null;

        await LlamaCppDownloadHelper.DownloadAsync(
            Window,
            _windowService,
            SelectedLlamaCppModel?.Model,
            variant,
            forceEngineDownload: true);

        RefreshLlamaCppModels();
        RefreshEngines(); // the engine binary just changed - re-evaluate its dot (amber -> green)
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.AiReview;
        settings.Engine = SelectedEngine;
        settings.OllamaModel = OllamaModel;
        settings.LlamaCppModelFileName = SelectedLlamaCppModel?.Model.FileName ?? string.Empty;
        settings.OpenAiCompatibleUrl = OpenAiCompatibleUrl.Trim();
        settings.OpenAiCompatibleModel = OpenAiCompatibleModel.Trim();
        settings.OpenAiCompatibleApiKey = OpenAiCompatibleApiKey.Trim();
        settings.RequestDelaySeconds = RequestDelaySeconds;
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
        string? apiKey = null;
        if (SelectedEngine == SeAiReview.EngineLlamaCpp)
        {
            var display = SelectedLlamaCppModel;
            if (display == null ||
                !await LlamaCppDownloadHelper.EnsureReadyAsync(Window, _windowService, display.Model.FileName,
                    LlamaCppServerManager.GetAllReviewModels(), persistAsTranslateModel: false))
            {
                RefreshLlamaCppModels();
                RefreshEngines();
                return;
            }

            RefreshLlamaCppModels(); // pick up the fresh install state (green dot)
            RefreshEngines();
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
        else if (SelectedEngine == SeAiReview.EngineOpenAiCompatible)
        {
            url = OpenAiCompatibleUrl.Trim();
            model = OpenAiCompatibleModel.Trim();
            apiKey = string.IsNullOrWhiteSpace(OpenAiCompatibleApiKey) ? null : OpenAiCompatibleApiKey.Trim();
            IsReviewing = true;
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
        var delay = TimeSpan.FromSeconds(Math.Max(0, RequestDelaySeconds));
        var lastRequestCompletedUtc = DateTime.MinValue;

        // Cloud engines enforce strict requests-per-minute limits and answer with 429 when they are
        // exceeded, so wait until the delay has passed since the previous request finished - the same
        // rule auto-translate uses. The first request of a run never waits.
        async Task<string> ChatWithDelayAsync(string content)
        {
            var remaining = delay - (DateTime.UtcNow - lastRequestCompletedUtc);
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, ct);
            }

            try
            {
                return await client.ChatAsync(url, model, systemPrompt, content, ct, apiKey);
            }
            finally
            {
                lastRequestCompletedUtc = DateTime.UtcNow;
            }
        }

        try
        {
            foreach (var chunk in chunks)
            {
                ct.ThrowIfCancellationRequested();
                StatusText = string.Format(l.ReviewingLineXOfY, chunk.Lines[0].Number, _subtitle.Paragraphs.Count);

                var userContent = AiReviewProtocol.BuildUserContent(chunk);
                var editableLines = chunk.Lines.ToDictionary(x => x.Number, x => x.Text);

                // Guard decisions (remaps/drops) are always written - they are rare, small and
                // the key evidence when a review pairs a correction with the wrong line. The
                // full request/reply per chunk respects the tools-log setting.
                var logGuard = (Action<string>)(s => Se.WriteToolsLog(s, true));

                List<AiReviewChange>? changes = null;
                try
                {
                    Se.WriteToolsLog($"AI review request (lines {chunk.Lines[0].Number}-{chunk.Lines[^1].Number}): {userContent}");
                    var reply = await ChatWithDelayAsync(userContent);
                    Se.WriteToolsLog($"AI review reply (lines {chunk.Lines[0].Number}-{chunk.Lines[^1].Number}): {reply}");
                    changes = AiReviewProtocol.ParseChanges(reply, editableLines, logGuard);
                    if (changes.Count == 0 && AiReviewProtocol.ExtractJsonObject(reply) == null)
                    {
                        // invalid reply - one retry for this chunk
                        Se.WriteToolsLog($"AI review: no JSON in reply for lines {chunk.Lines[0].Number}-{chunk.Lines[^1].Number} - retrying once", true);
                        reply = await ChatWithDelayAsync(userContent);
                        Se.WriteToolsLog($"AI review retry reply (lines {chunk.Lines[0].Number}-{chunk.Lines[^1].Number}): {reply}");
                        changes = AiReviewProtocol.ParseChanges(reply, editableLines, logGuard);
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
            Se.WriteToolsLog($"AI review: dropped change for line {change.Number} - formatting tags were altered (\"{before}\" -> \"{after}\")", true);
            return; // the model touched formatting tags - not trustworthy, skip
        }

        // A shifted model can copy from anywhere in its batch (a clean 3-line shift across a
        // whole batch has been seen in the wild), so the copy-source window must cover the
        // largest batch plus its read-only context lines - not just the closest neighbors.
        var window = Math.Max(2, Se.Settings.Tools.AiReview.MaxLinesPerBatch) + 6;
        var neighbors = new List<string>();
        for (var i = Math.Max(0, paragraphIndex - window); i <= Math.Min(_subtitle.Paragraphs.Count - 1, paragraphIndex + window); i++)
        {
            if (i != paragraphIndex && !string.IsNullOrWhiteSpace(_subtitle.Paragraphs[i].Text))
            {
                neighbors.Add(_subtitle.Paragraphs[i].Text);
            }
        }

        if (AiReviewProtocol.LooksMisaligned(before, after, neighbors))
        {
            Se.WriteToolsLog($"AI review: dropped change for line {change.Number} - the \"correction\" is a copy of a nearby line (\"{before}\" -> \"{after}\")", true);
            return; // the "correction" is really a copy of a nearby line - misnumbered by the model
        }

        var l = Se.Language.Tools.AiReview;
        var ratio = after.Length / (double)Math.Max(1, before.Length);
        var isMismatch = AiReviewProtocol.GetSimilarityPercent(before, after) < 50;
        var isWarning = ratio > 1.4 || ratio < 0.6 || isMismatch;
        var reason = change.Reason;
        if (isMismatch)
        {
            // A correction keeps most of its line - a "fix" that barely resembles the line is
            // usually a misnumbered reply whose copy-source we could not pin down. Never
            // pre-check those; applying one replaces the line with unrelated text.
            reason = string.IsNullOrEmpty(reason) ? l.MismatchWarning : $"{l.MismatchWarning} - {reason}";
            Se.WriteToolsLog($"AI review: flagged change for line {change.Number} - barely resembles the original (\"{before}\" -> \"{after}\")", true);
        }
        else if (isWarning)
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
        AddSuggestionItem(item);
    }

    /// <summary>
    /// Puts a built suggestion into the full list and, when it passes the active category filter,
    /// into the grid. Review() runs on the UI thread (its awaits resume on the captured context),
    /// so this is synchronous - posting via the dispatcher made the end-of-review status read a
    /// stale (possibly empty) suggestion count while the last chunk's items were still queued.
    /// </summary>
    internal void AddSuggestionItem(ReviewSuggestionItem item)
    {
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ReviewSuggestionItem.IsSelected))
            {
                OnSuggestionSelectedChanged(item);
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
        ApplyCommand.NotifyCanExecuteChanged();
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

    /// <summary>
    /// Plays the subtitle line the selected suggestion belongs to in the main video player and
    /// pauses at its end - the fastest way to judge whether a suggested fix is right.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPlayCurrentLine))]
    private void PlayCurrentLine()
    {
        var item = SelectedSuggestion;
        if (item == null || _playLine == null)
        {
            return;
        }

        _hasPlayed = true;
        _playLine(item.ParagraphIndex);
    }

    private bool CanPlayCurrentLine() => SelectedSuggestion != null;

    internal void OnSuggestionsGridDoubleTapped()
    {
        PlayCurrentLine();
    }

    [RelayCommand]
    private void SelectAll()
    {
        SetAllSelected(true);
    }

    [RelayCommand]
    private void SelectNone()
    {
        SetAllSelected(false);
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

    /// <summary>
    /// Writes the checked fixes and closes - the Ok half of the Ok/Apply pair, so finishing on the
    /// last pass is one click rather than Apply followed by a separate close.
    /// </summary>
    [RelayCommand]
    private void Ok()
    {
        SaveSettings();

        var applied = ApplySelectedSuggestions();
        FixedSubtitle = applied;

        if (_applyCallback == null)
        {
            // No live target: the caller picks the result up from FixedSubtitle after the dialog.
            OkPressed = true;
        }
        else
        {
            // The callback already delivered the fixes, so OkPressed stays false - a caller that
            // passes a callback and also reads FixedSubtitle would otherwise apply the pass twice.
            _applyCallback(applied);
        }

        _cancellationTokenSource.Cancel();
        Window?.Close();
    }

    /// <summary>
    /// Hands the checked fixes to the caller and leaves the window open: the applied rows drop out
    /// of the grid, the rest stay reviewable, and the next pass builds on the result - so a review
    /// that took minutes does not have to be run again to apply a second batch (issue #13807).
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply()
    {
        if (_applyCallback == null)
        {
            return;
        }

        SaveSettings();

        var applied = ApplySelectedSuggestions();
        FixedSubtitle = applied;
        _applyCallback(applied);

        // Keep working against what the caller now holds, and drop the suggestions that are in it -
        // an applied row is done, and its "before" text no longer exists in the subtitle.
        _subtitle = new Subtitle(applied, false);
        RemoveAppliedSuggestions();
        StatusText = string.Format(Se.Language.Main.FixedXLines, _appliedCount);
    }

    // Nothing checked means Apply would hand the caller an unchanged subtitle - an undo step and a
    // "fixed 0 lines" status for no change at all.
    private bool CanApply() => SelectedCount > 0;

    /// <summary>
    /// A copy of the working subtitle with every checked suggestion written into it.
    /// </summary>
    private Subtitle ApplySelectedSuggestions()
    {
        var applied = new Subtitle(_subtitle, false);
        _appliedCount = 0;
        foreach (var item in _allSuggestions.Where(s => s.IsSelected))
        {
            if (item.ParagraphIndex >= 0 && item.ParagraphIndex < applied.Paragraphs.Count)
            {
                applied.Paragraphs[item.ParagraphIndex].Text = item.After;
                _appliedCount++;
            }
        }

        return applied;
    }

    /// <summary>
    /// Drops the suggestions that were just applied from both the full list and the filtered grid,
    /// then refreshes the chip counts, the summary and the reason strip.
    /// </summary>
    private void RemoveAppliedSuggestions()
    {
        var applied = _allSuggestions.Where(s => s.IsSelected).ToList();
        if (applied.Count == 0)
        {
            return;
        }

        foreach (var item in applied)
        {
            _allSuggestions.Remove(item);
            Suggestions.Remove(item);
        }

        SelectedSuggestion = Suggestions.FirstOrDefault();
        UpdateChipCounts();
        UpdateSummary();
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/ai-review");
        }
        else if (IsPlayVisible && MatchesPlayShortcut(e))
        {
            e.Handled = true;
            PlayCurrentLine();
        }
    }

    /// <summary>
    /// True when the pressed keys match the user's main-window "play selected lines" (default F5)
    /// or second play/pause (default Ctrl/Cmd+Space) binding. Bare Space is deliberately not
    /// included: in this window it toggles the apply checkbox of the selected row.
    /// </summary>
    private static bool MatchesPlayShortcut(KeyEventArgs e)
    {
        return MainShortcutKeys.Matches(e, nameof(MainViewModel.PlaySelectedLinesWithoutLoopCommand), [nameof(Key.F5)]) ||
               MainShortcutKeys.Matches(e, nameof(MainViewModel.TogglePlayPause2Command), [MainShortcutKeys.CtrlOrCmd, nameof(Key.Space)]);
    }

    internal void OnClosing()
    {
        _cancellationTokenSource.Cancel();

        // Only stop what this window started - a video the user left playing before opening the
        // review should keep playing.
        if (_hasPlayed)
        {
            _stopPlayback?.Invoke();
        }

        UiUtil.SaveWindowPosition(Window);
    }
}
