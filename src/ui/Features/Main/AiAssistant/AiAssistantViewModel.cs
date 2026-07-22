using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppEngineSettings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Features.Main.AiAssistant;

/// <summary>
/// The subtitle text box AI assistant: quick actions and a free question for the
/// current line, with the neighbouring lines given as context. Shares the engine,
/// model and connection settings with Tools -> AI review (Ollama, llama.cpp, or an
/// OpenAI compatible server), and lets the user change them directly in the window;
/// nothing is applied automatically, the user reviews the suggestion and presses Apply.
/// </summary>
public partial class AiAssistantViewModel : ObservableObject
{
    [ObservableProperty] private string _currentText;
    [ObservableProperty] private string _contextText;
    [ObservableProperty] private string _questionText;
    [ObservableProperty] private string _resultText;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isNotBusy = true;
    [ObservableProperty] private bool _hasResult;
    [ObservableProperty] private string _thinkText = string.Empty;
    [ObservableProperty] private bool _hasThink;
    [ObservableProperty] private string _languageDisplay;
    [ObservableProperty] private bool _hasLanguage;
    [ObservableProperty] private ObservableCollection<string> _engines;
    [ObservableProperty] private string _selectedEngine;
    [ObservableProperty] private bool _isOllamaVisible;
    [ObservableProperty] private bool _isLlamaCppVisible;
    [ObservableProperty] private bool _isOpenAiCompatibleVisible;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private string _openAiCompatibleUrl;
    [ObservableProperty] private string _openAiCompatibleModel;
    [ObservableProperty] private string _openAiCompatibleApiKey;
    [ObservableProperty] private ObservableCollection<LlamaCppModelDisplay> _llamaCppModels;
    [ObservableProperty] private LlamaCppModelDisplay? _selectedLlamaCppModel;

    public Window? Window { get; set; }
    public bool ApplyPressed { get; private set; }
    public string ResultToApply { get; private set; } = string.Empty;

    private readonly IWindowService _windowService;
    private string _language;
    private double _maxCharsPerSecond;
    private int _maxSingleLineLength;
    private CancellationTokenSource? _cancellationTokenSource;

    public AiAssistantViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        _currentText = string.Empty;
        _contextText = string.Empty;
        _questionText = string.Empty;
        _resultText = string.Empty;
        _statusText = string.Empty;
        _languageDisplay = string.Empty;
        _language = "the same language as the text";
        _maxCharsPerSecond = 20;
        _maxSingleLineLength = 42;

        var review = Se.Settings.Tools.AiReview;
        Engines = new ObservableCollection<string> { SeAiReview.EngineLlamaCpp, SeAiReview.EngineOllama, SeAiReview.EngineOpenAiCompatible };
        SelectedEngine = Engines.Contains(review.Engine) ? review.Engine : SeAiReview.EngineLlamaCpp;
        OllamaModel = review.OllamaModel;
        OpenAiCompatibleUrl = review.OpenAiCompatibleUrl;
        OpenAiCompatibleModel = review.OpenAiCompatibleModel;
        OpenAiCompatibleApiKey = review.OpenAiCompatibleApiKey;
        LlamaCppModels = new ObservableCollection<LlamaCppModelDisplay>();
        SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(
            LlamaCppModels,
            LlamaCppServerManager.GetAllReviewModels(),
            review.LlamaCppModelFileName);

