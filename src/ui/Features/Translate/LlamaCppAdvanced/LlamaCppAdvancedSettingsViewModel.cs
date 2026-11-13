using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// Edits the global settings of the advanced llama.cpp translate engine: context options
/// (synopsis, glossary, rolling history), batch/style options and sampling overrides.
/// </summary>
public partial class LlamaCppAdvancedSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _synopsis = string.Empty;
    [ObservableProperty] private string _glossary = string.Empty;
    [ObservableProperty] private int _historyPairs;
    [ObservableProperty] private int _batchSize;
    [ObservableProperty] private bool _keepLineBreaks;
    [ObservableProperty] private ObservableCollection<string> _formalities = new();
    [ObservableProperty] private string? _selectedFormality;
    [ObservableProperty] private string _prompt = string.Empty;
    [ObservableProperty] private double? _temperature;
    [ObservableProperty] private double? _topP;
    [ObservableProperty] private int _topK;
    [ObservableProperty] private double? _repeatPenalty;
    [ObservableProperty] private int _maxTokens;
    [ObservableProperty] private int _contextSize;
    [ObservableProperty] private string _serverArguments = string.Empty;
    [ObservableProperty] private bool _serverArgumentsOnly;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _canGenerate;
    [ObservableProperty] private string _generateButtonText = Se.Language.Tools.AiReview.GenerateContext;
    [ObservableProperty] private string _generateStatus = string.Empty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private AiReviewContextGenerate? _generateContext;
    private CancellationTokenSource? _generateCancellation;

    /// <param name="generateContext">Drafts the synopsis from the subtitle with the local model
    /// (#16589); null hides the "Generate with AI" button (batch convert has no single subtitle).</param>
    public void Initialize(AiReviewContextGenerate? generateContext = null)
    {
        _generateContext = generateContext;
        CanGenerate = generateContext != null;

        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;

        Formalities = new ObservableCollection<string>
        {
            SeLlamaCppAdvanced.FormalityDefault,
            SeLlamaCppAdvanced.FormalityFormal,
            SeLlamaCppAdvanced.FormalityInformal,
        };
        SelectedFormality = Formalities.Contains(settings.Formality) ? settings.Formality : Formalities[0];

        Synopsis = settings.Synopsis;
        Glossary = settings.Glossary;
        HistoryPairs = settings.HistoryPairs;
        BatchSize = settings.BatchSize;
        KeepLineBreaks = settings.KeepLineBreaks;
        Prompt = settings.Prompt;
        Temperature = settings.Temperature;
        TopP = settings.TopP;
        TopK = settings.TopK;
        RepeatPenalty = settings.RepeatPenalty;
        MaxTokens = settings.MaxTokens;
        ContextSize = settings.ContextSize;
        ServerArguments = settings.ServerArguments;
        ServerArgumentsOnly = settings.ServerArgumentsOnly;
    }

    /// <summary>
    /// "Generate with AI": the same names, terms and synopsis draft as AI review's reference context,
    /// written into the synopsis box (Stop while it runs).
    /// </summary>
    [RelayCommand]
    private async Task GenerateContext()
    {
        if (IsGenerating)
        {
            _generateCancellation?.Cancel();
            return;
        }

        if (_generateContext == null || Window == null)
        {
            return;
        }

        var l = Se.Language.Tools.AiReview;
        if (!string.IsNullOrWhiteSpace(Synopsis))
        {
            var answer = await MessageBox.Show(Window, Se.Language.Translate.Synopsis, l.ReplaceContextQuestion,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        _generateCancellation = new CancellationTokenSource();
        IsGenerating = true;
        GenerateButtonText = Se.Language.General.Stop;
        try
        {
            var result = await _generateContext(Window, (part, count) =>
            {
                GenerateStatus = part == 0
                    ? l.GeneratingContextSynopsis
                    : string.Format(l.GeneratingContextPartXOfY, part, count);
            }, _generateCancellation.Token);

            if (result == null)
            {
                return; // engine not ready - already reported
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                await MessageBox.Show(Window, Se.Language.Translate.Synopsis, l.NoContextGenerated, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Synopsis = result;
        }
        catch (OperationCanceledException)
        {
            // stopped by the user - keep the old synopsis
        }
        catch (HttpRequestException e)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(l.EngineError, e.Message), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsGenerating = false;
            GenerateButtonText = l.GenerateContext;
            GenerateStatus = string.Empty;
            _generateCancellation?.Dispose();
            _generateCancellation = null;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        _generateCancellation?.Cancel();
        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        settings.Synopsis = Synopsis?.Trim() ?? string.Empty;
        settings.Glossary = Glossary?.Trim() ?? string.Empty;
        settings.HistoryPairs = Math.Clamp(HistoryPairs, 0, 50);
        settings.BatchSize = Math.Clamp(BatchSize, 1, 50);
        settings.KeepLineBreaks = KeepLineBreaks;
        settings.Formality = SelectedFormality ?? SeLlamaCppAdvanced.FormalityDefault;
        settings.Prompt = Prompt?.Trim() ?? string.Empty;
        settings.Temperature = Temperature ?? -1;
        settings.TopP = TopP ?? -1;
        settings.TopK = TopK;
        settings.RepeatPenalty = RepeatPenalty ?? -1;
        settings.MaxTokens = MaxTokens;
        settings.ContextSize = Math.Clamp(ContextSize, 2048, 262144);
        settings.ServerArguments = ServerArguments?.Trim() ?? string.Empty;
        settings.ServerArgumentsOnly = ServerArgumentsOnly;
        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _generateCancellation?.Cancel();
        Window?.Close();
    }

    [RelayCommand]
    private void ResetPrompt()
    {
        Prompt = LlamaCppAdvancedProtocol.DefaultPrompt;
    }

    internal void OnClosing()
    {
        _generateCancellation?.Cancel();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/auto-translate-advanced");
        }
    }
}
