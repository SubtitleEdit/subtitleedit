using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main.AiAssistant;

/// <summary>
/// The subtitle text box AI assistant: quick actions and a free question for the
/// current line, with the neighbouring lines given as context. Reuses the same
/// engines, models and settings as Tools -> AI review (Ollama, llama.cpp, or an
/// OpenAI compatible server); nothing is applied automatically, the user reviews
/// the suggestion and presses Apply.
/// </summary>
public partial class AiAssistantViewModel : ObservableObject
{
    [ObservableProperty] private string _currentText;
    [ObservableProperty] private string _contextText;
    [ObservableProperty] private string _questionText;
    [ObservableProperty] private string _resultText;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _selectedEngine;

    public Window? Window { get; set; }
    public bool ApplyPressed { get; private set; }
    public string ResultToApply { get; private set; } = string.Empty;

    private string _language;
    private double _maxCharsPerSecond;
    private int _maxSingleLineLength;
    private CancellationTokenSource? _cancellationTokenSource;

    public AiAssistantViewModel()
    {
        _currentText = string.Empty;
        _contextText = string.Empty;
        _questionText = string.Empty;
        _resultText = string.Empty;
        _statusText = string.Empty;
        _language = "the same language as the text";
        _maxCharsPerSecond = 20;
        _maxSingleLineLength = 42;
        _selectedEngine = string.IsNullOrEmpty(Se.Settings.Tools.AiReview.Engine)
            ? SeAiReview.EngineLlamaCpp
            : Se.Settings.Tools.AiReview.Engine;
    }

    public void Initialize(string currentText, string contextText, string? languageName,
        double maxCharsPerSecond, int maxSingleLineLength)
    {
        CurrentText = currentText ?? string.Empty;
        ContextText = contextText ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(languageName))
        {
            _language = languageName!;
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

        var systemPrompt =
            "You are a professional subtitle assistant. You are given the current subtitle line and, for context " +
            "only, the lines before and after it. Follow the instruction. Never invent content that is not implied " +
            "by the line. When returning a changed line, return only the line text.";

        var userContent =
            "Context (previous and next lines, do not change these):\n" + ContextText +
            "\n\nCurrent line:\n" + CurrentText +
            "\n\nInstruction:\n" + instruction;

        var review = Se.Settings.Tools.AiReview;
        string url;
        var model = string.Empty;
        string? apiKey = null;

        try
        {
            IsBusy = true;
            StatusText = SelectedEngine + "...";
            ResultText = string.Empty;

            if (SelectedEngine == SeAiReview.EngineOpenAiCompatible)
            {
                url = (review.OpenAiCompatibleUrl ?? string.Empty).Trim();
                model = (review.OpenAiCompatibleModel ?? string.Empty).Trim();
                apiKey = string.IsNullOrWhiteSpace(review.OpenAiCompatibleApiKey)
                    ? null
                    : review.OpenAiCompatibleApiKey.Trim();
            }
            else if (SelectedEngine == SeAiReview.EngineOllama)
            {
                url = review.OllamaUrl;
                model = (review.OllamaModel ?? string.Empty).Trim();
            }
            else
            {
                var reviewModel = LlamaCppServerManager.GetAllReviewModels()
                    .FirstOrDefault(m => m.FileName == review.LlamaCppModelFileName);
                if (reviewModel == null)
                {
                    await MessageBox.Show(Window, Se.Language.Tools.AiReview.Title,
                        Se.Language.Tools.AiReview.SetupHint, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                await LlamaCppServerManager.EnsureServerRunningAsync(reviewModel, CancellationToken.None);
                url = LlamaCppServerManager.ApiUrl;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            using var client = new AiReviewClient();
            var reply = await client.ChatAsync(url, model, systemPrompt, userContent,
                _cancellationTokenSource.Token, apiKey, preferJsonObject: false);
            ResultText = CleanReply(reply);
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

    private static string CleanReply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return string.Empty;
        }

        var text = reply.Trim();

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
}