        UpdateEngineVisibility();
    }

    public void Initialize(string currentText, string contextText, string? languageName,
        double maxCharsPerSecond, int maxSingleLineLength)
    {
        CurrentText = currentText ?? string.Empty;
        ContextText = contextText ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(languageName))
        {
            _language = languageName!;
            LanguageDisplay = languageName!;
        }

        if (maxCharsPerSecond > 0)
        {
            _maxCharsPerSecond = maxCharsPerSecond;
        }

        if (maxSingleLineLength > 0)
        {
            _maxSingleLineLength = maxSingleLineLength;
        }
    }

    partial void OnSelectedEngineChanged(string value)
    {
        UpdateEngineVisibility();
    }

    partial void OnIsBusyChanged(bool value)
    {
        IsNotBusy = !value;
    }

    partial void OnResultTextChanged(string value)
    {
        HasResult = !string.IsNullOrWhiteSpace(value);
    }

    partial void OnLanguageDisplayChanged(string value)
    {
        HasLanguage = !string.IsNullOrEmpty(value);
    }

    partial void OnThinkTextChanged(string value)
    {
        HasThink = !string.IsNullOrWhiteSpace(value);
    }

    private void UpdateEngineVisibility()
    {
        IsOllamaVisible = SelectedEngine == SeAiReview.EngineOllama;
        IsOpenAiCompatibleVisible = SelectedEngine == SeAiReview.EngineOpenAiCompatible;
        IsLlamaCppVisible = !IsOllamaVisible && !IsOpenAiCompatibleVisible;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.AiReview;
        settings.Engine = SelectedEngine;
        settings.OllamaModel = OllamaModel.Trim();
        settings.LlamaCppModelFileName = SelectedLlamaCppModel?.Model.FileName ?? string.Empty;
        settings.OpenAiCompatibleUrl = OpenAiCompatibleUrl.Trim();
        settings.OpenAiCompatibleModel = OpenAiCompatibleModel.Trim();
        settings.OpenAiCompatibleApiKey = OpenAiCompatibleApiKey.Trim();
        Se.SaveSettings();
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
    private Task FixErrors() => RunAsync(
        "Fix only spelling, grammar and punctuation errors in the current subtitle line, in " + _language + ". " +
        "Do not rephrase, do not change meaning, tone or style. Keep names, slang and intentional dialect. " +
        "Keep formatting tags and line breaks. Return only the corrected line, nothing else.");

    [RelayCommand]
    private Task FitReadingSpeed() => RunAsync(
        "Rephrase the current subtitle line in " + _language + " so it is shorter and easier to read, " +
        "keeping the exact meaning. Aim for at most " + _maxSingleLineLength.ToString(CultureInfo.InvariantCulture) +
        " characters per line and a comfortable reading speed of about " +
        _maxCharsPerSecond.ToString(CultureInfo.InvariantCulture) + " characters per second. " +
        "Keep formatting tags. Return only the rephrased line, nothing else.");

    [RelayCommand]
    private Task MakeFormal() => RunAsync(
        "Rewrite the current subtitle line in " + _language + " in a more formal register, keeping the meaning. " +
        "Keep formatting tags and line breaks. Return only the rewritten line, nothing else.");

    [RelayCommand]
    private Task MakeInformal() => RunAsync(
        "Rewrite the current subtitle line in " + _language + " in a more casual, colloquial register, keeping the meaning. " +
        "Keep formatting tags and line breaks. Return only the rewritten line, nothing else.");

    [RelayCommand]
    private Task Ask()
    {
        if (string.IsNullOrWhiteSpace(QuestionText))
        {
            return Task.CompletedTask;
        }

        return RunAsync(
            "You are helping a subtitler with the current subtitle line, in " + _language + ". " +
            "Use the surrounding lines only as context. Answer the request. " +
            "If the request asks to change the line, return only the new line text with no explanation; " +
            "otherwise answer briefly.\n\nRequest: " + QuestionText.Trim());
    }

    private async Task RunAsync(string instruction)
    {
        if (IsBusy || Window == null)
        {
            return;
        }

        SaveSettings();

        var systemPrompt =
            "You are a professional subtitle assistant. You are given the current subtitle line and, for context " +
            "only, the lines before and after it. Follow the instruction. Never invent content that is not implied " +
            "by the line. When returning a changed line, return only the line text.";

        var userContent =
            "Context (previous and next lines, do not change these):\n" + ContextText +
            "\n\nCurrent line:\n" + CurrentText +
            "\n\nInstruction:\n" + instruction;

        string url;
        var model = string.Empty;
        string? apiKey = null;

        try
        {
            IsBusy = true;
            StatusText = SelectedEngine + "...";
            ResultText = string.Empty;
            ThinkText = string.Empty;

            if (SelectedEngine == SeAiReview.EngineOpenAiCompatible)
            {
                url = OpenAiCompatibleUrl.Trim();
                model = OpenAiCompatibleModel.Trim();
                apiKey = string.IsNullOrWhiteSpace(OpenAiCompatibleApiKey)
                    ? null
                    : OpenAiCompatibleApiKey.Trim();
            }
            else if (SelectedEngine == SeAiReview.EngineOllama)
            {
                url = Se.Settings.Tools.AiReview.OllamaUrl;
                model = OllamaModel.Trim();
            }
            else
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

                await LlamaCppServerManager.EnsureServerRunningAsync(display.Model, CancellationToken.None);
                url = LlamaCppServerManager.ApiUrl;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            using var client = new AiReviewClient();
            var reply = await client.ChatAsync(url, model, systemPrompt, userContent,
                _cancellationTokenSource.Token, apiKey, preferJsonObject: false);
            ResultText = CleanReply(reply, out var thinkText);
            ThinkText = thinkText;
        }
        catch (OperationCanceledException)
        {
            // the user closed the window or cancelled the request
        }
        catch (Exception e)
        {
            ResultText = string.Empty;
            await MessageBox.Show(Window, Se.Language.General.Error, e.Message,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsBusy = false;
            StatusText = string.Empty;
        }
    }

    private static string CleanReply(string reply, out string thinkText)
    {
        thinkText = string.Empty;
        if (string.IsNullOrWhiteSpace(reply))
        {
            return string.Empty;
        }

        var text = reply.Trim();

        // Reasoning models (DeepSeek R1, Qwen3, ...) may emit their chain of thought in a
        // <think>...</think> block before the answer - some chat templates even swallow the
        // opening tag, so key off the closing tag and keep only what follows it. The
        // reasoning is kept so the UI can offer it behind an info button.
        var endThink = text.LastIndexOf("</think>", StringComparison.OrdinalIgnoreCase);
        if (endThink >= 0)
        {
            thinkText = StripThinkTags(text[..endThink]);
            text = text[(endThink + "</think>".Length)..].Trim();
        }
        else if (text.StartsWith("<think>", StringComparison.OrdinalIgnoreCase))
        {
            // Unterminated reasoning block - the answer never arrived (e.g. cut off by the
            // token limit), so there is nothing usable to show.
            thinkText = StripThinkTags(text);
            return string.Empty;
        }

        // A model may still answer with a JSON object (some ignore the plain-text
        // request); take the first string value out of it.
        if (text.StartsWith("{", StringComparison.Ordinal))
        {
            var fromJson = TryExtractJsonString(text);
            if (!string.IsNullOrWhiteSpace(fromJson))
            {
                text = fromJson.Trim();
            }
        }

        // Some models wrap the answer in a code fence; strip it.
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstBreak = text.IndexOf('\n');
            if (firstBreak > 0)
            {
                text = text[(firstBreak + 1)..];
            }

            if (text.EndsWith("```", StringComparison.Ordinal))
            {
                text = text[..^3];
            }

            text = text.Trim();
        }

        // Or in surrounding quotes.
        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
        {
            text = text[1..^1].Trim();
        }

        return text;
    }

    private static string StripThinkTags(string text)
    {
        return text
            .Replace("<think>", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("</think>", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string? TryExtractJsonString(string text)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var value = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
        }
        catch
        {
            // Not valid JSON; leave the text as it is.
        }

        return null;
    }

    [RelayCommand]
    private void Apply()
    {
        if (string.IsNullOrWhiteSpace(ResultText))
        {
            return;
        }

        SaveSettings();
        ResultToApply = ResultText.Trim();
        ApplyPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _cancellationTokenSource?.Cancel();
            Window?.Close();
        }
    }

    internal void OnClosing()
    {
        SaveSettings();
        _cancellationTokenSource?.Cancel();
        UiUtil.SaveWindowPosition(Window);
    }
}
